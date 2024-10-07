using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static readonly string token = "7602812752:AAEqoyJQ2w7A7BgFmyBdtAtDQA6edZ99P7M";
    private static readonly TelegramBotClient botClient = new TelegramBotClient(token);
    private static CancellationTokenSource cts;

    static async Task Main()
    {
        cts = new CancellationTokenSource();
        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: cts.Token);
        Console.WriteLine("Bot is running... Press Enter to exit.");
        Console.ReadLine();
        cts.Cancel();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine("Received an update.");

        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message.Text != null)
        {
            Console.WriteLine($"Received message: {update.Message.Text}");

            if (update.Message.Text.ToLower() == "/start")
            {
                await SendMainMenu(update.Message.Chat.Id, cancellationToken);
            }
            else if (update.Message.Text == "Погода")
            {
                string moscowWeather = await GetWeatherAsync("Moscow");
                string surgutWeather = await GetWeatherAsync("Surgut");
                string response = $"{moscowWeather}\n\n{surgutWeather}";
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, response, cancellationToken: cancellationToken);
            }
            else if (update.Message.Text == "Шутка")
            {
                string joke = await GetJokeAsync();
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, joke, cancellationToken: cancellationToken);
            }
            else if (update.Message.Text.ToLower() == "/weather_surgut")
            {
                string weather = await GetWeatherAsync("Surgut");
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, weather, cancellationToken: cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Неизвестная команда. Попробуйте /start.", cancellationToken: cancellationToken);
            }
        }
        else
        {
            Console.WriteLine("Received an update that is not a message.");
        }
    }

    private static async Task SendMainMenu(long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Погода", "Шутка" }
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(chatId, "Выберите опцию:", replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }

    private static async Task<string> GetWeatherAsync(string city)
    {
        using (HttpClient client = new HttpClient())
        {
            string apiKey = "072c9a46407d1d3fa2b5ed42496a46d8";
            string url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            try
            {
                var response = await client.GetStringAsync(url);
                dynamic weatherData = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                string temperature = weatherData.main.temp;
                string description = weatherData.weather[0].description;

                return $"Температура в {city}: {temperature}°C, {description}.";
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return "Ошибка при получении данных о погоде.";
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                return $"Произошла ошибка: {e.Message}";
            }
        }
    }

    private static async Task<string> GetJokeAsync()
    {
        using (HttpClient client = new HttpClient())
        {
            string url = "https://v2.jokeapi.dev/joke/Any";

            try
            {
                var response = await client.GetStringAsync(url);
                dynamic jokeData = Newtonsoft.Json.JsonConvert.DeserializeObject(response);

                if (jokeData.type == "single")
                {
                    return jokeData.joke;
                }
                else
                {
                    return $"{jokeData.setup}\n{jokeData.delivery}";
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return "Ошибка при получении шутки.";
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                return $"Произошла ошибка: {e.Message}";
            }
        }
    }
}
