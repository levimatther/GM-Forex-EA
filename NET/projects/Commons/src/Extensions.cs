using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Commons.Extensions
{
    public static class All
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string me, string what) => me.IndexOf(what) > -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIn<T>(this T item, params T[] range) => range.Contains(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Fmt(this string input, params object[] args) => string.Format(input, args);

        private static readonly decimal DecimalOne28 = new decimal(0x10000000, 0x3E250261, 0x204FCE5E, false, 28);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Normalize(this decimal dec) => dec / DecimalOne28;
    }

    public static class TaskExtensions
    {
        public static Task ContinueWithPost(this Task task, AsyncService svc, Action<Task> action) =>
            task.ContinueWith((t) => svc.Post(() => action(t)));

        public static Task ContinueWithPost<TResult>(this Task<TResult> task, AsyncService svc, Action<Task<TResult>> action) =>
            task.ContinueWith((t) => svc.Post(() => action(t)));
    }
}
