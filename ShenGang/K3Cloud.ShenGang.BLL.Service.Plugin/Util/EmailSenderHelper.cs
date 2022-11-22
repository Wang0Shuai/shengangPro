using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
    public class EmailSenderHelper
    {
        public static String SENDER = "wangleixf@163.com";

        public static string EMAILSERVCIE = "smtp.163.com";//邮件服务器
        public static int EMAILPORT = 25;//端口
        public static String EMAILPWD = "UTXGEJFQTCJNDAHQ";//端口
        public static void sendEmail(string title, string content, List<string> recEmailList)
        {

            MailMessage mailObject = new MailMessage();
            //设置发件人
            mailObject.From = new MailAddress(SENDER);
            //设置收件人
            //mailObject.To.Add(new MailAddress("927991205@qq.com"));
            foreach (var eamil in recEmailList)
            {
                if (!string.IsNullOrEmpty(eamil) && eamil.Trim().Length > 0)
                    mailObject.To.Add(new MailAddress(eamil));
            }
            //设置邮件主题
            mailObject.SubjectEncoding = Encoding.UTF8;
            mailObject.Subject = title;

            mailObject.BodyEncoding = Encoding.UTF8;//编码
            mailObject.Body = content;

            Attachment attachment = new Attachment(@"D:/BugReport.txt");
            // 添加附件            
            mailObject.Attachments.Add(attachment);
            //创建一个发送邮件的对像 服务地址和端口
            SmtpClient smtpClient = new SmtpClient(EMAILSERVCIE, EMAILPORT);//smtp.163.com
            //帐号密码一定要正确,使用一个默认的账号
            smtpClient.Credentials = new NetworkCredential(SENDER, EMAILPWD);
            smtpClient.Send(mailObject);

            Kingdee.BOS.Log.Logger.Info("send-email", "send Mail OK: " + title + ",to:" + mailObject.To.ToString());

        }


    }
}
