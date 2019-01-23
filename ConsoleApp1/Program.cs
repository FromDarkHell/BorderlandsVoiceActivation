using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Globalization;
using System.Windows.Forms;

namespace ConsoleApp1
{
    class Program
    {
        #region Variables

        #region TTS
        // Recognition Engine, this does the proper recognition.
        public static SpeechRecognitionEngine recogEngine = new SpeechRecognitionEngine(CultureInfo.CurrentCulture);
        // The grammar dictionary, this is what tells recogEngine how english works.
        public static DictationGrammar grammarDict = new DictationGrammar();
        // If we're using TTS.
        public static bool ttsEnabled = true;
        // The under bound (exclusive) of how low the confidence *could* be.
        public static double confidenceLimit = 0.1;
        #endregion

        #region Input
        static Dictionary<string, string> keyMap = new Dictionary<string, string>();
        #endregion

        #endregion


        static void Main(string[] args)
        {
            #region Input Setup
            Choices c = new Choices();
            // Now we ask the users for their key bindings.
            InputFunctions.setupKeys(ref keyMap, ref c);
            Console.Clear();
            #endregion

            #region Speech Setup

            // Set the recognition engine to use the microphone.
            recogEngine.SetInputToDefaultAudioDevice();

            // Load the general grammar dictionary for funsies.
            recogEngine.LoadGrammar(grammarDict);
            // Load our previously generated choices for more precision.
            recogEngine.LoadGrammar(new Grammar(new GrammarBuilder(c)));

            // Allow us to not have to recreate a new recognition engine for every line recognized.
            recogEngine.RecognizeAsync(RecognizeMode.Multiple);

            // Attach our hypothesizing / recognition events to the recognition engine.

            recogEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);
            recogEngine.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(SpeechHypothesizing);
            #endregion
            loop();
        }


        #region Speech Parsing
        private static void SpeechHypothesizing(object sender, SpeechHypothesizedEventArgs e)
        {
            // If our confidence is < the confidence limit or scroll lock is disabled, or TTS is disabled, return.
            if (e.Result.Confidence < confidenceLimit || !Control.IsKeyLocked(Keys.Scroll) || !ttsEnabled) return;

            // Clear the current line if it was already being written.
            clearLine();
            // Write out confidence / result.
            Console.Write(e.Result.Text + " - Confidence: " + e.Result.Confidence.ToString().Substring(0, 4));
        }

        private static void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // If our confidence is < the confidence limit or scroll lock is disabled, or TTS is disabled, return.
            if (e.Result.Confidence < confidenceLimit || !Control.IsKeyLocked(Keys.Scroll) || !ttsEnabled) return;

            // Clear our line in case it was hypothesizing.
            clearLine();
            // Create our new RecognitionResult to remove repeating ourselves.
            RecognitionResult result = e.Result;
            // Turn the output of our recognition into an output.
            string output = result.Text;

            // Write out our confidence / result.
            Console.WriteLine(output + " - Confidence: " + result.Confidence.ToString().Substring(0, 4));

            // If our Dictionary<string,string> of our key -> input to send, doesn't contains our key, return.
            if (!keyMap.ContainsKey(output.ToLower())) return;

            // Gather our key / input we should send.
            keyMap.TryGetValue(output.ToLower(), out string keyToSend);
            // This happens when the user said, "Stop". It'll stop all input.
            if (keyToSend.Equals("StopInput"))
            {
                InputFunctions.StopInput();
            }
            // If our input into the recognition engine wasn't a mouse click, then we need to get the key to send / send it for x seconds.
            else if (!keyToSend.Contains("MouseClick"))
            {
                // The key we're going to press.
                string properKey = keyToSend.Split('-')[0];
                // This happens when we're just pressing a key, not holding it.
                if (!keyToSend.Contains("-"))
                {
                    InputFunctions.sendKeystroke(keyToSend);
                    return;
                }
                // Hold the key for the specific amount of time in seconds.
                InputFunctions.holdKey(properKey, int.Parse(keyToSend.Split('-')[1]));
            }
            else
            {
                // Press the mouse (left or right), single click will hold for 0s, regular holding will hold for 5s.
                InputFunctions.holdMouse(keyToSend.Contains("Left"), keyToSend.Contains("Single") ? 0 : 5);
            }
        }

        #endregion

        #region Console Stuff

        // This clears the entire line.
        static void clearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        static void loop()
        {
            bool quitting = false;
            while (!quitting)
            {
                Console.WriteLine("Type \"E\" to exit.");
                Console.WriteLine("Type \"T\" to disable TTS recognition.");
                Console.WriteLine("Toggling off \"Scroll Lock\" will also disable TTS recognition.");

                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.T) ttsEnabled = !ttsEnabled;
                else if (keyInfo.Key == ConsoleKey.E) quitting = true;

                Console.Clear();
            }
        }

        #endregion

    }
}
