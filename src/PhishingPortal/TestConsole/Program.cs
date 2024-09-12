// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Mail;
using PhishingPortal.Common;

using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

var logFactory = LoggerFactory.Create(config => config.AddConsole());

var licProvider = new PhishingPortal.Licensing.LicenseProvider(logFactory.CreateLogger<PhishingPortal.Licensing.LicenseProvider>());

var lic = licProvider.Generate("ABC", new PhishingPortal.Dto.Subscription.SubscriptionInfo
{
    Modules = new List<PhishingPortal.Dto.Subscription.AppModules> { PhishingPortal.Dto.Subscription.AppModules.EmailCampaign },
    AllowedUserCount = 1000,
    ExpiryInUTC = DateTime.UtcNow.AddDays(30),
    SubscriptionType = PhishingPortal.Dto.Subscription.SubscriptionTypes.Limited,
    TenantEmail = "malaykp@gmail.com",
    TenantIdentifier = "12345678"
    
});

Console.WriteLine(lic.Content);

var isValid = licProvider.Validate(lic.Content, lic.PublicKey);
Console.WriteLine(isValid.ToString());

var subs = licProvider.GetSubscriptionInfo(lic.Content, lic.PublicKey);
Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(subs));

////////////////////////////////////////////////////////////////// smtp tests ////////////////////////////////////////
//var port = 587;
//var h = "smtp.office365.com";
//var to = "malay.pandey@credentinfotech.com";
//var from = "malay.pandey@credentinfotech.com";
//var pwd = "mkp@CREDENT098";
//var enableSsl = false;
//var useDefaults = false;

//var arguments = Environment.GetCommandLineArgs();

//if (arguments?.Length > 1)
//{
//    h = arguments[1];
//}

//if (arguments?.Length > 2)
//{
//    int.TryParse(arguments[2], out port);
//}

//if(arguments?.Length > 3)
//{
//    enableSsl = bool.Parse(arguments[3]);
//}

//if (arguments?.Length > 4)
//{
//    useDefaults = bool.Parse(arguments[4]);
//}

//if (arguments?.Length > 5)
//{
//    from = arguments[5];
//}

//if (arguments?.Length > 6)
//{
//    pwd = arguments[6];
//}

//if (arguments?.Length > 7)
//{
//    to = arguments[7];
//}


//Console.WriteLine($"Host: {h}");
//Console.WriteLine($"Port: {port}");

//Console.WriteLine($"To: {to}");
//Console.WriteLine($"EnableSsl: {enableSsl}");
//Console.WriteLine($"UseDefaults: {useDefaults}");

//if (!useDefaults)
//{
//    Console.WriteLine($"From: {from}");
//    Console.WriteLine($"Pwd: {pwd}");
//}


//var fromAddress = new MailAddress(from, "From Name");
//var toAddress = new MailAddress(to, "To Name");
//string fromPassword = pwd;
//const string subject = "TestConsole:Test Subject";
//const string body = "Test Body";

//var smtp = new SmtpClient
//{
//    Host = h,
//    Port = port,
//    //EnableSsl = true,
//    //DeliveryMethod = SmtpDeliveryMethod.Network,
//    //Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
//};

//if(enableSsl)
//    smtp.EnableSsl = true;

//if(useDefaults)
//    smtp.UseDefaultCredentials = true;
//else
//{
//    smtp.Credentials = new NetworkCredential(fromAddress.Address, pwd);
//}


//using (var message = new MailMessage(fromAddress, toAddress){ Subject = subject, Body = body }) 
//smtp.Send(message);

//Console.ReadKey();



