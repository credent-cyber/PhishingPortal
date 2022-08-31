using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using PhishingPortal.Common;

namespace PhishingPortal.Tests
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
                await client.SendEmailAsync("malay.pandey@credentinfotech.com", "test subject", "test content", true, Guid.NewGuid().ToString());

                Assert.True(true);
            }
            catch (Exception)
            {
                Assert.True(false);
                throw;
            }
        }

        [Fact]
        public async void SmtpClientTest()
        {
            try
            {
                var moqLogger = new Mock<ILogger<SmtpEmailClient>>();
                var client = new SmtpEmailClient(moqLogger.Object, GetConfigurationRoot(AppDomain.CurrentDomain.BaseDirectory));
                await client.SendEmailAsync("malay.pandey@credentinfotech.com", "test subject", "test content", true, Guid.NewGuid().ToString());

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