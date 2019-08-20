using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace CustomizeConfig
{
    public class MyKeyValueSetting: ConfigurationElement    // 集合中的每个元素
    {
        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get { return this["key"].ToString(); }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get { return this["value"].ToString(); }
            set { this["value"] = value; }
        }
    }
}
