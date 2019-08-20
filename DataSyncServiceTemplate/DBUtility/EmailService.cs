using System;
using System.Collections.Generic;
using System.Text;

namespace DBUtility
{
    public static class EmailService
    {
        /// <summary>
        /// 发送邮件的辅助类
        /// </summary>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <param name="Receivers"></param>
        /// <returns></returns>
        public static bool SendEmail(string Subject, string Body, string Receivers)
        {
            bool retValue = false;
            //System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("smtp.sina.com",587);
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
            client.Host = "smtp.sina.com";//使用163的SMTP服务器发送邮件
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential("cqmailsender@sina.com", "cq.net");
            //163的SMTP服务器需要用163邮箱的用户名和密码作认证，如果没有需要去163申请个,
            //这里假定你已经拥有了一个163邮箱的账户，用户名为abc，密码为*******
            System.Net.Mail.MailMessage Message = new System.Net.Mail.MailMessage();
            Message.From = new System.Net.Mail.MailAddress("cqmailsender@sina.com");
            //这里需要注意，163似乎有规定发信人的邮箱地址必须是163的，而且发信人的邮箱用户名必须和上面SMTP服务器认证时的用户名相同
            //因为上面用的用户名abc作SMTP服务器认证，所以这里发信人的邮箱地址也应该写为abc@163.com

            foreach (string toEmail in Receivers.TrimEnd(';').Split(';'))
            {
                Message.To.Add(toEmail);//将邮件发送给
            }
            Message.Subject = Subject;
            Message.Body = Body;
            Message.SubjectEncoding = System.Text.Encoding.UTF8;
            Message.BodyEncoding = System.Text.Encoding.UTF8;
            Message.Priority = System.Net.Mail.MailPriority.High;
            Message.IsBodyHtml = true;
            try
            {
                client.Send(Message);
                retValue = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return retValue;
        }
    }
}
