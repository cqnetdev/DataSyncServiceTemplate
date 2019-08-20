using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Collections;
using DBUtility;
using System.IO;
using CustomizeConfig;
using log4net;

namespace SyncDataService
{
    /// <summary>
    /// Example service, rename and modify it to suit your needs
    /// </summary>
    public class DataService
    {
        private static readonly ILog Logger = LogManager.GetLogger("MyLogger");

        private readonly Thread _thread;
        private RSLinxOPCDA rsLinxOPCDA;
        public DataService()
        {
            _thread = new Thread(DoWork);
        }

        public void Start()
        {
            _thread.Start();
        }
        public void Stop()
        {
            _thread.Abort();
        }


        private void DoWork()
        {
            SyncData();
        }

        private ConfigService configService;
        private XmlService xmlService;
        private string[] xmlNodes;
        private int nFrequency = 10000;
        private int excuteSqlCount = 200;
        private string lesseeId;
        private ConfigGroup configGroup = null;
        private string UserName = "服务还未设置名称！！！";
        private string MailReceiver = "";

        private static string ConnnectionUrl = string.Empty;
        private static string ConnnectionGroups = string.Empty;

        private DbHelperOra dbHelperOra = new DbHelperOra();

        List<ConfiguredTagData> tagList = new List<ConfiguredTagData>();
        private bool isDebug = false;
        private bool InitConfigData()
        {

            bool retValue = false;
            string msg = string.Empty;
            retValue = dbHelperOra.TestConnection(ref msg);
            Console.WriteLine("测试数据库连接状态返回的结果：{0}     错误信息：{1}", retValue ? "成功" : "失败", msg);
            retValue = false;
            try
            {
                string SectionGroupName = "LuoBei";
                configService = new ConfigService();
                xmlService = new XmlService();
                lesseeId = configService.getValue(SectionGroupName, "lesseeId");
                string sqlCount = configService.getValue(SectionGroupName, "excuteSqlCount");
                int.TryParse(sqlCount, out excuteSqlCount);
                string frequency = configService.getValue(SectionGroupName, "collectFrequency");
                MailReceiver = ConfigurationManager.AppSettings["MailReceiver"].ToString();
                string configUserName = configService.getValue(SectionGroupName, "UserName");
                if (string.IsNullOrEmpty(configUserName))
                {
                    UserName = SectionGroupName + UserName;
                }
                else
                {
                    UserName = configUserName;
                }

                if (frequency != "")
                {
                    int.TryParse(frequency, out nFrequency);
                }
                ConnnectionUrl = configService.getValue(SectionGroupName, "ConnnectionUrl");
                ConnnectionGroups = configService.getValue(SectionGroupName, "ConnnectionGroups");
                isDebug = configService.getValue(SectionGroupName, "WriteToDB").ToUpper() == "TRUE" ? false : true;

                configGroup = configService.getConfigGroup(SectionGroupName);
                xmlNodes = configGroup.xmlNode.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string xmlNode in xmlNodes)
                {
                    List<ConfiguredTagData> tags = xmlService.getXmlTagConfig(configGroup.config, xmlNode);
                    foreach (var item in tags)
                    {
                        tagList.Add(item);
                    }
                }
                retValue = true;
            }
            catch (Exception ex)
            {
                Logger.Fatal("执行InitConfigData方法出错！" + ex.ToString());
            }
            return retValue;
        }

        private static int retrycount = 5;
        private bool InitServerConn()
        {
            bool retValue = false;
            Console.WriteLine("正在尝试连接到OPCServer");
            while (retrycount > 0)
            {
                try
                {
                    rsLinxOPCDA = new RSLinxOPCDA();
                    bool isConnected = rsLinxOPCDA.Connect(ConnnectionUrl);
                    if (isConnected)
                    {
                        Logger.Info(string.Format("连接到OPC服务器{0}成功", ConnnectionUrl));
                        rsLinxOPCDA.CreateGroup(ConnnectionGroups, nFrequency);
                    }
                    if (tagList != null && tagList.Count > 0)
                    {
                        foreach (var item in tagList)
                        {
                            rsLinxOPCDA.AddItem(item.id);
                        }
                    }
                    retValue = true;
                    return retValue;
                }
                catch (Exception ex)
                {
                    if (retrycount == 5)
                    {
                        Logger.Fatal("执行InitServerConn方法出错！" + ex.ToString());
                    }
                }
                finally
                {
                    if (!retValue)
                    {
                        Console.WriteLine(string.Format("连接到OPC服务器失败，剩余尝试次数为{0}！", retrycount.ToString()));
                        System.Threading.Thread.Sleep(30000);
                    }
                }
                retrycount--;
            }
            return retValue;
        }

