using System;
using System.Collections.Generic;
using System.Text;

namespace CustomizeConfig
{
    public class MonitorData
    {
        public string Tag { get; set; }

        public DateTime CollectDate { get; set; }

        public DateTime Upload { get; set; }

        public double ValueData { get; set; }

        public string LesseeId { get; set; }
    }
}
