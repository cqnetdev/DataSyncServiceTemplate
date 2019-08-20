using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Reflection;
namespace CustomizeConfig
{
    public class ConfigService
    {
        public List<ConfigGroup> configGroupList = new List<ConfigGroup>();
        /// <summary>
        /// 获取配置参数
        /// </summary>
        /// <param name="cid">租户ID</param>
        /// <param name="key">key</param>
        /// <returns></returns>
        public string getValue(string cid, string key)
        {
            Config config = (Config)ConfigurationManager.GetSection(cid);
            string value =string.Empty;
            foreach (MyKeyValueSetting item in config.KeyValues)
            {
                if (item.Key == key)
                {
                    value = item.Value;
                }
            }
            return value;
        }

        /// <summary>
        /// 获取配置参数
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        public List<ConfigGroup> getConfigGroup(List<string> listIds)
        {
            foreach (string id in listIds)
            {
                ConfigGroup configGroup = new ConfigGroup();
                PropertyInfo [] propertyInfos = configGroup.GetType().GetProperties();
                foreach(PropertyInfo propertyInfo in propertyInfos)
                {
                    propertyInfo.SetValue(configGroup, getValue(id, propertyInfo.Name), null);
                }
                configGroup.cId = id;
                configGroupList.Add(configGroup);
            }
            return configGroupList;
        }


        /// <summary>
        /// 获取配置参数
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        public ConfigGroup getConfigGroup(string id)
        {
            ConfigGroup configGroup = new ConfigGroup();
            PropertyInfo[] propertyInfos = configGroup.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                propertyInfo.SetValue(configGroup, getValue(id, propertyInfo.Name), null);
            }
            configGroup.cId = id;
            return configGroup;
        }
    }
}
