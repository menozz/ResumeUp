using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace HhAndSuperJobResumeUp
{
    public interface IHrService
    {
        void Refresh();
    }

    public interface IAuthHelper
    {
        bool Auth();
    }

    public class Google : IAuthHelper
    {
        private readonly IWebDriver _driver;
        private readonly string _login;
        private readonly string _password;

        public Google(IWebDriver driver, string login, string password)
        {
            _driver = driver;
            _login = login;
            _password = password;
        }

        public bool Auth()
        {
            try
            {
                IWebElement element = _driver.FindElement(By.Id("Email"));
                element.SendKeys(_login);
                element = _driver.FindElement(By.Id("Passwd"));
                element.SendKeys(_password);
                element = _driver.FindElement(By.Id("signIn"));
                element.Click();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }

    public class Hh : IHrService
    {
        private readonly IWebDriver _driver;
        private readonly IAuthHelper _helper;

        public Hh(IWebDriver driver, IAuthHelper helper)
        {
            _driver = driver;
            _helper = helper;
        }


        public void Refresh()
        {
            _driver.Navigate().GoToUrl("http://www.hh.ru");
            IWebElement element = _driver.FindElement(By.LinkText("Вход в личный кабинет"));
            element.Click();
            element = _driver.FindElement(By.ClassName("button-flat_gplus"));
            string parentPage = _driver.CurrentWindowHandle;
            element.Click();
            string newPage = _driver.WindowHandles.Last();
            _driver.SwitchTo().Window(newPage);
            if (_helper.Auth())
            {
                Thread.Sleep(2000);
                _driver.SwitchTo().Window(parentPage);
                element = _driver.FindElement(By.LinkText("Мои резюме"));
                element.Click();
                element = _driver.FindElement(By.LinkText("Руководитель отдела разработки"));
                element.Click();
                element = _driver.FindElement(By.ClassName("HH-Resume-Touch-Button"));
                element.Click();
            }
        }
    }

    public class SuperJob : IHrService
    {
        private readonly IWebDriver _driver;
        private readonly IAuthHelper _helper;

        public SuperJob(IWebDriver driver, IAuthHelper helper)
        {
            _driver = driver;
            _helper = helper;
        }

        public void Refresh()
        {
            _driver.Navigate()
                .GoToUrl(
                    "https://www.superjob.ru/sn/redirect/google?both=1&state=http%253A%252F%252Fwww.superjob.ru%252F");

            if (_helper.Auth())
            {
                Thread.Sleep(2000);
                _driver.FindElement(By.LinkText("Моё резюме")).Click();
                _driver.FindElement(By.LinkText("обновить дату публикации")).Click();
            }
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var handle1 = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(delegate
            {
                var driver = new ChromeDriver();

                try
                {
                    var helper = new Google(driver, "menozgrande", "..");
                    IHrService service = new SuperJob(driver, helper);
                    service.Refresh();
                }
                catch { }
                finally
                {
                    driver.Quit();
                    handle1.Set();
                }
            }, null);

            var handle2 = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(delegate
            {
                var driverHr = new ChromeDriver();
                try
                {
                    var helperGo = new Google(driverHr, "menozgrande", "..");
                    IHrService hrService = new Hh(driverHr, helperGo);
                    hrService.Refresh();
                }
                catch { }
                finally
                {
                    driverHr.Quit();
                    handle2.Set();
                }
            }, null);

            handle1.WaitOne();
            handle2.WaitOne();
        }
    }
}