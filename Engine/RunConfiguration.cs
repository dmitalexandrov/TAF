namespace Bindings
{
    public class TestRunParameters
    {
        public Web Web { get; set; }
        public IOS iOS { get; set; }
    }

    public class Web
    {
        public string Browser { get; set; }
        public int ImplicitWait { get; set; }
        public int PageLoadTimeout { get; set; }
        public int AsyncJavaScriptTimeout { get; set; }
        public string DownloadFolder { get; set; }
    }
    
    public class IOS
    {
        public string AutomationName { get; set; }
        public int LaunchTimeout { get; set; }
        public string Language { get; set; }
        public double MatchTreshold { get; set; }
        public bool DebugMatchImage { get; set; }
        public int ColorHueTreshold { get; set; }
        public List<Device> Devices { get; set; }
    }
    
    public class Device
    {
        public string Name { get; set; }
        public string DeviceName { get; set; }
        public string PlatformVersion { get; set; }
        public string UdId { get; set; }
        public bool AutoAcceptAlertsFlag { get; set; }
        public string Url { get; set; }
    }

    public class Config
    {
        public List<TestSitesURL> TestSitesURL { get; set; }
        public List<Database> DataBases { get; set; }
        public APIServices APIServices { get; set; }
        public List<IOsMobileApp> iOsMobileApps { get; set; }
    }

    public class TestSitesURL
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Database
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }

    public class APIServices
    {
        public List<APIHeader> APIHeaders { get; set; }
        public List<EndPoint> EndPoints { get; set; }
    }

    public class APIHeader
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class EndPoint
    {
        public string ServerName { get; set; }
        public List<Endpoint> EndpointUrls { get; set; }
    }

    public class Endpoint
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class IOsMobileApp
    {
        public string Name { get; set; }
        public string AppPath { get; set; }
        public string AppBundle { get; set; }
        public string AllowButtonString { get; set; }
    }

    public class TestConfiguration
    {
        public string ConfigName { get; set; }
        public TestRunParameters TestRunParameters { get; set; }
        public Config Config { get; set; }
    }
}