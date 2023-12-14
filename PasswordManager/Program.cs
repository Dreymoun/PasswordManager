using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordManager.BLL;
using PasswordManager.DLL;
using System.Configuration;

namespace PasswordManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string databasePath = ConfigurationManager.AppSettings["DatabasePath"];
            var dataLayer = new PasswordManagerDLL(databasePath);
            var businessLayer = new PasswordManagerBLL(dataLayer);

            bool running = true;
            Console.WriteLine("Добро пожаловать в менеджер паролей!");

            while (running)
            {
                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1: Установить мастер-пароль");
                Console.WriteLine("2: Добавить запись о пароле");
                Console.WriteLine("3: Получить запись о пароле");
                Console.WriteLine("4: Поиск записей");
                Console.WriteLine("5: Удалить мастер-пароль");
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
                            GetUserInput("Введите пароль:")
                        );
                        break;
                    case "3":
                        var entry = businessLayer.GetPasswordEntry(
                            GetUserInput("Введите веб-сайт для поиска:"),
                            GetUserInput("Введите имя пользователя для поиска:")
                        );
                        Console.WriteLine(entry != null ? $"Найдена запись: {entry}" : "Запись не найдена.");
                        break;
                    case "4":
                        var entries = businessLayer.SearchEntries(GetUserInput("Введите запрос для поиска:"));
                        DisplayEntries(entries);
                        break;
                    case "5":
                        businessLayer.DeleteMasterPassword();
                        Console.WriteLine("Мастер-пароль удален.");
                        break;
                    case "0":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Неверный ввод. Пожалуйста, введите число от 0 до 5.");
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
    }
}