        private static string LastExcutionTime = string.Empty;
        private void SyncData()
        {
            Logger.Info("开始运行数据同步服务");
            if (!InitConfigData())  //配置信息读取错误，服务停止
            {
                Console.WriteLine("请仔细阅读以上异常后，点击任何按钮以关闭程序...");
                Console.Read();
                this.Stop();
            }
            if (!InitServerConn()) //无法连接到OPC服务器，服务停止
            {
                InstantEmailService.CreateNewEmail(UserName + "同步数据异常通知", UserName + "同步数据出现异常（初始化OPC服务连接），需要及时处理！");
                Console.WriteLine("请仔细阅读以上异常后，点击任何按钮以关闭程序...");
                Console.Read();
                this.Stop();
            }


            while (true)
            {
                try
                {
                    var dateNow = DateTime.Now;
                    if (dateNow.ToString("yyyy-MM-dd hh:mm") == LastExcutionTime)
                    {
                        System.Threading.Thread.Sleep(nFrequency / 100);
                        continue;
                    }
                    else
                    {
                        LastExcutionTime = dateNow.ToString("yyyy-MM-dd hh:mm");
                    }
                    List<MonitorData> dataList = new List<MonitorData>();

                    Opc.Da.ItemValueResult[] itemValues = rsLinxOPCDA.m_server.Read(rsLinxOPCDA.m_items);
                    foreach (Opc.Da.ItemValueResult itemValue in itemValues)
                    {
                        if (isDebug)
                        {
                            Logger.Debug("Time: " + itemValue.Timestamp.ToString() + " Item: " + itemValue.ItemName + "  采集项名称:" + tagList.Find(p => p.id == itemValue.ItemName).name + "  采集值: " + itemValue.Value.ToString());
                        }
                        double dobval = double.Parse(itemValue.Value.ToString());
                        MonitorData monitorData = new MonitorData();
                        monitorData.Tag = tagList.Find(p => p.id == itemValue.ItemName).name;
                        monitorData.CollectDate = itemValue.Timestamp;
                        monitorData.Upload = DateTime.Now;
                        monitorData.ValueData = Math.Round(dobval, 3);
                        monitorData.LesseeId = lesseeId;
                        dataList.Add(monitorData);
                    }

                    if (dataList.Count > 0)
                    {
                        Console.WriteLine(string.Format("应该写入数据库的数据记录条数为{0}", dataList.Count));
                        if (!isDebug)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(SaveRealData), dataList);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() != typeof(ThreadAbortException))
                    {
                        InstantEmailService.CreateNewEmail(UserName + "同步数据异常通知", UserName + "同步数据出现异常，需要及时处理！详细错误为：" + ex.ToString());
                    }
                    Console.Write("数据同步出现异常!!!" + ex.ToString());
                    Logger.Fatal("执行方法SyncData出错！" + ex.ToString());
                }
            }
        }



        /// <summary>
        /// 保存上传数据
        /// </summary>
        /// <param name="monitorId">标签</param>
        /// <param name="value">值</param>
        /// <param name="lesseeId">租户ID</param>
        /// <param name="collectDate">上传时间</param>
        /// <returns></returns>
        public void SaveRealData(object tag)
        {
            ArrayList sqlList = new ArrayList();
            List<MonitorData> list = (List<MonitorData>)tag;
            try
            {
                foreach (MonitorData data in list)
                {
                    sqlList.Add(string.Format(@"insert WHEN(NOT EXISTS(SELECT 1 FROM WATER.MONITOR_DATA WHERE MONITOR_ID='{0}' AND COLLECT_DATE=to_date('{1}','yyyy-MM-dd hh24:mi:ss'))) THEN 
                                                into WATER.MONITOR_DATA(MONITOR_ID,COLLECT_DATE,VALUE_DATA,UPLOAD,LESSEE_ID) 
                                                SELECT '{0}' MONITOR_ID,to_date('{1}','yyyy-MM-dd hh24:mi:ss') COLLECT_DATE,{2} VALUE_DATA,to_date('{3}','yyyy-MM-dd hh24:mi:ss') UPLOAD,'{4}' LESSEE_ID FROM DUAL",
                            data.Tag, data.CollectDate, data.ValueData, data.Upload, data.LesseeId));
                }

                // Logger.Info("应执行的数据库操作语句条数为：" + sqlList.Count.ToString() + "  ||最后一条语句为：" + sqlList[sqlList.Count - 1].ToString());
                dbHelperOra.ExecuteSqlTran(sqlList, excuteSqlCount);
                Logger.Info("执行数据写入数据库的记录个数为：" + sqlList.Count.ToString() + ",操作成功！！");
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    InstantEmailService.CreateNewEmail(UserName + "同步数据异常通知", UserName + "同步数据出现异常，需要及时处理！详细错误为：" + ex.ToString());
                }
                Console.WriteLine("将采集数据保存到数据库出现异常！");
                // Logger.Fatal("【" + list[0].Tag + "】数据保存出错，脚本为：" + Environment.NewLine + string.Join(";", (string[])sqlList.ToArray(typeof(string))));
                Logger.Fatal("执行方法TransSaveRealData出错！" + ex.ToString());
            }
        }
    }
}