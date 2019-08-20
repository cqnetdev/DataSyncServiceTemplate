using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Configuration;

namespace CustomizeConfig
{
    public class XmlService
    {
        /// <summary>  
        /// 返回XMl文件指定元素的指定属性值  
        /// </summary>  
        /// <param name="xmlElement">指定元素</param>  
        /// <param name="xmlAttribute">指定属性</param>  
        /// <returns></returns>  
        public List<TagData> getXmlValues(string fileName, string name)
        {
            List<TagData> list = new List<TagData>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName);
            XmlNode node = xmlDoc.SelectSingleNode("factorys//" + name);
            XmlNodeList nodes = node.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Name == "tag")
                {
                    TagData tagData = new TagData()
                    {
                        Tag = nodes[i].Attributes["id"].Value,
                        Type = nodes[i].Attributes["type"].Value
                    };
                    string rate = nodes[i].InnerText;
                    if (rate == "")
                    {
                        rate = "1";
                    }
                    double Rate = 1;
                    double.TryParse(rate,out Rate);
                    tagData.Rate = Rate;
                    list.Add(tagData);
                }
            }
            return list;
        }

        /// <summary>  
        /// 返回XMl文件指定元素的指定属性值  
        /// </summary>  
        /// <param name="xmlElement">指定元素</param>  
        /// <param name="xmlAttribute">指定属性</param>  
        /// <returns></returns>  
        public List<string> getXmlValues2(string fileName, string name)
        {
            List<string> list = new List<string>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName);
            XmlNode node = xmlDoc.SelectSingleNode("factorys//" + name);
            XmlNodeList nodes = node.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Name == "tag")
                {
                    list.Add(nodes[i].Attributes["id"].Value);
                }
            }
            return list;
        }

        public List<ConfiguredTagData> getXmlTagConfig(string fileName, string name)
        {
            List<ConfiguredTagData> list = new List<ConfiguredTagData>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName);
            XmlNode node = xmlDoc.SelectSingleNode("factorys//" + name);
            XmlNodeList nodes = node.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Name == "tag")
                {
                    ConfiguredTagData tag = new ConfiguredTagData();
                    tag.id = nodes[i].Attributes["id"].Value;
                    tag.name = nodes[i].Attributes["name"].Value;
                    tag.type = nodes[i].Attributes["type"].Value;
                    list.Add(tag);
                }
            }
            return list;
        }
    }
}
