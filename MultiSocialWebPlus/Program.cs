using System;
using System.IO;
using System.Windows.Forms;

namespace MultiSocialWebPlus
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Ensure LocalAppData folder exists for DB and profiles
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MultiSocialWebPlus");
            Directory.CreateDirectory(appData);

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ensure DB file location is set via env var for AppDbContext
            Environment.SetEnvironmentVariable("MSWPLUS_DATA_DIR", appData);

            // Init DB (migrations not used here - ensure created)
            using (var db = new Data.AppDbContext())
            {
                db.Database.EnsureCreated();
            }

            Application.Run(new Forms.MainForm());
        }
    }
}
