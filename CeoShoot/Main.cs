using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CeoShootMain
{
    internal static class Program
    {
        private const string AppName = "CEOSHOOT";

        public static readonly string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CeoShoot");
        public static readonly string ConfigPath = Path.Combine(ConfigFolder, "config.ini");

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!File.Exists(ConfigPath))
            {
                if (!Directory.Exists(ConfigFolder))
                {
                    Directory.CreateDirectory(ConfigFolder);
                }

                using (SetupForm setup = new SetupForm())
                {
                    if (setup.ShowDialog() == DialogResult.OK)
                    {
                        SetAutostart(setup.IsAutostartEnabled);

                        File.WriteAllText(ConfigPath, "FirstLaunch=False\nInstalledAt=" + DateTime.Now.ToString("yyyy-MM-dd"));
                    }
                    else
                    {
                        return;
                    }
                }
            }

            Application.Run(new BackgroundControllerForm());
        }

        public static void SetAutostart(bool enable)
        {
            try
            {
                const string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(runKey, true))
                {
                    if (key == null) return;

                    if (enable)
                        key.SetValue(AppName, $"\"{Application.ExecutablePath}\"");
                    else
                        key.DeleteValue(AppName, false);
                }
            }
            catch
            {
            }
        }
    }
}