﻿namespace Nacos.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class ConfigTest : TestBase
    {
        [Fact]
        public async Task GetConfig_Should_Succeed()
        {
            var request = new GetConfigRequest
            {
                // Tenant = "tenant"
                DataId = "dataId",
                Group = "DEFAULT_GROUP",
            };

            var res = await _configClient.GetConfigAsync(request).ConfigureAwait(false);
            Assert.NotNull(res);
            Assert.Equal("test", res);
        }

        [Fact]
        public async Task PublishConfig_Should_Succeed()
        {
            var request = new PublishConfigRequest
            {
                DataId = "dataId",
                Group = "DEFAULT_GROUP",

                // Tenant = "tenant",
                Content = "test",
                Type = "text",
                AppName = "appdemo"
            };

            var res = await _configClient.PublishConfigAsync(request).ConfigureAwait(false);
            Assert.True(res);
        }

        [Fact]
        public async Task RemoveConfig_Should_Succeed()
        {
            var request = new RemoveConfigRequest
            {
                // Tenant = "tenant"
                DataId = "dataId",
                Group = "DEFAULT_GROUP",
            };

            var res = await _configClient.RemoveConfigAsync(request).ConfigureAwait(false);
            Assert.True(res);
        }

        [Fact]
        public async Task ListenerConfig_Should_Succeed()
        {
            var request = new AddListenerRequest
            {
                DataId = "dataId",

                // Group = "DEFAULT_GROUP",
                // Tenant = "tenant",
                Callbacks = new List<Action<string>>
                {
                    x => { Console.WriteLine(x); },
                }
            };

            await _configClient.AddListenerAsync(request).ConfigureAwait(false);

            Assert.True(true);

            await Task.Delay(1000).ConfigureAwait(false);

            var rRequest = new RemoveListenerRequest
            {
                DataId = "dataId",
            };

            await _configClient.RemoveListenerAsync(rRequest).ConfigureAwait(false);

            await Task.Delay(50000).ConfigureAwait(false);
        }
    }
}
