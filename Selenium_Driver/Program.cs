using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Support.UI;
using System;
using System.Reflection;
using System.Reflection.Metadata;

namespace Selenium_Driver
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // Set Tiếng việt console
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            LogWriter log = new LogWriter();

            string accFile = "Acc.txt";
            List<AccFile> listAcc = ReadAccounts(accFile);

            string proxyFile = "Proxy.txt";
            List<ProxyFile> listProxy = ReadProxyies(proxyFile);

            for (int i = 0; i < listProxy.Count; i++)
            {
                int index = i;

                Thread thread = new Thread(() =>
                {
                    string proxyHost = listProxy[index].host;
                    string proxyPort = listProxy[index].port;
                    string proxyUsername = "passdayne";
                    string proxyPassword = "passdayne";

                    Proxy proxy = setProxy(proxyHost, proxyPort, proxyUsername, proxyPassword);
                    
                    ChromeOptions options = new ChromeOptions();
                    options.Proxy = proxy;
                    options.AddArgument("--window-size=428,923");
                    options.AddArgument($"--window-position={index * 50},0");
                    //options.AddArgument("--headless"); // ẩn chrome

                    ChromeDriver chrome = new ChromeDriver(options);

                    // Điều hướng đến trang Twitter
                    chrome.Url = "https://twitter.com/";
                    chrome.Navigate();
                    log.logWl($"luồng {index} mở https://twitter.com/", ConsoleColor.Green);

                    // Chờ cho phần tử có class "css-1qaijid" xuất hiện trước khi thực hiện click
                    WebDriverWait wait = new WebDriverWait(chrome, TimeSpan.FromSeconds(30));

                    WaitAndClickElement(chrome, By.ClassName("css-1qaijid"), 15);
                    log.logWl($"luồng {index} click login", ConsoleColor.Green);

                    WaitAndSendElement(chrome, By.ClassName("r-30o5oe"), 0, $"{listAcc[index].userName}");
                    log.logWl($"luồng {index} nhập user", ConsoleColor.Green);

                    WaitAndClickElement(chrome, By.ClassName("css-1rynq56"), 6);
                    log.logWl($"luồng {index} click next", ConsoleColor.Green);

                    WaitAndSendElement(chrome, By.ClassName("r-30o5oe"), 1, $"{listAcc[index].passWorld}");
                    log.logWl($"luồng {index} nhập password", ConsoleColor.Green);

                    WaitAndClickElement(chrome, By.ClassName("css-1rynq56"), 8);
                    log.logWl($"luồng {index} đăng nhập", ConsoleColor.Green);

                    string key = authenMyGen($"{listAcc[index].authenticator}");

                    WaitAndSendElement(chrome, By.ClassName("r-30o5oe"), 0, key);
                    log.logWl($"luồng {index} nhập key", ConsoleColor.Green);

                    WaitAndClickElement(chrome, By.ClassName("css-1rynq56"), 9);
                    log.logWl($"luồng {index} xác nhận key", ConsoleColor.Green);

                    // tab2
                    chrome.ExecuteScript("window.open();");
                    log.logWl($"luồng {index} mở tab mới", ConsoleColor.Red);
                    chrome.SwitchTo().Window(chrome.WindowHandles[1]);
                    Thread.Sleep(1000);
                    chrome.Url = "https://www.tiktok.com/";
                    chrome.Navigate();
                    log.logWl($"luồng {index} vào https://www.tiktok.com/", ConsoleColor.Red);
                    wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete")); // chờ web tải xong

                    WaitAndClickElement(chrome, By.ClassName("css-176z6he-DivShowMore"), 0);
                    log.logWl($"luồng {index} CLICK MŨI TÊN", ConsoleColor.Red);

                    //WaitAndClickElement(chrome, By.ClassName("css-7u35li-DivBoxContainer"), 4);

                    //// Chờ cho đến khi cửa sổ mới được mở
                    //wait.Until(driver => driver.WindowHandles.Count > 1);

                    //// cửa sổ gốc
                    //string originalWindowHandle = chrome.CurrentWindowHandle;
                    //Console.WriteLine($"Luồng 1 handle gốc {originalWindowHandle}");

                    //// Chuyển sang cửa sổ mới
                    //string newWindowHandle = chrome.WindowHandles.Last();
                    //chrome.SwitchTo().Window(newWindowHandle);
                    //Console.WriteLine($"Luồng 1 handle Mới {newWindowHandle}");
                    //Thread.Sleep(3000);

                    //wait.Until(elements => elements.FindElements(By.ClassName("submit")).Count >= 1);
                    //IReadOnlyList<IWebElement> elements9 = chrome.FindElements(By.ClassName("submit"));
                    //elements9[0].Click();

                    //chrome.SwitchTo().Window(originalWindowHandle);
                    //Console.WriteLine("Tới phân đoạn captcha");

                    //string pageSource = chrome.PageSource;
                });
                thread.Start();
            }

            //Đóng trình duyệt
            //chrome.Quit();
        }

        public static Proxy setProxy(string proxyHost, string proxyPort, string proxyUsername, string proxyPassword)
        {
            // Khởi tạo Proxy
            Proxy proxy = new Proxy();
            proxy.Kind = ProxyKind.Manual;
            proxy.IsAutoDetect = false;
            proxy.HttpProxy = $"{proxyHost}:{proxyPort}";
            proxy.SslProxy = $"{proxyHost}:{proxyPort}";

            // Nếu có thông tin xác thực proxy
            if (!string.IsNullOrEmpty(proxyUsername) && !string.IsNullOrEmpty(proxyPassword))
            {
                proxy.HttpProxy = proxy.SslProxy = $"{proxyUsername}:{proxyPassword}@{proxyHost}:{proxyPort}";
            }

            return proxy;
        }

        public static void WaitAndClickElement(ChromeDriver chrome, By locator, int index)
        {
            WebDriverWait wait = new WebDriverWait(chrome, TimeSpan.FromSeconds(30));
            try
            {
                wait.Until(elements => elements.FindElements(locator).Count >= index + 1);
                IReadOnlyList<IWebElement> elements = chrome.FindElements(locator);
                elements[index].Click();
            }
            catch (WebDriverTimeoutException)
            {
                LogWriter log = new LogWriter();
                // Xử lý khi hết thời gian chờ
                // Tải lại trang web
                chrome.Navigate().Refresh();
                // Gọi lại hàm WaitAndClickElement để thử lại
                WaitAndClickElement(chrome, locator, index);
                log.logWl("Đang tải lại trang web: ", ConsoleColor.Blue);
            }
        }

        public static void WaitAndSendElement(ChromeDriver chrome, By locator, int index, string Data2Send)
        {
            WebDriverWait wait = new WebDriverWait(chrome, TimeSpan.FromSeconds(30));
            wait.Until(elements => elements.FindElements(locator).Count >= index + 1);
            IReadOnlyList<IWebElement> elements = chrome.FindElements(locator);
            elements[index].SendKeys(Data2Send);
        }

        public static string authenMyGen(string key)
        {
            byte[] bytes = Base32Encoding.ToBytes(key);

            Totp totp = new Totp(bytes);

            var result = totp.ComputeTotp();
            var remainingTime = totp.RemainingSeconds();

            if (remainingTime <= 5)
            {
                while (remainingTime <= 5)
                {
                    Console.WriteLine("Key sap het han xin cho lay key moi: " + remainingTime);
                    Thread.Sleep(1000);
                    remainingTime = totp.RemainingSeconds();
                }
            }
            Console.WriteLine($"{result}, {remainingTime}");

            return result;
        }

        public static List<AccFile> ReadAccounts(string filePath)
        {
            List<AccFile> accounts = new List<AccFile>();

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 3)
                    {
                        AccFile acc = new AccFile
                        {
                            userName = parts[0],
                            passWorld = parts[1],
                            authenticator = parts[2]
                        };
                        accounts.Add(acc);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid line format: {line}");
                    }
                }
            }
            else
            {
                Console.WriteLine("File not found.");
            }

            return accounts;
        }

        public static List<ProxyFile> ReadProxyies(string filePath)
        {
            List<ProxyFile> proxyies = new List<ProxyFile>();

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        ProxyFile proxy = new ProxyFile
                        {
                            host = parts[0],
                            port = parts[1],
                        };
                        proxyies.Add(proxy);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid line format: {line}");
                    }
                }
            }
            else
            {
                Console.WriteLine("File not found.");
            }

            return proxyies;
        }
    }
}
