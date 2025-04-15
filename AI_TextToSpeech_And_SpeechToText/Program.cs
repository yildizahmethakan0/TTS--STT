using AI_TextToSpeech_And_SpeechToText_Example;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("HuggingFaceAPI", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://api-inference.huggingface.co");
    httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {builder.Configuration["HuggingFace:APIKey"]}");
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    }));

builder.Services.AddDirectoryBrowser();

var app = builder.Build();
app.UseCors();

#region index.html - style.css
app.UseDefaultFiles();
app.UseStaticFiles();
#endregion

#region wwwroot/audio_outputs
string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio_outputs");
if (!Directory.Exists(webRootPath))
    Directory.CreateDirectory(webRootPath);

IFileProvider fileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.WebRootPath, "audio_outputs"));

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = fileProvider,
    RequestPath = "/audio_outputs"
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = fileProvider,
    RequestPath = "/audio_outputs"
});
#endregion


app.MapPost("/text-to-speech", async (IHttpClientFactory httpClientFactory, HttpContext httpContext) =>
{
    try
    {
        HttpClient httpClient = httpClientFactory.CreateClient("HuggingFaceAPI");

        using StreamReader streamReader = new StreamReader(httpContext.Request.Body);
        string requestBody = await streamReader.ReadToEndAsync();
        TextToSpeechRequest request = JsonSerializer.Deserialize<TextToSpeechRequest>(requestBody)!;

        var requestData = new
        {
            inputs = request.Text
        };

        StringContent stringContent = new(
            JsonSerializer.Serialize(requestData),
            Encoding.UTF8,
            MediaTypeNames.Application.Json
            );

        Console.WriteLine("Ses oluşturuluyor...");

        HttpResponseMessage response = await httpClient.PostAsync("models/facebook/mms-tts-tur", stringContent);
        if (response.IsSuccessStatusCode)
        {
            byte[] audioBytes = await response.Content.ReadAsByteArrayAsync();
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio_outputs", $"output_{DateTime.Now:yyyyMMddHHmmss}.wav");

            //Ses dosyas� kaydediliyor
            File.WriteAllBytes(outputPath, audioBytes);
            httpContext.Response.ContentType = "audio/wav";
            await httpContext.Response.Body.WriteAsync(audioBytes);
        }
        else
        {
            string error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Hatasi\t: {response.StatusCode}");
            Console.WriteLine($"Hata Detayi\t: {error}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Hata olu�tu\t: {ex.Message}");
    }
});

app.MapPost("/speech-to-text", async (IHttpClientFactory httpClientFactory, HttpContext httpContext) =>
{
    try
    {
        IFormCollection form = await httpContext.Request.ReadFormAsync();
        IFormFile? file = form.Files.GetFile("audio");

        if (file is { Length: 0 } or null)
        {
            httpContext.Response.StatusCode = 400;
            await httpContext.Response.WriteAsJsonAsync(new { Error = "Ses dosyasi bulunamadi" });
            return;
        }

        using MemoryStream memoryStream = new();
        await file.CopyToAsync(memoryStream);
        byte[] audioBytes = memoryStream.ToArray();

        HttpClient httpClient = httpClientFactory.CreateClient("HuggingFaceAPI");

        ByteArrayContent byteArrayContent = new(audioBytes);
        byteArrayContent.Headers.Add(HeaderNames.ContentType, "audio/wav");

        //Whisper small model
        HttpResponseMessage response = await httpClient.PostAsync("models/openai/whisper-large-v3-turbo", byteArrayContent);
        string responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"API Yan�t�\t: {responseContent}");
        if (!response.IsSuccessStatusCode)
        {
            httpContext.Response.StatusCode = (int)response.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(new { Error = $"API Hatas�\t:{responseContent}" });
            return;
        }

        //Whisper model format
        Dictionary<string, string>? result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
        string? text = result?.GetValueOrDefault("text", "");

        if (string.IsNullOrEmpty(text))
        {
            httpContext.Response.StatusCode = 500;
            await httpContext.Response.WriteAsJsonAsync(new { Error = "Metin �evrilemedi!" });
            return;
        }

        await httpContext.Response.WriteAsJsonAsync(new { Text = text });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Hata\t: {ex.Message}");
        httpContext.Response.StatusCode = 500;
        await httpContext.Response.WriteAsJsonAsync(new { Error = $"Sunucu hatasi\t: {ex.Message}" });
    }
})
    .DisableAntiforgery();

app.Run();
