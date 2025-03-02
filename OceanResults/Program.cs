/* 
I will receive an array [0,1,2,3,4], where each index represents an OCEAN score from -4 to 4. Let's call this array OCEAN
Indexes 0,1,2,3,4 represent traits Openness, Conscientiousness, Extraversion, Agreeableness, Neuroticism respectively
Example - OCEAN[4,-4,0,2,2] represents,
O[0] = 4 : very high openness to experience
O[1] = -4 : very low conscientiousness
O[2] = 0 : neutral extraversion
O[3] = 2 : high agreeableness
O[4] = 2 : low neuroticism
*/
/* 
query will be exactly: 
given an OCEAN personality score of [4,-4,0,2,-2], each representing the score of that respective personality from -4 to 4, give me exactly 3 strengths, 3 weakness and 3 likes (start with "you may like") that this person might have. Each one being a second-person sentence with simple english seperated by a newline \n only, no other words or newlines.
*/
using System.Text;
using System.Text.Json;

string apiKey = "AIzaSyAaMwTZV7FraeojajY6rfHnEstmXNyiFWo";

Console.WriteLine("Using Google Gemini API Key from environment variable.");

// Example OCEAN array
int[] oceanArray = { 4 , 0 , -2 , 4 , 0};

// Format query prompt
string prompt = $"given an OCEAN personality score of [{string.Join(",", oceanArray)}], each representing the score of that respective personality from -4 to 4, give me exactly 3 strengths, 3 weaknesses, and 3 likes (start with 'you may like') that this person might have. Each one being a second-person sentence with simple English separated by a newline \\n only, no other words or newlines.";

// Get API response
string response = await GetGeminiResponse(apiKey, prompt);

// Parse response by newlines
string[] parsedResponse = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

foreach (string _ in parsedResponse) Console.WriteLine(_);

// Function to send request to Google Gemini API
async Task<string> GetGeminiResponse(string apiKey, string prompt)
{
    string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

    using HttpClient client = new();

    var requestBody = new
    {
        contents = new[]
        {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
    };

    string jsonBody = JsonSerializer.Serialize(requestBody);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
    string responseString = await response.Content.ReadAsStringAsync();

    // Debugging: Print the full response
    Console.WriteLine("\n🛠️ Full API Response:");
    Console.WriteLine(responseString);

    try
    {
        using JsonDocument doc = JsonDocument.Parse(responseString);

        // Extract "candidates" array
        if (doc.RootElement.TryGetProperty("candidates", out JsonElement candidates) &&
            candidates.GetArrayLength() > 0)
        {
            // Extract "content" object
            if (candidates[0].TryGetProperty("content", out JsonElement contentElement))
            {
                // Extract "parts" array inside "content"
                if (contentElement.TryGetProperty("parts", out JsonElement parts) &&
                    parts.GetArrayLength() > 0 &&
                    parts[0].TryGetProperty("text", out JsonElement textElement))
                {
                    return textElement.GetString() ?? "Error: Empty response from API.";
                }
            }
        }

        return "Error: Unexpected response structure from API.";
    }
    catch (JsonException)
    {
        return "Error: Failed to parse API response.";
    }
}