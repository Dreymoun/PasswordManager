using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordManager.BLL;
using PasswordManager.DLL;
using System.Configuration;
using System.Threading;

namespace PasswordManager
{
    class Program
    {
        private static PasswordManagerBLL businessLayer;
        static void Main(string[] args)
        {
            string databasePath = ConfigurationManager.AppSettings["DatabasePath"];
            var dataLayer = new PasswordManagerDLL(databasePath);
            businessLayer = new PasswordManagerBLL(dataLayer);

            bool running = true;
            Console.WriteLine("Добро пожаловать в менеджер паролей!");

            while (running)
            {
                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1: Установить мастер-пароль");
                Console.WriteLine("2: Добавить пароль");
                Console.WriteLine("3: Получить логин и пароль от сайта");
                Console.WriteLine("4: Поиск сайтов и имен пользователя");
                Console.WriteLine("5: Удалить мастер-пароль");
                Console.WriteLine("6: Показать все пароли");
                Console.WriteLine("7: Удалить пароль");
                Console.WriteLine("8: Изменить пароль");
                Console.WriteLine("0: Выход");

                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        businessLayer.SetMasterPassword(GetUserInput("Введите мастер-пароль:"));
                        break;
                    case "2":
                        businessLayer.AddPasswordEntry(
                            GetUserInput("Введите веб-сайт:"),
                            GetUserInput("Введите имя пользователя:"),
                            GetUserInput("Введите пароль:"),
                            GetUserInput("Введите мастер-пароль:")
                        ) ;
                        break;
                    case "3":
                        var entry = businessLayer.GetPasswordEntry(
                            GetUserInput("Введите веб-сайт для поиска:"),
                            GetUserInput("Введите имя пользователя для поиска:"),
                            GetUserInput("Введите мастер-пароль:")
                        );
                        Console.WriteLine(entry != null ? $"Найдена запись: {entry}" : "Запись не найдена.");
                        break;
                    case "4":
                        var entries = businessLayer.SearchEntries(GetUserInput("Введите запрос для поиска:"),
                            GetUserInput("Введите мастер-пароль:"));
                        DisplayEntries(entries);
                        break;

                    case "5":
                        businessLayer.DeleteMasterPassword(GetUserInput("Введите мастер-пароль:"));
                        Console.WriteLine("Мастер-пароль удален.");
                        break;
                    case "6":
                        DisplayAllPasswords();
                        break;
                    case "7":
                        DeletePasswordEntry();
                        break;
                    case "8":
                        ChangePassword();
                        break;
                    case "0":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Неверный ввод. Пожалуйста, введите число от 0 до 6.");
                        break;
                }
            }
        }

        static string GetUserInput(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine();
        }

        static void DisplayEntries(IEnumerable<PasswordEntry> entries)
        {
            foreach (var entry in entries)
            {
                Console.WriteLine($"Веб-сайт: {entry.Website}, Имя пользователя: {entry.Username}");
            }
        }
        static void DisplayAllPasswords()
        {
            try
            {
                var entries = businessLayer.GetAllPasswordEntries(GetUserInput("Введите мастер-пароль:"));
                if (entries.Any())
                {
                    foreach (var entry in entries)
                    {
                        Console.WriteLine($"Сайт: {entry.Website}, Пользователь: {entry.Username}, Пароль: {entry.DecryptedPassword}");
                    }
                }
                else
                {
                    Console.WriteLine("Пароли не найдены.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при выводе паролей: {ex.Message}");
                Environment.Exit(1);
            }
        }
        static void DeletePasswordEntry()
        {
            try
            {
                string website = GetUserInput("Введите веб-сайт для удаления:");
                string username = GetUserInput("Введите имя пользователя для удаления:");
                bool isDeleted = businessLayer.DeletePasswordEntry(website, username, GetUserInput("Введите мастер-пароль:"));
                if (isDeleted)
                {
                    Console.WriteLine("Пароль успешно удален.");
                }
                else
                {
                    Console.WriteLine("По указанному сайту и логину пароль не найден.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при удалении пароля: {ex.Message}");
                Environment.Exit(1);
            }
        }
        static void ChangePassword()
        {
            try
            {
                string website = GetUserInput("Введите веб-сайт:");
                string username = GetUserInput("Введите имя пользователя:");
                string oldPassword = GetUserInput("Введите старый пароль:");
                string newPassword = GetUserInput("Введите новый пароль:");
                bool passwordChanged = businessLayer.ChangePassword(website, username, oldPassword, newPassword, GetUserInput("Введите мастер-пароль:"));
                if (passwordChanged)
                {
                    Console.WriteLine("Пароль успешно изменен.");
                }
                else
                {
                    Console.WriteLine("Ошибка при изменении пароля. Убедитесь, что введены правильные данные.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при изменении пароля: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}
