using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Server.Models;

namespace Server.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIService> _logger;

        public AIService(HttpClient httpClient, IConfiguration configuration, ILogger<AIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AIPromptResponse> GenerateDrawingAsync(AIPromptRequest request)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var baseEndpoint = _configuration["Gemini:Endpoint"];
            var endpoint = $"{baseEndpoint}?key={apiKey}";

            var promptText = BuildPromptContent(request);

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = promptText }
                        }
                    }
                }
            };

            try
            {
                var requestJson = JsonSerializer.Serialize(body);

                // בניית retry policy מקומית כך שיש גישה ל־logger
                var retryPolicy = Policy<HttpResponseMessage>
                    .HandleResult(r => r.StatusCode == HttpStatusCode.TooManyRequests || (int)r.StatusCode >= 500)
                    .WaitAndRetryAsync(
                        retryCount: 5,
                        sleepDurationProvider: attempt =>
                        {
                            var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
                            return TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)) + jitter;
                        },
                        onRetry: (outcome, timespan, retryAttempt, context) =>
                        {
                            _logger.LogWarning("Retry {Attempt} after {Delay} due to {StatusCode}.", retryAttempt, timespan, outcome.Result?.StatusCode);
                        });

                // ביצוע הקריאה עם retry; יוצרים HttpRequestMessage חדש בכל נסיון
                var response = await retryPolicy.ExecuteAsync(async () =>
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
                    {
                        Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                    };

                    var resp = await _httpClient.SendAsync(httpRequest);

                    if (resp.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        if (resp.Headers.TryGetValues("Retry-After", out var values) &&
                            int.TryParse(values.FirstOrDefault(), out var seconds))
                        {
                            _logger.LogWarning("Gemini returned 429. Retry-After suggested: {Seconds} seconds.", seconds);
                        }
                        else
                        {
                            _logger.LogWarning("Gemini returned 429 without Retry-After header.");
                        }
                    }

                    return resp;
                });

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini failed with {StatusCode}: {Body}", response.StatusCode, errorBody);
                    // Fallback: מחזיר ציור ריק במקום לזרוק
                    return new AIPromptResponse { DrawingJson = "[]" };
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Gemini raw response: {ResponseJson}", responseJson);

                using var doc = JsonDocument.Parse(responseJson);

                var rawDrawingText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                _logger.LogInformation("Gemini extracted text: {RawDrawingText}", rawDrawingText);

                var cleanedDrawingJson = ExtractJsonFromString(rawDrawingText);

                var drawings = JsonSerializer.Deserialize<List<Shape>>(cleanedDrawingJson);

                if (drawings == null)
                {
                    _logger.LogWarning("Deserialized shapes list was null, returning empty drawing.");
                    return new AIPromptResponse { DrawingJson = "[]" };
                }

                return new AIPromptResponse
                {
                    DrawingJson = JsonSerializer.Serialize(drawings)
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to Gemini failed: {Message}", ex.Message);
                return new AIPromptResponse { DrawingJson = "[]" };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini response: {Message}", ex.Message);
                throw new Exception("Invalid response format from Gemini.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GenerateDrawingAsync: {Message}", ex.Message);
                return new AIPromptResponse { DrawingJson = "[]" };
            }
        }

        private string BuildPromptContent(AIPromptRequest request)
        {
            var systemMessage = @"
        You are a graphic generation assistant for a canvas drawing tool.
        You must always return a single **valid JSON array** of shapes based on the user's prompt and existing shapes.

        Use **only** these shape types: circle, rectangle, line, polygon, ellipse, arc.
        For each shape, use only valid properties. Do NOT add any text or explanation. JSON only.

        --- 🎨 Drawing rules ---
        1. Preserve all existing shapes.
        2. Only add new shapes needed to satisfy the user's latest prompt.
        3. Use common sense and spatial logic: avoid overlaps, scale objects naturally, and arrange them clearly on the canvas.
        4. Use **relative positioning**: e.g. a book in hand should be next to the hand, the sun should appear in the top corner, a baby is smaller than a person, etc.
        5. Use named colors (e.g. 'yellow', 'blue', 'red') – not custom color codes.
        6. Never repeat shapes that already exist.

        --- 🧠 Semantic guidance ---
        - A flower: circle center + several surrounding petal circles (smaller).
        - A baby: small circle for head, rectangles for body/limbs, soft colors like 'pink' or 'peachpuff'.
        - A book in hand: small rectangle next to a hand or arm.
        - A sun: yellow circle, usually top-left, with optional rays.

        ⚠️ Do not generate overlapping shapes unless strictly required.
        ⚠️ Never return multiple JSON arrays. Only return ONE valid array.

        ";

            var baseInstruction = "Now return a single valid JSON array with all existing shapes (if any), plus new ones to satisfy the user's prompt.";

            if (string.IsNullOrWhiteSpace(request.ExistingDrawingsJson))
            {
                return $@"{systemMessage} Draw: {request.Prompt} {baseInstruction}";
            }

            return $@"{systemMessage} Existing shapes: {request.ExistingDrawingsJson} Now add shapes to satisfy this new instruction: {request.Prompt}
        {baseInstruction}";
        }

        
        public string FormatCleanDrawingJson(string cleanJsonString)
        {
            var shapes = JsonSerializer.Deserialize<List<Shape>>(cleanJsonString);

            if (shapes == null || shapes.Count == 0)
            {
                return "No shapes to display.";
            }

            var sb = new StringBuilder();

            foreach (var shape in shapes)
            {
                var properties = new List<string>();

                foreach (var prop in shape.GetType().GetProperties())
                {
                    var value = prop.GetValue(shape);
                    if (value != null)
                    {
                        string formattedValue;
                        if (value is List<Point> points)
                        {
                            formattedValue = JsonSerializer.Serialize(points);
                        }
                        else
                        {
                            formattedValue = value.ToString();
                        }

                        properties.Add($"{prop.Name}: {formattedValue}");
                    }
                }
                sb.AppendLine(string.Join(", ", properties));
            }

            return sb.ToString().Trim();
        }

        private string ExtractJsonFromString(string input)
        {
            int start = input.IndexOf('[');
            int end = input.LastIndexOf(']');

            if (start == -1 || end == -1 || end <= start)
                throw new Exception("לא נמצא JSON תקין בטקסט");

            var json = input.Substring(start, end - start + 1);
            return json;
        }
    }
}
