using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SendMailforAWSorSMTP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SendMailforAWSorSMTP;
using System.IO;
using System.Net.Mail;
using CsvHelper;
using System.Globalization;
using SendMailFromAWSorSMTP.Helper;
using Microsoft.Extensions.Configuration;

namespace SendMailforAWSorSMTP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static IConfiguration _config;

        public HomeController(ILogger<HomeController> logger,IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.resMessage = "";
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormCollection form, IFormFile fm_htmlFile, ICollection<IFormFile> fm_Attachments, IFormFile fm_Contacts)
        {
            string returnMessage= string.Empty;
            string message = string.Empty;
            List<Attachment> attachments = new List<Attachment>();
            if (form["rbType"]=="HTML")
            {
                if (fm_htmlFile != null && fm_htmlFile.Length > 0)
                {
                    using (StreamReader reader = new StreamReader(fm_htmlFile.OpenReadStream()))
                    {
                        message = reader.ReadToEnd();
                    }
                }
                else
                {
                    returnMessage = "Please enter valid HTML file";
                }
            }
            else
            {
                message = form["fm_Editor"];
            }
            if(fm_Attachments!=null && fm_Attachments.Count > 0)
            {
                foreach(var item in fm_Attachments)
                {
                    Attachment attachment = new Attachment(item.OpenReadStream(), item.FileName);
                    attachments.Add(attachment);
                    
                }
            }
            if(fm_Contacts !=null && fm_Contacts.Length>0)
            {
                int count = 0;

                using (StreamReader stream = new StreamReader(fm_Contacts.OpenReadStream()))
                {
                    using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
                    {
                        //csv.Configuration.RegisterClassMap<PersonsMap>();
                        var records = csv.GetRecords<Persons>().ToList();
                        foreach (var item in records)
                        {
                            try
                            {
                                string newmessage = message.Replace("{{Name}}", item.Name);
                                newmessage = newmessage.Replace("{{Email}}", item.Email);
                                newmessage = newmessage.Replace("{{Mobile}}", item.Mobile);
                                newmessage = newmessage.Replace("{{var1}}", item.var1);
                                newmessage = newmessage.Replace("{{var2}}", item.var2);
                                newmessage = newmessage.Replace("{{var3}}", item.var3);
                                newmessage = newmessage.Replace("{{var4}}", item.var4);
                                newmessage = newmessage.Replace("{{var5}}", item.var5);
                                newmessage = newmessage.Replace("{{var6}}", item.var6);
                                newmessage = newmessage.Replace("{{var7}}", item.var7);
                                newmessage = newmessage.Replace("{{var8}}", item.var7);
                                newmessage = newmessage.Replace("{{var9}}", item.var8);
                                newmessage = newmessage.Replace("{{var10}}", item.var10);
                                AmazonSESService service = new AmazonSESService(form["mailFrom"],item.Email, form["fm_Subject"], newmessage, attachments, _config,null, form["replyTo"]);
                                var sent = service.SendMail();
                                if (sent != "error")
                                {
                                    MySQLContext context = HttpContext.RequestServices.GetService(typeof(MySQLContext)) as MySQLContext;
                                    EmailLogs email = new EmailLogs()
                                    {
                                        Email = item.Email,
                                        Subject = form["fm_Subject"],
                                        MessageID = sent,
                                        Message = newmessage,
                                        Name = item.Name,
                                        Campaign = form["campaign"],
                                        replyTo = form["replyTo"],
                                        sentOn = DateTime.Now
                                    };
                                    context.SaveEmailLog(email);
                                    count++;
                                }
                            }
                            catch (Exception ex)
                            {
                                returnMessage = "Error occured sending email: " + ex.Message.ToString();
                            }
                        }
                    }
                }
                returnMessage = "Email Sent to " + count + " Contacts";
            }
            else
            {
                returnMessage = "Please upload CSV file to send emails";
            }
            ViewBag.resMesage = returnMessage;
            return RedirectToAction("Index");

        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ReadAllRecords()
        {
            var model = new List<EmailLogs>();
            MySQLContext context = HttpContext.RequestServices.GetService(typeof(MySQLContext)) as MySQLContext;
            model = context.GetEmailLogs();
            return View(model);
        }
    }
}
