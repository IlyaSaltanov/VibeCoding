using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PersonalFinanceManager
{
    // Класс для представления финансовой транзакции
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public TransactionType Type { get; set; }
        
        public Transaction(int id, DateTime date, decimal amount, string category, string description, TransactionType type)
        {
            Id = id;
            Date = date;
            Amount = amount;
            Category = category;
            Description = description;
            Type = type;
        }
    }

    // Тип транзакции (доход или расход)
    public enum TransactionType
    {
        Income,
        Expense
    }

    // Класс для анализа финансовых данных
    public class FinancialAnalyzer
    {
        private List<Transaction> transactions;
        
        public FinancialAnalyzer(List<Transaction> transactions)
        {
            this.transactions = transactions;
        }
        
        // Получить общий баланс
        public decimal GetBalance()
        {
            decimal income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            decimal expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            return income - expenses;
        }
        
        // Получить расходы по категориям
        public Dictionary<string, decimal> GetExpensesByCategory()
        {
            return transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        }
        
        // Получить доходы по категориям
        public Dictionary<string, decimal> GetIncomeByCategory()
        {
            return transactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        }
        
        // Получить транзакции за определенный период
        public List<Transaction> GetTransactionsByPeriod(DateTime startDate, DateTime endDate)
        {
            return transactions
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .OrderByDescending(t => t.Date)
                .ToList();
        }
        
        // Получить рекомендации по оптимизации расходов
        public List<string> GetOptimizationRecommendations()
        {
            var recommendations = new List<string>();
            var expensesByCategory = GetExpensesByCategory();
            
            if (expensesByCategory.Count == 0)
                return recommendations;
            
            // Находим категорию с наибольшими расходами
            var maxExpenseCategory = expensesByCategory.OrderByDescending(e => e.Value).First();
            
            if (maxExpenseCategory.Value > 0)
            {
                recommendations.Add($"Наибольшие расходы в категории '{maxExpenseCategory.Key}': {maxExpenseCategory.Value:C}. Рекомендуется проанализировать и сократить эти траты.");
            }
            
            // Проверяем, есть ли мелкие ежедневные расходы, которые можно сократить
            var dailyExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense && t.Amount < 500 && t.Date >= DateTime.Now.AddDays(-30))
                .Sum(t => t.Amount);
                
            if (dailyExpenses > 3000)
            {
                recommendations.Add($"За последний месяц вы потратили {dailyExpenses:C} на мелкие ежедневные расходы. Попробуйте их сократить.");
            }
            
            // Проверяем баланс
            var balance = GetBalance();
            if (balance < 0)
            {
                recommendations.Add("Ваш баланс отрицательный! Необходимо срочно сократить расходы или увеличить доходы.");
            }
            else if (balance < 10000)
            {
                recommendations.Add($"Ваш баланс ({balance:C}) низкий. Рекомендуется создать финансовую подушку безопасности.");
            }
            
            return recommendations;
        }
        
        // Получить статистику за месяц
        public Dictionary<string, decimal> GetMonthlyStatistics(int year, int month)
        {
            var result = new Dictionary<string, decimal>();
            
            var monthlyTransactions = transactions
                .Where(t => t.Date.Year == year && t.Date.Month == month)
                .ToList();
                
            var monthlyIncome = monthlyTransactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);
                
            var monthlyExpenses = monthlyTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);
                
            result["Доходы"] = monthlyIncome;
            result["Расходы"] = monthlyExpenses;
            result["Баланс"] = monthlyIncome - monthlyExpenses;
            
            return result;
        }
    }

    // Класс для управления финансовыми данными
    public class FinanceManager
    {
        private List<Transaction> transactions;
        private int nextId;
        private string dataFilePath;
        
        public FinanceManager(string filePath = "transactions.json")
        {
            dataFilePath = filePath;
            transactions = new List<Transaction>();
            nextId = 1;
            LoadTransactions();
        }
        
        // Добавить транзакцию
        public void AddTransaction(Transaction transaction)
        {
            transaction.Id = nextId++;
            transactions.Add(transaction);
            SaveTransactions();
        }
        
        // Удалить транзакцию по ID
        public bool RemoveTransaction(int id)
        {
            var transaction = transactions.FirstOrDefault(t => t.Id == id);
            if (transaction != null)
            {
                transactions.Remove(transaction);
                SaveTransactions();
                return true;
            }
            return false;
        }
        
        // Редактировать транзакцию
        public bool UpdateTransaction(int id, DateTime date, decimal amount, string category, string description, TransactionType type)
        {
            var transaction = transactions.FirstOrDefault(t => t.Id == id);
            if (transaction != null)
            {
                transaction.Date = date;
                transaction.Amount = amount;
                transaction.Category = category;
                transaction.Description = description;
                transaction.Type = type;
                SaveTransactions();
                return true;
            }
            return false;
        }
        
        // Получить все транзакции
        public List<Transaction> GetAllTransactions()
        {
            return transactions.OrderByDescending(t => t.Date).ToList();
        }
        
        // Сохранить транзакции в файл
        private void SaveTransactions()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(transactions, options);
                File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
            }
        }
        
        // Загрузить транзакции из файла
        private void LoadTransactions()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    var json = File.ReadAllText(dataFilePath);
                    var loadedTransactions = JsonSerializer.Deserialize<List<Transaction>>(json);
                    
                    if (loadedTransactions != null && loadedTransactions.Count > 0)
                    {
                        transactions = loadedTransactions;
                        nextId = transactions.Max(t => t.Id) + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}. Будет создан новый файл.");
            }
        }
        
        // Получить транзакции по категории
        public List<Transaction> GetTransactionsByCategory(string category)
        {
            return transactions
                .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.Date)
                .ToList();
        }
        
        // Получить категории расходов
        public List<string> GetExpenseCategories()
        {
            return transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Select(t => t.Category)
                .Distinct()
                .ToList();
        }
        
        // Получить категории доходов
        public List<string> GetIncomeCategories()
        {
            return transactions
                .Where(t => t.Type == TransactionType.Income)
                .Select(t => t.Category)
                .Distinct()
                .ToList();
        }
    }

    // Основной класс приложения
    class Program
    {
        static FinanceManager financeManager;
        static FinancialAnalyzer financialAnalyzer;
        
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            
            financeManager = new FinanceManager();
            financialAnalyzer = new FinancialAnalyzer(financeManager.GetAllTransactions());
            
            Console.WriteLine("=== ПЕРСОНАЛЬНЫЙ ФИНАНСОВЫЙ МЕНЕДЖЕР ===");
            Console.WriteLine("Программа для анализа и оптимизации расходов\n");
            
            ShowMainMenu();
        }
        
        static void ShowMainMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== ГЛАВНОЕ МЕНЮ ===");
                Console.WriteLine("1. Просмотреть все транзакции");
                Console.WriteLine("2. Добавить транзакцию");
                Console.WriteLine("3. Удалить транзакцию");
                Console.WriteLine("4. Редактировать транзакцию");
                Console.WriteLine("5. Анализ финансов");
                Console.WriteLine("6. Рекомендации по оптимизации");
                Console.WriteLine("7. Статистика за месяц");
                Console.WriteLine("8. Выход");
                
                Console.Write("\nВыберите действие: ");
                string choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        ShowAllTransactions();
                        break;
                    case "2":
                        AddTransaction();
                        break;
                    case "3":
                        DeleteTransaction();
                        break;
                    case "4":
                        EditTransaction();
                        break;
                    case "5":
                        ShowFinancialAnalysis();
                        break;
                    case "6":
                        ShowOptimizationRecommendations();
                        break;
                    case "7":
                        ShowMonthlyStatistics();
                        break;
                    case "8":
                        Console.WriteLine("\nСпасибо за использование программы! До свидания!");
                        return;
                    default:
                        Console.WriteLine("\nНеверный выбор. Попробуйте снова.");
                        break;
                }
            }
        }
        
        static void ShowAllTransactions()
        {
            var transactions = financeManager.GetAllTransactions();
            
            Console.WriteLine("\n=== ВСЕ ТРАНЗАКЦИИ ===\n");
            
            if (transactions.Count == 0)
            {
                Console.WriteLine("Транзакций не найдено.");
                return;
            }
            
            Console.WriteLine($"{"ID",-5} {"Дата",-12} {"Тип",-8} {"Категория",-15} {"Сумма",-12} {"Описание"}");
            Console.WriteLine(new string('-', 80));
            
            foreach (var transaction in transactions)
            {
                string type = transaction.Type == TransactionType.Income ? "Доход" : "Расход";
                Console.WriteLine($"{transaction.Id,-5} {transaction.Date:dd.MM.yyyy,-12} {type,-8} {transaction.Category,-15} {transaction.Amount,12:C} {transaction.Description}");
            }
            
            Console.WriteLine($"\nВсего транзакций: {transactions.Count}");
        }
        
        static void AddTransaction()
        {
            Console.WriteLine("\n=== ДОБАВЛЕНИЕ НОВОЙ ТРАНЗАКЦИИ ===");
            
            DateTime date;
            while (true)
            {
                Console.Write("Дата (дд.мм.гггг) [сегодня]: ");
                string dateInput = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(dateInput))
                {
                    date = DateTime.Today;
                    break;
                }
                
                if (DateTime.TryParseExact(dateInput, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    break;
                }
                
                Console.WriteLine("Неверный формат даты. Попробуйте снова.");
            }
            
            decimal amount;
            while (true)
            {
                Console.Write("Сумма: ");
                string amountInput = Console.ReadLine();
                
                if (decimal.TryParse(amountInput, out amount) && amount > 0)
                {
                    break;
                }
                
                Console.WriteLine("Неверная сумма. Введите положительное число.");
            }
            
            Console.Write("Категория: ");
            string category = Console.ReadLine();
            
            Console.Write("Описание: ");
            string description = Console.ReadLine();
            
            TransactionType type;
            while (true)
            {
                Console.Write("Тип (1 - Доход, 2 - Расход): ");
                string typeInput = Console.ReadLine();
                
                if (typeInput == "1")
                {
                    type = TransactionType.Income;
                    break;
                }
                else if (typeInput == "2")
                {
                    type = TransactionType.Expense;
                    break;
                }
                
                Console.WriteLine("Неверный выбор. Введите 1 или 2.");
            }
            
            var transaction = new Transaction(0, date, amount, category, description, type);
            financeManager.AddTransaction(transaction);
            
            Console.WriteLine("\nТранзакция успешно добавлена!");
        }
        
        static void DeleteTransaction()
        {
            Console.WriteLine("\n=== УДАЛЕНИЕ ТРАНЗАКЦИИ ===");
            
            ShowAllTransactions();
            
            Console.Write("\nВведите ID транзакции для удаления: ");
            
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                bool success = financeManager.RemoveTransaction(id);
                
                if (success)
                {
                    Console.WriteLine("Транзакция успешно удалена!");
                }
                else
                {
                    Console.WriteLine("Транзакция с указанным ID не найдена.");
                }
            }
            else
            {
                Console.WriteLine("Неверный формат ID.");
            }
        }
        
        static void EditTransaction()
        {
            Console.WriteLine("\n=== РЕДАКТИРОВАНИЕ ТРАНЗАКЦИИ ===");
            
            ShowAllTransactions();
            
            Console.Write("\nВведите ID транзакции для редактирования: ");
            
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Неверный формат ID.");
                return;
            }
            
            var transactions = financeManager.GetAllTransactions();
            var transaction = transactions.FirstOrDefault(t => t.Id == id);
            
            if (transaction == null)
            {
                Console.WriteLine("Транзакция с указанным ID не найдена.");
                return;
            }
            
            Console.WriteLine("\nТекущие данные транзакции:");
            Console.WriteLine($"Дата: {transaction.Date:dd.MM.yyyy}");
            Console.WriteLine($"Сумма: {transaction.Amount:C}");
            Console.WriteLine($"Категория: {transaction.Category}");
            Console.WriteLine($"Описание: {transaction.Description}");
            Console.WriteLine($"Тип: {(transaction.Type == TransactionType.Income ? "Доход" : "Расход")}");
            
            Console.WriteLine("\nВведите новые данные (оставьте пустым, чтобы не изменять):");
            
            DateTime date;
            while (true)
            {
                Console.Write($"Дата (дд.мм.гггг) [{transaction.Date:dd.MM.yyyy}]: ");
                string dateInput = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(dateInput))
                {
                    date = transaction.Date;
                    break;
                }
                
                if (DateTime.TryParseExact(dateInput, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    break;
                }
                
                Console.WriteLine("Неверный формат даты. Попробуйте снова.");
            }
            
            decimal amount;
            while (true)
            {
                Console.Write($"Сумма [{transaction.Amount}]: ");
                string amountInput = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(amountInput))
                {
                    amount = transaction.Amount;
                    break;
                }
                
                if (decimal.TryParse(amountInput, out amount) && amount > 0)
                {
                    break;
                }
                
                Console.WriteLine("Неверная сумма. Введите положительное число.");
            }
            
            Console.Write($"Категория [{transaction.Category}]: ");
            string categoryInput = Console.ReadLine();
            string category = string.IsNullOrWhiteSpace(categoryInput) ? transaction.Category : categoryInput;
            
            Console.Write($"Описание [{transaction.Description}]: ");
            string descriptionInput = Console.ReadLine();
            string description = string.IsNullOrWhiteSpace(descriptionInput) ? transaction.Description : descriptionInput;
            
            TransactionType type;
            while (true)
            {
                Console.Write($"Тип (1 - Доход, 2 - Расход) [{(transaction.Type == TransactionType.Income ? "1" : "2")}]: ");
                string typeInput = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(typeInput))
                {
                    type = transaction.Type;
                    break;
                }
                
                if (typeInput == "1")
                {
                    type = TransactionType.Income;
                    break;
                }
                else if (typeInput == "2")
                {
                    type = TransactionType.Expense;
                    break;
                }
                
                Console.WriteLine("Неверный выбор. Введите 1 или 2.");
            }
            
            bool success = financeManager.UpdateTransaction(id, date, amount, category, description, type);
            
            if (success)
            {
                Console.WriteLine("\nТранзакция успешно обновлена!");
            }
            else
            {
                Console.WriteLine("\nОшибка при обновлении транзакции.");
            }
        }
        
        static void ShowFinancialAnalysis()
        {
            Console.WriteLine("\n=== ФИНАНСОВЫЙ АНАЛИЗ ===\n");
            
            // Обновляем анализатор с текущими данными
            financialAnalyzer = new FinancialAnalyzer(financeManager.GetAllTransactions());
            
            decimal balance = financialAnalyzer.GetBalance();
            Console.WriteLine($"Общий баланс: {balance:C}");
            
            var expensesByCategory = financialAnalyzer.GetExpensesByCategory();
            if (expensesByCategory.Count > 0)
            {
                Console.WriteLine("\nРасходы по категориям:");
                foreach (var category in expensesByCategory.OrderByDescending(c => c.Value))
                {
                    Console.WriteLine($"  {category.Key}: {category.Value:C}");
                }
            }
            
            var incomeByCategory = financialAnalyzer.GetIncomeByCategory();
            if (incomeByCategory.Count > 0)
            {
                Console.WriteLine("\nДоходы по категориям:");
                foreach (var category in incomeByCategory.OrderByDescending(c => c.Value))
                {
                    Console.WriteLine($"  {category.Key}: {category.Value:C}");
                }
            }
            
            // Показываем последние 10 транзакций
            var recentTransactions = financeManager.GetAllTransactions().Take(10).ToList();
            if (recentTransactions.Count > 0)
            {
                Console.WriteLine("\nПоследние транзакции:");
                foreach (var transaction in recentTransactions)
                {
                    string type = transaction.Type == TransactionType.Income ? "+" : "-";
                    Console.WriteLine($"  {transaction.Date:dd.MM.yyyy} {type} {transaction.Amount,10:C} {transaction.Category} - {transaction.Description}");
                }
            }
        }
        
        static void ShowOptimizationRecommendations()
        {
            Console.WriteLine("\n=== РЕКОМЕНДАЦИИ ПО ОПТИМИЗАЦИИ РАСХОДОВ ===\n");
            
            // Обновляем анализатор с текущими данными
            financialAnalyzer = new FinancialAnalyzer(financeManager.GetAllTransactions());
            
            var recommendations = financialAnalyzer.GetOptimizationRecommendations();
            
            if (recommendations.Count == 0)
            {
                Console.WriteLine("Поздравляем! Ваши финансы в порядке. Рекомендаций по оптимизации нет.");
                return;
            }
            
            Console.WriteLine("На основе анализа ваших финансовых данных:");
            for (int i = 0; i < recommendations.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {recommendations[i]}");
            }
        }
        
        static void ShowMonthlyStatistics()
        {
            Console.WriteLine("\n=== СТАТИСТИКА ЗА МЕСЯЦ ===");
            
            int year, month;
            
            while (true)
            {
                Console.Write("Введите год [текущий]: ");
                string yearInput = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(yearInput))
                {
                    year = DateTime.Now.Year;
                    break;
                }
                
                if (int.TryParse(yearInput, out year) && year >= 2000 && year <= 2100)
                {
                    break;
                }
                
                Console.WriteLine("Неверный год. Введите год между 2000 и 2100.");
            }
            
            while (true)
            {
                Console.Write("Введите месяц (1-12) [текущий]: ");
                string monthInput = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(monthInput))
                {
                    month = DateTime.Now.Month;
                    break;
                }
                
                if (int.TryParse(monthInput, out month) && month >= 1 && month <= 12)
                {
                    break;
                }
                
                Console.WriteLine("Неверный месяц. Введите число от 1 до 12.");
            }
            
            // Обновляем анализатор с текущими данными
            financialAnalyzer = new FinancialAnalyzer(financeManager.GetAllTransactions());
            
            var statistics = financialAnalyzer.GetMonthlyStatistics(year, month);
            
            Console.WriteLine($"\nСтатистика за {month:00}.{year}:");
            Console.WriteLine(new string('-', 30));
            
            foreach (var stat in statistics)
            {
                Console.WriteLine($"{stat.Key,-10} {stat.Value,20:C}");
            }
        }
    }
}