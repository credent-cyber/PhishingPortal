using System.Runtime.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PhishingPortal.OutlookAddin.Model;

namespace PhishingPortal.OutlookAddin.Pages
{
    public partial class Index
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        public IJSObjectReference JSModule { get; set; } = default!;

        public MailRead? MailReadData { get; set; }
        public string ItemId { get; set; }
        public string HtmlBody { get; set; }

        public string DisplayName { get; set; }

        public bool EmailForward = false;

        public bool Responce = false;
        public string TrademarkMessage1 { get; set; } = "Copyright © " + @DateTime.Now.Year + " PhishSims.";
        public string TrademarkMessage2 { get; set; } = "All rights reserved.";

        /// <summary>
        /// NOTE: This can also go in the @code block in Index.razor
        /// </summary>
        /// <param name = "firstRender" ></ param >
        /// < returns ></ returns >
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            //NOTE: This fires after Index.razor.OnInitialized()
            Console.WriteLine($"Index.razor.cs (OnAfterRenderAsync): firstRender: {firstRender}");

            if (firstRender)
            {
                //NOTE: JSRuntime.InvokeAsync invokes OutlookBlazorWasmApp.Client.lib.module.js(afterStarted), then Index.razor.js(Office.onReady) but only when hosted in full browser instance outside of the Outlook Task pane. When in Outlook, the order after this event completes is:
                //OutlookBlazorWasmApp.Client.lib.module.js(Office.onReady) - after beforeStart has already fired() and triggered Index.razor.OnInitialized()
                //Index.razor.js(Office.onReady) 

                Console.WriteLine($"firstRender: Importing Index.razor.js...");

                JSModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/Index.razor.js");

                Console.WriteLine($"firstRender: Index.razor.js imported");
            }

            if (MailReadData == null)
            {
                Console.WriteLine($"========================== Index.razor.cs (OnAfterRenderAsync): Calling GetEmailData()... ==========================");

                MailReadData = await GetEmailData();
                ItemId = await GetItemId();
                HtmlBody = await GetHtmlBody();
                DisplayName = await GetDisplayName();


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

        public async void Forward()
        {
            //var toRecipients = new List<Recipient>()
            //         {
            //             new Recipient
            //             {
            //                 EmailAddress = new EmailAddress
            //                 {
            //                     Name = "PhishSims",
            //                     Address = "phishing-mail@credentinfotech.com"
            //                 }
            //             }
            //         };

            //var comment = "spam";

            //GraphServiceClient graphClient = new GraphServiceClient(authProvider);

            //await graphClient.Me.Messages[ItemId]
            //.Forward(toRecipients, null, comment)
            //.Request()
            //.PostAsync();

            //Outlook._Application _Application = new Outlook.Application();
            //Outlook.MailItem mail = (Outlook.MailItem)_Application.CreateItem(Outlook.OlItemType.olMailItem);
            //mail.To = "<Email address>";
            //mail.Subject = "Test e-mail from Addin";
            //mail.Body = "This is a test email";
            //mail.Importance = Outlook.OlImportance.olImportanceNormal;
            //((Outlook._MailItem)mail).Send();



            //var a = await client.OutlookReport(ItemId);
            var a = true;
            EmailForward = true;
            Responce = a == true ? true : false;
            StateHasChanged();

        }
        public void onClose()
        {
            EmailForward = false;
        }

        public async Task<string> GetHtmlBody(string itemId)
        {
            return await JSRuntime.InvokeAsync<string>(
                "getEmailHtmlBody",
                itemId,
                (string html) => { /* do something with html */ });
        }

    }
}
