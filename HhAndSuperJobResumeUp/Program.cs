using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace HhAndSuperJobResumeUp
{
    public interface IAuth
    {
        bool IsAuth { get; }
        bool Refresh();
    }

    public interface IAuthHelper
    {
        bool Auth(string page);
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

        public bool Auth(string page)
        {
            try
            {

                IWebElement element = _driver.FindElement(By.Id("Email"));
                element.SendKeys(_login);
                element = _driver.FindElement(By.Id("Passwd"));
                element.SendKeys(_password);
                element = _driver.FindElement(By.Id("signIn"));
                element.Click();
               if (_driver.CurrentWindowHandle!=page)
                    return false;
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
    public class Hh
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
            var parentPage = _driver.CurrentWindowHandle;
            element.Click();
            var newPage = _driver.WindowHandles.Last();
            _driver.SwitchTo().Window(newPage);
            if (_helper.Auth(parentPage))
            {
                Thread.Sleep(2000);
                _driver.FindElement(By.LinkText("Мои резюме")).Click();
                _driver.FindElement(By.LinkText("Руководитель отдела разработки")).Click(); 
                
                
            }
        }
    }

    public class SuperJob : IAuth
    {
        private readonly IWebDriver _driver;

        public SuperJob(IWebDriver driver, string login, string password)
        {
            _driver = driver;
            try
            {
                driver.Navigate()
                    .GoToUrl(
                        "https://www.superjob.ru/sn/redirect/google?both=1&state=http%253A%252F%252Fwww.superjob.ru%252F");
                IWebElement element = driver.FindElement(By.Id("Email"));
                element.SendKeys(login);
                element = driver.FindElement(By.Id("Passwd"));
                element.SendKeys(password);
                element = driver.FindElement(By.Id("signIn"));
                element.Click();
                IsAuth = true;
            }
            catch (Exception)
            {
                IsAuth = false;
            }
        }

        public bool IsAuth { get; private set; }

        public bool Refresh()
        {
            Contract.Requires(IsAuth == true);
            Thread.Sleep(2000);
            try
            {
                _driver.FindElement(By.LinkText("Моё резюме")).Click();
                _driver.FindElement(By.LinkText("обновить дату публикации")).Click();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            //var driver = new ChromeDriver();
            //var service = new SuperJob(new ChromeDriver(), "menozgrande", "ghjf");
            //if (service.IsAuth) service.Refresh();
            //driver.Quit();

            var driver = new ChromeDriver();
            var helper = new Google(driver, "menozgrande", "...");
            var service = new Hh(driver, helper);
            service.Refresh();
            driver.Quit();
        }
    }
}