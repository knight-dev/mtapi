using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MirrorTrader.Enums;
using QuickFix;
using QuickFix.Fields;
using FIX44 = QuickFix.FIX44;

namespace MirrorTrader.Fix44
{
    public class QuickFixApp : MessageCracker, IApplication
    {
        private SessionID _sessionID { get; set; }
        Session _session = null;

        public void FromApp(Message message, SessionID sessionID) {
            Console.WriteLine("IN:  " + message.ToString());
            try
            {
                Crack(message, sessionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==Cracker exception==");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }
        public void OnCreate(SessionID sessionID) {
           
        }
        public void OnLogout(SessionID sessionID) { Console.WriteLine("Logout - " + sessionID.ToString()); }
        public void OnLogon(SessionID sessionID) {
            _sessionID = sessionID;
            _session = Session.LookupSession(sessionID);
            Console.WriteLine("Logon test - " + sessionID.ToString());
            //MarketDataRequest("EURUSD"); // get price quotes
        }
        public void FromAdmin(Message msg, SessionID sessionID) {
            //Console.WriteLine("From admin - " + msg);
        }
        public void ToAdmin(Message message, SessionID sessionID) {
            if (message.GetType() == typeof(QuickFix.FIX44.Logon))
            {
                message.SetField(new Username("3701900"));
                message.SetField(new Password("Wapzan.com2"));
                //message.SetField(new QuickFix.Fields.ResetSeqNumFlag(true));
            }

            //Console.WriteLine("to admin");

        }
        public void ToApp(Message message, SessionID sessionID) {
            try
            {
                bool possDupFlag = false;
                if (message.Header.IsSetField(QuickFix.Fields.Tags.PossDupFlag))
                {
                    possDupFlag = QuickFix.Fields.Converters.BoolConverter.Convert(
                        message.Header.GetString(QuickFix.Fields.Tags.PossDupFlag)); /// FIXME
                }
                if (possDupFlag)
                    throw new DoNotSend();
            }
            catch (FieldNotFoundException)
            { }


            Console.WriteLine();
            Console.WriteLine("OUT: " + message.ToString());
        }

        private void SendMessage(Message m)
        {
            if (_session != null)
                _session.Send(m);
            else
            {
                // This probably won't ever happen.
                Console.WriteLine("Can't send message: session not created.");
            }
        }

        public void NewOrderSingle(Order order, OrderType orderType, double lots, string currency)
        {
            Console.WriteLine("\nNew Order: "+currency);

            FIX44.NewOrderSingle m = NewOrderSingle44(order, orderType, lots, currency);

            SendMessage(m);
        }
        public void CloseOrder(Order order, OrderType orderType, double lots, string currency)
        {
            Console.WriteLine("\nClose Order: " + currency);

            //FIX44.OrderCancelReplaceRequest m = OrderCancelReplaceRequest44(order, orderType, lots, currency);
            FIX44.NewOrderSingle m = CloseNewOrderSingle44(order, orderType, lots, currency);

            SendMessage(m);
        }

        private QuickFix.FIX44.NewOrderSingle NewOrderSingle44(Order order, OrderType orderType, double lots, string currency)
        {
            QuickFix.Fields.OrdType ordType = null;
            FixSymbol instrument = FixSymbols.Find(currency);
            Side side = new Side();
            if (orderType == OrderType.Buy)
            {
                side = new Side(Side.BUY);
            }
            else if (orderType == OrderType.Sell)
            {
                side = new Side(Side.SELL);
            }
            // set lot sizing : 100000 units = 1 lot
            int quantity = (int)(lots * 100000);
            QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
                new ClOrdID(order.TicketId.ToString()),
                new Symbol(instrument.id.ToString()),
                side,
                new TransactTime(DateTime.Now),
                ordType = new OrdType(OrdType.MARKET));

            //newOrderSingle.Set(new HandlInst('1'));
            newOrderSingle.Set(new OrderQty(Convert.ToDecimal(quantity)));
            newOrderSingle.Set(new TimeInForce(TimeInForce.IMMEDIATE_OR_CANCEL));
            /*if (ordType.getValue() == OrdType.LIMIT || ordType.getValue() == OrdType.STOP_LIMIT)
                newOrderSingle.Set(new Price(Convert.ToDecimal(order.Price)));
            if (ordType.getValue() == OrdType.STOP || ordType.getValue() == OrdType.STOP_LIMIT)
                newOrderSingle.Set(new Price(Convert.ToDecimal(order.SourcePrice)));*/

            return newOrderSingle;
        }

        private QuickFix.FIX44.NewOrderSingle CloseNewOrderSingle44(Order order, OrderType orderType, double lots, string currency)
        {
            QuickFix.Fields.OrdType ordType = null;
            FixSymbol instrument = FixSymbols.Find(currency);
            Side side = new Side();
            if (orderType == OrderType.Buy)
            {
                side = new Side(Side.BUY);
            }
            else if (orderType == OrderType.Sell)
            {
                side = new Side(Side.SELL);
            }
            // set lot sizing : 100000 units = 1 lot
            int quantity = (int)(lots * 100000);
            QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
                new ClOrdID(order.TicketId.ToString()),
                new Symbol(instrument.id.ToString()),
                side,
                new TransactTime(DateTime.Now),
                ordType = new OrdType(OrdType.MARKET));

            //newOrderSingle.Set(new HandlInst('1'));
            newOrderSingle.Set(new OrderQty(Convert.ToDecimal(quantity)));
            //newOrderSingle.Set(new TimeInForce(TimeInForce.IMMEDIATE_OR_CANCEL));
            //newOrderSingle.SetField(new OrigClOrdID(order.TicketId.ToString())); // set to the ClOrdID of the NewOrderSingle
            newOrderSingle.SetField(new PosMaintRptID(order.PositionId.ToString()));
            /*if (ordType.getValue() == OrdType.LIMIT || ordType.getValue() == OrdType.STOP_LIMIT)
                newOrderSingle.Set(new Price(Convert.ToDecimal(order.Price)));
            if (ordType.getValue() == OrdType.STOP || ordType.getValue() == OrdType.STOP_LIMIT)
                newOrderSingle.Set(new Price(Convert.ToDecimal(order.SourcePrice)));*/

            return newOrderSingle;
        }

        private QuickFix.FIX44.OrderCancelReplaceRequest OrderCancelReplaceRequest44(Order order, OrderType orderType, double lots, string currency)
        {
            
            FixSymbol instrument = FixSymbols.Find(currency);
            Side side = new Side();
            if (orderType == OrderType.Buy)
            {
                side = new Side(Side.BUY);
            }
            else if (orderType == OrderType.Sell)
            {
                side = new Side(Side.SELL);
            }
            // set lot sizing : 100000 units = 1 lot
            int quantity = (int)(lots * 100000);

            var orderCancelReplaceRequest = new QuickFix.FIX44.OrderCancelReplaceRequest();
            orderCancelReplaceRequest.SetField(new OrigClOrdID(order.TicketId.ToString())); // set to the ClOrdID of the NewOrderSingle
            orderCancelReplaceRequest.SetField(new ClOrdID(order.TicketId.ToString() + "C")); // set to a new ClOrdID for the cancel/replace request
            orderCancelReplaceRequest.SetField(new Symbol(instrument.id.ToString()));
            orderCancelReplaceRequest.SetField(side);
            orderCancelReplaceRequest.SetField(new OrdType(OrdType.MARKET));
            orderCancelReplaceRequest.SetField(new OrderQty(Convert.ToDecimal(quantity))); // set to the quantity you want to close
            return orderCancelReplaceRequest;
        }

        public void MarketDataRequest(string Currency)
        {
            Console.WriteLine("\nMarketDataRequest");

            FIX44.MarketDataRequest m = MarketDataRequest44(FixSymbols.Find(Currency).id.ToString());

            SendMessage(m);
        }

        private FIX44.MarketDataRequest MarketDataRequest44(string Currency)
        {
            MDReqID mdReqID = new MDReqID("MARKETDATAID");
            SubscriptionRequestType subType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES);
            MarketDepth marketDepth = new MarketDepth(1);

            FIX44.MarketDataRequest.NoMDEntryTypesGroup marketDataEntryGroup = new FIX44.MarketDataRequest.NoMDEntryTypesGroup();
            marketDataEntryGroup.Set(new MDEntryType(MDEntryType.BID));

            FIX44.MarketDataRequest.NoRelatedSymGroup symbolGroup = new FIX44.MarketDataRequest.NoRelatedSymGroup();
            symbolGroup.Set(new Symbol(Currency));

            FIX44.MarketDataRequest message = new FIX44.MarketDataRequest(mdReqID, subType, marketDepth);
            message.AddGroup(marketDataEntryGroup);
            message.AddGroup(symbolGroup);

            return message;
        }

