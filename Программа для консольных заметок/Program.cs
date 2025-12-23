using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NoteTakingApp
{
    class Program
    {
        static List<Note> notes = new List<Note>();
        static string dataFilePath = "notes.json";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            LoadNotes();
            
            bool exit = false;
            
            while (!exit)
            {
                DisplayMenu();
                
                Console.Write("\nВыберите действие (1-6): ");
                string choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        ViewAllNotes();
                        break;
                    case "2":
                        AddNote();
                        break;
                    case "3":
                        EditNote();
                        break;
                    case "4":
                        DeleteNote();
                        break;
                    case "5":
                        SearchNotes();
                        break;
                    case "6":
                        exit = true;
                        Console.WriteLine("\nВыход из программы...");
                        break;
                    default:
                        Console.WriteLine("\nНеверный выбор. Попробуйте снова.");
                        break;
                }
                
                if (!exit)
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }
        
        static void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════");
            Console.WriteLine("          ПРОГРАММА ДЛЯ ЗАМЕТОК");
            Console.WriteLine("════════════════════════════════════════════");
            Console.WriteLine($"Всего заметок: {notes.Count}");
            Console.WriteLine("════════════════════════════════════════════");
            Console.WriteLine("1. Просмотреть все заметки");
            Console.WriteLine("2. Добавить новую заметку");
            Console.WriteLine("3. Редактировать заметку");
            Console.WriteLine("4. Удалить заметку");
            Console.WriteLine("5. Поиск заметок");
            Console.WriteLine("6. Выход");
            Console.WriteLine("════════════════════════════════════════════");
        }
        
        static void ViewAllNotes()
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════");
            Console.WriteLine("             ВСЕ ЗАМЕТКИ");
            Console.WriteLine("════════════════════════════════════════════");
            
            if (notes.Count == 0)
            {
                Console.WriteLine("Заметок нет.");
                return;
            }
            
            // Сортировка по дате (сначала новые)
            var sortedNotes = notes.OrderByDescending(n => n.CreatedDate).ToList();
            
            for (int i = 0; i < sortedNotes.Count; i++)
            {
                var note = sortedNotes[i];
                Console.WriteLine($"\n{i + 1}. {note.Title}");
                Console.WriteLine($"   Дата: {note.CreatedDate:dd.MM.yyyy HH:mm}");
                Console.WriteLine($"   Теги: {string.Join(", ", note.Tags)}");
                
                if (note.Content.Length > 100)
                {
                    Console.WriteLine($"   Содержание: {note.Content.Substring(0, 100)}...");
                }
                else
                {
                    Console.WriteLine($"   Содержание: {note.Content}");
                }
                
                Console.WriteLine($"   Категория: {note.Category}");
                Console.WriteLine("   ──────────────────────────────");
            }
            
            Console.WriteLine("\nДля просмотра деталей заметки нажмите Enter, для возврата - любую другую клавишу...");
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                Console.Write("\nВведите номер заметки для просмотра: ");
                if (int.TryParse(Console.ReadLine(), out int noteNumber) && noteNumber > 0 && noteNumber <= sortedNotes.Count)
                {
                    ViewNoteDetails(sortedNotes[noteNumber - 1]);
                }
            }
        }
        
        static void ViewNoteDetails(Note note)
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════");
            Console.WriteLine("           ДЕТАЛИ ЗАМЕТКИ");
            Console.WriteLine("════════════════════════════════════════════");
            Console.WriteLine($"Заголовок: {note.Title}");
            Console.WriteLine($"Дата создания: {note.CreatedDate:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Дата изменения: {note.LastModifiedDate:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Категория: {note.Category}");
            Console.WriteLine($"Теги: {string.Join(", ", note.Tags)}");
            Console.WriteLine("\nСодержание:");
            Console.WriteLine("─────────────────────────────────────────");
            Console.WriteLine(note.Content);
            Console.WriteLine("─────────────────────────────────────────");
        }
        
        static void AddNote()
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════");
            Console.WriteLine("          ДОБАВЛЕНИЕ НОВОЙ ЗАМЕТКИ");
            Console.WriteLine("════════════════════════════════════════════");
            
            Console.Write("Введите заголовок заметки: ");
            string title = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("Заголовок не может быть пустым!");
                return;
            }
            
            Console.WriteLine("\nВведите содержание заметки (для завершения введите '//end' на новой строке):");
            Console.WriteLine("─────────────────────────────────────────");
            
            StringBuilder contentBuilder = new StringBuilder();
            string line;
            
            while (true)
            {
                line = Console.ReadLine();
                if (line == "//end")
                    break;
                contentBuilder.AppendLine(line);
            }
            
            string content = contentBuilder.ToString().Trim();
            
            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine("Содержание не может быть пустым!");
                return;
            }
            
            Console.Write("\nВведите категорию (работа, личное, идеи и т.д.): ");
            string category = Console.ReadLine();
            
            Console.Write("Введите теги через запятую: ");
            string tagsInput = Console.ReadLine();
            List<string> tags = tagsInput.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
            
            Note newNote = new Note
            {
                Id = Guid.NewGuid(),
                Title = title,
                Content = content,
                Category = string.IsNullOrWhiteSpace(category) ? "Без категории" : category,
                Tags = tags,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };
            
            notes.Add(newNote);
            SaveNotes();
            
            Console.WriteLine($"\n✓ Заметка '{title}' успешно добавлена!");
        }
        
        static void EditNote()
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════");
            Console.WriteLine("           РЕДАКТИРОВАНИЕ ЗАМЕТКИ");
            Console.WriteLine("════════════════════════════════════════════");
            
            if (notes.Count == 0)
            {
                Console.WriteLine("Заметок нет для редактирования.");
                return;
            }
            
            // Показываем только заголовки для выбора
            for (int i = 0; i < notes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {notes[i].Title} ({notes[i].CreatedDate:dd.MM.yyyy})");
            }
            
            Console.Write("\nВведите номер заметки для редактирования: ");
            if (int.TryParse(Console.ReadLine(), out int noteNumber) && noteNumber > 0 && noteNumber <= notes.Count)
            {
                var noteToEdit = notes[noteNumber - 1];
                
                Console.WriteLine($"\nРедактирование: {noteToEdit.Title}");
                Console.WriteLine("─────────────────────────────────────────");
                
                Console.Write($"Новый заголовок [{noteToEdit.Title}]: ");
                string newTitle = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newTitle))
                    noteToEdit.Title = newTitle;
                
                Console.WriteLine($"\nТекущее содержание:\n{noteToEdit.Content}");
                Console.WriteLine("\nВведите новое содержание (оставьте пустым для сохранения старого):");
                Console.WriteLine("Для завершения введите '//end' на новой строке:");
                
                StringBuilder contentBuilder = new StringBuilder();
                string line;
                bool hasNewContent = false;
                
                while (true)
                {
                    line = Console.ReadLine();
                    if (line == "//end")
                        break;
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        contentBuilder.AppendLine(line);
                        hasNewContent = true;
                    }
                }
                
                if (hasNewContent)
                {
                    string newContent = contentBuilder.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(newContent))
                        noteToEdit.Content = newContent;
                }
                
                Console.Write($"\nНовая категория [{noteToEdit.Category}]: ");
                string newCategory = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newCategory))
                    noteToEdit.Category = newCategory;
                
                Console.Write($"Новые теги [{string.Join(", ", noteToEdit.Tags)}]: ");
                string newTagsInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newTagsInput))
                {
                    List<string> newTags = newTagsInput.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                    noteToEdit.Tags = newTags;
                }
                
                noteToEdit.LastModifiedDate = DateTime.Now;
                SaveNotes();
                
                Console.WriteLine($"\n✓ Заметка успешно обновлена!");
            }
            else
            {
                Console.WriteLine("Неверный номер заметки!");
            }
        }
        
        static void DeleteNote()
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════");
            Console.WriteLine("             УДАЛЕНИЕ ЗАМЕТКИ");
            Console.WriteLine("════════════════════════════════════════════");
            
            if (notes.Count == 0)
            {
                Console.WriteLine("Заметок нет для удаления.");
                return;
            }
            
            for (int i = 0; i < notes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {notes[i].Title} ({notes[i].CreatedDate:dd.MM.yyyy})");
            }
            
            Console.Write("\nВведите номер заметки для удаления: ");
            if (int.TryParse(Console.ReadLine(), out int noteNumber) && noteNumber > 0 && noteNumber <= notes.Count)
            {
                var noteToDelete = notes[noteNumber - 1];
                
                Console.Write($"\nВы уверены, что хотите удалить заметку '{noteToDelete.Title}'? (y/n): ");
                if (Console.ReadLine().ToLower() == "y")
                {
                    notes.RemoveAt(noteNumber - 1);
                    SaveNotes();
                    Console.WriteLine("✓ Заметка удалена!");
                }
                else
                {
                    Console.WriteLine("Удаление отменено.");
                }
            }
            else
            {
                Console.WriteLine("Неверный номер заметки!");
            }
        }
        
        static void SearchNotes()
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════");
            Console.WriteLine("               ПОИСК ЗАМЕТОК");
            Console.WriteLine("════════════════════════════════════════════");
            
            Console.WriteLine("1. Поиск по ключевым словам");
            Console.WriteLine("2. Поиск по тегам");
            Console.WriteLine("3. Поиск по категории");
            Console.WriteLine("4. Показать по дате (последние 7 дней)");
            Console.Write("\nВыберите тип поиска (1-4): ");
            
            string searchType = Console.ReadLine();
            List<Note> searchResults = new List<Note>();
            
            switch (searchType)
            {
                case "1":
                    Console.Write("Введите ключевые слова для поиска: ");
                    string keywords = Console.ReadLine().ToLower();
                    
                    searchResults = notes.Where(n => 
                        n.Title.ToLower().Contains(keywords) || 
                        n.Content.ToLower().Contains(keywords))
                        .ToList();
                    break;
                    
                case "2":
                    Console.Write("Введите тег для поиска: ");
                    string tag = Console.ReadLine().ToLower();
                    
                    searchResults = notes.Where(n => 
                        n.Tags.Any(t => t.ToLower().Contains(tag)))
                        .ToList();
                    break;
                    
                case "3":
                    Console.Write("Введите категорию для поиска: ");
                    string category = Console.ReadLine().ToLower();
                    
                    searchResults = notes.Where(n => 
                        n.Category.ToLower().Contains(category))
                        .ToList();
                    break;
                    
                case "4":
                    DateTime weekAgo = DateTime.Now.AddDays(-7);
                    searchResults = notes.Where(n => n.CreatedDate >= weekAgo)
                        .OrderByDescending(n => n.CreatedDate)
                        .ToList();
                    break;
                    
                default:
                    Console.WriteLine("Неверный выбор!");
                    return;
            }
            
            Console.WriteLine("\n════════════════════════════════════════════");
            Console.WriteLine($"Найдено заметок: {searchResults.Count}");
            Console.WriteLine("════════════════════════════════════════════");
            
            if (searchResults.Count == 0)
            {
                Console.WriteLine("Заметок не найдено.");
                return;
            }
            
            foreach (var note in searchResults)
            {
                Console.WriteLine($"\n• {note.Title}");
                Console.WriteLine($"  Дата: {note.CreatedDate:dd.MM.yyyy}");
                Console.WriteLine($"  Категория: {note.Category}");
                Console.WriteLine($"  Теги: {string.Join(", ", note.Tags)}");
            }
        }
        
        static void LoadNotes()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    string json = File.ReadAllText(dataFilePath);
                    notes = JsonSerializer.Deserialize<List<Note>>(json) ?? new List<Note>();
                    Console.WriteLine($"Загружено {notes.Count} заметок.");
                }
                else
                {
                    notes = new List<Note>();
                    Console.WriteLine("Файл с заметками не найден. Создан новый список.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке заметок: {ex.Message}");
                notes = new List<Note>();
            }
        }
        
        static void SaveNotes()
        {
            try
            {
                string json = JsonSerializer.Serialize(notes, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении заметок: {ex.Message}");
            }
        }
    }
    
    class Note
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        
        public Note()
        {
            Tags = new List<string>();
        }
    }
}