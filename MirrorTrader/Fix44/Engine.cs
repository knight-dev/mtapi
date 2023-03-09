using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using System.Threading;
using MirrorTrader.Enums;
using System.IO;

namespace MirrorTrader.Fix44
{
    public enum messageType
    {
        Quote,
        Trade
    }
    public class Engine
    {
        //* cTrader *//
        private int _pricePort = 5211; // ssl
        private int _tradePort = 5212; // ssl

        //private int _pricePort = 5201; // non ssl
        //private int _tradePort = 5202; // non ssl

        private string _host = "h43.p.ctrader.com";
        private string _username = "3701900";
        private string _password = "Wapzan.com2";
        private string _senderCompID = "demo.ctrader.3701900";
        private string _senderSubID = "3701900m";
        private string _senderTradeSubID = "3701900T";

        private string _targetCompID = "cServer";

        //* LMAX *//
        /*private int _pricePort = 443; // ssl
        private int _tradePort = 443; // ssl

        //private int _pricePort = 5201; // non ssl
        //private int _tradePort = 5202; // non ssl

        private string _host = "fix-order.london-demo.lmax.com"; // marketdata: fix-marketdata.london-demo.lmax.com
        private string _username = "antony2kx";
        private string _password = "~!Waper123";
        private string _senderCompID = "antony2kx";
        private string _senderSubID = "antony2kxm";
        private string _senderTradeSubID = "antony2kxT";

        private string _targetCompID = "LMXBD"; // market data: LMXBDM*/

        private string _testCurrency = "XBTUSD";
        private int _priceMessageSequenceNumber = 1;
        private int _tradeMessageSequenceNumber = 1;

        private int _testRequestID = 1;
        private TcpClient _priceClient;
        private SslStream _priceStreamSSL;
        private TcpClient _tradeClient;
        private SslStream _tradeStreamSSL;
        private MessageConstructor _messageConstructor;
        private MessageConstructor _messageTradeConstructor;
        private AsyncCallback quoteReceived;

        byte[] _priceBuffer = new byte[4096];
        byte[] _tradeBuffer = new byte[4096];

        static System.Timers.Timer _timer;
        static System.Timers.Timer _timer2;
        bool heartbeatPause = false;
        const double TIMEOUT = 30000;
        const double TIMEOUT2 = 10000;

        private string TestReqID = "";
        private string posID = "";

        // fix msg separator chars
        private const char cEql = (char)61;
        private const char cFixSep = (char)1;

        // currency
        FixSymbol symbol;

