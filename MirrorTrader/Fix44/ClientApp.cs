using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFix;
using FIX44 = QuickFix.FIX44;
using QuickFix.Fields;
using QuickFix.Transport;
using MirrorTrader.Enums;

namespace MirrorTrader.Fix44
{
    public class ClientApp
    {
        private SocketInitiator client;
        QuickFixApp myApp;
        SessionID sessionID;
        public ClientApp(string sessionConfig)
        {
            SessionSettings settings = new SessionSettings();
            var dictionary = new QuickFix.Dictionary();

            // Set the session settings
            dictionary.SetString("BeginString", "FIX.4.4");
            //dictionary.SetString("SenderCompID", "demo.ctrader.3701900");
            //dictionary.SetString("TargetCompID", "cServer");
            dictionary.SetString("SocketConnectHost", "h43.p.ctrader.com");
            /*dictionary.SetString("SocketConnectPort", "5201");
            dictionary.SetString("SenderSubID", "QUOTE");
            dictionary.SetString("TargetSubID", "QUOTE");*/
            dictionary.SetString("SocketConnectPort", "5202");
            dictionary.SetString("SenderSubID", "TRADE");
            dictionary.SetString("TargetSubID", "TRADE");
            dictionary.SetString("ConnectionType", "initiator");
            dictionary.SetString("ReconnectInterval", "60");
            dictionary.SetString("FileStorePath", "store");
            dictionary.SetString("FileLogPath", "log");
            dictionary.SetString("HeartBtInt", "20");
            dictionary.SetString("StartTime", "00:30:00");
            dictionary.SetString("EndTime", "23:30:00");
            dictionary.SetString("ResetSeqNumFlag", "Y");
            dictionary.SetString("ResetOnLogon", "Y");
            dictionary.SetString("AllowUnknownMsgFields", "Y");
            //dictionary.SetString("SocketUseSSL", "Y");
            //dictionary.SetString("SSLValidateCertificates", "N");
            //dictionary.SetString("UseDataDictionary", "N");
            dictionary.SetString("DataDictionary", "Fix44/spec/FIX44.xml");

            dictionary.SetString("ValidateUserDefinedFields", "N");
            dictionary.SetString("ValidateIncomingMessage", "N");
            dictionary.SetString("ValidateOutgoingMessage", "N");
            //dictionary.Add("SocketUseSSL", "Y");
            //dictionary.Add("SocketKeyStore", "YOUR_KEYSTORE_PATH");
            //dictionary.Add("SocketKeyStorePassword", "YOUR_KEYSTORE_PASSWORD");
            //dictionary.Add("SocketTrustStore", "YOUR_TRUSTSTORE_PATH");
            //dictionary.Add("SocketTrustStorePassword", "YOUR_TRUSTSTORE_PASSWORD");

            // Set the username
            //dictionary.SetString("Username", "3701900");
            // Set the password
            //dictionary.SetString("Password", "Wapzan.com2");

            // Create the session ID
            //var sessionID = new SessionID("FIX.4.4", "demo.ctrader.3701900", "QUOTE", "cServer", "QUOTE");
            var sessionID = new SessionID("FIX.4.4", "demo.ctrader.3701900", "TRADE", "cServer", "TRADE");
            // Add the dictionary to the session settings
            settings.Set(sessionID, dictionary);

            myApp = new QuickFixApp();
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            //var logFactory = new ScreenLogFactory(settings); // use ScreenLog factory
            client = new SocketInitiator(
                myApp,
                storeFactory,
                settings,
                logFactory);

            
            client.Start();

        }

        public void Start()
        {    
            client.Start();
        }

        public void Stop()
        {
            client.Stop();
        }

        public void GetMarketData(string symbol)
        {
            myApp.MarketDataRequest(symbol);
        }

        public void PlaceMarketOrder(Order order, OrderType orderType, double lots, string currency)
        {
            myApp.NewOrderSingle(order, orderType, lots, currency);
        }

        public void CloseMarketOrder(Order order, OrderType orderType, double lots, string currency)
        {
            myApp.CloseOrder(order, orderType, lots, currency);
        }
    }
}
