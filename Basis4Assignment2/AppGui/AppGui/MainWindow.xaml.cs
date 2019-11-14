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
using Newtonsoft.Json.Linq;
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
        private MmiCommunication mmi_speechMod;
        private LifeCycleEvents lce_speechMod;
        private IWebDriver driver;
        private InputSimulator sim;
        private List<string> tabs;

        private Int32 minWordLen;
        private Int32 minWordCount; 
        public MainWindow()
        {

            InitializeComponent();

            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;

            minWordCount = 3;
            minWordLen = 4;

            driver = new ChromeDriver(".");
            driver.Manage().Window.Maximize();
            System.Threading.Thread.Sleep(3000);
            sim = new InputSimulator();
            tabs = driver.WindowHandles.ToList();

            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            //Initialize MMI to send messages to speech mod

            lce_speechMod = new LifeCycleEvents("ASR", "FUSION", "speech-2", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            mmi_speechMod = new MmiCommunication("localhost", 8000, "User2", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmi_speechMod.Send(lce_speechMod.NewContextRequest());


        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);
            List<String> words = ((JArray)json["recognized"]).ToObject<List<String>>();

            var rec0 = words[0].ToString();
            var rec1 = "";
            int cont = 0;
            foreach (string word in words)
            {
                cont++;
            }
            if (cont == 1)
            {
                rec1 = "";
            }
            else { rec1 = words[1].ToString(); }

            string searchText = "";


            foreach (string word in words)
            {
                if (word != "search")
                {
                    searchText += word + " ";
                }
            }

            List<String> numbers = new List<String>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            switch (rec0)
            {
                case "search":

                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_E);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    Console.WriteLine(searchText);
                    sim.Keyboard.Sleep(200);
                    sim.Keyboard.TextEntry(searchText);
                    System.Threading.Thread.Sleep(200);
                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

                    SendTtsMessage("Estes são os teus resultados");

                    break;
                case "save":
                    switch (rec1)
                    {
                        default:
                            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                            sim.Keyboard.KeyPress(VirtualKeyCode.VK_D);
                            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                            System.Threading.Thread.Sleep(100);
                            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                            SendTtsMessage("Adicionado com sucesso aos teus favoritos");
                            break;
                    }
                    break;


                case "minimize":
                    switch (rec1)
                    {
                        default:
                            driver.Manage().Window.Minimize();
                            break;
                    }
                    break;
                case "maximize":
                    switch (rec1)
                    {
                        default:
                            driver.Manage().Window.Maximize();
                            break;
                    }
                    break;
                case "zoom_in":
                    switch (rec1)
                    {
                        default:
                            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                            sim.Keyboard.KeyPress(VirtualKeyCode.OEM_PLUS);
                            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                            break;
                    }
                    break;

                case "zoom_out":
                    switch (rec1)
                    {
                        default:
                            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                            sim.Keyboard.KeyPress(VirtualKeyCode.OEM_MINUS);
                            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                            break;
                    }
                    break;
                case "previous":
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                    tabs = driver.WindowHandles.ToList();
                    int index1 = tabs.IndexOf(driver.CurrentWindowHandle);
                    if (index1 == 0)
                    {
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                    }
                    else
                    {
                        driver.SwitchTo().Window(tabs[index1 - 1]);
                    }
                    break;
                case "next":
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    //Console.WriteLine(tabs.ToString());
                    tabs = driver.WindowHandles.ToList();
                    int index2 = tabs.IndexOf(driver.CurrentWindowHandle);
                    if (index2 == tabs.Count() - 1)
                    {
                        driver.SwitchTo().Window(driver.WindowHandles.First());
                    }
                    else
                    {
                        driver.SwitchTo().Window(tabs[index2 + 1]);
                    }


                    break;
                case "close":

                    tabs = driver.WindowHandles.ToList();
                    if (tabs.Count() > 1)
                    {
                        driver.Close();
                        driver.SwitchTo().Window(driver.WindowHandles.First());
                    }

                    break;
                case "open":
                    switch (rec1)
                    {
                        case "tab":
                            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                            sim.Keyboard.KeyPress(VirtualKeyCode.VK_T);
                            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            break;

                        case "historic":
                            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                            sim.Keyboard.KeyPress(VirtualKeyCode.VK_H);
                            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            SendTtsMessage("Aqui está o teu histórico de navegação");
                            break;
                        case "favourites":
                            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                            sim.Keyboard.KeyPress(VirtualKeyCode.VK_O);
                            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                            SendTtsMessage("Aqui estão os teus favoritos");
                            break;

                        default:
                            if (driver.Url.Contains("https://www.google.com/search?"))
                            {
                                if (numbers.Contains(rec1))
                                {

                                    int linkNumber = Int32.Parse(rec1);

                                    ICollection<IWebElement> webElements = driver.FindElements(By.TagName("a"));
                                    List<IWebElement> final = new List<IWebElement>();
                                    foreach (IWebElement we in webElements)
                                    {
                                        ICollection<IWebElement> results = we.FindElements(By.TagName("h3"));
                                        if (results.Count != 0)
                                        {
                                            Console.WriteLine(results.First().Text.ToString() + "\t" + we.GetAttribute("href"));
                                            final.Add(we);
                                        }
                                    }


                                    if (linkNumber < final.Count())
                                    {
                                        IWebElement element = final[linkNumber - 1];
                                        element.Click();
                                        string text = "";

                                        ICollection<IWebElement> textElements = driver.FindElements(By.TagName("p"));
                                        textElements.Concat(driver.FindElements(By.TagName("h1")));
                                        textElements.Concat(driver.FindElements(By.TagName("h2")));
                                        textElements.Concat(driver.FindElements(By.TagName("h3")));
                                        foreach (IWebElement elem in textElements) { text += elem.Text.ToString() + " "; }
                                        string[] palavras = text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                                        List<string> pal = palavras.ToList();

                                        var counts = new Dictionary<string, int>();

                                        foreach (string value in pal)
                                        {
                                            if (counts.ContainsKey(value))
                                                counts[value] = counts[value] + 1;
                                            else
                                                counts.Add(value, 1);
                                        }
                                        List<String> keywords = new List<string>();
                                        foreach (string key in counts.Keys)
                                        {
                                            if (counts[key] >= minWordCount && key.Length > minWordLen)
                                            {
                                                keywords.Add(key);
                                            }
                                        }
                                        foreach (string s in keywords)
                                        {
                                            Console.WriteLine(s + " ");
                                        }

                                        //Update speech mod with new words to add to the grammar
                                        SendNewWords(keywords);
                                        break;
                                    }
                                    else
                                    {
                                        int links = final.Count() - 1;
                                        SendTtsMessage("Apenas detetei " + links.ToString() + " links que possas abrir");
                                    }

                                    break;
                                }
                                break;
                            }
                            break;

                    }
                    break;

            }


        }

        private void SendTtsMessage(string messageToSpeak)
        {
            string json = "{ \"action\" : \"speak\" , \"text_to_speak\" : \"" + messageToSpeak + "\"}";
            var exNot = lce_speechMod.ExtensionNotification("", "", 0, json);
            mmi_speechMod.Send(exNot);
        }

        private void SendNewWords(List<string> words)
        {
            string json = "{ \"action\": \"newWords\", \"newWords\" : [";
            foreach (string word in words)
            {

                json += "\"" + word + "\", ";

            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";
            var exNot = lce_speechMod.ExtensionNotification("", "", 0, json);
            mmi_speechMod.Send(exNot);
        }


    }
}