using SendMailFromAWSorSMTP;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net;

namespace SendMailFromAWSorSMTP.Helper
{
    public class AmazonSESService
    {
        private IConfiguration _config;
        /// <summary>
        /// Gets or sets to address.
        /// </summary>
        /// <value>
        /// To address.
        /// </value>
        public string ToAddress { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the HTML body.
        /// </summary>
        /// <value>
        /// The HTML body.
        /// </value>
        public string HTMLBody { get; set; }

        /// <summary>
        /// Gets or sets the attachment.
        /// </summary>
        /// <value>
        /// The attachment.
        /// </value>
        public string Attachment { get; set; }

        /// <summary>
        /// Gets or sets from address.
        /// </summary>
        /// <value>
        /// From address.
        /// </value>
        public string FromAddress { get; set; }

        public string[] BCCEmailIds { get; set; }

        public List<Attachment> Attachments { get; set; }

        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="AmazonSESService"/> class from being created.
        /// </summary>
        private AmazonSESService(IConfiguration configuration)
        {
            _config = configuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonSESService"/> class.
        /// </summary>
        /// <param name="toAddress">To address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="htmlBody">The HTML body.</param>
        /// <param name="attachment">The attachment.</param>
        public AmazonSESService(string fromAddress, string toAddress, string subject, string htmlBody, List<Attachment> attachments, IConfiguration configuration, string[] BCCAddress = null, string replyToEmail = "")
        {
            ToAddress = toAddress;
            Subject = subject;
            HTMLBody = htmlBody;
            Attachments = attachments;
            if (BCCAddress != null)
            {
                BCCEmailIds = BCCAddress;
            }
            ReplyToEmail = replyToEmail;
            _config = configuration;
            FromAddress = fromAddress;
        }

        /// <summary>
        /// Amazon SES message.
        /// </summary>
        /// <returns></returns>
        private MimeKit.MimeMessage SESMessage()
        {
            var message = new MimeKit.MimeMessage();
            try
            {
                //string HeaderId = Convert.ToString(Properties.Settings.Default["headerId"]);
                //string HeaderValue = Convert.ToString(Properties.Settings.Default["headerValue"]);
                var from = MailboxAddress.Parse(FromAddress);
                var to = MailboxAddress.Parse(ToAddress);
                message.From.Add(from);
                message.To.Add(to);
                if (!string.IsNullOrWhiteSpace(ReplyToEmail))
                {
                    var reply = MailboxAddress.Parse(ReplyToEmail);
                    message.ReplyTo.Add(reply);
                }
                if (BCCEmailIds != null)
                {
                    //ErrorLog("BccEmail Count::" + Convert.ToString(BCCEmailIds.Length));
                    for (int i = 0; i < BCCEmailIds.Length; i++)
                    {
                        var bcc = MailboxAddress.Parse(BCCEmailIds[i]);
                        //ErrorLog("BCCMail Id::" + BCCEmailIds[i]);
                        message.Bcc.Add(bcc);
                    }
                }
                message.Subject = Subject;

                var builder = new BodyBuilder();
                builder.HtmlBody = HTMLBody;

                if (null != this.Attachments)
                {
                    foreach (var item in Attachments)
                    {
                        builder.Attachments.Add(item.Name, item.ContentStream);
                    }
                }

                message.Body = builder.ToMessageBody();
            }
            catch (Exception ex)
            {
                //ErrorLog("BccEmail Count::" + ex.Message);
                //ErrorLog("BccEmail Count::" + ex.StackTrace);
                throw;
            }

            return message;
        }

        public string SendMail()
        {
            string resMesage;
            try
            {
                string accessKey = Convert.ToString(_config.GetSection("AWS").GetSection("accessKey").Value);
                string secretKey = Convert.ToString(_config.GetSection("AWS").GetSection("secretKey").Value);
                var client = new SmtpClient("email-smtp.ap-south-1.amazonaws.com", 587)
                {
                    Credentials = new NetworkCredential(accessKey, secretKey),
                    EnableSsl = true
                };
                var fromAddress = new MailAddress(FromAddress);
                MailMessage message = new MailMessage();
                if (Attachments != null)
                {
                    foreach (var att in Attachments)
                    {
                        message.Attachments.Add(att);
                    }
                    // System.Net.Mime.ContentType htmltype = new System.Net.Mime.ContentType("text/html");

                    // System.Net.Mime.ContentType contype = new System.Net.Mime.ContentType("text/calendar");
                    // contype.Parameters.Add("method", "REQUEST");
                    // //  contype.Parameters.Add("name", "Meeting.ics");
                    // AlternateView avHtml = AlternateView.CreateAlternateViewFromString(body, htmltype);
                    // AlternateView avCal = AlternateView.CreateAlternateViewFromString(icalMessage, contype);
                    // message.AlternateViews.Add(avHtml);
                    // message.AlternateViews.Add(avCal);
                    // message.Headers.Add("Content-class", "urn:content-classes:calendarmessage");
                }
                var headerId = Guid.NewGuid().ToString();
                message.Headers.Add("Identity", headerId);
                message.Body = HTMLBody;
                message.Subject = Subject;
                message.From = fromAddress;
                message.IsBodyHtml = true;
                message.To.Add(ToAddress);
                client.EnableSsl = true;
                client.Send(message);                
                resMesage = headerId;
            }
            catch (Exception ex)
            {
                resMesage = "error";
            }
            return resMesage;
            
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <returns></returns>
        // public async Task<string> SendMail()
        // {
        //     string resMessage = string.Empty;
        //     try
        //     {
        //         string accessKey = Convert.ToString(_config.GetSection("AWS").GetSection("accessKey").Value);
        //         string secretKey = Convert.ToString(_config.GetSection("AWS").GetSection("secretKey").Value);

        //         // var message = SESMessage();
        //         // var stream = new MemoryStream();

        //         // message.WriteTo(stream);
        // // var destination = new Destination(new List<string> { ToAddress });

        //         // AWSCredentials credentials = new AWSCredentials(accessKey, secretKey);

        //         using (var client = new AmazonSimpleEmailServiceClient(accessKey,secretKey, RegionEndpoint.APSouth1))
        //         {
        //             Message rawMessage = new Message()
        //             {
        //                 Body = new Body { Html = new Content { Data = HTMLBody, Charset="UTF-8"} },
        //                 Subject = new Content(Subject)
        //             };
        //             Destination destination = new Destination { ToAddresses = new List<string>() { "ToAddress"} };
        //             // rawMessage.Data = stream;
        //             var request = new SendEmailRequest("abhishek@religocapital.com", destination, rawMessage);
        //             var response = await client.SendEmailAsync(request);
        //             resMessage = response.MessageId;
        //             // var request = new SendRawEmailRequest(rawMessage);
        //             // var response = client.SendRawEmailAsync(request);
        //             // resMessage = response.Result.MessageId;
        //         }

        //     }
        //     catch (Exception e)
        //     {
        //         // ErrorLog(e.Message);
        //         //ErrorLog(e.StackTrace);
        //         if (e.InnerException != null)
        //         {
        //             //ErrorLog(e.InnerException.Message);
        //             //ErrorLog(e.InnerException.StackTrace);
        //         }
        //         resMessage= "error";
        //     }
        //     return resMessage;

        // }
    }
}