        public Engine()
        {
            _priceClient = new TcpClient(_host, _pricePort);
            _priceStreamSSL = new SslStream(_priceClient.GetStream(), false,
                        new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            _priceStreamSSL.AuthenticateAsClient(_host);
            _tradeClient = new TcpClient(_host, _tradePort);
            _tradeStreamSSL = new SslStream(_tradeClient.GetStream(), false,
                        new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            _tradeStreamSSL.AuthenticateAsClient(_host);
            _messageConstructor = new MessageConstructor(_host, _username,
                _password, _senderCompID, _senderSubID, _targetCompID);
            _messageTradeConstructor = new MessageConstructor(_host, _username,
                _password, _senderCompID, _senderTradeSubID, _targetCompID);
            // set symbol
            symbol = FixSymbols.Find(_testCurrency); // find selected symbol
        }

        private void OnTick(IAsyncResult ar)
        {
            int bytesRead = _priceStreamSSL.EndRead(ar);

            // reset timer
            //_timer2.Interval = TIMEOUT2;

            //if (!heartbeatPause)
            //{
            if (bytesRead > 0)
            {
                string msg = Encoding.ASCII.GetString(_priceBuffer, 0, bytesRead);
                //Console.WriteLine(msg);
                string[] data = msg.Split(new char[] { cFixSep, cEql });
                if (data.Length == 33)
                {
                    //BidLabel.Text = data[12].Substring(4);
                    //AskLabel.Text = data[14].Substring(4);

                    //updatePrices(data[25], data[29]);
                    double bid = Convert.ToDouble(data[25]);
                    double ask = Convert.ToDouble(data[29]);
                    
                    //TickHandler.OnTick(bid, ask);
                    
                    
                    //Console.WriteLine("Ask: {0}, Bid: {1}", ask, bid);
                    //double bid = Convert.ToDouble(data[12].Substring(4));
                    //double ask = Convert.ToDouble(data[14].Substring(4));
                    //Console.WriteLine("Ask: {0}, Bid: {1}", ask, bid);
                    TestReqID = "";
                }
                else if (data.Length == 23)
                {
                    if (data[5] == "1")
                    {
                        Console.WriteLine("test request");
                    }

                    TestReqID = data[19];
                    btnHeartbeat_Click(null, null);
                    Console.WriteLine(TestReqID);
                    Console.WriteLine(msg);
                }
                else
                {
                    Console.WriteLine("data length: " + data.Length + ", \nmsg: " + msg);
                }
                /*var data = msg.Split('');
                    if (data.Length == 17)
                    {
                        //BidLabel.Text = data[12].Substring(4);
                        //AskLabel.Text = data[14].Substring(4);

                        //updatePrices(data[12].Substring(4), data[14].Substring(4));
                        double bid = Convert.ToDouble(data[12].Substring(4));
                        double ask = Convert.ToDouble(data[14].Substring(4));
                        TickHandler.OnTick(bid, ask);
                        Console.WriteLine("Ask: {0}, Bid: {1}", ask, bid);
                    }*/

                //double bid = Convert.ToDouble(data[12].Substring(3));
                //double ask = Convert.ToDouble(data[14].Substring(3));
                //Console.WriteLine("Ask: {0}, Bid: {1}", ask, bid);
                //var mkdata = new QuickFix.FIX44.MarketDataSnapshotFullRefresh();
                //mkdata.FromString(msg, true, dd, dd, _defaultMsgFactory);
                //Console.WriteLine(mkdata);
                _priceStreamSSL.BeginRead(_priceBuffer, 0, 4096, quoteReceived, null);
            }
            //}
        }

        private void updatePrices(string bid, string ask)
        {
            //BidLabel.BeginInvoke((MethodInvoker)delegate () { BidLabel.Text = bid; });
            //AskLabel.BeginInvoke((MethodInvoker)delegate () { AskLabel.Text = ask; });
            //BidLabel.Text = bid;
            //AskLabel.Text = ask;
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;
        }

        private void ClearText()
        {
            //txtMessageSend.Text = "";
            //txtMessageReceived.Text = "";
        }

        private async Task<string> SendPriceMessage(string message, bool readResponse = true)
        {
            var msg = "";
            msg = await SendMessage(message, _priceStreamSSL, messageType.Quote, readResponse);

            return msg;
        }

        private async Task<string> SendTradeMessage(string message, bool readResponse = true)
        {
            var msg = "";
            msg = await SendMessage(message, _tradeStreamSSL, messageType.Trade, readResponse);

            return msg;
        }
        /*private async Task<string> SendMessage(string message, SslStream stream, messageType type, bool readResponse = false)
        {
            // Convert message to byte array
            var messageBytes = Encoding.ASCII.GetBytes(message);

            // Write message to stream
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            var buffer = new byte[4096];

            // Read response from stream
            if (readResponse)
            {
                var responseStream = new MemoryStream();
                
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // Cancel after 5 seconds

                try
                {
                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token)) > 0)
                    {
                        responseStream.Write(buffer, 0, bytesRead);
                    }

                    var responseBytes = responseStream.ToArray();
                    var responseString = Encoding.ASCII.GetString(responseBytes);

                    // Handle message type
                    if (type == messageType.Quote)
                    {
                        _priceMessageSequenceNumber++;
                        _priceBuffer = responseBytes;
                    }
                    else if (type == messageType.Trade)
                    {
                        _tradeMessageSequenceNumber++;
                        _tradeBuffer = responseBytes;
                    }

                    return responseString;
                }
                catch (OperationCanceledException ex)
                {
                    // Handle timeout
                    Console.WriteLine("Fix message timeout: " + ex.Message);
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    Console.WriteLine("Error reading response: " + ex.Message);
                    return string.Empty;
                }
            }
            else
            {
                var responseString = Encoding.ASCII.GetString(buffer);

                // Handle message type
                if (type == messageType.Quote)
                {
                    _priceMessageSequenceNumber++;
                    _priceBuffer = buffer;
                }
                else if (type == messageType.Trade)
                {
                    _tradeMessageSequenceNumber++;
                    _tradeBuffer = buffer;
                }

                return responseString;
            }

        }
*/
        private async Task<string> SendMessage(string message, SslStream stream, messageType type, bool readResponse = true)
        {
            // check msg type            
            var byteArray = Encoding.ASCII.GetBytes(message);
            await stream.WriteAsync(byteArray, 0, byteArray.Length);
            byte[] buff = new byte[4096];
            if (readResponse)
            {
                Thread.Sleep(100);
                await stream.ReadAsync(buff, 0, 4096);
            }

            // handle separate msg types
            if (type == messageType.Quote)
            {
                _priceMessageSequenceNumber++;
                _priceBuffer = buff;
            }
            else if (type == messageType.Trade)
            {
                _tradeMessageSequenceNumber++;
                _tradeBuffer = buff;
            }
            var returnMessage = Encoding.ASCII.GetString(buff);
            return returnMessage;
        }

        public void PriceFeedLogon()
        {
            //ClearText();
            var message = _messageConstructor.LogonMessage(MessageConstructor.SessionQualifier.QUOTE, _priceMessageSequenceNumber, 30, false);
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendPriceMessage(message);
            Console.WriteLine("Price Feed Logon: " + SendPriceMessage(message));

            // Part 1: set up the timer for 30 seconds.
            var cpuThread = new Thread(new ThreadStart(
                    delegate {
                        _timer = new System.Timers.Timer(TIMEOUT);
                        _timer.Elapsed += new ElapsedEventHandler(btnHeartbeat_Click);
                        _timer.Enabled = true;
                    }));
            cpuThread.IsBackground = true;
            cpuThread.Start();

            //
            //_timer2 = new System.Timers.Timer(TIMEOUT2);
            //_timer2.Elapsed += new ElapsedEventHandler(btnResendRequest_Click);
            //_timer2.Enabled = true;
        }

        public async void TradeFeedLogon()
        {
            //ClearText();
            var message = _messageConstructor.LogonMessage(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber, 30, false);
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendTradeMessage(message);
            Console.WriteLine("Trade Feed Logon: " + await SendTradeMessage(message));
            
            // Part 1: set up the timer for 30 seconds.
            var cpuThread = new Thread(new ThreadStart(
                    delegate {
                        _timer2 = new System.Timers.Timer(30000);
                        _timer2.Elapsed += new ElapsedEventHandler(btnHeartbeatT_Click);
                        _timer2.Enabled = true;
                    }));
            cpuThread.IsBackground = true;
            cpuThread.Start();
        }

        private void btnTestRequest_Click(object sender, EventArgs e)
        {
            //ClearText();
            var message = _messageConstructor.TestRequestMessage(MessageConstructor.SessionQualifier.QUOTE, _priceMessageSequenceNumber, _testRequestID);
            _testRequestID++;
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendPriceMessage(message);
            Console.WriteLine(SendPriceMessage(message));
        }

        public void PriceFeedLogout()
        {
            //ClearText();
            var message = _messageConstructor.LogoutMessage(MessageConstructor.SessionQualifier.QUOTE, _priceMessageSequenceNumber);
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendPriceMessage(message);
            Console.WriteLine("Price Feed Logout: " + SendPriceMessage(message));
            _priceMessageSequenceNumber = 1;
        }

        private void btnMarketDataRequest_Click(object sender, EventArgs e)
        {
            ClearText();
            var message = _messageConstructor.MarketDataRequestMessage(MessageConstructor.SessionQualifier.QUOTE, _priceMessageSequenceNumber, "EURUSD:WDqsoT", 1, 0, 0, 1, 1);
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendPriceMessage(message);
        }

        private void btnHeartbeat_Click(object sender, EventArgs e)
        {
            //ClearText();
            var message = _messageConstructor.HeartbeatMessage(MessageConstructor.SessionQualifier.QUOTE, _priceMessageSequenceNumber, TestReqID);
            ////txtMessageSend.Text = message;
            Console.WriteLine(SendPriceMessage(message, false));
            //btnTestRequest_Click();
            //heartbeatPause = false;
            Console.WriteLine("heartbeat {0}", DateTime.Now);
        }

        private void btnResendRequest_Click(object sender, EventArgs e)
        {
            ClearText();
            var message = _messageConstructor.ResendMessage(MessageConstructor.SessionQualifier.QUOTE, _priceMessageSequenceNumber, _priceMessageSequenceNumber - 1);
            _testRequestID++;
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendPriceMessage(message);
            Console.WriteLine(SendPriceMessage(message));
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            ClearText();
            var message = _messageConstructor.RejectMessage(MessageConstructor.SessionQualifier.QUOTE, _priceMessageSequenceNumber, 0);
            _testRequestID++;
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendPriceMessage(message);
        }

        private void btnSequenceReset_Click(object sender, EventArgs e)
        {
            ClearText();
            var message = _messageConstructor.SequenceResetMessage(MessageConstructor.SessionQualifier.QUOTE, _priceMessageSequenceNumber, 0);
            _testRequestID++;
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendPriceMessage(message);
        }

        public async void OpenMarketOrder(Order order, OrderType orderType, double lots, string currency)
        {
            // set buy/sell
            int side = 0;
            if (orderType == OrderType.Buy)
            {
                side = 1;
            }
            else if (orderType == OrderType.Sell)
            {
                side = 2;
            }
            // currency
            FixSymbol instrument = FixSymbols.Find(currency);
            long orderID = (long)order.TicketId;
            
            // set lot sizing : 100000 units = 1 lot
            int quantity = (int)(lots * 100000);
            //
            var message = _messageTradeConstructor.NewOrderSingleMessage(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber, orderID.ToString(), instrument.id, side, DateTime.UtcNow.ToString("yyyyMMdd-HH:mm:ss"), quantity, 1, "1");
            _testRequestID++;
            var msg = await SendTradeMessage(message, true);
            

            string[] data = msg.Split(new char[] { cFixSep, cEql });

            if (data.Length == 47 && data[11] == "TRADE")
            {
                posID = data[43];
                long ticketid = long.Parse(data[19]);
                order.PositionId = ulong.Parse(data[43]);
                order.Magic = ulong.Parse(data[43]);
                Console.WriteLine(msg);
                Console.WriteLine("opened position id: " + order.PositionId);
            }
            else
            {
                Console.WriteLine(message);
                Console.WriteLine(msg);
                OrderStatus(orderID);
                Console.WriteLine("open order response len: " + data.Length);
            }


        }

        public async void CloseMarketOrder(Order order, OrderType orderType, double lots, string currency)
        {
            // get order status if position id empty
            /*if (posID == "")
            {
                OrderStatus(orderID);
            }*/

            // set buy/sell -- reverse trade position to close
            int side = 0;
            if (orderType == OrderType.Buy)
            {
                side = 1;
            }
            else if (orderType == OrderType.Sell)
            {
                side = 2;
            }
            // currency
            FixSymbol instrument = FixSymbols.Find(currency);
            long orderID = (long)order.TicketId;
            string positionId = order.Magic.ToString();

            // set lot sizing : 100000 units = 1 lot
            int quantity = (int)(lots * 100000);
            //
            var message = _messageTradeConstructor.NewOrderSingleMessage(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber, orderID.ToString(), instrument.id, side, DateTime.UtcNow.ToString("yyyyMMdd-HH:mm:ss"), quantity, 1, "1", 0, 0, "", positionId);
            _testRequestID++;
            var msg = await SendTradeMessage(message);
            Console.WriteLine(message);
            //Console.WriteLine(msg);
            Console.WriteLine("position to close: " + positionId);
            string[] data = msg.Split(new char[] { cFixSep, cEql });

            if (data.Length == 47 || data.Length == 51)
            {
                posID = "";
                Console.WriteLine("response: " + msg);
            }
            else
            {
                Console.WriteLine("Close failed: " + msg);
                Console.WriteLine("Close order response: " + data.Length);
            }
        }

        public async void OrderStatus(long orderID)
        {
            //ClearText();
            var message = _messageTradeConstructor.OrderStatusRequest(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber, orderID.ToString());
            _testRequestID++;
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendTradeMessage(message);
            var msg = await SendTradeMessage(message);
            Console.WriteLine(msg);

            string[] data = msg.Split(new char[] { cFixSep, cEql });
            //long ticketid = long.Parse(data[19]);
            Order order = Program.SourceOrderList.Where(x => x.TicketId == (ulong)orderID).FirstOrDefault();
            if (data.Length == 47)
            {
                posID = data[43];
                if(order != null)
                {
                    order.PositionId = ulong.Parse(data[43]);
                    order.Magic = ulong.Parse(data[43]);
                }
                else
                {
                    ulong ticketid = ulong.Parse(data[19]);
                    order = Program.SourceOrderList.Where(x => x.TicketId == ticketid).FirstOrDefault();
                    order.PositionId = ulong.Parse(data[43]);
                    order.Magic = ulong.Parse(data[43]);
                }
                Console.WriteLine("status position id: " + order.Magic);
            }
            else
            {
                Console.WriteLine("status order response" + data.Length);
            }
        }

        private string Timestamp()
        {
            return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString();
        }

        private void btnRequestForPositions_Click(object sender, EventArgs e)
        {
            ClearText();
            var message = _messageConstructor.RequestForPositions(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber, "1408471");
            _testRequestID++;
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendTradeMessage(message);
        }

        private void btnLogonT_Click(object sender, EventArgs e)
        {
            ClearText();
            var message = _messageTradeConstructor.LogonMessage(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber, 30, false);
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendTradeMessage(message);
        }

        private async void btnHeartbeatT_Click(object sender, EventArgs e)
        {
            
            var message = _messageTradeConstructor.HeartbeatMessage(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber);
            
            var msg = await SendTradeMessage(message, false);
            /*Console.WriteLine(msg);

            string[] data = msg.Split(new char[] { cFixSep, cEql });

            if (data.Length == 47 && data[11] == "TRADE")
            {
                posID = data[43];

                //Order order = Program.SourceOrderList.Where(x => x.TicketId == (ulong)orderID).FirstOrDefault();
                ulong ticketid = ulong.Parse(data[19]);
                Order order = Program.SourceOrderList.Where(x => x.TicketId == ticketid).FirstOrDefault();
                if(order != null)
                {
                    order.PositionId = ulong.Parse(data[43]);
                    order.Magic = ulong.Parse(data[43]);
                    Console.WriteLine("heartbeat position id: " + order.Magic);
                }

                
            }
            else if (data.Length == 21)
            {
                Console.WriteLine("heartbeat response len: " + data.Length);
            }
            else
            {
                Console.WriteLine("unexpected response len: " + data.Length);
            }*/
            Console.WriteLine("trade heartbeat {0}", DateTime.Now);
        }

        private void btnTestRequestT_Click(object sender, EventArgs e)
        {
            ClearText();
            var message = _messageConstructor.TestRequestMessage(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber, _testRequestID);
            _testRequestID++;
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendTradeMessage(message);
        }

        private void btnLogoutT_Click(object sender, EventArgs e)
        {
            ClearText();
            var message = _messageConstructor.LogoutMessage(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber);
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendTradeMessage(message);
            _tradeMessageSequenceNumber = 1;
        }

        private void btnResendRequestT_Click(object sender, EventArgs e)
        {
            ClearText();
            var message = _messageConstructor.ResendMessage(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber, _tradeMessageSequenceNumber - 1);
            _testRequestID++;
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendTradeMessage(message);
        }

        public void PriceFeedStreamConnect()
        {
            //ClearText();

            var message = _messageConstructor.MarketDataRequestMessage(MessageConstructor.SessionQualifier.QUOTE, _priceMessageSequenceNumber, "MARKETDATAID", 1, 1, 0, 1, symbol.id);
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendPriceMessage(message);
            Console.WriteLine(SendPriceMessage(message));

            // on tick
            quoteReceived = new AsyncCallback(OnTick);
            _priceStreamSSL.BeginRead(_priceBuffer, 0, 4096, quoteReceived, null);
            //Console.WriteLine(SendPriceMessage(message));
            /*while (true)
            {
                //txtMessageReceived.Text = SendPriceMessage(message);
            }*/
            //

        }

        private void btnStopOrder_Click(object sender, EventArgs e)
        {
            ClearText();
            var message = _messageConstructor.NewOrderSingleMessage(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber, "10", 1, 1, DateTime.UtcNow.ToString("yyyyMMdd-HH:mm:ss"), 1000, 3, "3", 0, (decimal)1.08);
            _testRequestID++;
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendTradeMessage(message);
        }

        private void btnLimitOrder_Click(object sender, EventArgs e)
        {
            ClearText();
            var message = _messageConstructor.NewOrderSingleMessage(MessageConstructor.SessionQualifier.TRADE, _tradeMessageSequenceNumber, "10", 1, 1, DateTime.UtcNow.ToString("yyyyMMdd-HH:mm:ss"), 1000, 2, "3", (decimal)1.08);
            _testRequestID++;
            //txtMessageSend.Text = message;
            //txtMessageReceived.Text = SendTradeMessage(message);
        }
    }
}

