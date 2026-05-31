using System.Text.Json;
using BaiThucHanh3.Models;

namespace BaiThucHanh3.Services
{
    public class BookService
    {
        private static readonly Book[] DefaultBooks =
        [
            new Book { Id = 1, Name = "Clean Code", Price = 20 },
            new Book { Id = 2, Name = "ASP.NET MVC", Price = 15 },
            new Book { Id = 3, Name = "Design Pattern", Price = 25 }
        ];

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        private readonly string _filePath;
        private readonly object _lock = new();
        private List<Book> _books = [];

        public BookService(IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "books.json");
            Load();
        }

        public IReadOnlyList<Book> GetAll()
        {
            lock (_lock)
            {
                return _books.ToList();
            }
        }

        public Book? GetById(int id)
        {
            lock (_lock)
            {
                return _books.FirstOrDefault(b => b.Id == id);
            }
        }

        public void Add(Book book)
        {
            lock (_lock)
            {
                book.Id = _books.Count == 0 ? 1 : _books.Max(b => b.Id) + 1;
                _books.Add(book);
                Save();
            }
        }

        public bool Update(Book book)
        {
            lock (_lock)
            {
                var index = _books.FindIndex(b => b.Id == book.Id);
                if (index < 0)
                {
                    return false;
                }

                _books[index] = book;
                Save();
                return true;
            }
        }

        public bool Delete(int id)
        {
            lock (_lock)
            {
                var book = _books.FirstOrDefault(b => b.Id == id);
                if (book == null)
                {
                    return false;
                }

                _books.Remove(book);
                Save();
                return true;
            }
        }

        private void Load()
        {
            lock (_lock)
            {
                if (!File.Exists(_filePath))
                {
                    _books = DefaultBooks.Select(b => new Book
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Price = b.Price
                    }).ToList();
                    Save();
                    return;
                }

                var json = File.ReadAllText(_filePath);
                _books = JsonSerializer.Deserialize<List<Book>>(json, JsonOptions) ?? [];
                if (_books.Count == 0)
                {
                    _books = DefaultBooks.Select(b => new Book
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Price = b.Price
                    }).ToList();
                    Save();
                }
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_books, JsonOptions);
            File.WriteAllText(_filePath, json);
        }
    }
}
