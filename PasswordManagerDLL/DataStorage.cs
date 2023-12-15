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
        public string DecryptedPassword { get; set; }
        public PasswordEntry() { }

        public PasswordEntry(string website, string username, string encryptedPassword)
        {
            Website = website;
            Username = username;
            EncryptedPassword = encryptedPassword;
        }
        public override string ToString()
        {
            // Возвращаем информацию в удобочитаемом формате
            return $"Веб-сайт: {Website}, Имя пользователя: {Username}, Зашифрованный пароль: {EncryptedPassword}";
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
        public void SaveMasterPassword(string masterPassword)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var masterPasswordCol = db.GetCollection<MasterPassword>("masterPassword");
                masterPasswordCol.DeleteAll(); // Удаляем старый мастер-пароль, если он есть
                masterPasswordCol.Insert(new MasterPassword { Password = masterPassword });
            }
        }

        // Метод для загрузки мастер-пароля
        public string LoadMasterPassword()
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var masterPasswordCol = db.GetCollection<MasterPassword>("masterPassword");
                var masterPassword = masterPasswordCol.FindOne(Query.All());
                return masterPassword?.Password;
            }
        }

        // Класс для представления мастер-пароля в базе данных
        public class MasterPassword
        {
            public int Id { get; set; } // Идентификатор для LiteDB
            public string Password { get; set; } // Мастер-пароль
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
        public IEnumerable<PasswordEntry> GetEntries(Func<PasswordEntry, bool> predicate)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var entries = db.GetCollection<PasswordEntry>("entries");
                return entries.FindAll().Where(predicate).ToList();
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
        public bool UpdatePasswordEntry(string website, string username, string newPassword)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var entries = db.GetCollection<PasswordEntry>("entries");
                var entry = entries.FindOne(e => e.Website == website && e.Username == username);
                if (entry != null)
                {
                    entry.EncryptedPassword = newPassword; // предполагается, что newPassword уже зашифрован
                    entries.Update(entry);
                    return true;
                }
                return false;
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
        public void DeletePasswordEntry(string website, string username)
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                var entries = db.GetCollection<PasswordEntry>("entries");

                // Найти все записи, соответствующие критериям
                var results = entries.Find(e => e.Website == website && e.Username == username);

                // Удалить найденные записи
                foreach (var entry in results)
                {
                    entries.Delete(entry.Id); // Предполагается, что у PasswordEntry есть свойство Id
                }
            }
        }

    }
}
