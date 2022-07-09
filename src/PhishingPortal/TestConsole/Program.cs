// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Mail;
using PhishingPortal.Common;

using HtmlAgilityPack;

//var html = File.ReadAllText("d:\\mail.txt");

//var htmlDoc = new HtmlDocument();
//htmlDoc.LoadHtml(html);

//foreach (var childNode in htmlDoc.DocumentNode.Descendants(2))
//{

//    if (childNode.Name != "img")
//        continue;

//    Console.WriteLine(childNode.Name);
//    var src = childNode.Attributes["src"].Value;
//    var alt = childNode.Attributes["alt"]?.Value;

//    var type = src.Split(";")[0];
//    var encoded_value = src.Split(";")[1];
//    var code = encoded_value.Split(",")[0];
//    var base64Content = encoded_value.Split(",")[1];
//    var bytes = Convert.FromBase64String(base64Content);

//    System.IO.File.WriteAllBytes(@"D:\Credent\Git\PhishingPortal\src\PhishingPortal\PhishingPortal.UI.Blazor\wwwroot\img\1.png", bytes);

//    childNode.SetAttributeValue("src", @"img\1.png");


//    Console.WriteLine(src);

//    Console.WriteLine(alt);

//}

//var output = htmlDoc.DocumentNode.WriteTo();

//var result = output.ToMultipartMailBody(@"D:\Credent\Git\PhishingPortal\src\PhishingPortal\PhishingPortal.UI.Blazor\wwwroot\");
//var result2 = output.ToMimePartCollection(@"D:\Credent\Git\PhishingPortal\src\PhishingPortal\PhishingPortal.UI.Blazor\wwwroot\");

//Console.WriteLine(output);

//Console.WriteLine("Done");

var port = 587;
var h = "smtp.office365.com";
var to = "malay.pandey@credentinfotech.com";
var from = "malay.pandey@credentinfotech.com";
var pwd = "mkp@CREDENT098";
var enableSsl = false;
var useDefaults = false;

var arguments = Environment.GetCommandLineArgs();

if (arguments?.Length > 1)
{
    h = arguments[1];
}

if (arguments?.Length > 2)
{
    int.TryParse(arguments[2], out port);
}

if(arguments?.Length > 3)
{
    enableSsl = bool.Parse(arguments[3]);
}

if (arguments?.Length > 4)
{
    useDefaults = bool.Parse(arguments[4]);
}

if (arguments?.Length > 5)
{
    from = arguments[5];
}

if (arguments?.Length > 6)
{
    pwd = arguments[6];
}

if (arguments?.Length > 7)
{
    to = arguments[7];
}


Console.WriteLine($"Host: {h}");
Console.WriteLine($"Port: {port}");

Console.WriteLine($"To: {to}");
Console.WriteLine($"EnableSsl: {enableSsl}");
Console.WriteLine($"UseDefaults: {useDefaults}");

if (!useDefaults)
{
    Console.WriteLine($"From: {from}");
    Console.WriteLine($"Pwd: {pwd}");
}


var fromAddress = new MailAddress(from, "From Name");
var toAddress = new MailAddress(to, "To Name");
string fromPassword = pwd;
const string subject = "TestConsole:Test Subject";
const string body = "Test Body";

var smtp = new SmtpClient
{
    Host = h,
    Port = port,
    //EnableSsl = true,
    //DeliveryMethod = SmtpDeliveryMethod.Network,
    //Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
};

if(enableSsl)
    smtp.EnableSsl = true;

if(useDefaults)
    smtp.UseDefaultCredentials = true;
else
{
    smtp.Credentials = new NetworkCredential(fromAddress.Address, pwd);
}


using (var message = new MailMessage(fromAddress, toAddress){ Subject = subject, Body = body }) 
smtp.Send(message);

Console.ReadKey();



