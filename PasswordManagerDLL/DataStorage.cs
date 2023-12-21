using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace PasswordManager.DAL
{
    public class PasswordEntry
    {
        // Уникальный идентификатор записи
        public int Id { get; set; }
        // Имя веб-сайта
        public string Website { get; set; }
        // Имя пользователя
        public string Username { get; set; }
        // Зашифрованный пароль
        public string EncryptedPassword { get; set; }
        // Дешифрованный пароль (не используется в базе данных)
        public string DecryptedPassword { get; set; }

        // Конструктор по умолчанию
        public PasswordEntry() { }

        // Конструктор с параметрами для создания новой записи
        public PasswordEntry(string website, string username, string encryptedPassword)
        {
            Website = website;
            Username = username;
            EncryptedPassword = encryptedPassword;
        }

        // Переопределение метода ToString для удобного отображения информации о записи
        public override string ToString()
        {
            return $"Веб-сайт: {Website}, Имя пользователя: {Username}, Зашифрованный пароль: {EncryptedPassword}";
        }
    }

    // Класс для взаимодействия с базой данных
    public class PasswordManagerDAL
    {
        // Путь к файлу базы данных
        private readonly string _databasePath;

        // Конструктор класса с параметром пути к базе данных
        public PasswordManagerDAL(string databasePath)
        {
            _databasePath = databasePath;
        }
        // Внутренний класс для представления мастер-пароля в базе данных
        public class MasterPassword
        {
            public int Id { get; set; }
            public string Password { get; set; }
        }
        // Метод для сохранения мастер-пароля в базе данных
        public void SaveMasterPassword(string masterPassword)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var masterPasswordCol = db.GetCollection<MasterPassword>("masterPassword");
                    masterPasswordCol.DeleteAll();
                    masterPasswordCol.Insert(new MasterPassword { Password = masterPassword });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при сохранении мастер-пароля: {ex.Message}");
            }
        }

        // Метод для загрузки мастер-пароля из базы данных
        public string LoadMasterPassword()
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var masterPasswordCol = db.GetCollection<MasterPassword>("masterPassword");
                    var masterPassword = masterPasswordCol.FindOne(Query.All());
                    return masterPassword?.Password;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при получении мастер-пароля: {ex.Message}");
                return null;
            }
        }

        

        // Метод для сохранения новой записи пароля или обновления существующей
        public void SavePassword(PasswordEntry entry)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при сохранении паролей: {ex.Message}");
            }
        }

        // Метод для получения записи пароля по веб-сайту и имени пользователя
        public PasswordEntry GetPassword(string website, string username)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var entries = db.GetCollection<PasswordEntry>("entries");
                    return entries.FindOne(e => e.Website == website && e.Username == username);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при поиске паролей: {ex.Message}");
                return null;
            }
        }

        // Метод для получения всех записей паролей из базы данных
        public List<PasswordEntry> GetAllPasswords()
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var entries = db.GetCollection<PasswordEntry>("entries");
                    return new List<PasswordEntry>(entries.FindAll());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при выводе паролей: {ex.Message}");
                return new List<PasswordEntry>();
            }
        }

        // Метод для получения записей паролей, удовлетворяющих заданному условию
        public IEnumerable<PasswordEntry> GetPasswords(Func<PasswordEntry, bool> predicate)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var entries = db.GetCollection<PasswordEntry>("entries");
                    return entries.FindAll().Where(predicate).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при выводе паролей: {ex.Message}");
                return new List<PasswordEntry>();
            }
        }

        // Метод для удаления записи пароля по идентификатору
        public void DeletePassword(int id)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var entries = db.GetCollection<PasswordEntry>("entries");
                    entries.Delete(id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при удалении мастер-пароля: {ex.Message}");
            }
        }

        // Метод для обновления записи пароля
        public bool UpdatePassword(string website, string username, string newPassword)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var entries = db.GetCollection<PasswordEntry>("entries");
                    var entry = entries.FindOne(e => e.Website == website && e.Username == username);
                    if (entry != null)
                    {
                        entry.EncryptedPassword = newPassword;
                        entries.Update(entry);
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при изменении пароля: {ex.Message}");
                return false;
            }
        }

        // Метод для инициализации базы данных
        public void InitializeDatabase()
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var entries = db.GetCollection<PasswordEntry>("entries");
                    entries.EnsureIndex(e => e.Website);
                    entries.EnsureIndex(e => e.Username);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при подключении к базе данных: {ex.Message}");
            }
        }

        // Метод для удаления записи пароля по веб-сайту и имени пользователя
        public void DeletePassword(string website, string username)
        {
            try
            {
                using (var db = new LiteDatabase(_databasePath))
                {
                    var entries = db.GetCollection<PasswordEntry>("entries");
                    var results = entries.Find(e => e.Website == website && e.Username == username);
                    foreach (var entry in results)
                    {
                        entries.Delete(entry.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при удалении паролей: {ex.Message}");
            }
        }
    }
}
