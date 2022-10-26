using MtApi5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorTrader
{
    public class Order
    {
        public ulong TicketId { get; set; }
        public ulong Magic { get; set; }
        public bool Cloned { get; set; }
        public ulong DealId { get; set; }
        public ulong PositionId { get; set; }
        public string Symbol { get; set; }
        public double Price { get; set; }
        public double HighestPrice { get; set; }
        public double LowestPrice { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }
        public double Diff { get; set; }
        public double Factor { get; set; }
        public double ThresholdFactor { get; set; }
        public int Level { get; set; } = 0;
        public double baseTP { get; set; }
        public double thresholdTP { get; set; }
        public double TP { get; set; }
        public double SL { get; set; }
        public double Volume { get; set; }
        public ENUM_ORDER_TYPE OrderType { get; set; }
        public ENUM_TRADE_TRANSACTION_TYPE TransactionType { get; set; }
        public ENUM_TRADE_TRANSACTION_TYPE DealType { get; set; }
        public System.Timers.Timer timer { get; set; }
        public bool Closed { get; set; }
    }
}
