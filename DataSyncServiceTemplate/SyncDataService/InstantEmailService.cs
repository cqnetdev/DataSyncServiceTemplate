using DBUtility;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Timers;

namespace SyncDataService
{
    public class InstantEmailService
    {
        private static readonly ILog Logger = LogManager.GetLogger("MyLogger");

        private readonly Thread _thread;

        private static TList<EmailKeeper> HistoryEmails;

        private static string MailReceiver = string.Empty;
        public InstantEmailService()
        {
            HistoryEmails = new TList<EmailKeeper>();
            _thread = new Thread(DoWork);
        }

        public void Start()
        {
            Logger.Info("开始执行即时邮件通知服务");
            _thread.Start();
        }
        public void Stop()
        {
            _thread.Abort();
            Logger.Info("即时邮件通知服务已停止");
        }


        private void DoWork()
        {
            InitConfigData();
            OnTimedEvent(null, null);
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 60 * 1000;
            aTimer.Enabled = true;
            GC.KeepAlive(aTimer);
        }

        private void InitConfigData()
        {
            MailReceiver = ConfigurationManager.AppSettings["MailReceiver"].ToString();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                //查询邮件列表是否有待发送邮件，有就发送邮件
                if (HistoryEmails != null && HistoryEmails.Count > 0)
                {
                    string emailBody = string.Empty;
                    foreach (EmailKeeper item in HistoryEmails)
                    {
                        if (item.RemainSendCount > 0 && !item.isEmailSend)
                        {
                            emailBody += string.Format("标题【{0}】内容：{1}", item.header, item.body) + "<br/>";
                            item.isEmailSend = true;
                            item.EmailSendTime = DateTime.Now;
                        }
                        if (DateTime.Now.Subtract(item.LastErrorTime).Hours >= 1)
                        {
                            HistoryEmails.Remove(item);
                        }
                    }
                    if (!string.IsNullOrEmpty(MailReceiver) && !string.IsNullOrEmpty(emailBody))
                    {
                        EmailService.SendEmail("来自数据监测服务的通知", "【" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "】" + emailBody, MailReceiver);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("发送邮件失败！", ex);
            }
            //将邮件表1小时之前的错误清除（再次出错就）
        }

        public static void CreateNewEmail(string header, string body)
        {
            try
            {
                if (HistoryEmails == null)
                {
                    HistoryEmails.Add(new EmailKeeper { header = header, body = body, RemainSendCount = 3, LastErrorTime = DateTime.Now, isEmailSend = false });
                }
                else
                {
                    foreach (EmailKeeper item in HistoryEmails)
                    {
                        if (item.header == header && item.body == body)
                        {
                            EmailKeeper CurrentEmail = item;
                            if (CurrentEmail != null)
                            {
                                CurrentEmail.RemainSendCount -= 1;
                                CurrentEmail.isEmailSend = false;
                                CurrentEmail.LastErrorTime = DateTime.Now;
                            }
                        }
                        else
                        {
                            HistoryEmails.Add(new EmailKeeper { header = header, body = body, RemainSendCount = 3, LastErrorTime = DateTime.Now, isEmailSend = false });
                        }
                    }
                }
                /*if (HistoryEmails == null || HistoryEmails.Where(p => p.header == header && p.body == body).FirstOrDefault() == null)
                {
                    HistoryEmails.Add(new EmailKeeper { header = header, body = body, RemainSendCount = 3, LastErrorTime = DateTime.Now, isEmailSend = false });
                }
                else if (HistoryEmails.Where(p => p.header == header && p.body == body).FirstOrDefault() != null)
                {
                    EmailKeeper CurrentEmail = HistoryEmails.Where(p => p.header == header && p.body == body).FirstOrDefault();
                    if (CurrentEmail != null)
                    {
                        CurrentEmail.RemainSendCount -= 1;
                        CurrentEmail.isEmailSend = false;
                        CurrentEmail.LastErrorTime = DateTime.Now;
                    }
                }
                else
                {
                    Logger.Error(string.Format("标题：【{0}】内容：【{1}】的消息无法发送Email", header, body));
                }*/
            }
            catch (Exception ex)
            {
                Logger.Fatal(string.Format("执行创建新邮件的过程中出现异常，标题为【{0}】内容为：{1}", header, body), ex);
            }
        }
    }
}
