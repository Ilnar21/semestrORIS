using System.Text;
using HttpServerLibrary;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;
using MyHTTPServer.Sessions;

namespace MyHTTPServer.EndPoints
{
    public class IndexEndpoint : BaseEndPoint
    {
        [Get("index")]
        public IHttpResponceResult GetIndex()
        {
            // Подключение к базе данных
            var ormContext = new ORMContext("Host=localhost; Port=5433; Username=postgres; Password=1903; Database=postgres");

            // Получаем сериалы, фильмы, боевики, драмы и комедии
            var seriesList = ormContext.GetAllSeries();
            var moviesList = ormContext.GetAllMovies();
            var fightersList = ormContext.GetAllFighters();
            var dramasList = ormContext.GetAllDramas();
            var comediesList = ormContext.GetAllComedies();

            // Чтение шаблона HTML
            var filePath = @"Templates/Pages/Dashboard/index.html";
            var fileContent = File.ReadAllText(filePath);
            
            var isAdmin = SessionStorage.IsAdmin(Context);
            var isAuthorized = SessionStorage.IsAuthorized(Context);

            // Генерация динамических данных для сериалов
            var seriesHtml = new StringBuilder();
            foreach (var series in seriesList)
            {
                            seriesHtml.Append($@"
                <div class='series-card'>
                   <a href='http://localhost:6529/movie?id={series.Id}'>
                        <img src='{series.ImagePath}' alt='{series.Title}'/>
                    </a>
                    <div class='series-card-description'>
                        <h3>{series.Title}</h3>
                        <h6>{series.Genre} | {series.Year}</h6>
                    </div>
                </div>
                ");
            }

            seriesHtml.Append(@"
                <button class='serial-carousel-button prev'>&#10094;</button>
                <button class='serial-carousel-button next'>&#10095;</button>");

            // Генерация динамических данных для фильмов
            var moviesHtml = new StringBuilder();
            foreach (var movie in moviesList)
            {
                moviesHtml.Append($@"
    <div class='movie-card'>
        <a href='http://localhost:6529/movie?id={movie.Id}'> 
            <img src='{movie.ImagePath}' alt='{movie.Title}'/>
        </a>
        <div class='series-card-description'>
            <h3>{movie.Title}</h3>
            <h6>{movie.Genre} | {movie.Year}</h6>
        </div>
    </div>
    ");
            }

            moviesHtml.Append(@"
                <button class='movie-carousel-button prev'>&#10094;</button>
                <button class='movie-carousel-button next'>&#10095;</button>");

            // Генерация динамических данных для Fighters
            var fightersHtml = new StringBuilder();
            foreach (var fighter in fightersList)
            {
                        fightersHtml.Append($@"
                <div class='action-movie-card'>
                    <a href='http://localhost:6529/movie?id={fighter.Id}'>
                        <img src='{fighter.ImagePath}' alt='{fighter.Title}'/>
                    </a>
                    <div class='series-card-description'>
                        <h3>{fighter.Title}</h3>
                        <h6>{fighter.Genre} | {fighter.Year}</h6>
                    </div>
                </div>");
            }
            fightersHtml.Append(@"
                <button class=""action-movie-carousel-button next"">&#10095;</button>
                <button class=""action-movie-carousel-button prev"">&#10094;</button>");

            // Генерация динамических данных для Dramas
            var dramasHtml = new StringBuilder();
            foreach (var drama in dramasList)
            {
                dramasHtml.Append($@"
        <div class='drama-card'>
            <a href='http://localhost:6529/movie?id={drama.Id}'>
                <img src='{drama.ImagePath}' alt='{drama.Title}'/>
            </a>
            <div class='series-card-description'>
                <h3>{drama.Title}</h3>
                <h6>{drama.Genre} | {drama.Year}</h6>
            </div>
        </div>
        ");
            }
            dramasHtml.Append(@"
                <button class=""drama-carousel-button next"">&#10095;</button>
                <button class=""drama-carousel-button prev"">&#10094;</button>");
        
            // Генерация динамических данных для Comedies
            var comediesHtml = new StringBuilder();
            foreach (var comedy in comediesList)
            {
                comediesHtml.Append($@"
        <div class='thrillers-card'>
            <a href='http://localhost:6529/movie?id={comedy.Id}'>
                <img src='{comedy.ImagePath}' alt='{comedy.Title}'/>
            </a>
            <div class='series-card-description'>
                <h3>{comedy.Title}</h3>
                <h6>{comedy.Genre} | {comedy.Year}</h6>
            </div>
        </div>
        ");
            }
            comediesHtml.Append(@"
                <button class=""thrillers-carousel-button next"">&#10095;</button>
                <button class=""thrillers-carousel-button prev"">&#10094;</button>");

            // Вставляем динамическое содержимое в шаблон
            fileContent = fileContent.Replace("{{SERIES_CARDS}}", seriesHtml.ToString());
            fileContent = fileContent.Replace("{{MOVIES_CARDS}}", moviesHtml.ToString());
            fileContent = fileContent.Replace("{{FIGHTERS_CARDS}}", fightersHtml.ToString());
            fileContent = fileContent.Replace("{{DRAMAS_CARDS}}", dramasHtml.ToString());
            fileContent = fileContent.Replace("{{COMEDIES_CARDS}}", comediesHtml.ToString());

            // Заменяем кнопки навигации в зависимости от авторизации
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

            // Вставляем информацию о статусе администратора
            fileContent = fileContent.Replace("{{IsAdmin}}", isAdmin.ToString().ToLower());

            return Html(fileContent);
        }
    }
}
