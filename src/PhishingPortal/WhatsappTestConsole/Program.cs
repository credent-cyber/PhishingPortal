
using Newtonsoft.Json.Linq;
using System.Web;
using System.Xml;

//Console.WriteLine("DealSms Testing");

//var httpClient = new HttpClient();
//httpClient.BaseAddress = new Uri("http://bhashsms.com/");

//var response = await httpClient.GetAsync($"/api/sendmsg.php?user=success&pass=sms@2023&sender=BHAINF&phone=8826171494&text=api  test2 BHASHSMS&priority=ndnd&stype=normal");
//response.EnsureSuccessStatusCode();

//var content = await response.Content.ReadAsStringAsync();

//Console.WriteLine("Sms Response");
//Console.WriteLine($"StatusCode : {response.StatusCode}");
//Console.WriteLine(content);
// WHATSAPP
//var httpClient = new HttpClient();
//httpClient.BaseAddress = new Uri("https://dealsms.in");
//var uri = $"/api/send.php?number=917987163540&type=text&message=%3Cp%3ETest%20Message%3C%2Fp%3E%3Ca%20href%3D%22https%3A%2F%2Fgoogle.com%22%3EClick%20Here%3C%2Fa%3E&instance_id=641864A8AA901&access_token=6511cc6939dc30cff0874eac82c8121b";
//var response = await httpClient.GetAsync(uri);
//response.EnsureSuccessStatusCode();
//var content = await response.Content.ReadAsStringAsync();

//Console.WriteLine("Whatsapp Response");
//Console.WriteLine($"StatusCode : {response.StatusCode}");
//Console.WriteLine(content);

/////////
//////
///
Console.WriteLine("viralmarketingtools Whatsapp Test");
var httpClient = new HttpClient();
// Request to create an instance
//string createInstanceUrl = "https://wa.viralmarketingtools.in/api/create_instance?access_token=6576c7c6da78d";
//var createInstanceResponse = await httpClient.GetAsync(createInstanceUrl);
//createInstanceResponse.EnsureSuccessStatusCode();

//var createInstanceContent = await createInstanceResponse.Content.ReadAsStringAsync();
//var createInstanceJson = JObject.Parse(createInstanceContent);
//string instanceId = createInstanceJson["instance_id"].ToString();
/////////
////
///


//send message
//string message = "hello%0Aworld!"; // Use HttpUtility.UrlEncode to encode the message
string message = "This+is *HTML* HTML text. %0A Second+line."; // Use HttpUtility.UrlEncode to encode the message

string to = "917987163540";
to = to.Length == 10 ? "91" + to : to;
string uri = $"https://wa.viralmarketingtools.in/api/send?number={to}&type=text&message={message}&instance_id=66644B39333FA&access_token=6576c7c6da78d";
var response = await httpClient.GetAsync(uri);
response.EnsureSuccessStatusCode();
var content = await response.Content.ReadAsStringAsync();

Console.WriteLine("Whatsapp Response");
Console.WriteLine($"StatusCode : {response.StatusCode}");
Console.WriteLine(content);

