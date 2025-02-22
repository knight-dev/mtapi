﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MtApi5;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Timers;
using MirrorTrader.Fix44;
using MirrorTrader.Enums;

namespace MirrorTrader
{
    class Program
    {
        static readonly EventWaitHandle _connnectionWaiter = new AutoResetEvent(false);
        static readonly MtApi5Client _mtapiSource = new MtApi5Client();
        static readonly MtApi5Client _mtapiDestination = new MtApi5Client();
        static List<Order> SourceOrderHistory = new List<Order>();
        public static List<Order> SourceOrderList = new List<Order>();
        static List<Order> DestinationOrderHistory = new List<Order>();
        static Engine fix44 = new Engine();
        static ClientApp fixApp;
        static int sourcePort = 8228;
        static int destinationPort = 8229;
        static double lots = 0.03;
        static int timerDurationSeconds = 60;
        static double Factor = 0.4;
        static double ThresholdFactor = 0.2;
        static string configfile = "fxconfig.json";
        //private readonly List<Action> _groupOrderCommands = new List<Action>();
        //private readonly TimerTradeMonitor _timerTradeMonitor;
        //private readonly TimeframeTradeMonitor _timeframeTradeMonitor;
        static void _mtapi_SourceConnectionStateChanged(object sender, Mt5ConnectionEventArgs e)
        {
            switch (e.Status)
            {
                case Mt5ConnectionState.Connecting:
                    Console.WriteLine($"Source {sourcePort} Connnecting...");
                    break;
                case Mt5ConnectionState.Connected:
                    Console.WriteLine($"Source {sourcePort} Connnected.");
                    _connnectionWaiter.Set();
                    break;
                case Mt5ConnectionState.Disconnected:
                    Console.WriteLine($"Source {sourcePort} Disconnected.");
                    _connnectionWaiter.Set();
                    // attempt reconnect
                    Thread.Sleep(600000);
                    _mtapiSource.BeginConnect(sourcePort);
                    _connnectionWaiter.WaitOne();
                    break;
                case Mt5ConnectionState.Failed:
                    Console.WriteLine($"Source {sourcePort} Connection failed.");
                    _connnectionWaiter.Set();
                    // attempt reconnect
                    Thread.Sleep(600000);
                    _mtapiSource.BeginConnect(sourcePort);
                    _connnectionWaiter.WaitOne();
                    break;
            }
        }

        static void _mtapi_DestinationConnectionStateChanged(object sender, Mt5ConnectionEventArgs e)
        {
            switch (e.Status)
            {
                case Mt5ConnectionState.Connecting:
                    Console.WriteLine($"Destination {destinationPort} Connnecting...");
                    break;
                case Mt5ConnectionState.Connected:
                    Console.WriteLine($"Destination {destinationPort} Connnected.");
                    _connnectionWaiter.Set();
                    break;
                case Mt5ConnectionState.Disconnected:
                    Console.WriteLine($"Destination {destinationPort} Disconnected.");
                    _connnectionWaiter.Set();

                    // attempt reconnect
                    Thread.Sleep(600000);
                    _mtapiDestination.BeginConnect(destinationPort);
                    _connnectionWaiter.WaitOne();
                    break;
                case Mt5ConnectionState.Failed:
                    Console.WriteLine($"Destination {destinationPort} Connection failed.");
                    _connnectionWaiter.Set();
                    // attempt reconnect
                    Thread.Sleep(600000);
                    _mtapiDestination.BeginConnect(destinationPort);
                    _connnectionWaiter.WaitOne();
                    break;
            }
        }
        static void _mtapi_QuoteAdded(object sender, Mt5QuoteEventArgs e)
        {
            Console.WriteLine("Quote added with symbol {0}", e.Quote.Instrument);
        }

        static void _mtapi_QuoteRemoved(object sender, Mt5QuoteEventArgs e)
        {
            Console.WriteLine("Quote removed with symbol {0}", e.Quote.Instrument);
        }

        static void _mtapi_QuoteUpdate(object sender, Mt5QuoteEventArgs e)
        {
            Console.WriteLine("Quote updated: {0} - {1} : {2}", e.Quote.Instrument, e.Quote.Bid, e.Quote.Ask);
        }

