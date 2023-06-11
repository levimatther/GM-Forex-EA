using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Timers = System.Timers;


namespace Commons
{
    public class AsyncService
    {
        public delegate bool WillBlockDelegate();

        private volatile int _running = 0;
        private volatile Thread? _thread;
        private CancellationTokenSource? _stopSource;
        private CancellationToken _stopToken;
        private readonly BlockingCollection<Action> _callbackQueue;
        private readonly WillBlockDelegate _onWillBlock;

        public AsyncService(int queueLimit, WillBlockDelegate? onWillBlock = null)
        {
            _callbackQueue = new BlockingCollection<Action>(new ConcurrentQueue<Action>(), queueLimit);
            _onWillBlock = onWillBlock ?? (() => true);
        }

        public int QueuedCount { get => _callbackQueue.Count; }

        public void Run(string? name = null)
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) == 0)
            {
                _stopSource = new CancellationTokenSource();
                _stopToken = _stopSource.Token;
                _thread = new Thread(EventLoop) { Name = name };
                _thread.Start();
            }
        }

        public void Stop()
        {
            if (_thread != null && Interlocked.CompareExchange(ref _running, -1, 1) == 1)
            {
                if (_thread.ThreadState != ThreadState.Unstarted && _thread.ThreadState != ThreadState.Stopped)
                {
                    _stopSource?.Cancel();
                    if (Thread.CurrentThread.ManagedThreadId != _thread.ManagedThreadId) _thread.Join();
                }
                _thread = null;
                _running = 0;
            }
        }

        public void Post(Action callback)
        {
            if (_running == 1)
                try
                {
                    bool done = _callbackQueue.TryAdd(callback);
                    if (!done && _onWillBlock()) _callbackQueue.TryAdd(callback, Timeout.Infinite, _stopToken);
                }
                catch (OperationCanceledException)
                { }
        }

        public void ClearQueue()
        {
            while (_callbackQueue.TryTake(out var callback, 0));
        }

        private void EventLoop()
        {
            try
            {
                while (_callbackQueue.TryTake(out var callback, Timeout.Infinite, _stopToken))
                    callback?.Invoke();
            }
            catch (OperationCanceledException)
            { }
        }
    }

    public class AsyncScheduler
    {
        private readonly AsyncService _async;
        private readonly ConcurrentDictionary<long, Timers.Timer> _timers = new ConcurrentDictionary<long, Timers.Timer>();
        private long _lastTimerId = 0;

        public AsyncScheduler(AsyncService service) => _async = service;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Post(Action callback) => _async.Post(callback);

        public long Post(Action callback, double delay)
        {
            var timerId = Interlocked.Increment(ref _lastTimerId);
            var theTimer = new Timers.Timer(delay) { AutoReset = false };
            theTimer.Elapsed += (sender, e) => { if (_timers.TryRemove(timerId, out theTimer)) theTimer.Dispose(); _async.Post(callback); };
            if (_timers.TryAdd(timerId, theTimer)) theTimer.Start();
            return timerId;
        }

        public void ClearTimer(long timerId)
        {
            if (_timers.TryRemove(timerId, out var theTimer)) { theTimer.Stop(); theTimer.Dispose(); }
        }

        public void ClearAllTimers()
        {
            foreach (long timerId in _timers.Keys.ToArray())
                if (_timers.TryRemove(timerId, out var theTimer)) { theTimer.Stop(); theTimer.Dispose(); }
        }
    }
}
