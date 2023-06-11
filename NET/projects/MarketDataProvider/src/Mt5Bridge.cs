using System;
using System.Runtime.InteropServices;


namespace MarketDataProvider
{
    public static class Mt5Bridge
    {
        public enum Timeframe : uint { M1 = 1, M2 = 2, M5 = 5, M15 = 15, M30 = 30, H1 = 60 }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RawBuffer
        {
            public readonly IntPtr DataPtr;
            public readonly uint BufferSize;
            public uint DataSize;

            public RawBuffer(IntPtr dataPtr, uint bufferSize)
            {
                DataPtr = dataPtr;
                BufferSize = bufferSize;
                DataSize = 0;
            }
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DataParams
        {
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct TradeSetup
            {
                public uint TrendPeriod;
            }

            public Timeframe MainTimeframe;
            public Timeframe TrendTimeframe;
            public TradeSetup WithTrend;
            public TradeSetup AgainstTrend;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MarketData
        {
            [Flags]
            public enum WhatEnum : uint { None = 0, Tick = 0x01, Signal = 0x02 };

            public enum DirEnum : int { None = 0, Up = +1, Down = -1 };

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct TickData
            {
                public ulong Time;
                public double Price;
            };

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct PriceAndDir
            {
                public double Price;
                public DirEnum Dir;
            };

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct SignalData
            {
                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                public struct Median
                {
                    public PriceAndDir M_0;
                    public PriceAndDir M_1;
                }

                public ulong Time;
                public PriceAndDir VA_0;
                public PriceAndDir VA_1;
                public PriceAndDir VA_2;
                public Median MedianW;
                public Median MedianA;
                public double ATR;
            };

            public WhatEnum What;
            public TickData Tick;
            public SignalData Signal;
        }

        [DllImport("mt5bridge_s.dll")]
        public static extern bool MT5S_Init([MarshalAs(UnmanagedType.LPStr)] string serviceURI);

        [DllImport("mt5bridge_s.dll")]
        public static extern bool MT5S_PollRequest(out bool haveRequest, ref RawBuffer clientId, ref RawBuffer symbol, ref DataParams dataParams);

        [DllImport("mt5bridge_s.dll")]
        public static extern bool MT5S_SendData(in RawBuffer clientId, in MarketData data);

        [DllImport("mt5bridge_s.dll")]
        public static extern bool MT5S_Shutdown();

        [DllImport("mt5bridge_s.dll")]
        public static extern void MT5S_GetLastMessage(ref int errNo, ref RawBuffer msg);
    }
}
