using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFix;
using QuickFix.Transport;
using QuickFix.Fields;
using QuickFix.Fields.Converters;
using FIX44 = QuickFix.FIX44;
using Message = QuickFix.Message;

namespace MirrorTrader.Fix44
{
    public class FixClient : MessageCracker, IApplication
    {
        // fix msg separator chars
        private const char cEql = (char)61;
        private const char cFixSep = (char)1;
        string _testCurrency = "XBTUSD";

        // session
        public Session _session = null;
        public void FromApp(Message message, SessionID sessionID)
        {
            //Console.WriteLine("-IN:  " + message.ToString());
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
        public void OnCreate(SessionID sessionID)
        {
            _session = Session.LookupSession(sessionID);
            if (!_session.IsLoggedOn)
            {
                Console.WriteLine("not logged");
                //_session.Logon();
            }
        }
        public void OnLogout(SessionID sessionID)
        {
            Console.WriteLine("Logout - " + sessionID.ToString());
        }
        public void OnLogon(SessionID sessionID)
        {
            Console.WriteLine("Logon: " + sessionID);
            QueryMarketDataRequest(_testCurrency);
        }
        public void FromAdmin(Message msg, SessionID sessionID) { }
        public void ToAdmin(Message message, SessionID sessionID)
        {
            // add user info to message header
            if (message.Header.GetString(Tags.MsgType) == MsgType.LOGON)
            {
                message.SetField(new Username("3701900"));
                message.SetField(new Password("Wapzan.com2"));
                message.SetField(new ResetSeqNumFlag(true));
                message.SetField(new SenderSubID("3701900"));
            }
        }
        public void ToApp(Message message, SessionID sessionID)
        {
            try
            {
                bool possDupFlag = false;
                if (message.Header.IsSetField(Tags.PossDupFlag))
                {
                    possDupFlag = BoolConverter.Convert(
                        message.Header.GetString(Tags.PossDupFlag)); /// FIXME
                }
                if (possDupFlag)
                    throw new DoNotSend();
            }
            catch (FieldNotFoundException)
            { }

            Console.WriteLine();
            Console.WriteLine("OUT: " + message.ToString());
        }

        #region MessageCracker handlers
        public void OnMessage(FIX44.MarketDataSnapshotFullRefresh m, SessionID s)
        {
            // get ask and bid prices
            string[] data = m.ToString().Split(new char[] { cFixSep, cEql });
            if (data.Length == 33)
            {
                double bid = Convert.ToDouble(data[25]);
                double ask = Convert.ToDouble(data[29]);
                //TickHandler.OnTick(bid, ask);
                //Console.WriteLine("Received market data. Bid: {0}, Ask: {1}", bid, ask);
            }
            //Console.WriteLine("Received market data. Bid: {0}, Ask: {1}", bid, ask);
        }
        public void OnMessage(FIX44.Heartbeat m, SessionID s)
        {
            Console.WriteLine("Received heartbeat.");
        }
        public void OnMessage(FIX44.MarketDataRequest m, SessionID s)
        {
            Console.WriteLine("Received market data request");
        }
        public void OnMessage(FIX44.Logon m, SessionID s)
        {
            Console.WriteLine("Received logon msg");
        }
        public void OnMessage(FIX44.ExecutionReport m, SessionID s)
        {
            Console.WriteLine("Received execution report");
        }

        public void OnMessage(FIX44.OrderCancelReject m, SessionID s)
        {
            Console.WriteLine("Received order cancel reject");
        }
        #endregion

        private void SendMessage(Message message)
        {
            if (_session != null)
                _session.Send(message);
            else
            {
                // This probably won't ever happen.
                Console.WriteLine("Can't send message: session not created.");
            }
        }

        public void QueryMarketDataRequest(string Currency)
        {
            Console.WriteLine("\nMarketDataRequest");

            FIX44.MarketDataRequest m = QueryMarketDataRequest44(FixSymbols.Find(Currency).id);

            SendMessage(m);
        }

        private FIX44.MarketDataRequest QueryMarketDataRequest44(int Currency)
        {
            MDReqID mdReqID = new MDReqID("MARKETDATAID");
            SubscriptionRequestType subType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES);
            MarketDepth marketDepth = new MarketDepth(1);

            FIX44.MarketDataRequest.NoMDEntryTypesGroup marketDataEntryGroup = new FIX44.MarketDataRequest.NoMDEntryTypesGroup();
            marketDataEntryGroup.Set(new MDEntryType(MDEntryType.BID));
            marketDataEntryGroup.Set(new MDEntryType(MDEntryType.OFFER));

            FIX44.MarketDataRequest.NoRelatedSymGroup symbolGroup = new FIX44.MarketDataRequest.NoRelatedSymGroup();
            symbolGroup.Set(new Symbol(Currency.ToString()));

            FIX44.MarketDataRequest message = new FIX44.MarketDataRequest(mdReqID, subType, marketDepth);
            message.SetField(new MDUpdateType(MDUpdateType.INCREMENTAL_REFRESH));
            message.AddGroup(marketDataEntryGroup);
            message.AddGroup(symbolGroup);

            return message;
        }

    }


    public class QuickFixT
    {
        SocketInitiator initiator;
        FixClient fixClient;
        public void Init()
        {
            SessionSettings settings = new SessionSettings("fixsettings.cfg");
            fixClient = new FixClient();
            string _testCurrency = "XBTUSD"; //"BTCUSD";

            // set currency specific file store
            SessionID sessionID = settings.GetSessions().FirstOrDefault();
            var customSettings = settings.Get(sessionID);
            customSettings.SetString("FILESTOREPATH", "store" + _testCurrency);
            settings.Remove(sessionID);
            settings.Set(sessionID, customSettings);

            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            initiator = new SocketInitiator(fixClient, storeFactory, settings);

            Console.WriteLine("Starting initiator...");
            initiator.Start();
            //initiator.Stop();
        }

        public void GetMarketData(string Currency)
        {
            // get currency data
            fixClient.QueryMarketDataRequest(Currency);
        }

        public void Destroy()
        {
            // stop connection
            initiator.Stop();
        }

        public void Restart()
        {
            // restart connection
            if (initiator.IsStopped)
            {
                Console.WriteLine("Restarting initiator...");
                initiator.Start();
            }
        }
    }
}

