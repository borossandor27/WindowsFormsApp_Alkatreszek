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
        private readonly string _databasePath;

        public DatabaseManager(string databasePath)
        {
            _databasePath = databasePath;
        }

        public bool CheckDatabaseExists()
        {
            if (!File.Exists(_databasePath))
                return false;

            try
            {
                string connectionString = new SQLiteConnectionStringBuilder
                {
                    DataSource = _databasePath,
                    ReadOnly = true,
                    FailIfMissing = true
                }.ToString();

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' LIMIT 1";
                        command.ExecuteScalar();
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
                using (var connection = new SQLiteConnection($"Data Source={_databasePath}"))
                {
                    connection.Open();
                    CreateInitialTables(connection);
                    Console.WriteLine("Új adatbázis inicializálva.");
                }
            }
        }

        internal bool insertKategoria(string v)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={_databasePath}"))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO kategoriak (nev) VALUES (@nev)";
                        command.Parameters.AddWithValue("@nev", v);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        private void CreateInitialTables(SQLiteConnection connection)
        {
            var createTables = new[]
            {
                "CREATE TABLE IF NOT EXISTS kategoriak (" +
                "kategoria_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "nev TEXT NOT NULL, " +
                "UNIQUE(nev) " +
                ");",
                "CREATE TABLE IF NOT EXISTS alkatreszek (" +
                "alkatresz_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "megnevezes TEXT NOT NULL, " +
                "gyarto TEXT, " +
                "modell TEXT, " +
                "kategoria_id INTEGER, " +
                "bevetelezes TEXT DEFAULT (datetime('now','localtime')), " +
                "FOREIGN KEY(kategoria_id) REFERENCES kategoriak(kategoria_id) " +
                ");",
                "CREATE TABLE IF NOT EXISTS szamitogepek (" +
                "szamitogep_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "nev TEXT NOT NULL, " +
                "leiras TEXT," +
                "bevetelezes TEXT DEFAULT (datetime('now','localtime')) " +
                ");",
                "CREATE TABLE IF NOT EXISTS szamitogep_alkatresz (" +
                "szamitogep_id INTEGER NOT NULL, " +
                "alkatresz_id INTEGER NOT NULL, " +
                "PRIMARY KEY (szamitogep_id, alkatresz_id), " +
                "FOREIGN KEY(szamitogep_id) REFERENCES szamitogepek(szamitogep_id), " +
                "FOREIGN KEY(alkatresz_id) REFERENCES alkatreszek(alkatresz_id) " +
                ");"
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