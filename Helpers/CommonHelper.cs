using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Helpers
{
    public class CommonHelper
    {
        public static string CutDesc(string desc, int length)
        {
            if (desc != null)
                if (desc.Length > length)
                    desc = desc.Substring(0, length) + "...";
            return desc;
        }

        public static string CutDesc(string desc, int length, string replaceSymbol)
        {
            if (desc != null)
                if (desc.Length > length)
                    desc = desc.Substring(0, length) + replaceSymbol;
            return desc;
        }

        public static string CutDesc(string desc, int startIndex, int length, string replaceSymbol)
        {
            if (desc != null)
                if (desc.Length > length)
                    desc = desc.Substring(startIndex, length) + replaceSymbol;
            return desc;
        }

        public static Boolean Email(string message, string subject, string toEmail, string fromEmail, string fromName)
        {
            System.Net.Mail.MailMessage mailObj = new System.Net.Mail.MailMessage();
            mailObj.From = new System.Net.Mail.MailAddress(fromEmail, fromName);
            mailObj.To.Add(toEmail);
            mailObj.Subject = subject;
            mailObj.Body = message;
            mailObj.IsBodyHtml = true;
            System.Net.Mail.SmtpClient SMTPServer = new System.Net.Mail.SmtpClient("mail.bigpond.com");
            //System.Net.Mail.SmtpClient SMTPServer = new System.Net.Mail.SmtpClient("localhost");
            try
            {
                SMTPServer.Send(mailObj);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}