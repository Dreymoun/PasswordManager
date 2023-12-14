using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace PasswordManager.DLL
{
    public class PasswordEntry
    {
        public int Id { get; set; } // Уникальный идентификатор для LiteDB
        public string Website { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }

        public PasswordEntry() { }

        public PasswordEntry(string website, string username, string encryptedPassword)
        {
            Website = website;
            Username = username;
            EncryptedPassword = encryptedPassword;
        }
    }

    public class PasswordManagerDLL
    {
        private readonly string _databasePath;

        // Конструктор, принимающий путь к базе данных в качестве аргумента
        public PasswordManagerDLL(string databasePath)
        {
            _databasePath = databasePath;
        }

        public void SaveEntry(PasswordEntry entry)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var entries = db.GetCollection<PasswordEntry>("entries");
                if (entry.Id == 0)
                    entries.Insert(entry);
                else
                    entries.Update(entry);
            }
        }

        public PasswordEntry GetEntry(string website, string username)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var entries = db.GetCollection<PasswordEntry>("entries");
                return entries.FindOne(e => e.Website == website && e.Username == username);
            }
        }

        public List<PasswordEntry> GetAllEntries()
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var entries = db.GetCollection<PasswordEntry>("entries");
                return new List<PasswordEntry>(entries.FindAll());
            }
        }

        public void DeleteEntry(int id)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var entries = db.GetCollection<PasswordEntry>("entries");
                entries.Delete(id);
            }
        }
        public void InitializeDatabase()
        {
            // Проверка существования файла базы данных и его создание, если он не существует
            using (var db = new LiteDatabase(_databasePath))
            {
                // Инициализация может включать создание индексов, если это необходимо
                var entries = db.GetCollection<PasswordEntry>("entries");
                entries.EnsureIndex(e => e.Website);
                entries.EnsureIndex(e => e.Username);
            }
        }
    }
}
