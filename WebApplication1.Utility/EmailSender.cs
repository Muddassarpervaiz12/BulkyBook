using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //email send using two package mimekit and mailkit
            //use for default
            // return Task.CompletedTask;
            var emailToSend = new MimeMessage();
            //config this email to send and from which email address you use for message (like admin have company mail)
            emailToSend.From.Add(MailboxAddress.Parse("muddassarpervaiz802@gmail.com"));
            //load email of the user that company will send email to him/her
            emailToSend.To.Add(MailboxAddress.Parse(email));
            //subject of email
            emailToSend.Subject = subject;
            //body of email
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html){ Text= htmlMessage};

            //send email using SMTP Client (use mailkit package add line through ctrl+. usingMailkit.Net.Smpt;)
            using (var emailClient = new SmtpClient())
            {
                //connect with our gmail account (so we use smpt.gmai.com because google is also smpt server)
                //second is 587 this is google port number
                //Third is security with that we will be able to connect the server.
                emailClient.Connect("smtp.gmail.com",587,MailKit.Security.SecureSocketOptions.StartTls);

                //so for establish connection to the smpt server 
                //we need to enter username and password
                emailClient.Authenticate("muddassarpervaiz802@gmail.com", "hgkyzycxanhctuxc");
                emailClient.Send(emailToSend);
                //after send email you disconnect with client
                emailClient.Disconnect(true);
            }
            return Task.CompletedTask;
        }
    }
}
