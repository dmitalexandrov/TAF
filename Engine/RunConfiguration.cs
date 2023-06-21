namespace Engine
{
    public class RunConfiguration
    {
        public string Browser { get; set; }

        public int ImplicitWait { get; set; }

        public int PageLoadTimeout { get; set; }

        public int AsyncJavaScriptTimeout { get; set; }

        public string DownloadFolder { get; set; }
/* 
        public string AutomationName { get; set; }

        public string DeviceName { get; set; }

        public string PlatformVersion { get; set; }

        public string UdId { get; set; }

        public bool AutoAcceptAlertsFlag { get; set; }

        public int LaunchTimeout { get; set; }

        public string Language { get; set; }

        public string Url { get; set; }

        public double MatchTreshold { get; set; }

        public bool DebugMatchImage { get; set; }

        public double ColorHueTreshold { get; set; } */
    }
}