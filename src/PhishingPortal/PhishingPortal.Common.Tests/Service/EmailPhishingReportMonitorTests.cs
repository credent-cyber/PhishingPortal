using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PhishingPortal.Services.Notification;
using PhishingPortal.Dto;
using PhishingPortal.DataContext;

namespace PhishingPortal.Tests.Service
{

    public class EmailPhishingReportMonitorTests
    {
        [Fact]
        public void CheckAzureMailBoxTest()
        {
            //var moqLogger = new Mock<ILogger>();
            //var config = GetConfigurationRoot(AppDomain.CurrentDomain.BaseDirectory);
            //var moqDbConnectionMgr = new Mock<ITenantDbConnManager>();
            //var moqTenant = new Mock<Tenant>();
            //var moqDbContex = new Mock<TenantDbContext>();

            //moqDbContex.Setup(o => o.Settings).Returns(o => new Dictionary<string>()
            //{

            //}.);
            //moqDbConnectionMgr.Setup(o => o.GetContext(It.IsAny<string>())).Returns();

        }

        private IConfigurationRoot GetConfigurationRoot(string basePath)
        {
            return new ConfigurationBuilder().SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
