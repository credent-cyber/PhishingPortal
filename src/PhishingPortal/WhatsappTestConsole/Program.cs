Console.WriteLine("DealSms Testing");

var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("http://bhashsms.com/");

var response = await httpClient.GetAsync($"/api/sendmsg.php?user=success&pass=sms@2023&sender=BHAINF&phone=8826171494&text=api  test2 BHASHSMS&priority=ndnd&stype=normal");
response.EnsureSuccessStatusCode();

var content = await response.Content.ReadAsStringAsync();

Console.WriteLine("Sms Response");
Console.WriteLine($"StatusCode : {response.StatusCode}");
Console.WriteLine(content);

httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("https://dealsms.in");

response = await httpClient.GetAsync($"/api/send.php?number=918826171494&type=media&message=test%20message&media_url=https://drjsharanlab.com/report/1560260_9430152159.pdf&filename=rahul.pdf&instance_id=63E5F247D7D9D&access_token=783c698eef4306a2d370c9de3862744c");
response.EnsureSuccessStatusCode();
content = await response.Content.ReadAsStringAsync();

Console.WriteLine("Whatsapp Response");
Console.WriteLine($"StatusCode : {response.StatusCode}");
Console.WriteLine(content);