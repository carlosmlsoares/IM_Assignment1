using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmisharp;
using Microsoft.Speech.Recognition;
using System.IO;
using System.Globalization;
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

        public SpeechMod()
        {
                        
            speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.SetOutputToDefaultAudioDevice();
            speechSynthesizer.SpeakAsync("Olá, sou o assistente do teu navegador! Algo que precises é só dizeres!");
            speechSynthesizer.SpeakAsync("Adeus");


            //init LifeCycleEvents..
            lce = new LifeCycleEvents("ASR", "FUSION","speech-1", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            //mmic = new MmiCommunication("localhost",9876,"User1", "ASR");  //PORT TO FUSION - uncomment this line to work with fusion later
            mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)

            mmic.Send(lce.NewContextRequest());

            


            var sampleDoc = new SrgsDocument(Environment.CurrentDirectory + "\\ptG.grxml");
            sampleDoc.Culture = CultureInfo.GetCultureInfo("pt-PT");

            /*SrgsRule searchWordsRule = new SrgsRule("SearchWords");
            searchWordsRule.Add(new SrgsOneOf(                                          
                new SrgsItem("footebol")
            ));

            sampleDoc.Rules.Add(searchWordsRule);*/
            gr = new Grammar(sampleDoc);

            //load pt recognizer
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("pt-PT");
            sre = new SpeechRecognitionEngine(culture);

            /*
            builder = new GrammarBuilder();
            builder.Culture = culture;
            builder.AppendRuleReference(Environment.CurrentDirectory + "\\ptG.grxml");
            //gr = new Grammar(Environment.CurrentDirectory + "\\ptG.grxml", "rootRule");
            gr = new Grammar(builder);
            */
            sre.LoadGrammar(gr);
            
            sre. SetInputToDefaultAudioDevice();
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
                if(resultSemantic.Key == "SearchObject")
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

            var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime+"", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration)+"",e.Result.Confidence, json);
            Console.WriteLine(exNot.ToString());
            mmic.Send(exNot);
        }
    }
}
