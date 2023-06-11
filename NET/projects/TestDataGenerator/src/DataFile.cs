using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


namespace TestDataGenerator
{
    public class DataFile
    {
        [Flags]
        public enum Tag : byte { Empty = 0, HaveData = 0x1 }

        public enum Dir : sbyte { None = 0, Up = +1, Down = -1 };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Entry
        { 
            public Tag Tag;
            public Dir Dir;
            public double VA;
            public double Median;

            public Entry(Dir dir, double va, double median) { Tag = Tag.HaveData; Dir = dir; VA = va; Median = median; }
        }
        
        private const string Magic = ":CCDATA:";
        private static readonly long HeaderSize = Magic.Length + sizeof(uint) + 2 * sizeof(long);
        private static readonly long EntrySize = Marshal.SizeOf<Entry>();

        private FileStream _fs;
        private string _symbol;
        private VO.Timeframe _timeframe;
        private int _year;
        private long _yearTimestamp;
        private long _entryCount;
        private bool _hasData = false;

        public DataFile(FileStream fs, string symbol, VO.Timeframe timeframe, int year)
        { 
            _fs = fs;
            _symbol = symbol;
            _timeframe = timeframe;
            _year = year;
        }

        public void Init()
        {
            var yearStart = new DateTime(_year, 1, 1);
            _yearTimestamp = new DateTimeOffset(DateTime.SpecifyKind(yearStart, DateTimeKind.Utc)).ToUnixTimeSeconds();
            long dailyCount = 1440L / (long) _timeframe;
            long totalDays = (long) (yearStart.AddYears(1) - yearStart).TotalDays;
            _entryCount = totalDays * dailyCount;
            long size = HeaderSize + _entryCount * EntrySize;
            if (_fs.Length == 0)
            {
                _fs.SetLength(size);
                _fs.Seek(0, SeekOrigin.Begin);
                using var writer = new BinaryWriter(_fs, Encoding.ASCII, true);
                writer.Write(Magic.ToCharArray(), 0, Magic.Length);
                writer.Write((uint) _timeframe);
                writer.Write(_yearTimestamp);
                writer.Write(_entryCount);
                var emptyDailyEntries = new byte[dailyCount * EntrySize];
                for (long i = 1; i <= totalDays; i++) _fs.Write(emptyDailyEntries);
            }
            else if (_fs.Length == size)
            {
                using var reader = new StreamReader(_fs, Encoding.ASCII, leaveOpen: true);
                var buf = new char[Magic.Length];
                reader.Read(buf, 0, buf.Length);
                if (new string(buf) != Magic)
                    throw new ApplicationException("Wrong magic is encountered in data file.");
                _hasData = true;

            }
            else
                throw new ApplicationException("Unexpected data file length.");
        }

        public void WriteEntries(DateTime startTime, Entry[] entries)
        { 
            long timestamp = new DateTimeOffset(DateTime.SpecifyKind(startTime, DateTimeKind.Utc)).ToUnixTimeSeconds();
            if (timestamp < _yearTimestamp) throw new ApplicationException($"Time {startTime} does not belong to the year {_year}.");
            long startEntryIdx = Math.DivRem(timestamp - _yearTimestamp, 60 * ((long) _timeframe), out long rem);
            if (rem != 0) throw new ApplicationException($"Time {startTime} is invalid for the timeframe '{_timeframe}'.");
            long entriesLen = entries.Length;
            long endEntryIdx = startEntryIdx + entriesLen;
            if (_entryCount < endEntryIdx) throw new ApplicationException($"Those {entriesLen} entries would overflow the data file.");
            long startOffset = HeaderSize + startEntryIdx * EntrySize;
            using var writer = new BinaryWriter(_fs, Encoding.ASCII, true);
            _fs.Seek(startOffset, SeekOrigin.Begin);
            for (int i = 0 ; i < entriesLen ; i ++)
            { 
                ref Entry entry = ref entries[i];
                if (entry.Tag != Tag.Empty)
                {
                    writer.Write((byte) entry.Tag);
                    writer.Write((sbyte) entry.Dir);
                    writer.Write(entry.VA);
                    writer.Write(entry.Median);
                }
                else
                    _fs.Seek(EntrySize, SeekOrigin.Current);
            }
        }

        public void Close() => _fs.Dispose();
    }
}
