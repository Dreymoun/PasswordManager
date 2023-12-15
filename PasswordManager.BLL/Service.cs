using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PasswordManager.DLL;

namespace PasswordManager.BLL
{
    public class PasswordManagerBLL
    {
        private PasswordManagerDLL _dataLayer;
        private string _masterPassword;

        public PasswordManagerBLL(PasswordManagerDLL dataLayer)
        {
            _dataLayer = dataLayer;

        }

        private bool CheckMasterPassword(string inputPassword)
        {
            return _masterPassword == inputPassword;
        }

        public void SetMasterPassword(string password)
        {
            _masterPassword = password;
            Console.WriteLine("Мастер-пароль успешно создан");
        }

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

        public void AddPasswordEntry(string website, string username, string password, string inputPassword)
        {
            if (CheckMasterPassword(inputPassword))
            {
                string encryptedPassword = EncryptPassword(password, 4);
                var entry = new PasswordEntry(website, username, encryptedPassword);
                _dataLayer.SaveEntry(entry);
                Console.WriteLine("Запись успешно добавлена");
            }
            else
            {
                Console.WriteLine("Неверный мастер-пароль");
            }
        }

        public PasswordEntry GetPasswordEntry(string website, string username, string inputPassword)
        {
            if (CheckMasterPassword(inputPassword))
            {
                var entry = _dataLayer.GetEntry(website, username);
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

        public List<PasswordEntry> SearchEntries(string query, string inputPassword)
        {
            if (CheckMasterPassword(inputPassword))
            {
                var allEntries = _dataLayer.GetAllEntries();
                return allEntries.Where(e => e.Website.Contains(query) || e.Username.Contains(query)).ToList();
            }
            else
            {
                Console.WriteLine("Неверный мастер-пароль");
                return new List<PasswordEntry>();
            }
        }

        public IEnumerable<PasswordEntry> GetAllPasswordEntries(string inputPassword)
        {
            if (CheckMasterPassword(inputPassword))
            {
                var encryptedEntries = _dataLayer.GetAllEntries();
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

        public bool DeletePasswordEntry(string website, string username, string inputPassword)
        {
            if (CheckMasterPassword(inputPassword))
            {
                var entriesToDelete = _dataLayer.GetEntries(e => e.Website == website && e.Username == username).ToList();
                if (entriesToDelete.Any())
                {
                    foreach (var entry in entriesToDelete)
                    {
                        _dataLayer.DeleteEntry(entry.Id);
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

        public bool ChangePassword(string website, string username, string oldPassword, string newPassword, string inputPassword)
        {
            if (CheckMasterPassword(inputPassword))
            {
                var entry = GetPasswordEntry(website, username, inputPassword); // Теперь передаем inputPassword
                if (entry != null)
                {
                    string decryptedOldPassword = DecryptPassword(entry.EncryptedPassword, 4);
                    if (decryptedOldPassword == oldPassword)
                    {
                        string encryptedNewPassword = EncryptPassword(newPassword, 4);
                        entry.EncryptedPassword = encryptedNewPassword;
                        _dataLayer.SaveEntry(entry);
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

        private string EncryptPassword(string password, int key)
        {
            char[] buffer = password.ToCharArray();
            for (int i = 0; i < buffer.Length; i++)
            {
                // Сдвигаем каждый символ на 'key' позиций вперед
                char letter = buffer[i];
                letter = (char)(letter + key);
                buffer[i] = letter;
            }
            return new string(buffer);
        }

        private string DecryptPassword(string encryptedPassword, int key)
        {
            char[] buffer = encryptedPassword.ToCharArray();
            for (int i = 0; i < buffer.Length; i++)
            {
                // Сдвигаем каждый символ на 'key' позиций назад
                char letter = buffer[i];
                letter = (char)(letter - key);
                buffer[i] = letter;
            }
            return new string(buffer);
        }

    }
}
