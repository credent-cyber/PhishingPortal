using Microsoft.AspNetCore.Identity.UI.Services;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;

namespace PhishingPortal.Services.Notification
{
    public interface ICampaignProvider
    {
        Task CheckAndPublish(CancellationToken stopppingToken);

    }
}