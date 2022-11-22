using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
    public class MailTools
    {
        public static void Sendmail(string from, string displayName, List<string> toList, string subject, string body, string SMTPHost, int port, bool enableSSL, Dictionary<string, Stream> dictStream, string userName, string pass, string medieType = "application/pdf")
        {
            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(from, displayName);
                foreach (string addresses in toList)
                {
                    mailMessage.To.Add(addresses);
                }
                mailMessage.Subject = subject;
                mailMessage.SubjectEncoding = Encoding.UTF8;
                mailMessage.Body = body;
                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.IsBodyHtml = true;
                if (dictStream != null && dictStream.Count > 0)
                {
                    foreach (KeyValuePair<string, Stream> keyValuePair in dictStream)
                    {
                        Attachment item = new Attachment(keyValuePair.Value, keyValuePair.Key,medieType);
                        mailMessage.Attachments.Add(item);
                    }
                }
                using (SmtpClient smtpClient = new SmtpClient(SMTPHost, port))
                {
                    smtpClient.UseDefaultCredentials = true;
                    if (!string.IsNullOrWhiteSpace(userName))
                    {
                        smtpClient.Credentials = new NetworkCredential(userName, pass);
                    }
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    mailMessage.Priority = MailPriority.Normal;
                    smtpClient.EnableSsl = enableSSL;
                    MailTools.Sendmail(mailMessage, smtpClient);
                }
            }
        }

        public static void SendmailWithCC(string from, string displayName, List<string> toList, List<string> toListCC, string subject, string body, string SMTPHost, int port, bool enableSSL, Dictionary<string, Stream> dictStream, string userName, string pass)
        {
            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(from, displayName);
                foreach (string addresses in toList)
                {
                    mailMessage.To.Add(addresses);
                }
                if (toListCC.Count>0)
                {
                    foreach (string addresses in toListCC)
                    {
                        mailMessage.CC.Add(addresses);
                    }
                }
                mailMessage.Subject = subject;
                mailMessage.SubjectEncoding = Encoding.UTF8;
                mailMessage.Body = body;
                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.IsBodyHtml = true;
                if (dictStream != null && dictStream.Count > 0)
                {
                    foreach (KeyValuePair<string, Stream> keyValuePair in dictStream)
                    {
                        Attachment item = new Attachment(keyValuePair.Value, keyValuePair.Key);
                        mailMessage.Attachments.Add(item);
                    }
                }
                using (SmtpClient smtpClient = new SmtpClient(SMTPHost, port))
                {
                    
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    mailMessage.Priority = MailPriority.Normal;
                    smtpClient.EnableSsl = enableSSL;
                    smtpClient.Timeout = 600000;
                    smtpClient.UseDefaultCredentials = true;
                    if (!string.IsNullOrWhiteSpace(userName))
                    {
                        smtpClient.Credentials = new NetworkCredential(userName, pass);
                    }
                    MailTools.Sendmail(mailMessage, smtpClient);
                }
            }
        }

        // Token: 0x06001242 RID: 4674 RVA: 0x00035BA0 File Offset: 0x00033DA0
        private static void Sendmail(MailMessage mailMsg, SmtpClient client)
        {
            client.Send(mailMsg);
        }

    }
}
