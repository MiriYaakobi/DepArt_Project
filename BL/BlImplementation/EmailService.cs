using System;
using System.Net;
using System.Net.Mail;

namespace BlImplementation 
{
    /// <summary>
    /// A service for sending email notifications.
    /// We used AI to understand how the email service works in code and how to implement it.
    /// </summary>
    internal static class EmailService
    {
        private const string SenderEmail = "depart.ilv@gmail.com";
        private static string SenderPassword = "srmp xvma gyau ftpx";/*System.IO.File.ReadAllText(@"C:\Users\1\source\repos\secrets.txt");*/

        /// <summary>
        /// sends an email notification asynchronously.
        /// </summary>
        /// <param name="recipientEmail"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public static void SendNotification(string recipientEmail, string subject, string body)
        {
            // if no recipient, do nothing
            if (string.IsNullOrEmpty(recipientEmail)) 
                return;

            // send email in the background to avoid blocking the main thread
            Task.Run(() =>
            {
                try
                {
                    // define SMTP client and email message
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                        EnableSsl = true,
                    };

                    // create the email message
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(SenderEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false,
                    };

                    // add recipient
                    mailMessage.To.Add(recipientEmail);

                    // send the email
                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Background Email Failed: " + ex.Message);
                }
            });
        }
    }
}