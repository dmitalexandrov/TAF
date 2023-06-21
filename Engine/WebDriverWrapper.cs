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
        private RunConfiguration config;
        private ITestOutputHelper output;
        private IWebDriver webDriver;

        private string OriginalWindow;

        public WebDriverWrapper(RunConfiguration c, ITestOutputHelper o)
        {
            config = c;
            output = o;
            webDriver = CreateWebDriver();
        }

        private IWebDriver CreateWebDriver()
        {
            IWebDriver specificDriver;

            string baseDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ??
                                    @".\";
            string directory = Path.Combine(baseDirectory, config.DownloadFolder);
            
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("incognito");
            chromeOptions.AddUserProfilePreference("download.default_directory", directory);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            chromeOptions.AddUserProfilePreference("browser.download.manager.showWhenStarting", false);
            chromeOptions.AddUserProfilePreference("safebrowsing.enabled", "true");
            specificDriver = new ChromeDriver(chromeOptions);      

            var eventFiringWebDriver = new EventFiringWebDriver(specificDriver);
            eventFiringWebDriver.Navigating += (sender, e) => Console.WriteLine(e.Url);
            eventFiringWebDriver.ScriptExecuting += (sender, e) => Console.WriteLine(e.Script);

            eventFiringWebDriver.Manage().Timeouts().ImplicitWait =
                TimeSpan.FromMilliseconds(config.ImplicitWait);
            eventFiringWebDriver.Manage().Timeouts().PageLoad =
                TimeSpan.FromMilliseconds(config.PageLoadTimeout);
            eventFiringWebDriver.Manage().Timeouts().AsynchronousJavaScript =
                TimeSpan.FromMilliseconds(config.AsyncJavaScriptTimeout);

            eventFiringWebDriver.Manage().Window.Maximize();
            OriginalWindow = eventFiringWebDriver.CurrentWindowHandle;

            return eventFiringWebDriver;
        }

        public IWebElement FindElement(string XPath, int timeout = 4)
        {
            try
            {
                output.WriteLine($"=search `{XPath}`, timestamp {DateTime.Now.ToString("HH:mm:ss:ff")}");
                new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeout))
                    .Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(XPath)));
                IWebElement e = webDriver.FindElement(By.XPath(XPath));
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
                    output.WriteLine($"StaleElementReferenceException, timestamp {DateTime.Now.ToString("HH:mm:ss:ff")}");
                    IWebElement e = webDriver.FindElement(By.XPath(XPath));
                    return e;
                }
                catch (OpenQA.Selenium.NoSuchElementException)
                {
                    output.WriteLine($"Element with xPath `{XPath}` does not Exist.");
                    return null;
                }
            }
            catch (OpenQA.Selenium.NoSuchElementException)
            {
                output.WriteLine($"Element with xPath `{XPath}` does not Exist.");
                return null;
            }
            catch (WebDriverTimeoutException)
            {
                output.WriteLine($"Element with xPath `{XPath}` did not waited.");
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
                output.WriteLine($"Navigate to URL `{url}`");
                webDriver.Navigate().GoToUrl(url);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot navigate to URL" + ex.Message);
            }
        }

        public void AcceptAlert()
        {
            output.WriteLine($"Accept alert.");
            webDriver.SwitchTo().Alert().Accept();
            webDriver.SwitchTo().DefaultContent();
        }

        public void DismissAlert()
        {
            output.WriteLine($"Dismissing alert.");
            webDriver.SwitchTo().Alert().Dismiss();
        }

        public string GetAlertText()
        {
            output.WriteLine($"GetAlert text.");
            string text = webDriver.SwitchTo().Alert().Text;
            //this.WebDriver.SwitchTo().DefaultContent();
            return text;
        }

        public void ReloadPage()
        {
            webDriver.Navigate().Refresh();
        }

        public void CloseDriver()
        {
            output.WriteLine($"Close driver.");
            webDriver.Quit();
        }        
    }
}