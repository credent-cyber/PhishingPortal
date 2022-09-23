using PhishingPortal.Dto;
using PhishingPortal.UI.Blazor.Client;
using Microsoft.AspNetCore.Components;


namespace PhishingPortal.UI.Blazor
{
    public class Call
    {
        IEnumerable<Campaign> campaigns;
        private TenantClient client;
        private NavigationManager _navigationManager;
        public async Task TestMethod(string lbl, string id)
        {
           // _navigationManager.NavigateTo("/layout");
            campaigns = await client.GetAllCampaignsAsync();
            var a = campaigns.Where(o => o.Name == lbl);

        }
    }

}
