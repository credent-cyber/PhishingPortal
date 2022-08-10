// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Mail;

var smtp = new SmtpClient
{
    Host = "WIN-SERIR442ICV",
    Port = 25,
    //EnableSsl = true,
    //DeliveryMethod = SmtpDeliveryMethod.Network,
    //Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
};



using (var message = new MailMessage("offers@fbook.com", "malay.pandey@credentinfotech.com") { Subject = "Mailtest", Body = "Mail Test Body" })
{
    try
    {
        smtp.Send(message);
        Console.WriteLine("Mail sent!");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

Console.WriteLine("Press any key to continue...");
Console.ReadKey();



