using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;

namespace PhishingPortal.Common.Tests
{
    public class EmailClientTests
    {
        [Fact]
        public async void Office365SmtpClientTest()
        {
            try
            {
                var moqLogger = new Mock<ILogger<Office365SmtpClient>>();
                var client = new Office365SmtpClient(moqLogger.Object, GetConfigurationRoot(AppDomain.CurrentDomain.BaseDirectory));
                await client.SendEmailAsync("malay.pandey@credentinfotech.com", "test subject", "test content", Guid.NewGuid().ToString());

                Assert.True(true);
            }
            catch (Exception)
            {
                Assert.True(false);
                throw;
            }
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