// See https://aka.ms/new-console-template for more information
using System.Net.Http;
using System.Net.Http.Json;

Console.WriteLine("Whatsapp Test Console");


var client = new HttpClient();
client.BaseAddress = new Uri("http://142.132.202.49/");

var key = "68bf86afef99416898e5fbe7ecf5364d";
var mobile = "8826171494";
var msg = "Hi";

var options = Environment.GetCommandLineArgs();

if(options.Length >= 2)
{
    key = options[1];
}

if (options.Length >= 3)
{
    mobile = options[2];
}

if(options.Length >= 4)
{
    msg = options[3];
}

var uri = $"/wapp/api/send?apikey={key}&mobile={mobile}&msg={msg}";

Console.WriteLine(uri);

var response  = await client.PostAsJsonAsync<object>(uri, new { });

response.EnsureSuccessStatusCode();

if (response.IsSuccessStatusCode)
    Console.WriteLine("Success :)");
else
{
    Console.WriteLine("Failed :(");
}



