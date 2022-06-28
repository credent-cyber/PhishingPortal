// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Mail;
using PhishingPortal.Common;

using HtmlAgilityPack;

var html = File.ReadAllText("d:\\mail.txt");

var htmlDoc = new HtmlDocument();
htmlDoc.LoadHtml(html);

foreach (var childNode in htmlDoc.DocumentNode.Descendants(2))
{

    if (childNode.Name != "img")
        continue;

    Console.WriteLine(childNode.Name);
    var src = childNode.Attributes["src"].Value;
    var alt = childNode.Attributes["alt"]?.Value;

    var type = src.Split(";")[0];
    var encoded_value = src.Split(";")[1];
    var code = encoded_value.Split(",")[0];
    var base64Content = encoded_value.Split(",")[1];
    var bytes = Convert.FromBase64String(base64Content);

    System.IO.File.WriteAllBytes(@"D:\Credent\Git\PhishingPortal\src\PhishingPortal\PhishingPortal.UI.Blazor\wwwroot\img\1.png", bytes);

    childNode.SetAttributeValue("src", @"img\1.png");


    Console.WriteLine(src);

    Console.WriteLine(alt);

}

var output = htmlDoc.DocumentNode.WriteTo();

var result = output.ToMultipartMailBody(@"D:\Credent\Git\PhishingPortal\src\PhishingPortal\PhishingPortal.UI.Blazor\wwwroot\");
var result2 = output.ToMimePartCollection(@"D:\Credent\Git\PhishingPortal\src\PhishingPortal\PhishingPortal.UI.Blazor\wwwroot\");

Console.WriteLine(output);

//Console.WriteLine("Done");

var port = 587;
var h = "smtp.gmail.com";
var to = "malaykp.devices@gmail.com";
var from = "malaykp.devices@gmail.com";

var arguments = Environment.GetCommandLineArgs();

if (arguments?.Length > 1)
{
    h = arguments[1];
}

if (arguments?.Length > 2)
{
    int.TryParse(arguments[2], out port);
}

var fromAddress = new MailAddress("malay.pandey@credentinfotech.com", "From Name");
var toAddress = new MailAddress("malay.pandey@credentinfotech.com", "To Name");
const string fromPassword = "mkp@CREDENT098";
const string subject = "Test Subject";
const string body = "Test Body";

var smtp = new SmtpClient
{
    Host = "smtp.office365.com",
    Port = 587,
    EnableSsl = true,
    DeliveryMethod = SmtpDeliveryMethod.Network,
    UseDefaultCredentials = false,
    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
};
using (var message = new MailMessage(fromAddress, toAddress)
{
    Subject = subject,
    //Body = contentBody,

})
{
    message.AlternateViews.Add(result);
    smtp.Send(message);
}

Console.ReadKey();



