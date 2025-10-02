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

        internal bool insertKategoria(string v)
        {
            try
            {
                if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
                {
                    _connection = new SQLiteConnection($"Data Source={_databasePath}");
                    _connection.Open();
                }
                // beszúrjuk az új kategóriát
                _command.CommandText = "INSERT INTO kategoria (nev) VALUES (?)";
                _command.Parameters.AddWithValue("nev", v);
                _command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally { 
                if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            return true;
        }
        private void CreateInitialTables(SQLiteConnection connection)
        {
            // Példa táblák létrehozása
            var createTables = new[]
             {
                // 1. Kategóriák tábla (Különleges UNIQUE index is hozzáadva)
                // Az SQLite-ban a PRIMARY KEY INTEGER AUTOMATIKUSAN AUTOINCREMENT-et is jelent.
                "CREATE TABLE IF NOT EXISTS kategoriak (" +
                "kategoria_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "nev TEXT NOT NULL, " +
                "UNIQUE(nev) " + // UNIQUE index a név mezőre
                ");",
                // teszt adatok beszúrása
                //"INSERT OR IGNORE INTO kategoriak (nev) VALUES (null,'Processzor'), (null,'Memória'), (null,'Merevlemez'), ('Videókártya'), ('Alaplap');",
                // 2. Alkatrészek tábla (Külső kulcs a kategoriak táblára)
                "CREATE TABLE IF NOT EXISTS alkatreszek (" +
                "alkatresz_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "megnevezes TEXT NOT NULL, " +
                "gyarto TEXT, " +
                "modell TEXT, " +
                "kategoria_id INTEGER, " +
                "bevetelezes TEXT DEFAULT (datetime('now','localtime')), " +
                "FOREIGN KEY(kategoria_id) REFERENCES kategoriak(id) " +
                ");",

                // 3. Számítógépek tábla
                "CREATE TABLE IF NOT EXISTS szamitogepek (" +
                "szamitogep_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "nev TEXT NOT NULL, " +
                "leiras TEXT," +
                "bevetelezes TEXT DEFAULT (datetime('now','localtime')) " +
                ");",

                // 4. szamitogep_alkatresz (Összekötő tábla, Két külső kulcs)
                "CREATE TABLE IF NOT EXISTS szamitogep_alkatresz (" +
                "szamitogep_id INTEGER NOT NULL, " +
                "alkatresz_id INTEGER NOT NULL, " +
                "PRIMARY KEY (szamitogep_id, alkatresz_id), " + // Összetett elsődleges kulcs
                "FOREIGN KEY(szamitogep_id) REFERENCES szamitogepek(id), " +
                "FOREIGN KEY(alkatresz_id) REFERENCES alkatreszek(id) " +
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