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



using (var message = new MailMessage("offers1@fbook.com", "malay.pandey@credentinfotech.com") { Subject = "Mailtest", 
    Body = $"<p>Hurray!!!!!!</p><p><br></p><p>Buy 2 And Get 1 FREE!!</p><p><a href=\"/T-20220619003439/0b3afb19adf41d75c33fd4405cf3529e\" target=\"_blank\">Click here</a> to redeem!</p><p><br></p><p><img src=\"img/email/af65dd13-d99a-4e99-b17c-d0f23b254a1c.jpeg\" style=\"width: 508px;\" data-filename=\"Screenshot 2022-07-10 171452.png\"><br></p><div style='display:none'>####0b3afb19adf41d75c33fd4405cf3529e####</div>" 

    })
{
    message.IsBodyHtml = true;
    message.Headers.Add("internetMessageId", Guid.NewGuid().ToString());
    message.HeadersEncoding = System.Text.Encoding.UTF8;
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



