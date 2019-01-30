using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Speech.Recognition;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class InputFunctions
    {

        #region PImports
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        #endregion

        #region Enums
        // An enum for the possible mouse inputs/outputs we can send.
        [Flags]
        public enum MouseEventFlags
        {
            // Left
            LeftDown = 2,
            LeftUp = 4,
            // Right
            RightDown = 8,
            RightUp = 16,
            // Currently unused Middle Mouse Button
            MiddleDown = 32,
            MiddleUp = 64
        }
        #endregion

        #region Methods

        #region Mouse

        // This'll click the mouse singularly.
        public static void clickMouse(bool leftClick)
        {
            mouse_event(
                ((int)(leftClick ? MouseEventFlags.LeftDown : MouseEventFlags.RightDown) + (int)(leftClick ? MouseEventFlags.LeftUp : MouseEventFlags.RightDown)),
                0, 0, 0, 0);
        }

        // This'll hold the mouse for x seconds.
        public static void holdMouse(bool leftClick, int duration)
        {
            Task t = new Task(() =>
            {
                mouse_event(((int)(leftClick ? MouseEventFlags.LeftDown : MouseEventFlags.RightDown)), 0, 0, 0, 0);
                System.Threading.Thread.Sleep(duration * 1000);

                mouse_event(((int)(leftClick ? MouseEventFlags.LeftUp : MouseEventFlags.RightUp)), 0, 0, 0, 0);
            });
            t.Start();
        }
        #endregion

        #region Keyboard
        // Send just one single [set] of keys.
        public static void sendKeystroke(string keys)
        {
            SendKeys.SendWait(keys);
            SendKeys.Flush();
        }

        // Hold a key for x seconds.
        public static void holdKey(string keys, int duration)
        {
            byte key = Encoding.ASCII.GetBytes(keys).FirstOrDefault();
            Task t = new Task(() =>
            {
                // Send Key Down
                keybd_event(key, 0, 0x0001, 0);

                System.Threading.Thread.Sleep(duration * 1000);

                // Release the key
                keybd_event(key, 0, 0x0002, 0);
            });
            t.Start();
        }

        // This one stops all input from any keys (not all but shush)
        public static void StopInput()
        {
            Task t = new Task(() =>
            {
                // An array of all of our keys that we can press.
                string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "[", "]", "\\", "a", "s", "d", "f", "g", "h", "j", "k", "l", "; ", "'", "z", "x", "c", "v", "b", "n", "m", ",", ".", "/", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Z", "X", "C", "V", "B", "N", "M" };

                // Iterate through all of our keys in the array.
                foreach (string key in keys)
                {
                    // Get the byte value for our key.
                    byte k = Encoding.ASCII.GetBytes(key).FirstOrDefault();
                    // "Release" our key.
                    keybd_event(k, 0, 0x0002, 0);
                }
                // Release the lmb if clicked down.
                mouse_event((int)MouseEventFlags.LeftUp, 0, 0, 0, 0);
                // Release the rmb if clicked down.
                mouse_event((int)MouseEventFlags.RightUp, 0, 0, 0, 0);
            });
            t.Start();
        }
        #endregion

        public static void setupKeys(ref Dictionary<string, string> dict, ref Choices c)
        {
            // All of this code prompts the user for their controls, and then fills dict / c with the command / input associated.
            #region Movement
            Console.WriteLine("What is your move forward key?");
            ConsoleKeyInfo key = Console.ReadKey();
            dict.Add("up", key.KeyChar.ToString().ToUpper() + "-5");
            c.Add("up");

            // A helper which'll run for 15s instead of 5.
            dict.Add("run", key.KeyChar.ToString().ToUpper() + "-15");
            c.Add("run");

            // This just moves for one step.
            dict.Add("walk", key.KeyChar.ToString().ToUpper());
            c.Add("walk");
            Console.Clear();

            Console.WriteLine("What is your move backward key?");
            key = Console.ReadKey();
            dict.Add("down", key.KeyChar.ToString().ToUpper() + "-5");
            c.Add("down");
            Console.Clear();

            Console.WriteLine("What is your move left key?");
            key = Console.ReadKey();
            dict.Add("left", key.KeyChar.ToString().ToUpper() + "- 5");
            c.Add("left");
            Console.Clear();

            Console.WriteLine("What is your move right key?");
            key = Console.ReadKey();
            dict.Add("right", key.KeyChar.ToString().ToUpper() + "-5");
            c.Add("right");
            Console.Clear();

            Console.WriteLine("What is your crouch key?");
            key = Console.ReadKey();
            dict.Add("crouch", key.KeyChar.ToString().ToUpper());
            c.Add("crouch");
            Console.Clear();

            dict.Add("jump", " ");
            c.Add("jump");


            #endregion

            #region Utility

            Console.WriteLine("What is your \"Use\" key?");
            key = Console.ReadKey();
            dict.Add("use", key.KeyChar.ToString().ToUpper());
            c.Add("use");

            dict.Add("revive", key.KeyChar.ToString().ToUpper() + "-15");
            c.Add("revive");
            Console.Clear();

            dict.Add("talk", key.KeyChar.ToString().ToUpper());
            c.Add("talk");

            Console.WriteLine("What is your \"Inventory\" key?");
            key = Console.ReadKey();
            if (key.Key == ConsoleKey.Tab)
                dict.Add("inventory", "{TAB}");
            else
                dict.Add("inventory", key.KeyChar.ToString().ToUpper());

            c.Add("inventory");
            Console.Clear();

            dict.Add("pause", "{ESC}");
            c.Add("pause");
            c.Add("pause");

            Console.WriteLine("What is your \"Reload\" key?");
            key = Console.ReadKey();
            dict.Add("reload", key.KeyChar.ToString().ToUpper());
            c.Add("reload");

            Console.Clear();
            Console.WriteLine("What is your \"Honk\" key?");
            key = Console.ReadKey();
            dict.Add("honk", key.KeyChar.ToString().ToUpper());
            c.Add("honk");

            Console.Clear();
            dict.Add("stop", "StopInput");
            c.Add("stop");
            #endregion

            #region Combat
            dict.Add("shoot", "LeftMouseClick");
            c.Add("shoot");
            dict.Add("fire", "LeftMouseClickSingle");
            c.Add("fire");
            dict.Add("aim", "RightMouseClick");
            c.Add("aim");

            Console.WriteLine("What is your \"Melee\" key?");
            key = Console.ReadKey();
            dict.Add("punch", key.KeyChar.ToString().ToUpper());
            c.Add("punch");
            Console.Clear();

            Console.WriteLine("What is your \"Action Skill\" key?");
            key = Console.ReadKey();
            dict.Add("action", key.KeyChar.ToString().ToUpper());
            c.Add("action");
            Console.Clear();

            Console.WriteLine("What is your \"Grenade\" key?");
            key = Console.ReadKey();
            dict.Add("grenade", key.KeyChar.ToString().ToUpper());
            c.Add("grenade");
            Console.Clear();

            #endregion
        }

        #endregion

    }
}
