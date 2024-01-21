using System.Text.Json;
using Bindings;
using TechTalk.SpecFlow;

namespace Engine
{
    public class Bindings
    {
        Log log;
        TestConfigurationContext testConfigurationContext;
        TestContext testContext;
        public Bindings(Log l, TestConfigurationContext t, TestContext tc)
        {
            log = l;
            testConfigurationContext = t;
            testContext = tc;
        }

        [BeforeScenario(Order = int.MinValue)]
        public void Before_GetTestConfigs()
        {
            foreach (string filePath in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory)
                //.Where(f => !Path.GetFileName(fi).Equals("config.main.json", StringComparison.InvariantCultureIgnoreCase))
                .Where(f => Path.GetFileName(f).StartsWith("config."))
                .Where(f => Path.GetFileName(f).EndsWith(".json"))
                .ToList())
            {
                log.Write("Adding config from", filePath);
                string configString = File.ReadAllText(filePath);
                TestConfiguration[] testConfigs = JsonSerializer.Deserialize<TestConfiguration[]>(configString);
                foreach (TestConfiguration testConfig in testConfigs)
                {
                    testConfigurationContext.AddConfig(testConfig);
                    log.Write("Added config", testConfig.ConfigName);
                }
            }
        }

        [BeforeScenario(Order = int.MinValue + 1)]
        public void Before_ApplyConfigsAndCreateDrivers()
        {
            List<string>configsChain = testContext.featureContext.FeatureInfo.Tags.Where(t => t.StartsWith("ConfigFeature")).First().Split(':').ToList();
           
            testContext.featureContext.Set(new WebDriverWrapper(testConfigurationContext.currentConfig, log.output), "WebDriver");
            //testContext.featureContext.Set(new IOSDriverWrapper(log.output, testRunConfiguration), "IOSDriver");
        }

        [BeforeScenario(Order = int.MinValue + 2)]
        public void Before_DownloadsErasing()
        {
            string assemblyDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string downloadsPath =  Path.Combine(assemblyDirectory, testConfigurationContext.currentConfig.TestRunParameters.Web.DownloadFolder);
            DirectoryInfo di = new DirectoryInfo(downloadsPath);
            if (di.Exists)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete(); 
                }
            }
            else
                di.Create();
        }
    }
}