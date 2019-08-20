using System;
using System.Collections.Generic;
using System.Text;

namespace CustomizeConfig
{
    public class TagData
    {
        public string Tag { get; set; }

        public string Type { get; set; }

        public double Rate { get; set; }
    }

    public class ConfiguredTagData
    {
        public string id { get; set; }

        public string name { get; set; }

        public string type { get; set; }
    }
}
