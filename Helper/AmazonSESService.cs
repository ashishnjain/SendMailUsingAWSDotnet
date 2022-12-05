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
        public AmazonSESService(string fromAddress,string toAddress, string subject, string htmlBody, List<Attachment> attachments, IConfiguration configuration, string[] BCCAddress = null, string replyToEmail = "")
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

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <returns></returns>
        public string SendMail()
        {
            string resMessage = string.Empty;
            try
            {
                string accessKey = Convert.ToString(_config.GetSection("AWS").GetSection("accessKey").Value);
                string secretKey = Convert.ToString(_config.GetSection("AWS").GetSection("secretKey").Value);

                var message = SESMessage();
                var stream = new MemoryStream();

                message.WriteTo(stream);

                AWSCredentials credentials = new BasicAWSCredentials(accessKey, secretKey);

                using (var client = new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.APSouth1))
                {
                    RawMessage rawMessage = new RawMessage();
                    rawMessage.Data = stream;
                    var request = new SendRawEmailRequest(rawMessage);
                    var response = client.SendRawEmailAsync(request);
                    resMessage = response.Result.MessageId;
                }
                
            }
            catch (Exception e)
            {
                //ErrorLog(e.Message);
                //ErrorLog(e.StackTrace);
                if (e.InnerException != null)
                {
                    //ErrorLog(e.InnerException.Message);
                    //ErrorLog(e.InnerException.StackTrace);
                }
                resMessage= "error";
            }
            return resMessage;

        }
    }
}