using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PasswordManager.DAL;

namespace PasswordManager.BLL
{
    public class PasswordManagerBLL
    {
        // Поле для хранения ссылки на уровень доступа к данным
        private PasswordManagerDAL _dataLayer;

        // Поле для хранения мастер-пароля
        private string _masterPassword;

        // Конструктор класса, инициализирующий уровень доступа к данным
        public PasswordManagerBLL(PasswordManagerDAL dataLayer)
        {
            _dataLayer = dataLayer;
        }

        // Метод для проверки правильности мастер-пароля
        private bool CheckMasterPassword(string inputPassword)
        {
            return _masterPassword == inputPassword;
        }

        // Метод для установки мастер-пароля
        public void SetMasterPassword(string password)
        {
            _masterPassword = password;
            Console.WriteLine("Мастер-пароль успешно создан");
        }

        // Метод для удаления мастер-пароля
        public void DeleteMasterPassword(string inputPassword)
        {
            if (CheckMasterPassword(inputPassword))
            {
                _masterPassword = null;
                Console.WriteLine("Мастер-пароль успешно удален.");
            }
            else
            {
                Console.WriteLine("Неверный мастер-пароль");
            }
        }

        // Метод для добавления новой записи пароля
        public void AddPassword(string website, string username, string password, string inputPassword)
        {
            try
            {
                if (CheckMasterPassword(inputPassword))
                {
                    string encryptedPassword = EncryptPassword(password, 4);
                    var entry = new PasswordEntry(website, username, encryptedPassword);
                    _dataLayer.SavePassword(entry);
                    Console.WriteLine("Запись успешно добавлена");
                }
                else
                {
                    Console.WriteLine("Неверный мастер-пароль");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при добавлении записи: " + ex.Message);
            }
        }

        // Метод для получения записи пароля по веб-сайту и имени пользователя
        public PasswordEntry GetPasswordEntry(string website, string username, string inputPassword)
        {
            try
            {
                if (CheckMasterPassword(inputPassword))
                {
                    var entry = _dataLayer.GetPassword(website, username);
                    if (entry != null)
                    {
                        entry.DecryptedPassword = DecryptPassword(entry.EncryptedPassword, 4);
                        return entry;
                    }
                }
                else
                {
                    Console.WriteLine("Неверный мастер-пароль");
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении записи: " + ex.Message);
                return null;
            }
        }

        // Метод для поиска записей паролей по запросу
        public List<PasswordEntry> SearchEntries(string query, string inputPassword)
        {
            try
            {
                if (CheckMasterPassword(inputPassword))
                {
                    var allEntries = _dataLayer.GetAllPasswords();
                    return allEntries.Where(e => e.Website.Contains(query) || e.Username.Contains(query)).ToList();
                }
                else
                {
                    Console.WriteLine("Неверный мастер-пароль");
                    return new List<PasswordEntry>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при поиске записей: " + ex.Message);
                return new List<PasswordEntry>();
            }
        }

        // Метод для получения всех записей паролей
        public IEnumerable<PasswordEntry> GetAllPasswordEntries(string inputPassword)
        {
            try
            {
                if (CheckMasterPassword(inputPassword))
                {
                    var encryptedEntries = _dataLayer.GetAllPasswords();
                    var decryptedEntries = new List<PasswordEntry>();

                    foreach (var entry in encryptedEntries)
                    {
                        string decryptedPassword = DecryptPassword(entry.EncryptedPassword, 4);
                        decryptedEntries.Add(new PasswordEntry(entry.Website, entry.Username, entry.EncryptedPassword)
                        {
                            DecryptedPassword = decryptedPassword
                        });
                    }

                    return decryptedEntries;
                }
                else
                {
                    Console.WriteLine("Неверный мастер-пароль");
                    return new List<PasswordEntry>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении всех записей: " + ex.Message);
                return new List<PasswordEntry>();
            }
        }

        // Метод для удаления записи пароля
        public bool DeletePasswordEntry(string website, string username, string inputPassword)
        {
            try
            {
                if (CheckMasterPassword(inputPassword))
                {
                    var entriesToDelete = _dataLayer.GetPasswords(e => e.Website == website && e.Username == username).ToList();
                    if (entriesToDelete.Any())
                    {
                        foreach (var entry in entriesToDelete)
                        {
                            _dataLayer.DeletePassword(entry.Id);
                        }
                        return true;
                    }
                    else
                    {
                        return false; // Запись не найдена
                    }
                }
                else
                {
                    Console.WriteLine("Неверный мастер-пароль");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при удалении записи: " + ex.Message);
                return false;
            }
        }

        // Метод для изменения пароля в записи
        public bool ChangePassword(string website, string username, string oldPassword, string newPassword, string inputPassword)
        {
            try
            {
                if (CheckMasterPassword(inputPassword))
                {
                    var entry = GetPasswordEntry(website, username, inputPassword);
                    if (entry != null)
                    {
                        string decryptedOldPassword = DecryptPassword(entry.EncryptedPassword, 4);
                        if (decryptedOldPassword == oldPassword)
                        {
                            string encryptedNewPassword = EncryptPassword(newPassword, 4);
                            entry.EncryptedPassword = encryptedNewPassword;
                            _dataLayer.SavePassword(entry);
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    Console.WriteLine("Неверный мастер-пароль");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при изменении пароля: " + ex.Message);
                return false;
            }
        }

        // Метод для шифрования пароля
        private string EncryptPassword(string password, int key)
        {
            try
            {
                char[] buffer = password.ToCharArray();
                for (int i = 0; i < buffer.Length; i++)
                {
                    char letter = buffer[i];
                    letter = (char)(letter + key); // Простое шифрование путем сдвига символов
                    buffer[i] = letter;
                }
                return new string(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при шифровании: " + ex.Message);
                return null;
            }
        }

        // Метод для дешифрования пароля
        private string DecryptPassword(string encryptedPassword, int key)
        {
            try
            {
                char[] buffer = encryptedPassword.ToCharArray();
                for (int i = 0; i < buffer.Length; i++)
                {
                    char letter = buffer[i];
                    letter = (char)(letter - key); // Простое дешифрование путем обратного сдвига символов
                    buffer[i] = letter;
                }
                return new string(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при дешифровании: " + ex.Message);
                return null;
            }
        }
    }
}
