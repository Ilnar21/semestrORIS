using HttpServerLibrary;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;
using MyHTTPServer.Sessions;
using System.IO;

namespace MyHTTPServer.EndPoints
{
    public class MovieEndpoint : BaseEndPoint
    {
        private readonly ORMContext _context;

        // Конструктор с параметром для передачи ORMContext
        public MovieEndpoint()
        {
            var connectionString = "Host=localhost; Port=5433; Username=postgres; Password=1903; Database=postgres"; // Укажите вашу строку подключения
            _context = new ORMContext(connectionString);  // Создаем ORMContext с параметром
        }

        [Get("movie")]
        public IHttpResponceResult GetMovie(int id) // Получаем id из запроса
        {
            var movieDetails = _context.GetMovieDetailsById(id); // Используем переданный id
            if (movieDetails == null)
            {
                return Html("<h1>Фильм не найден</h1>");
            }

            var filePath = @"Templates/Pages/Movie/movie.html";
            var fileContent = File.ReadAllText(filePath);

            // Вставляем данные в шаблон
            fileContent = fileContent.Replace("{{title}}", movieDetails.Title)
                .Replace("{{description}}", movieDetails.Description)
                .Replace("{{release_year}}", movieDetails.ReleaseYear.ToString())
                .Replace("{{genre}}", movieDetails.Genre);

            // Форматируем список актеров
            var actorsHtml = string.Empty;
            foreach (var actor in movieDetails.Actors)
            {
                actorsHtml += $"<div>{actor.Name} - {actor.Role}</div>";
            }

            // Если актеры не найдены, можно вывести сообщение или оставить поле пустым
            if (string.IsNullOrEmpty(actorsHtml))
            {
                actorsHtml = "<p>Нет информации об актерах.</p>";
            }

            // Вставляем актеров в шаблон
            fileContent = fileContent.Replace("{{actors}}", actorsHtml);

            // Вставляем трейлер в шаблон
            string trailerHtml = string.Empty;
            if (!string.IsNullOrEmpty(movieDetails.TrailerUrl))
            {
                trailerHtml = $"<video src='{movieDetails.TrailerUrl}' autoplay muted loop></video>";
            }
            else
            {
                trailerHtml = "<p>Трейлер не найден.</p>";
            }

            // Вставляем трейлер в шаблон
            fileContent = fileContent.Replace("{{trailer_url}}", trailerHtml);

            // Логика для динамической кнопки в navbar
            var isAdmin = SessionStorage.IsAdmin(Context);
            var isAuthorized = SessionStorage.IsAuthorized(Context);

            // Заменяем кнопку в navbar в зависимости от состояния сессии
            if (isAuthorized)
            {
                if (isAdmin)
                {
                    fileContent = fileContent.Replace("{{NAVBAR_BUTTON}}", "<a href='/admin-dashboard'><button>Админ</button></a>");
                }
                else
                {
                    fileContent = fileContent.Replace("{{NAVBAR_BUTTON}}", "<a href='/profile'><button>Профиль</button></a>");
                }
            }
            else
            {
                fileContent = fileContent.Replace("{{NAVBAR_BUTTON}}", "<a href='/login'><button>Войти</button></a>");
            }

            return Html(fileContent);
        }
    }
}
