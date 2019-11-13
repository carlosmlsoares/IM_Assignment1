using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using WindowsInput;
using WindowsInput.Native;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;
        private IWebDriver driver;
        private InputSimulator sim;
        public MainWindow()
        {

            //InitializeComponent();


            driver = new ChromeDriver(".");
            System.Threading.Thread.Sleep(3000);
            sim = new InputSimulator();

            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            switch ((string)json.recognized[0].ToString())
            {

                case "comando":
                    break;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                switch ((string)json.recognized[1].ToString())
                {

                    case "1": //bookmarking
                        sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_D);
                        sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                        System.Threading.Thread.Sleep(100);
                        sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                        break;


                    case "2": //maximize
                        driver.Manage().Window.Maximize();
                        break;
                    case "3": //minimize
                        driver.Manage().Window.Minimize();
                        break;
                    case "4": //new tab
                        sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_T);
                        sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                        driver.SwitchTo().Window(driver.Url);

                        break;
                    case "5": //next tab
                        sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                        driver.SwitchTo().Window(driver.Url);

                        break;
                    case "6": //prev tab
                        sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        driver.SwitchTo().Window(driver.Url);
                        break;
                    case "7": //open history
                        sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_H);
                        sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                        driver.SwitchTo().Window(driver.Url);
                        break;
                    case "8": //close tab
                        sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_W);
                        sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                        driver.SwitchTo().Window(driver.Url);
                        break;
                    case "9": //zoom in
                        sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_PLUS);
                        sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                        break;
                    case "10": //zoom out
                        sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_MINUS);
                        sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                        break;
                    case "11": //open google
                        //driver.Url = "www.google.com";
                        break;
                    case "12": //open favorites
                        sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_O);
                        sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);

                        break;

                    case "13":
                        ICollection<IWebElement> webElements = driver.FindElements(By.TagName("a"));
                        foreach (IWebElement we in webElements)
                        {
                            ICollection<IWebElement> results = we.FindElements(By.TagName("h3"));
                            if (results.Count != 0)
                            {
                                Console.WriteLine(results.First().Text.ToString() + "\t" + we.GetAttribute("href"));
                            }

                        }


                        //results.First().Click();w

                        break;

                }
            });



        }
    }
}