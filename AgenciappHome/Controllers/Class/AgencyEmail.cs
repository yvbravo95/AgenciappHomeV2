using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers.Class
{
    public class AgencyEmail
    {
        private databaseContext _context;
        private string templateFolderPath;
        private string host, uss, pss, emailSend;
        private int port;

        public AgencyEmail(databaseContext context)
        {
            _context = context;
            //var config = _context.Config.First();
            templateFolderPath = Path.GetFullPath("~/Views/EmailTemplate/").Replace("~\\", "");
            //emailSend = config.Email_User;
            //host = config.Email_Server; //"smtp.elasticemail.com";
            //uss = config.Email_User;
            //pss = config.Email_Pass;
            //port = config.Email_Port;
        }
        
        public bool SendEmail(string to, string subject, string templateName, Dictionary<string, Object> values)
        {
            var emailBody = File.ReadAllText(templateFolderPath + "/" + templateName);
            for (int i = 0; i < values.Count; i++)
            {
                var key = values.Keys.ElementAt(i);
                var t = values[key];
                emailBody = emailBody.Replace("[" + key + "]", (string)values[key]);
            }

            try
            {
                MailMessage mailMessage = new MailMessage();
                MailAddress fromAddress = new MailAddress(emailSend);
                mailMessage.From = fromAddress;
                mailMessage.To.Add(to);
                mailMessage.Body = emailBody;
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = subject;
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = host;
                smtpClient.Port = port;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new System.Net.NetworkCredential(uss, pss);
                smtpClient.Send(mailMessage);
                return true;
            }
            catch {
                return false;
            }
        }
    }
}