        static void _mtapi_SourceTradeUpdate(object sender, Mt5TradeTransactionEventArgs e)
        {
            //Console.WriteLine($"{e.Trans.OrderType}: symbol {e.Trans.Symbol} lots = {e.Trans.Volume}, deal = {e.Trans.DealType}, result = {e.Result}");
            /*Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"expert handle = {e.ExpertHandle}, request {e.Request}, transaction = {e.Trans},  result = {e.Result}");
            Console.ForegroundColor = ConsoleColor.White;*/
            var orderId = e.Trans.Order;
            var orderType = e.Trans.OrderType;
            var transType = e.Trans.Type;
            var volume = lots;//e.Trans.Volume;
            var position = e.Trans.Position;
            var symbol = e.Trans.Symbol;
            var dealId = e.Trans.Deal;
            var price = e.Trans.Price;
            //var comment = e.Trans.
            if (transType == ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_POSITION)
            {

            }

                // order opened
                if (transType == ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_ORDER_ADD)
            {
                Order order = SourceOrderList.Where(x => x.TicketId == position).FirstOrDefault();
                if (order != null)
                {
                    if (order.TicketId != orderId)
                    {
                        // close order
                        //CloseRequest(_mtapiDestination, order.Magic);

                        //uncomment this /*ClosePendingOrder(_mtapiDestination, order.Magic);*/ // close destination order

                        //CloseLossOrder(_mtapiDestination, order);
                        //fix44.CloseMarketOrder(order, orderType == ENUM_ORDER_TYPE.ORDER_TYPE_BUY ? OrderType.Buy : OrderType.Sell, 0.01, symbol);
                        fixApp.CloseMarketOrder(order, orderType == ENUM_ORDER_TYPE.ORDER_TYPE_BUY ? OrderType.Buy : OrderType.Sell, 0.01, symbol);
                        //CheckOrderProfit(_mtapiSource, order);

                        // remove from list
                        SourceOrderList.Remove(order);
                        Console.ForegroundColor = ConsoleColor.Red;
                        /*QuoteRequest(_mtapiSource, symbol);*/
                        //Console.WriteLine($"Source order closed: {symbol} {volume} - Type: {orderType}, Price: {price}, Ticket: {orderId}, Position: {position}, Now: {DateTime.Now}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else
                {
                    // open order
                    order = new Order();
                    order.TicketId = orderId;
                    order.OrderType = orderType;
                    order.TransactionType = transType;
                    order.Volume = volume;
                    order.Symbol = symbol;
                    order.PositionId = position;
                    order.Price = price;

                    // open destination order
                    //OpenRequest(_mtapiDestination, order);
                    //uncomment this/*PendingOrderRequest(_mtapiDestination, order);*/

                    
                    
                    // add to history
                    SourceOrderList.Add(order); // orders removed once closed

                    // fix api - call after source order added
                    //fix44.OpenMarketOrder(order, orderType == ENUM_ORDER_TYPE.ORDER_TYPE_BUY ? OrderType.Buy : OrderType.Sell, 0.01, symbol);
                    fixApp.PlaceMarketOrder(order, orderType == ENUM_ORDER_TYPE.ORDER_TYPE_BUY ? OrderType.Buy : OrderType.Sell, 0.01, symbol);
                    
                    SourceOrderHistory.Add(order); // orders not removed.. keep a full log to compare with destination orders
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    QuoteRequest(_mtapiSource, symbol);
                    //Console.WriteLine($"Source order opened: {symbol} {volume} - Type: {orderType}, Price: {price}, Ticket: {orderId}, Position: {position}, Now: {DateTime.Now}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                
            }

            // order opened
            if (transType == ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_DEAL_ADD)
            {
                /*Order order = SourceOrderList.Where(x => x.TicketId == orderId).FirstOrDefault();
                if(order != null)
                {
                    // update order deal
                    order.Price = price;
                    order.DealId = dealId;
                    order.DealType = transType;
                    order.PositionId = position;

                    Console.ForegroundColor = ConsoleColor.Green;
                    *//*QuoteRequest(_mtapiSource, symbol);*//*
                    Console.WriteLine($"Source deal made (Ticket:{orderId}, Position:{position}): {symbol} {volume} - Type: {orderType}, Price: {price}, Ticket: {orderId}, Position: {position}, Now: {DateTime.Now}");
                    Console.ForegroundColor = ConsoleColor.White;
                }*/
            }

            // order closed 
            //Console.WriteLine($"transaction = {e.Trans}");
            //Console.WriteLine("Trade updated: {0} - {1} : {2}", e.Quote.Instrument, e.Quote.Bid, e.Quote.Ask);
        }

        static void _mtapi_DestinationTradeUpdate(object sender, Mt5TradeTransactionEventArgs e)
        {
            //Console.WriteLine($"{e.Trans.OrderType}: symbol {e.Trans.Symbol} lots = {e.Trans.Volume}, deal = {e.Trans.DealType}, result = {e.Result}");
            //Console.WriteLine($"expert handle = {e.ExpertHandle}, request {e.Request}, transaction = {e.Trans},  result = {e.Result}");
            var orderId = e.Trans.Order;
            var orderType = e.Trans.OrderType;
            var transType = e.Trans.Type;
            var volume = lots;//e.Trans.Volume;
            var position = e.Trans.Position;
            var symbol = e.Trans.Symbol;
            var dealId = e.Trans.Deal;
            var price = e.Trans.Price;

            // order opened
            if (price > 0 && transType == ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_ORDER_ADD)
            {
                Order order = DestinationOrderHistory.Where(x => x.TicketId == position).FirstOrDefault();
                if (order != null)
                {
                    if (order.TicketId != orderId)
                    {
                        // close order  
                        order.timer.Stop();
                        order.timer.Close();
                        order.CloseTime = DateTime.Now.ToUniversalTime();
                        DestinationOrderHistory.Remove(order);
                        Console.WriteLine($"Destination order closed: {symbol} {volume} - Type: {orderType}, Price: {price}, Ticket: {orderId}, Position: {position}, Now: {DateTime.Now}");
                    }
                }
                else
                {
                    // open order
                    order = new Order();
                    order.TicketId = orderId;
                    order.OrderType = orderType;
                    order.TransactionType = transType;
                    order.Volume = volume;
                    order.Symbol = symbol;
                    order.PositionId = position;
                    order.OpenTime = DateTime.Now.ToUniversalTime();
                    order.Factor = Factor;
                    order.ThresholdFactor = ThresholdFactor;
                    order.Price = price;

                    // add to history
                    DestinationOrderHistory.Add(order);                    

                    //
                    Console.WriteLine($"Destination order opened (Ticket:{orderId}, Position:{position}): {symbol} {volume} - Type: {orderType}, Price: {price}, Ticket: {orderId}, Position: {position}, Now: {DateTime.Now.ToUniversalTime()}");
                }

            }

            // order opened
            if (transType == ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_DEAL_ADD)
            {
                Order order = DestinationOrderHistory.Where(x => x.TicketId == orderId).FirstOrDefault();
                if (order != null)
                {
                    // update order deal
                    
                    order.DealId = dealId;
                    order.DealType = transType;
                    //order.OrderType = orderType;
                    order.PositionId = position;
                    order.DealTime = DateTime.Now.ToUniversalTime();
                    Order sourceOrder = SourceOrderHistory.Where(x => x.Magic == position || x.Magic == orderId).FirstOrDefault();
                    order.Ask = sourceOrder != null ? sourceOrder.Ask : -1;
                    order.Bid = sourceOrder != null ? sourceOrder.Bid : -1;
                    order.Price = price;
                    order.SourcePrice = sourceOrder != null ? sourceOrder.Price : -1;
                    order.Diff = sourceOrder != null ? sourceOrder.Diff : -1;
                    order.TP = sourceOrder != null ? sourceOrder.TP : -1;
                    order.baseTP = sourceOrder != null ? sourceOrder.baseTP : -1;
                    order.thresholdTP = sourceOrder != null ? sourceOrder.thresholdTP : -1;
                    order.SL = sourceOrder != null ? sourceOrder.SL : -1;
                    //Console.WriteLine("source: {0}",sourceOrder);
                    // set price if 0
                    if (price == 0)
                    {
                        OrderQuoteRequest(_mtapiDestination, symbol, order);
                    }

                    // timer 15 mins - close if loss
                    order.timer = new System.Timers.Timer
                    {
                        Interval = timerDurationSeconds * 1000
                    };
                    order.timer.Enabled = true;
                    order.timer.Elapsed += (s, ev) => OnTimerEvent(s, ev, order);
                    // run once
                    order.timer.AutoReset = true;
                    order.timer.Start();

                    Console.WriteLine($"Destination deal made(Ticket:{orderId}, Position:{position}): {symbol} {volume} - Type: {orderType}, Price: {price}, Ticket: {orderId}, Position: {position}, Now: {DateTime.Now.ToUniversalTime()}");
                }
            }

            // order closed 
            //Console.WriteLine($"transaction = {e.Trans}");
            //Console.WriteLine("Trade updated: {0} - {1} : {2}", e.Quote.Instrument, e.Quote.Bid, e.Quote.Ask);
        }

        private static void OnTimerEvent(object sender, ElapsedEventArgs e, Order order)
        {
            Console.WriteLine("Close order timer elapsed");
            System.Timers.Timer timer = (System.Timers.Timer)sender;
            // close loss order after elapsed time
            if(order != null)
            {
                //timer.AutoReset = false;
                CloseOpenOrder(_mtapiDestination, order, timer);
                //timer.Stop();
            }
            else
            {
                timer.Stop();
                timer.Close();
            }
            
            
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Application started.");
            // create orderlog if it doesn't exist
            Directory.CreateDirectory("logs");
            //File.Create

            if (File.Exists(configfile))
            {
                string json = File.ReadAllText(configfile);
                FxConfig config = JsonConvert.DeserializeObject<FxConfig>(json);

                // get from file
                sourcePort = config.sourcePort;
                destinationPort = config.destinationPort;
                lots = config.lots;
                timerDurationSeconds = config.timerDuration;
                Factor = config.Factor;
                ThresholdFactor = config.ThresholdFactor;
            }
            else
            {
                Console.WriteLine("Source port:");
                sourcePort = int.Parse(Console.ReadLine());
                Console.WriteLine("Destination port:");
                destinationPort = int.Parse(Console.ReadLine());
                Console.WriteLine("Lot Size:");
                lots = int.Parse(Console.ReadLine());
            }
            

            _mtapiSource.ConnectionStateChanged += _mtapi_SourceConnectionStateChanged;
            //_mtapiSource.QuoteAdded += _mtapi_QuoteAdded;
            //_mtapiSource.QuoteRemoved += _mtapi_QuoteRemoved;
            //_mtapiSource.QuoteUpdate += _mtapi_QuoteUpdate;
            _mtapiSource.OnTradeTransaction += _mtapi_SourceTradeUpdate;

            _mtapiSource.BeginConnect(sourcePort);
            _connnectionWaiter.WaitOne();

            /*_mtapiDestination.ConnectionStateChanged += _mtapi_DestinationConnectionStateChanged;
            //_mtapiDestination.QuoteAdded += _mtapi_QuoteAdded;
            //_mtapiDestination.QuoteRemoved += _mtapi_QuoteRemoved;
            //_mtapiDestination.QuoteUpdate += _mtapi_QuoteUpdate;
            _mtapiDestination.OnTradeTransaction += _mtapi_DestinationTradeUpdate;

            _mtapiDestination.BeginConnect(destinationPort);
            _connnectionWaiter.WaitOne();*/

            if (_mtapiSource.ConnectionState == Mt5ConnectionState.Connected)
            {
                Run();
            }

            Console.WriteLine("Application finished. Press any key...");
            Console.ReadKey();
        }

        private static void Run()
        {
            //fix44.PriceFeedLogon();
            //fix44.TradeFeedLogon();
            //fix44.PriceFeedStreamConnect();

            fixApp = new ClientApp("Fix44/cserver_quote_session.cfg");
            //fixApp.Start();
            //fixApp.GetMarketData(FixSymbols.Find("EURUSD").id.ToString());

            ConsoleKeyInfo cki;
            do
            {                
                cki = Console.ReadKey();
                switch (cki.KeyChar.ToString())
                {
                    case "b":
                        Buy();
                        break;
                    case "s":
                        Sell();
                        break;
                    case "c":
                        Close();
                        break;
                }
            } while (cki.Key != ConsoleKey.Escape);

            _mtapiSource.BeginDisconnect();
            _connnectionWaiter.WaitOne();

            _mtapiDestination.BeginDisconnect();
            _connnectionWaiter.WaitOne();
        }

        private static async void Buy()
        {
            
            //fix44.OpenMarketOrder(1, OrderType.Buy, 0.01, "EURUSD");
            const string symbol = "EURUSD";
            const double volume = 0.1;
            MqlTradeResult tradeResult = null;
            var retVal = await Execute(() => _mtapiSource.Buy(out tradeResult, volume, symbol));
            Console.WriteLine($"Buy: symbol EURUSD retVal = {retVal}, result = {tradeResult}");
        }

        
        private static async void Sell()
        {            
            //fix44.OpenMarketOrder(1, OrderType.Sell, 0.01);
            const string symbol = "EURUSD";
            const double volume = 0.1;
            MqlTradeResult tradeResult = null;
            var retVal = await Execute(() => _mtapiSource.Sell(out tradeResult, volume, symbol));
            Console.WriteLine($"Sell: symbol EURUSD retVal = {retVal}, result = {tradeResult}");
        }

        private static async void OpenRequest(MtApi5Client client, Order order)
        {
            MqlTradeResult tradeResult = null;
            var retVal = false;
            string symbol = order.Symbol.Replace(".pro", "");
            if (order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_BUY)
            {
                retVal = await Execute(() => client.Buy(out tradeResult, 0.01, symbol));
                Console.WriteLine($"Buy: symbol {order.Symbol} retVal = {retVal}, result = {tradeResult}");
            }

            if (order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_SELL)
            {
                retVal = await Execute(() => client.Sell(out tradeResult, order.Volume, order.Symbol));
                Console.WriteLine($"Sell: symbol {order.Symbol} retVal = {retVal}, result = {tradeResult}");
            }

            if (retVal)
            {
                Order o = order;
                o.Cloned = true;
                o.Magic = tradeResult.Order;
            }
            
        }

        private static async void ClosePendingOrder(MtApi5Client client, ulong ticket)
        {
            MqlTradeRequest request = new MqlTradeRequest();
            MqlTradeResult tradeResult = null;
            request.Order = ticket;
            request.Action = ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_REMOVE;

            var retVal = await Execute(() => client.OrderSend(request, out tradeResult));
            Console.WriteLine($"Close: Position - {ticket} retVal = {retVal}");
        }

        private static async void CheckOrderProfit(MtApi5Client client, Order order)
        {
            MqlTradeRequest request = new MqlTradeRequest();
            string symbol = order.Symbol.Replace(".pro", "");
            MqlTick tick = null;
            double profit = 0;
            var val = await Execute(() => client.SymbolInfoTick(order.Symbol, out tick));

            if (order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_BUY)
            {
                var retVal = await Execute(() => client.OrderCalcProfit(order.OrderType, order.Symbol, order.Volume, order.Price, tick.bid, out profit));
                Console.WriteLine($"Close: Position - {order.TicketId}, Profit: {profit}, retVal = {retVal}");
            }
            else if (order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_SELL)
            {
                var retVal = await Execute(() => client.OrderCalcProfit(order.OrderType, order.Symbol, order.Volume, order.Price, tick.ask, out profit));
                Console.WriteLine($"Close: Position - {order.TicketId}, Profit: {profit}, retVal = {retVal}");
            }
                
        }

        public static void AddOrUpdateOrderLog(Order order) 
        {
            try
            {
                DateTime now = DateTime.Now.ToUniversalTime();
                var jamaicaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var jamaicaTime = TimeZoneInfo.ConvertTimeFromUtc(now, jamaicaTimeZone);

                string today = jamaicaTime.ToString("dd-MM-yy");
                string dailyLog = "logs/log." + today + ".json"; // log name

                if (!File.Exists(dailyLog))
                {
                    File.Create(dailyLog);
                }

                // open file
                using (StreamWriter sw = File.AppendText(dailyLog))
                {
                    if(order != null)
                    {
                        string jsonOrder = JsonConvert.SerializeObject(order);
                        sw.WriteLine(jsonOrder + ",");
                    }                    
                }
            }catch(Exception ex)
            {
                Console.WriteLine("Error logging order: "+ex.ToString());
            }
        }

        private static async void CloseOpenOrder(MtApi5Client client, Order order, System.Timers.Timer timer = null)
        {
            MqlTradeRequest request = new MqlTradeRequest();
            string symbol = order.Symbol.Replace(".pro", "");
            MqlTick tick = null;
            double profit = 0;
            var val = await Execute(() => client.SymbolInfoTick(symbol, out tick));
            long digits = await Execute(() => client.SymbolInfoInteger(symbol, ENUM_SYMBOL_INFO_INTEGER.SYMBOL_DIGITS));
            //int digits = tick.bid.ToString().Split('.')[1].Length;
            //Console.WriteLine("digits: " + digits);
            //Console.WriteLine($"Ask: {tick.ask}, Bid: {tick.bid}");
            //Console.WriteLine(order.Price);

            // yeahh...this trailing algo is the reverse of what it should but it works...
            if (order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_BUY_STOP || order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_BUY_LIMIT)
            {
                //var retVal = await Execute(() => client.OrderCalcProfit(order.OrderType, order.Symbol, order.Volume, order.Price, tick.bid, out profit));
                //Console.WriteLine("profit: "+Math.Round(profit, 2));
                double p = Math.Round((tick.bid - order.Price) * Math.Pow(10, digits), 2);
                Console.WriteLine($"{symbol} {order.OrderType}_ID - {order.TicketId} - Open Price: {order.Price}, Ask: {tick.ask}, Bid: {tick.bid}, Profit: {p}, Level: {order.Level}, SL: {order.SL}, TP: {order.TP}");
                //Console.WriteLine("price: " + p);

                
                if (tick.ask >= order.TP && p > 0)
                {
                    double diff = order.Diff;
                    order.baseTP = order.TP;
                    order.thresholdTP = order.Bid + (diff * (order.Factor - ThresholdFactor));
                    order.SL = order.thresholdTP;
                    order.TP = order.Bid + (diff * (order.Factor * 2));
                    order.Factor = Math.Round(order.Factor * 2, 2);
                    order.Level++;
                    //Console.WriteLine("Level: {0}", order.Level);
                    Console.WriteLine($"Buy {symbol} ID - {order.TicketId}, Profit: {p}, Level = {order.Level}, Diff: {order.Diff}, TP: {order.TP}, SL: {order.SL}, baseTP: {order.baseTP}, thresholdTP: {order.thresholdTP}, factor: {order.Factor}, thresholdFactor: {order.ThresholdFactor}, highPrice: {order.HighestPrice}, lowPrice: {order.LowestPrice}");
                }

                // adjust sl if too high initially
                if (order.Level == 0 && tick.bid >= order.SL)
                {
                    double takeprofit = Math.Round((tick.ask + (order.Diff * Factor)), 5);
                    double stoploss = Math.Round((tick.bid - order.Diff), 5);

                    order.TP = takeprofit;
                    order.SL = stoploss;
                }

                if (tick.ask > order.HighestPrice)
                {
                    order.HighestPrice = tick.ask;
                }
                if (tick.bid < order.LowestPrice)
                {
                    order.LowestPrice = tick.bid;
                }

                // take trailed profits
                /*if(tick.bid <= order.thresholdTP && order.HighestPrice >= order.baseTP)
                {
                    // order reversal - close
                    DestinationCloseRequest(order.TicketId);
                    if (order.timer != null)
                    {
                        order.timer.Enabled = false;
                        order.timer.Stop();
                        order.timer.Close();
                        order.CloseTime = DateTime.Now.ToUniversalTime();
                        //AddOrUpdateOrderLog(order); // log trade
                        DestinationOrderHistory.Remove(order);
                    }
                    Console.WriteLine($"Close: Position - {order.TicketId}, Profit: {p}");
                }*/

                // accept loss
                if (/*p < 0*/ tick.bid <= order.SL)
                {
                    //CloseRequest(client, order.Magic);
                    DestinationCloseRequest(order.TicketId);
                    if(order.timer != null)
                    {
                        order.timer.Enabled = false;
                        order.timer.Stop();
                        order.timer.Close();
                        order.CloseTime = DateTime.Now.ToUniversalTime();
                        //AddOrUpdateOrderLog(order); // log trade
                        DestinationOrderHistory.Remove(order);
                    }
                    Console.WriteLine($"Close: Position - {order.TicketId}, Profit: {p}");
                }
                
            }
            else if (order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_SELL_STOP || order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_SELL_LIMIT)
            {
                //var retVal = await Execute(() => client.OrderCalcProfit(order.OrderType, order.Symbol, order.Volume, order.Price, tick.ask, out profit));
                //Console.WriteLine("profit: " + Math.Round(profit, 2));
                double p = Math.Round((order.Price - tick.ask) * Math.Pow(10, digits), 2);
                Console.WriteLine($"{symbol} {order.OrderType}_ID - {order.TicketId} - Open Price: {order.Price}, Ask: {tick.ask}, Bid: {tick.bid}, Profit: {p}, Level: {order.Level}, SL: {order.SL}, TP: {order.TP}");
                               

                if (tick.ask <= order.TP && p > 0)
                {
                    double diff = order.Diff;
                    order.baseTP = order.TP;
                    order.thresholdTP = order.Bid - (diff * (order.Factor - ThresholdFactor));
                    order.SL = order.thresholdTP;
                    order.TP = order.Bid - (diff * (order.Factor * 2));
                    order.Factor = Math.Round(order.Factor * 2, 2);
                    order.Level++;
                    //Console.WriteLine("Level: {0}", order.Level);
                    Console.WriteLine($"Sell {symbol} ID - {order.TicketId}, Profit: {p}, Level = {order.Level}, Diff: {order.Diff}, TP: {order.TP}, SL: {order.SL}, baseTP: {order.baseTP}, thresholdTP: {order.thresholdTP}, factor: {order.Factor}, thresholdFactor: {order.ThresholdFactor}, highPrice: {order.HighestPrice}, lowPrice: {order.LowestPrice}");
                }

                // adjust sl if too low initially
                if (order.Level == 0 && tick.ask <= order.SL)
                {
                    double takeprofit = Math.Round((tick.bid - (order.Diff * Factor)), 5);
                    double stoploss = Math.Round((tick.ask + order.Diff), 5);

                    order.TP = takeprofit;
                    order.SL = stoploss;
                }

                if (tick.ask > order.HighestPrice)
                {
                    order.HighestPrice = tick.ask;
                }
                if (tick.bid < order.LowestPrice)
                {
                    order.LowestPrice = tick.bid;
                }

                // take trailed profits
                /*if (tick.ask >= order.thresholdTP && order.LowestPrice <= order.baseTP)
                {
                    // order reversal - close
                    DestinationCloseRequest(order.TicketId);
                    if (order.timer != null)
                    {
                        order.timer.Enabled = false;
                        order.timer.Stop();
                        order.timer.Close();
                        order.CloseTime = DateTime.Now.ToUniversalTime();
                        //AddOrUpdateOrderLog(order); // log trade
                        DestinationOrderHistory.Remove(order);
                    }
                    Console.WriteLine($"Close: Position - {order.TicketId}, Profit: {p}");
                }*/

                // accept loss
                if (/*p < 0*/ tick.ask > order.SL)
                {
                    //CloseRequest(client, order.Magic);
                    DestinationCloseRequest(order.TicketId);
                    if (order.timer != null)
                    {
                        order.timer.Enabled = false;
                        order.timer.Stop();
                        order.timer.Close();
                        order.CloseTime = DateTime.Now.ToUniversalTime();
                        //AddOrUpdateOrderLog(order); // log trade
                        DestinationOrderHistory.Remove(order);
                    }
                    Console.WriteLine($"Close: Position - {order.TicketId}, Profit: {p}");
                }
            }

        }

        static int guessDigits(double price1, double price2, double price3)
        {
            int l1 = price1.ToString().Length;
            int l2 = price2.ToString().Length;
            int l3 = price3.ToString().Length;

            int digits = 5;

            if(l1 > l2) digits = l1;
            if(l2 > l3) digits = l2;
            if(l3 > l1) digits = l3;

            return digits;
        }

        private static async void PendingOrderRequest(MtApi5Client client, Order order)
        {
            MqlTradeResult tradeResult = null;
            var retVal = false;
            string symbol = order.Symbol.Replace(".pro", "");
            int factor = 1;

            if (order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_SELL)
            {
                MqlTick tick = null;
                var val = await Execute(() => client.SymbolInfoTick(symbol, out tick));
                if (val)
                {
                    double spread = tick.ask - tick.bid;
                    double limit = spread * factor;
                    double priceStopOrder = tick.bid - limit;
                    double priceLimitOrder = tick.ask + limit;
                    double diff = Math.Round(Math.Abs(tick.bid - order.Price),5);

                    double profit = Math.Round((order.Price - tick.ask) * Math.Pow(10, 5), 2);

                    // todo: use profit to determine buy or sell
                    
                    // fake diff since it's zero
                    if (diff == 0)
                    {
                        // try to get next tick
                        await Execute(() => client.SymbolInfoTick(symbol, out tick));
                        diff = Math.Round(Math.Abs(tick.ask - order.Price), 5);
                        //diff = diff * 10;

                        // if still zero, do this
                        if(diff == 0)
                        {
                            int digits = guessDigits(tick.ask, tick.bid, order.Price);
                            
                            if(digits == 5 || digits == 4)
                            diff = 0.0001;

                            if (digits == 3)
                                diff = 0.001;

                            if (digits == 2)
                                diff = 0.01;
                        }
                    }
                    double takeprofit = Math.Round((tick.bid - (diff * Factor)), 5);
                    double stoploss = Math.Round((tick.ask + diff), 5);
                    //double takeprofit = Math.Round((tick.bid + (diff * Factor)), 5);
                    //double stoploss = Math.Round((tick.ask - diff), 5);
                    //Console.WriteLine($"Buy: symbol {order.Symbol}, diff = {diff}, bid = {tick.bid}, openprice = {order.Price}, tp = {takeprofit}, sl = {stoploss}");
                    // setup trailing info
                    order.Ask = tick.ask;
                    order.Bid = tick.bid;
                    order.HighestPrice = tick.ask;
                    order.LowestPrice = tick.bid;
                    order.Diff = diff;
                    order.Factor = Factor;
                    order.baseTP = takeprofit;
                    order.TP = takeprofit;
                    order.SL = stoploss;

                    MqlTradeRequest request = new MqlTradeRequest();
                    request.Action = ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_PENDING;
                    request.Symbol = symbol;
                    request.Volume = order.Volume;
                    //request.Price = priceStopOrder;
                    request.Price = profit < 0 ? priceStopOrder : priceStopOrder;
                    //request.Tp = takeprofit;
                    //request.Sl = stoploss;
                    //request.Type = ENUM_ORDER_TYPE.ORDER_TYPE_SELL_LIMIT/*ENUM_ORDER_TYPE.ORDER_TYPE_BUY_LIMIT*/;
                    //request.Type = ENUM_ORDER_TYPE.ORDER_TYPE_BUY_STOP;
                    request.Type = profit < 0 ? ENUM_ORDER_TYPE.ORDER_TYPE_SELL_STOP : ENUM_ORDER_TYPE.ORDER_TYPE_BUY_LIMIT;
                    request.Type_filling = ENUM_ORDER_TYPE_FILLING.ORDER_FILLING_RETURN;
                    request.Type_time = ENUM_ORDER_TYPE_TIME.ORDER_TIME_DAY;

                    retVal = await Execute(() => client.OrderSend(request, out tradeResult));
                    order.Magic = tradeResult.Order;
                    Console.WriteLine($"Sell pending: {order.Symbol} retVal = {retVal}, result = {tradeResult}");
                }

            }

            if (order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_BUY)
            {
                MqlTick tick = null;
                var val = await Execute(() => client.SymbolInfoTick(symbol, out tick));
                if (val)
                {
                    double spread = tick.ask - tick.bid;
                    double limit = spread * factor;
                    double priceStopOrder = tick.bid + limit;
                    double priceLimitOrder = tick.ask - limit;
                    double diff = Math.Round(Math.Abs(tick.bid - order.Price), 5);

                    double profit = Math.Round((tick.bid - order.Price) * Math.Pow(10, 5), 2);
                    
                    // fake diff since it's zero
                    if (diff == 0)
                    {
                        // try to get next tick
                        await Execute(() => client.SymbolInfoTick(symbol, out tick));
                        diff = Math.Round(Math.Abs(tick.ask - order.Price), 5);
                        //diff = diff * 10;

                        // if still zero, do this
                        if (diff == 0)
                        {
                            int digits = guessDigits(tick.ask, tick.bid, order.Price);

                            if (digits == 5 || digits == 4)
                                diff = 0.0001;

                            if (digits == 3)
                                diff = 0.001;

                            if (digits == 2)
                                diff = 0.01;
                        }
                    }
                    double takeprofit = Math.Round((tick.bid + (diff * Factor)), 5)/*Math.Round((tick.bid - (diff * Factor)), 5)*/;
                    double stoploss = Math.Round((tick.ask - diff), 5)/*Math.Round((tick.ask + diff), 5)*/;
                    //Console.WriteLine($"Sell: {order.Symbol}, diff = {diff}, bid = {tick.bid}, openprice = {order.Price}, tp = {takeprofit}, sl = {stoploss}");
                    // setup trailing info
                    order.Ask = tick.ask;
                    order.Bid = tick.bid;
                    order.HighestPrice = tick.ask;
                    order.LowestPrice = tick.bid;
                    order.Diff = diff;
                    order.Factor = Factor;
                    order.baseTP = takeprofit;
                    order.TP = takeprofit;
                    order.SL = stoploss;

                    MqlTradeRequest request = new MqlTradeRequest();
                    request.Action = ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_PENDING;
                    request.Symbol = symbol;
                    request.Volume = order.Volume;
                    //request.Price = priceStopOrder;
                    request.Price = profit < 0 ? priceStopOrder : priceStopOrder;
                    //request.Tp = takeprofit;
                    //request.Sl = stoploss;
                    //request.Type = ENUM_ORDER_TYPE.ORDER_TYPE_BUY_LIMIT/*ENUM_ORDER_TYPE.ORDER_TYPE_SELL_LIMIT*/;
                    //request.Type = ENUM_ORDER_TYPE.ORDER_TYPE_SELL_STOP;
                    request.Type = profit < 0 ? ENUM_ORDER_TYPE.ORDER_TYPE_BUY_STOP : ENUM_ORDER_TYPE.ORDER_TYPE_SELL_LIMIT;
                    request.Type_filling = ENUM_ORDER_TYPE_FILLING.ORDER_FILLING_RETURN;
                    request.Type_time = ENUM_ORDER_TYPE_TIME.ORDER_TIME_DAY;
                    //request.

                    retVal = await Execute(() => client.OrderSend(request, out tradeResult));
                    order.Magic = tradeResult.Order;
                    Console.WriteLine($"Buy pending: {order.Symbol} retVal = {retVal}, result = {tradeResult}");
                }
            }

        }

        private static async void SellRequest(string symbol, double volume)
        {
            MqlTradeResult tradeResult = null;
            var retVal = await Execute(() => _mtapiSource.Sell(out tradeResult, volume, symbol));
            Console.WriteLine($"Sell: symbol EURUSD retVal = {retVal}, result = {tradeResult}");
        }

        private static async void CloseRequest(ulong ticket)
        {
            var retVal = await Execute(() => _mtapiSource.PositionClose(ticket));
            Console.WriteLine($"Close: Position - {ticket} retVal = {retVal}");
        }

        private static async void DestinationCloseRequest(ulong ticket)
        {
            var retVal = await Execute(() => _mtapiDestination.PositionClose(ticket));
            Console.WriteLine($"Close: Position - {ticket} retVal = {retVal}");
        }

        private static async void CloseRequest(MtApi5Client client, ulong ticket)
        {
            var retVal = await Execute(() => client.PositionClose(ticket));
            Console.WriteLine($"Close: Position - {ticket} retVal = {retVal}");
        }


        private static async void QuoteRequest(MtApi5Client client, string symbol)
        {
            MqlTick tick = null;
            var retVal = await Execute(() => client.SymbolInfoTick(symbol, out tick));
            Console.WriteLine($"Ask: {tick.ask}, Bid: {tick.bid}");
        }

        private static async void OrderQuoteRequest(MtApi5Client client, string symbol, Order order)
        {
            MqlTick tick = null;
            var retVal = await Execute(() => client.SymbolInfoTick(symbol, out tick));
            Console.WriteLine($"Ask: {tick.ask}, Bid: {tick.bid}");
            if (order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_BUY || order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_BUY_LIMIT)
            {
                order.Price = tick.bid;
            }
            else if (order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_SELL || order.OrderType == ENUM_ORDER_TYPE.ORDER_TYPE_SELL_LIMIT)
            {
                order.Price = tick.ask;
            }
        }

        private static async void Close()
        {
            ulong ticket = SourceOrderList.Last().TicketId;
            var retVal = await Execute(() => _mtapiSource.PositionClose(ticket));
            Console.WriteLine($"Close: Position - {ticket} retVal = {retVal}");
        }

        private static async Task<TResult> Execute<TResult>(Func<TResult> func)
        {
            return await Task.Factory.StartNew(() =>
            {
                var result = default(TResult);
                try
                {
                    result = func();
                }
                catch (ExecutionException ex)
                {
                    Console.WriteLine($"Exception: {ex.ErrorCode} - {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }

                return result;
            });
        }
    }

}
