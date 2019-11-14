using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmisharp;
using Microsoft.Speech.Recognition;
using System.IO;
using System.Globalization;
using System.Xml.Linq;
using Newtonsoft.Json;
using Microsoft.Speech.Recognition.SrgsGrammar;
using multimodal;
using Microsoft.Speech.Synthesis;

namespace speechModality
{
    public class SpeechMod
    {
        private SpeechRecognitionEngine sre;
        private GrammarBuilder builder;
        private Grammar gr;
        public event EventHandler<SpeechEventArg> Recognized;
        private SpeechSynthesizer speechSynthesizer;
        protected virtual void onRecognized(SpeechEventArg msg)
        {
            EventHandler<SpeechEventArg> handler = Recognized;
            if (handler != null)
            {
                handler(this, msg);
            }
        }

        private LifeCycleEvents lce;
        private MmiCommunication mmic;
        private MmiCommunication mmic_gui;

        public SpeechMod()
        {

            speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.SetOutputToDefaultAudioDevice();
            speechSynthesizer.SpeakAsync("Olá, sou o assistente do teu navegador! Algo que precises é só dizeres!");
            speechSynthesizer.SpeakAsync("Adeus");

            //init LifeCycleEvents..
            lce = new LifeCycleEvents("ASR", "FUSION", "speech-1", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            //mmic = new MmiCommunication("localhost",9876,"User1", "ASR");  //PORT TO FUSION - uncomment this line to work with fusion later
            mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic.Send(lce.NewContextRequest());

            //Initialize mmi to receive messages from GUI
            mmic_gui = new MmiCommunication("localhost", 8000, "User2", "GUI");
            mmic_gui.Message += MmiC_Message;
            mmic_gui.Start();

            CultureInfo culture = CultureInfo.GetCultureInfo("pt-PT");

            var sampleDoc = new SrgsDocument(Environment.CurrentDirectory + "\\ptG.grxml");
            sampleDoc.Culture = culture;

            /*SrgsRule searchWordsRule = new SrgsRule("SearchWords");
            searchWordsRule.Add(new SrgsOneOf(                                          
                new SrgsItem("footebol")
            ));

            sampleDoc.Rules.Add(searchWordsRule);*/
            /*
            List<SrgsRule> list = sampleDoc.Rules.ToList<SrgsRule>();
            SrgsRule searchWordsRule = sampleDoc.Rules.ToList<SrgsRule>()[list.Count()-1];
            searchWordsRule.Elements.Clear();
            SrgsOneOf oneOf = new SrgsOneOf();
            List<String> myList = new List<String>() { "aveiro", "porto", "lisboa" };
            foreach(string word in myList)
            {
                oneOf.Add(new SrgsItem(word));
            }
            searchWordsRule.Add(oneOf);

             */

            gr = new Grammar(sampleDoc);

            //load pt recognizer
            sre = new SpeechRecognitionEngine(culture);

            sre.LoadGrammar(gr);

            sre.SetInputToDefaultAudioDevice();
            sre.RecognizeAsync(RecognizeMode.Multiple);
            sre.SpeechRecognized += Sre_SpeechRecognized;
            sre.SpeechHypothesized += Sre_SpeechHypothesized;

        }

        private void Sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = false });

        }

        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {


            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true });


            //SEND
            // IMPORTANT TO KEEP THE FORMAT {"recognized":["SHAPE","COLOR"]}
            string json = "{ \"recognized\": [";
            foreach (var resultSemantic in e.Result.Semantics)
            {
                Console.WriteLine(resultSemantic.Key.ToString());
                if (resultSemantic.Key == "SearchObject")
                {
                    foreach (var val in resultSemantic.Value.ToArray())
                    {
                        json += "\"" + val.Value.Value + "\", ";
                    }
                }
                else
                {
                    json += "\"" + resultSemantic.Value.Value + "\", ";
                }

            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";

            var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime + "", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration) + "", e.Result.Confidence, json);
            Console.WriteLine(exNot.ToString());
            mmic.Send(exNot);
        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

        }
    }
    }
