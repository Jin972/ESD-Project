using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace ESD_Project.Common
{
    public class Service
    {
        public bool SendAutometicalEmail(string receiver, string _subject, string _body)
        {
            try
            {
                var senderEmail = new MailAddress("fpt3584@gmail.com", "Admin");
                var Receiver = new MailAddress(receiver, "Receiver");
                var password = "123@123a";

                MailMessage mess = new MailMessage(senderEmail.Address, Receiver.Address);
                mess.Subject = _subject;
                mess.Body = _body;
                mess.IsBodyHtml = true;
                mess.BodyEncoding = System.Text.Encoding.UTF8;
                mess.SubjectEncoding = System.Text.Encoding.UTF8;
                mess.ReplyToList.Add(new MailAddress(senderEmail.Address));
                mess.Sender = new MailAddress(receiver);


                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(senderEmail.Address, password);
                smtp.Send(mess);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
           
        }
    }
}