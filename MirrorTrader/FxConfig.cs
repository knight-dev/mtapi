using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorTrader
{
    class FxConfig
    {
        public int sourcePort = 8228;
        public int destinationPort = 8229;
        public double lots = 0.03;
        public int timerDuration = 60;
        public double Factor = 0.4;
        public double ThresholdFactor = 0.2;
    }
}
