using System;
using System.Net;
using System.Net.Mail;

namespace Project.Lib;
public class SMTPSender
{
    private string smtpServer;
    private int smtpPort;
    private string smtpUsername;
    private string smtpPassword;

    public SMTPSender(string smtpServer, int smtpPort, string smtpUsername, string smtpPassword)
    {
        this.smtpServer = smtpServer;
        this.smtpPort = smtpPort;
        this.smtpUsername = smtpUsername;
        this.smtpPassword = smtpPassword;
    }

    public void SendEmail(string to, string subject, string body, string from, bool isHtml = false)
    {
        using (SmtpClient smtpClient = new SmtpClient(smtpServer))
        {
            smtpClient.Port = smtpPort;
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            smtpClient.EnableSsl = true;

            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(from);
                mailMessage.To.Add(to);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = isHtml;

                smtpClient.Send(mailMessage);
            }
        }
    }
}