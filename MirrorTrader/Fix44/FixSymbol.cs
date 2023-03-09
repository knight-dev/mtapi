using System.Collections.Generic;
using System.Linq;

namespace MirrorTrader.Fix44
{
    public class FixSymbol
    {
        public string name;
        public int id;
    }
    public static class FixSymbols
    {
        private static List<FixSymbol> symbols = new List<FixSymbol>();

        static FixSymbols()
        {
            symbols.Add(new FixSymbol() { id = 1, name = "EURUSD" });
            symbols.Add(new FixSymbol() { id = 2, name = "GBPUSD" });
            symbols.Add(new FixSymbol() { id = 3, name = "EURJPY" });
            symbols.Add(new FixSymbol() { id = 4, name = "USDJPY" });
            symbols.Add(new FixSymbol() { id = 5, name = "AUDUSD" });
            symbols.Add(new FixSymbol() { id = 6, name = "USDCHF" });
            symbols.Add(new FixSymbol() { id = 7, name = "GBPJPY" });
            symbols.Add(new FixSymbol() { id = 8, name = "USDCAD" });
            symbols.Add(new FixSymbol() { id = 9, name = "EURGBP" });
            symbols.Add(new FixSymbol() { id = 10, name = "EURCHF" });
            symbols.Add(new FixSymbol() { id = 11, name = "AUDJPY" });
            symbols.Add(new FixSymbol() { id = 12, name = "NZDUSD" });
            symbols.Add(new FixSymbol() { id = 13, name = "CHFJPY" });
            symbols.Add(new FixSymbol() { id = 14, name = "EURAUD" });
            symbols.Add(new FixSymbol() { id = 15, name = "CADJPY" });
            symbols.Add(new FixSymbol() { id = 16, name = "GBPAUD" });
            symbols.Add(new FixSymbol() { id = 17, name = "EURCAD" });
            symbols.Add(new FixSymbol() { id = 18, name = "AUDCAD" });
            symbols.Add(new FixSymbol() { id = 19, name = "GBPCAD" });
            symbols.Add(new FixSymbol() { id = 20, name = "AUDNZD" });
            symbols.Add(new FixSymbol() { id = 41, name = "XAUUSD" }); // GOLD
            symbols.Add(new FixSymbol() { id = 42, name = "XAGUSD" }); // SILVER
            symbols.Add(new FixSymbol() { id = 22395, name = "BTCUSD" }); // BTC
            symbols.Add(new FixSymbol() { id = 22395, name = "XBTUSD" }); // BTC
        }

        public static FixSymbol Find(string currency)
        {
            // find currency of interest
            return symbols.First(x => x.name == currency);
        }
    }
}

