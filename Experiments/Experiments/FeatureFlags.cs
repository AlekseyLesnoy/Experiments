using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Experiments
{
    [TestFixture]
    public class FeatureFlags
    {
        [Test]
        public async Task Test1()
        {
            const string feature1 = "feature1";
            Environment.SetEnvironmentVariable($"FeatureManagement:{feature1}", "true");
            IConfiguration config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(config).AddFeatureManagement();
            var provider = serviceCollection.BuildServiceProvider();
            var featureManager = provider.GetRequiredService<IFeatureManager>();

            Assert.True(await featureManager.IsEnabledAsync(feature1));
        }

        [Test]
        public async Task Test2()
        {
            const string feature1 = "feature1";
            Environment.SetEnvironmentVariable($"FeatureManagement:{feature1}", "true");
            IConfiguration config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(config).AddFeatureManagement();
            var provider = serviceCollection.BuildServiceProvider();
            var featureManager = provider.GetRequiredService<IFeatureManager>();

            Assert.True(await featureManager.IsEnabledAsync(feature1));

            Environment.SetEnvironmentVariable($"FeatureManagement:{feature1}", "false");

            Assert.True(await featureManager.IsEnabledAsync(feature1)); //Env.Variables are not reloaded, maybe need TestServer or real one
        }

        [Test]
        public async Task Test3()
        {
            // appsettings.json:
            // {
            //     "FeatureManagement": {
            //         "feature1": true,
            //         "feature2": false
            //     }
            // }
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();
            var testServer = new TestServer(WebHost.CreateDefaultBuilder().ConfigureServices(services =>
                {
                    services.AddSingleton(config).AddFeatureManagement();
                    services.AddMvcCore(o => o.EnableEndpointRouting = false);
                }).Configure(app => app.UseMvc()));

            var gateAllResponse = await testServer.CreateClient().GetAsync("gateAll");
            var gateAnyResponse = await testServer.CreateClient().GetAsync("gateAny");
            Assert.That(gateAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(gateAnyResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            AddOrUpdateAppSetting("FeatureManagement:feature2", true);
            Thread.Sleep(TimeSpan.FromSeconds(10)); //we have to wait for cached appsettings.json to be reloaded

            gateAllResponse = await testServer.CreateClient().GetAsync("gateAll");
            gateAnyResponse = await testServer.CreateClient().GetAsync("gateAny");
            Assert.That(gateAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(gateAnyResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            // rollback for run until failure
            AddOrUpdateAppSetting("FeatureManagement:feature1", true);
            AddOrUpdateAppSetting("FeatureManagement:feature2", false);
        }


        public static void AddOrUpdateAppSetting<T>(string key, T value)
        {
            try
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                var json = File.ReadAllText(filePath);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);

                var sectionPath = key.Split(":")[0];
                if (!string.IsNullOrEmpty(sectionPath))
                {
                    var keyPath = key.Split(":")[1];
                    jsonObj[sectionPath][keyPath] = value;
                }
                else
                {
                    jsonObj[sectionPath] = value;
                }

                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(filePath, output);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }
    }


    [Route("")]
    public class TestController : Controller
    {
        [Route("/gateAll")]
        [HttpGet]
        [FeatureGate(Features.feature1, Features.feature2)]
        public IActionResult GateAll()
        {
            return Ok();
        }

        [Route("/gateAny")]
        [HttpGet]
        [FeatureGate(RequirementType.Any, Features.feature1, Features.feature2)]
        public IActionResult GateAny()
        {
            return Ok();
        }
    }

    public enum Features
    {
        feature1 = 0,
        feature2
    }
}