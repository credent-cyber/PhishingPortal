using Microsoft.Office.Interop.Outlook;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PhishingPortal.OutlookAddInVSTO.ApiResponce;
using Office = Microsoft.Office.Core;

namespace PhishingPortal.OutlookAddInVSTO
{
    [ComVisible(true)]
    public class Ribbon1 : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI ribbon;
        private Explorer currentExplorer;

        public string WebLink = string.Empty;
        public bool EmailForward = false;
        public bool Responce = false;
        public bool NoSpam = false;

        public Ribbon1()
        {
        }

        public void OnButtonClick(Office.IRibbonControl control)
        {
            // Get the selected item in the Explorer
            MailItem selectedMail = GetSelectedMailItem();
            WebLink = string.Empty;

            if (selectedMail != null)
            {
                // Access email details
                string subject = selectedMail.Subject;
                string senderName = selectedMail.SenderName;
                string mailBody = selectedMail.Body;
                

                #region Test
                var Links = ExtractLinksFromHtml(mailBody);
                foreach (var Link in Links)
                {
                    if (Link.Contains("phishsims.com") || Link.Contains("localhost:7018"))
                    {
                        WebLink = Link;
                        break;
                    }
                }
                #endregion
                var result = Forward();
                // Show a popup message with email details
                string message = "You have successfully reported this mail as phishing!";
                MessageBox.Show(message, "Message");
            }
            else
            {
                MessageBox.Show("No email selected.", "Message");
            }
        }
        #region Ribbon Callbacks
        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;

            // Hook into the ExplorerSelectionChange event
            var outlookApp = Globals.ThisAddIn.Application;
            currentExplorer = outlookApp.ActiveExplorer();
            currentExplorer.SelectionChange += Explorer_SelectionChange;
        }

        private void Explorer_SelectionChange()
        {
            // Update the ribbon UI when the selection changes in the Explorer
            ribbon.Invalidate();
        }

        private MailItem GetSelectedMailItem()
        {
            // Get the selected item in the Explorer
            Selection selection = currentExplorer.Selection;

            if (selection != null && selection.Count > 0)
            {
                if (selection[1] is MailItem)
                {
                    // Return the selected MailItem
                    return (MailItem)selection[1];
                }
            }

            return null;
        }
        #endregion

        private List<string> ExtractLinksFromHtml(string html)
        {
            const string pattern = "https?://\\S+";
            List<string> links = new List<string>();

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                links.Add(match.ToString().TrimEnd('>'));
            }

            Console.WriteLine("Links: " + string.Join(", ", links));
            return links;
        }

        public async Task<bool> Forward()
        {
            NoSpam = false;
            Responce = false;
            EmailForward = false;

            var BaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
            //var BaseUrl = "https://localhost:7018";

            string input = WebLink;
            //string input = "https://localhost:7018/cmpgn/T-20220619003439/daafd66f6327d2569151231ba79be8d9";
            if (input == null || input.Contains("/training/details"))
            {
                EmailForward = true;
                return true;
            }

            string[] parts = input.Split(new string[] { "https://phishsims.com/cmpgn/" }, StringSplitOptions.None);
            //string[] parts = input.Split(new string[] { "https://localhost:7018/cmpgn/" }, StringSplitOptions.None);

            if (parts.Length > 1)
            {
                string remainingPart = parts[1];
                string TenantId = remainingPart.Split('/')[0];
                string Key = remainingPart.Split('/')[1];

                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(BaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var request = new GenericApiRequest<string> { Param = Key };
                    var json = JsonSerializer.Serialize(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    EmailForward = true;

                    var response = await client.PostAsync($"api/tenant/campaign-spam-report?t={TenantId}", content);
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<string>>(responseJson);

                    Responce = responseJson.Contains("true");
                    Log.Information($"");
                    EmailForward = true;
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.ToString());
                }

            }
            return Responce;
        }

        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("PhishingPortal.OutlookAddInVSTO.Ribbon1.xml");
        }

        #endregion

       

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion

    }
}
