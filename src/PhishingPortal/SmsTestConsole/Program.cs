
using System.Net;
using System.Text;
using System.Web;

string user = "demo08";

//Your authentication key

string password = "6008b6a7edXX";

//Multiple mobiles numbers separated by comma

string mobiles = "+917987163540";

//Sender ID,While using route4 sender id should be 6 characters long.

string senderid = "RELABL";

//Your message to send, Add URL encoding here.
string msg = $"RENEWALa REMINDER Products testProd Due Date :{DateTime.Now}  Amount : 500RS Helpline : https://phishsims.com/ RS";
string sms = HttpUtility.UrlEncode(msg);

string tempid = "1707163211312635261";
string entityid = "1401529690000019209";

//Prepare you post parameters

StringBuilder sbPostData = new StringBuilder();

sbPostData.AppendFormat("user={0}", user);

sbPostData.AppendFormat("&password={0}", password);

sbPostData.AppendFormat("&mobiles={0}", mobiles);

sbPostData.AppendFormat("&sms={0}", sms);

sbPostData.AppendFormat("&senderid={0}", senderid);

sbPostData.AppendFormat("&accusage={0}", "1");
sbPostData.AppendFormat("&tempid={0}", tempid);
sbPostData.AppendFormat("&entityid={0}", entityid);
sbPostData.AppendFormat("&shorturl={0}", "1");



try

{

    //Call Send SMS API

    string sendSMSUrl = "http://dndsms.reliableindya.info/sendsms.jsp?";


    HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(sendSMSUrl);

    //Prepare and Add URL Encoded data

    UTF8Encoding encoding = new UTF8Encoding();

    byte[] data = encoding.GetBytes(sbPostData.ToString());

    //Specify post method

    httpWReq.Method = "POST";

    httpWReq.ContentType = "application/x-www-form-urlencoded";

    httpWReq.ContentLength = data.Length;

    using (Stream stream = httpWReq.GetRequestStream())

    {

        stream.Write(data, 0, data.Length);

    }

    //Get the response

    HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();

    StreamReader reader = new StreamReader(response.GetResponseStream());

    string responseString = reader.ReadToEnd();



    //Close the response

    reader.Close();

    response.Close();

}

catch (SystemException ex)

{

    Console.WriteLine(ex.Message.ToString());

}