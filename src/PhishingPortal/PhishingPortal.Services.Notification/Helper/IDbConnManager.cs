using Microsoft.EntityFrameworkCore;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;

namespace PhishingPortal.Services.Notification.Helper
{
    public interface IDbConnManager : IDisposable
    {
       public DemoRequestor GetContext();
       public bool SetContext(DemoRequestor demo);

       public DbContextOptionsBuilder<PhishingPortalDbContext> SetupDbContextBuilder(DemoRequestor requestor, string connString);
    }
}