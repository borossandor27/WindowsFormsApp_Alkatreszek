using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;

namespace WindowsFormsApp_Alkatreszek
{
    public class DatabaseManager
    {
        private static string _databasePath;
        private static SQLiteConnection _connection;
        private static SQLiteCommand _command;

        public DatabaseManager(string databasePath)
        {
            _databasePath = databasePath;
        }

        public bool CheckDatabaseExists()
        {
            // Egyszerű fájl létezés ellenőrzés
            if (!File.Exists(_databasePath))
                return false;

            // Részletes ellenőrzés: érvényes SQLite fájl-e
            try
            {
                string connectionString = new SQLiteConnectionStringBuilder
                {
                    DataSource = _databasePath,
                    ReadOnly = true,
                    FailIfMissing = true
                }.ToString();

                using (_connection = new SQLiteConnection(connectionString))
                {
                    _connection.Open();

                    // Egy egyszerű lekérdezést futtatunk, hogy ellenőrizzük az érvényességet
                    using (_command = _connection.CreateCommand())
                    {
                        _command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' LIMIT 1";
                        _command.ExecuteScalar();
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void InitializeDatabase()
        {
            if (!CheckDatabaseExists())
            {
                using (_connection = new SQLiteConnection($"Data Source={_databasePath}"))
                {
                    _connection.Open();

                    // Ide jöhetnek a kezdeti táblák létrehozása
                    CreateInitialTables(_connection);

                    Console.WriteLine("Új adatbázis inicializálva.");
                }
            }
        }

        private void CreateInitialTables(SQLiteConnection connection)
        { 
            // Példa táblák létrehozása
            var createTables = new[]
            {
            "CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Email TEXT)",
            "CREATE TABLE IF NOT EXISTS Products (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Price REAL)"
        };

            foreach (var createTable in createTables)
            {
                using (var command = new SQLiteCommand(createTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}