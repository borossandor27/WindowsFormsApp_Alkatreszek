using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using WindowsFormsApp_Alkatreszek;

namespace WindowsFormsApp_Alkatreszek
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var dbManager = new DatabaseManager("adatbazis.db");
            if (dbManager.CheckDatabaseExists())
            {
                Console.WriteLine("Adatbázis létezik és érvényes.");
            }
            else
            {
                Console.WriteLine("Adatbázis nem létezik, inicializálás...");
                dbManager.InitializeDatabase();
            }
            dbManager.insertKategoria("teszt");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
