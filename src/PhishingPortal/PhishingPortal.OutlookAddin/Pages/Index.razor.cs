using System;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PhishingPortal.OutlookAddin.Model;
using PhishingPortal.OutlookAddin.Client;

namespace PhishingPortal.OutlookAddin.Pages
{
    public partial class Index
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        public IJSObjectReference JSModule { get; set; } = default!;

        [Inject]
        protected IConfiguration Configuration { get; set; }

        public MailRead? MailReadData { get; set; }
        public string ItemId { get; set; }
        public string HtmlBody { get; set; }
        public string WebLink { get; set; }
        public string DisplayName { get; set; }

        public bool EmailForward = false;

        public bool Responce = false;
        public string TrademarkMessage1 { get; set; } = "Copyright © " + @DateTime.Now.Year + " PhishSims.";
        public string TrademarkMessage2 { get; set; } = "All rights reserved.";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            Console.WriteLine($"Index.razor.cs (OnAfterRenderAsync): firstRender: {firstRender}");

            if (firstRender)
            {

                Console.WriteLine($"firstRender: Importing Index.razor.js...");

                JSModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/Index.razor.js");

                Console.WriteLine($"firstRender: Index.razor.js imported");
            }

            if (MailReadData == null)
            {
                Console.WriteLine($"========================== Index.razor.cs (OnAfterRenderAsync): Calling GetEmailData()... ==========================");

                MailReadData = await GetEmailData();
                ItemId = await GetItemId();
                DisplayName = await GetDisplayName();
                HtmlBody = await GetHtmlBody();
                if (HtmlBody != "")
                {
                    var Links = await ExtractLinks(HtmlBody);
                    foreach (var Link in Links)
                    {
                        if (Link.Contains("phishsims.com") || Link.Contains("localhost:7018"))
                        {
                            WebLink = Link;
                            break;
                        }
                    }
                }



                if (string.IsNullOrEmpty(MailReadData?.AttachmentBase64Data) == false)
                {
                    MailReadData.DecodeBase64();
                }

                StateHasChanged();

                Console.WriteLine($"========================== Index.razor.cs (OnAfterRenderAsync): Returning from GetEmailData()... ==========================");
            }


            Console.WriteLine($"Index.razor.cs (OnAfterRenderAsync): Done ...");
        }

        private async Task<MailRead?> GetEmailData()
        {
            MailRead? mailreaditem = await JSModule.InvokeAsync<MailRead>("getEmailData");

            Console.WriteLine("Subject C#: ");
            Console.WriteLine(mailreaditem?.Subject);
            Console.WriteLine("ItemId => " + mailreaditem?.ItemId);


            return mailreaditem;
        }
        private async Task<string> GetItemId()
        {
            var mailreaditem = await JSModule.InvokeAsync<string>("getItemId");
            return mailreaditem;
        }

        private async Task<string> GetHtmlBody()
        {
            var html = await JSModule.InvokeAsync<string>("getEmailHtmlBody");
            return html;
        }
        private async Task<string> GetDisplayName()
        {
            var user = await JSModule.InvokeAsync<string>("getDisplayName");
            return user;
        }
        private async Task<string[]> ExtractLinks(string html)
        {
            var links = await JSModule.InvokeAsync<string[]>("extractLinksFromHtml", html);
            return links;
        }


        public async void Forward()
        {
            // var input = "https://phishsims.com/cmpgn/PhishSim-T-20220908134615/7794bb787c99639457ff50136e40f9b7";
            var input = "https://localhost:7018/cmpgn/T-20220619003439/78d49cb73871d59b32119b8b9db47e3d";

            //  string parts = input.Split("https://phishsims.com/cmpgn/")[1];
            string parts = input.Split("https://localhost:7018/cmpgn/")[1];
            string TenantId = parts.Split('/')[0];
            string Key = parts.Split('/')[1];
            //////////
            var BaseUrl = "https://phishsims.com/";
            var BaseUrl1 = "https://localhost:7018/";
            //var BaseUrl = Configuration.GetValue<string>("ApiBaseUrl");
            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl1);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new GenericApiRequest<string> { Param = Key };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"api/tenant/campaign-spam-report?t={TenantId}", content);
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<string>>(responseJson);

            if (result.IsSuccess)
            {
                Responce = true;
            }
            else
            {
                Responce = true;
            }

            ///////////

            //var a = true;
            //Responce = a == true ? true : false;

            EmailForward = true;
            StateHasChanged();

        }
        public void onClose()
        {
            EmailForward = false;
        }


    }
}


