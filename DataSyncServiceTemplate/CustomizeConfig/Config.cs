using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace CustomizeConfig
{
    public class Config:ConfigurationSection
    {
        private static readonly ConfigurationProperty s_property = new ConfigurationProperty(string.Empty, typeof(MyKeyValueCollection),
            null, ConfigurationPropertyOptions.IsDefaultCollection);
        [ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public MyKeyValueCollection KeyValues
        {
            get
            {
                return (MyKeyValueCollection)base[s_property];
            }
        }
    }
}
