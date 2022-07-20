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
        public double Volume { get; set; }
        public ENUM_ORDER_TYPE OrderType { get; set; }
        public ENUM_TRADE_TRANSACTION_TYPE TransactionType { get; set; }
        public ENUM_TRADE_TRANSACTION_TYPE DealType { get; set; }
    }
}
