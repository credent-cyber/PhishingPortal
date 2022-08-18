using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using PhishingPortal.Repositories;
using PhishingPortal.Server.Services;
using System.Threading.Tasks;

namespace PhishingPortal.Common.Tests
{
    public class AzActDirClientServiceTests
    {
        [Fact]
        public async void TestGetAdUsers()
        {
            var mocklogger = new Mock<ILogger>();
            Mock<ISettingsRepository> mockSettingRepo = CreateMockSettings();

            var azAdClient = new AzActDirClientService(mocklogger.Object, mockSettingRepo.Object);

            var result = await azAdClient.GetAdUsers();

            Assert.True(result != null);
        }

        
        [Fact]
        public async void TestGetAdUserGroups()
        {
            var mocklogger = new Mock<ILogger>();
            var mockSettingRepo = CreateMockSettings();
            var azAdClient = new AzActDirClientService(mocklogger.Object, mockSettingRepo.Object);

            var result = await azAdClient.GetAllUserGroups();
            
            Assert.True(result != null);

        }

        [Fact]
        public async void TestGetUserByGroupId()
        {
            var mocklogger = new Mock<ILogger>();
            var mockSettingRepo = CreateMockSettings();

            var azAdClient = new AzActDirClientService(mocklogger.Object, mockSettingRepo.Object);
            
            var result = await azAdClient.GetGroupMembers("00bb87ef-3e1f-4505-a7eb-6bb66c6fcf45");

            Assert.True(result != null);

        }

        private static Mock<ISettingsRepository> CreateMockSettings()
        {
            var mockSettingRepo = new Mock<ISettingsRepository>();

            mockSettingRepo.Setup(o => o.GetSetting<string>("az_client_id")).Returns(async () => await Task.FromResult("e1fea56c-ef5c-4e10-9fa6-ab28d077e34c"));
            mockSettingRepo.Setup(o => o.GetSetting<string>("az_client_secret")).Returns(async () => await Task.FromResult("~5A8Q~iYjzn188PLNj3kzdm8QVSPkwzS1owqBb.U"));
            mockSettingRepo.Setup(o => o.GetSetting<string>("az_tenant_id")).Returns(async () => await Task.FromResult("cf92019c-152d-42f6-bbcc-0cf96e6b0108"));
            return mockSettingRepo;
        }

    }
}