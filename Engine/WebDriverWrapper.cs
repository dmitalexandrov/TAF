using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;
using Xunit.Abstractions;

namespace Engine
{
    public class WebDriverWrapper
    {
        private TestConfiguration config;
        private ITestOutputHelper output;
        private IWebDriver webDriver;
        private Log log;

        private string OriginalWindow;

        public WebDriverWrapper(TestConfiguration c, ITestOutputHelper o)
        {
            config = c;
            log = new Log(o, "=drv");
            webDriver = CreateWebDriver();
        }

        private IWebDriver CreateWebDriver()
        {
            IWebDriver specificDriver;

            string baseDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ??
                                    @".\";
            string directory = Path.Combine(baseDirectory, config.TestRunParameters.Web.DownloadFolder);
            
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("incognito");
            chromeOptions.BrowserVersion = "119";
            chromeOptions.AddArguments("--lang=en");
            //chromeOptions.AddArguments("--headless=new");
            chromeOptions.AddUserProfilePreference("download.default_directory", directory);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            chromeOptions.AddUserProfilePreference("browser.download.manager.showWhenStarting", false);
            chromeOptions.AddUserProfilePreference("safebrowsing.enabled", "true");
            var experimentalFlags = new List<string>();
            experimentalFlags.Add("insecure-download-warnings@2"); //chrome://flags/#insecure-download-warnings
            chromeOptions.AddLocalStatePreference("browser.enabled_labs_experiments", experimentalFlags);
            specificDriver = new ChromeDriver(chromeOptions);      

            var eventFiringWebDriver = new EventFiringWebDriver(specificDriver);
            eventFiringWebDriver.Navigating += (sender, e) => Console.WriteLine(e.Url);
            eventFiringWebDriver.ScriptExecuting += (sender, e) => Console.WriteLine(e.Script);

            eventFiringWebDriver.Manage().Timeouts().ImplicitWait =
                TimeSpan.FromMilliseconds(config.TestRunParameters.Web.ImplicitWait);
            eventFiringWebDriver.Manage().Timeouts().PageLoad =
                TimeSpan.FromMilliseconds(config.TestRunParameters.Web.PageLoadTimeout);
            eventFiringWebDriver.Manage().Timeouts().AsynchronousJavaScript =
                TimeSpan.FromMilliseconds(config.TestRunParameters.Web.AsyncJavaScriptTimeout);

            eventFiringWebDriver.Manage().Window.Maximize();
            OriginalWindow = eventFiringWebDriver.CurrentWindowHandle;

            return eventFiringWebDriver;
        }

        public IWebElement FindElement(string xPath, int timeout = 4)
        {
            try
            {
                log.Write("Search by", xPath, DateTime.Now);
                new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeout))
                    .Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(xPath)));
                IWebElement e = webDriver.FindElement(By.XPath(xPath));
                /* TODO: highlighting the element that is currently active/in use
                var jsDriver = (IJavaScriptExecutor)this.WebDriver;
                string highlightJavascript = @"arguments[0].style.cssText = ""border-width: 2px; border-style: solid; border-color: red"";";
                jsDriver.ExecuteScript(highlightJavascript, new object[] { e }); */

                new Actions(webDriver)
                    .MoveToElement(e)
                    .Perform();
                string t = e.Text; //Try to catch StaleElementReferenceException

                return e;
            }
            catch (StaleElementReferenceException)
            {
                try
                {
                    log.Write("Try to fix StaleElementReferenceException", DateTime.Now);
                    IWebElement e = webDriver.FindElement(By.XPath(xPath));
                    return e;
                }
                catch (OpenQA.Selenium.NoSuchElementException)
                {
                    log.Write("Element with xPath does not exist, NoSuchElementException", xPath, DateTime.Now);
                    return null;
                }
            }
            catch (OpenQA.Selenium.NoSuchElementException)
            {
                log.Write("Element with xPath does not exist, NoSuchElementException", xPath, DateTime.Now);
                return null;
            }
            catch (WebDriverTimeoutException)
            {
                log.Write("Element with xPath does not exist, WebDriverTimeoutException", xPath, DateTime.Now);
                return null;
            }
        }
        
        public string JavaScriptExecutor(string script, params object[] args)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
            return (string)js.ExecuteScript(script, args);
        }

        public void Click(IWebElement element)
        {
            try
            {
                element.Click();
            }
            catch (ElementClickInterceptedException)
            {
                JavaScriptExecutor(
                    "arguments[0].scrollIntoView({'block':'center','inline':'center'})", element);
                element.Click();
            }
        }

        public void SetInputOrFileValue(IWebElement element, string value)
        {
            output.WriteLine($"Set value `{value}` to `{element.TagName}`");
            element.SendKeys(Keys.Control + "a" + Keys.Delete); // HACK: Sometimes .Clear() doesn't work.
            element.Clear();
            element.SendKeys(value);
        }

        public void SetCheckboxValue(IWebElement element, bool newCheckBoxValue)
        {
            output.WriteLine($"Set `{newCheckBoxValue}` to `{element.TagName}`");
            if (!newCheckBoxValue.Equals(element.Selected))
            {
                output.WriteLine("Actual and expected values are different, click on element");
                new Actions(webDriver)
                    .Click()
                    .Perform();
            }
            else
                output.WriteLine("Expected and actual values are equal, nothing changes");
        }

        public void SelectFromDropdownByText(IWebElement dropdownElement, string selectValue)
        {
            new Actions(webDriver)
                .Click()
                .Perform();
            SelectElement selectElement = new SelectElement(dropdownElement);

            selectElement.SelectByText(selectValue);
        }

        public void NavigateToUrl(string url)
        {
            try
            {
                webDriver.Navigate().GoToUrl(log.Write("Navigate to URL", url, DateTime.Now));
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot navigate to URL" + ex.Message);
            }
        }
        
        public void ReloadPage()
        {
            webDriver.Navigate().Refresh();
        }

        public void CloseDriver()
        {
            log.Write("Close driver", DateTime.Now);
            webDriver.Quit();
        }        
    }
}