        #region MessageCracker handlers
        public void OnMessage(FIX44.MarketDataRequest marketData, SessionID sessionID)
        {
           Console.WriteLine("Received market data");
        }
        public void OnMessage(FIX44.MarketDataIncrementalRefresh marketDataSnapshotFullRefresh, SessionID sessionID)
        {
            Console.WriteLine("Received market data incr refr");
        }
        public void OnMessage(FIX44.MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh, SessionID sessionID)
        {
            Console.WriteLine("Received market data full refr");
        }
        public void OnMessage(FIX44.MarketDataRequestReject marketDataRequestReject, SessionID sessionID)
        {
            Console.WriteLine("Rejected market data request");
        }
        public void OnMessage(FIX44.NewOrderSingle ord, SessionID sessionID)
        {
            Console.WriteLine("New order");
        }

        public void OnMessage(FIX44.SecurityDefinition secDef, SessionID sessionID)
        {
            //GotSecDef(secDef);
            Console.WriteLine("New security definition");
        }
        
        public void OnMessage(FIX44.ExecutionReport executionReport, SessionID s)
        {
            string orderId = executionReport.ClOrdID.getValue();
            string reportType = executionReport.ExecType.getValue().ToString();
            if (!string.IsNullOrEmpty(orderId)) {

                // Find order based on mt5 ticket id
                var ticket = ulong.Parse(orderId);
                Order order = Program.SourceOrderList.Where(x => x.TicketId == ticket).FirstOrDefault();

                // update position id
                if (order != null)
                {
                    string rawMsg = executionReport.ToString();
                    // fix msg separator chars
                    char cEql = (char)61;
                    char cFixSep = (char)1;
                    string[] data = rawMsg.Split(new char[] { cFixSep, cEql });

                    string positionId = executionReport.OrderID.getValue();

                    if(data.Length == 47 && reportType == "0")
                        order.PositionId = ulong.Parse(data[43]);

                    /*if (data.Length == 51)
                        order.PositionId = ulong.Parse(data[47]);*/

                    //order.Magic = ulong.Parse(positionId);
                }
            }
            Console.WriteLine("Received execution report");
        }

        public void OnMessage(FIX44.OrderCancelReject m, SessionID s)
        {
            Console.WriteLine("Received order cancel reject");
        }
        #endregion
    }
}
