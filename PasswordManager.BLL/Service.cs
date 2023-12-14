using System;
using System.Collections.Generic;
using System.Linq;
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

        public void SetMasterPassword(string password)
        {
            // Хранение мастер-пароля, в будущем, с шифрованием
            _masterPassword = password;
        }
        public void DeleteMasterPassword()
        {
            // Очистка мастер-пароля
            _masterPassword = null;

        }

        public void AddPasswordEntry(string website, string username, string password)
        {
            // Шифрование пароля
            string encryptedPassword = EncryptPassword(password, _masterPassword);

            // Создание и сохранение новой записи
            var entry = new PasswordEntry(website, username, encryptedPassword);
            _dataLayer.SaveEntry(entry);
        }

        public PasswordEntry GetPasswordEntry(string website, string username)
        {
            // Получение и дешифровка записи
            var entry = _dataLayer.GetEntry(website, username);
            if (entry != null)
            {
                entry.EncryptedPassword = DecryptPassword(entry.EncryptedPassword, _masterPassword);
            }
            return entry;
        }

        public List<PasswordEntry> SearchEntries(string query)
        {
            // Поиск записей по запросу
            var allEntries = _dataLayer.GetAllEntries();
            return allEntries.Where(e => e.Website.Contains(query) || e.Username.Contains(query)).ToList();
        }

        private string EncryptPassword(string password, string masterPassword)
        {
            // Реализация шифрования пароля
            // Примечание: это должна быть более сложная логика, чем просто XOR
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private string DecryptPassword(string encryptedPassword, string masterPassword)
        {
            // Реализация дешифровки пароля
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encryptedPassword));
        }
    }
}
