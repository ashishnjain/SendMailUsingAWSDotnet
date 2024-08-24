using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SendMailforAWSorSMTP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
//using SendMailforAWSorSMTP;
using System.IO;
using System.Net.Mail;
using CsvHelper;
using System.Globalization;
using SendMailFromAWSorSMTP.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Web;
using System.Text;

namespace SendMailforAWSorSMTP.Controllers
{
    [Authorize(Roles ="Admin, Member")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static IConfiguration _config;

        public HomeController(ILogger<HomeController> logger,IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            ClaimsIdentity identity = null;
            bool isAuthenticated = false;
            if (Email == "Administrator" && Password=="Nbl#2019$")
            {
                identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name,Email),
                    new Claim(ClaimTypes.Role,"Admin")
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                isAuthenticated = true;
            }
            else if(Email=="sales@naapbooks.in" && Password=="ProEx#2013$Nbl")
            {
                identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name,Email),
                    new Claim(ClaimTypes.Role,"Member")
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                isAuthenticated = true;
            }
            if(isAuthenticated)
            {
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(principal));
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public IActionResult Index()
        {
            ViewBag.resMessage = "";
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormCollection form, IFormFile fm_htmlFile, ICollection<IFormFile> fm_Attachments, IFormFile fm_Contacts, string SendSMS)
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
                                newmessage= newmessage.Replace("</p>","</p>\n");
                                if(!string.IsNullOrEmpty(SendSMS))
                                {
                                    string result = "";
                                    String url = "";
                                    string userid = "2000186085";
                                    string passwd = "gkoCNjbta";
                                    WebRequest request = null;
                                    HttpWebResponse response = null;
                                    HtmlDocument node = new HtmlDocument();
                                    node.LoadHtml(newmessage.Replace("<br>","\n").Replace("\n\n","\n"));
                                    newmessage=node.DocumentNode.InnerText;
                                    newmessage = HttpUtility.UrlEncode(newmessage);
                                    url = "https://enterprise.smsgupshup.com/GatewayAPI/rest?msg=" + newmessage + "&v=1.1&userid=" + userid + "&password=" + passwd + "&send_to=91" + item.Mobile + "&msg_type=text&method=sendMessage&mask="+form["fm_Mask"];
                                    request = WebRequest.Create(url);
                                    response = (HttpWebResponse)request.GetResponse();
                                    Stream webstream = response.GetResponseStream();
                                    Encoding ec = System.Text.Encoding.GetEncoding("utf-8");
                                    StreamReader reader = new System.IO.StreamReader(webstream, ec);
                                    result = reader.ReadToEnd();
                                    reader.Close();
                                    webstream.Close();
                                    count++;                

                                }
                                else
                                {
                                
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
                            }
                            catch (Exception ex)
                            {
                                returnMessage = "Error occured sending email: " + ex.Message.ToString();
                            }
                        }
                    }
                }
                returnMessage = "Campaign Sent to " + count + " Contacts";
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
        [Authorize(Roles ="Admin")]
        public IActionResult ReadAllRecords()
        {
            var model = new List<EmailLogs>();
            MySQLContext context = HttpContext.RequestServices.GetService(typeof(MySQLContext)) as MySQLContext;
            model = context.GetEmailLogs();
            return View(model);
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Home");
        }
        [Authorize(Roles ="Admin")]
        public IActionResult GetCampaignDetails(string campaign)
        {
            var model = new List<EmailLogs>();
            MySQLContext context = HttpContext.RequestServices.GetService(typeof(MySQLContext)) as MySQLContext;
            model = context.GetCampaignLogs(campaign);
            return View(model);
        }
    }
}
