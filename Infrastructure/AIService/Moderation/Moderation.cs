

using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using EduShpere.Shared;
using Microsoft.Extensions.Configuration;

namespace EduShpere.Infrastructure.AIService
{
    public class Moderation
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        public Moderation(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
            _apiKey = _config["OpenAi:ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
                return;
                //throw new Exception("❌ Không tìm thấy gpt:ApiKey trong appsettings.json");
        }
        public async Task<ModerationResult> Moderate(ModerationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Input))
                throw new BadRequestException("❌ Nội dung kiểm duyệt không được để trống.");

            // Prompt kiểm duyệt và gợi ý chỉnh sửa
            string prompt = $$""""
Bạn là một hệ thống kiểm duyệt nội dung tiếng Việt.

Nhiệm vụ của bạn gồm 2 bước:

1. **Kiểm duyệt nội dung**:
   - Nếu văn bản dưới đây vi phạm các tiêu chí sau: **tục tĩu**, **bạo lực**, **quấy rối**, **phân biệt đối xử**, **khiêu dâm**, **thù ghét**, **đe dọa**, **xuyên tạc sự thật**, **vi phạm thuần phong mỹ tục**,**Sử dụng những từ như "Con và thằng"** — thì đánh dấu là `true`, ngược lại là `false`.

2. **Gợi ý chỉnh sửa**:
   - Nếu nội dung bị gắn cờ (vi phạm), hãy **đề xuất một phiên bản chỉnh sửa phù hợp hơn** về mặt ngôn từ để có thể đăng tải công khai**.
   - Nếu không vi phạm, hãy trả lời `"null"` trong phần đề xuất.
  

Kết quả trả về phải đúng định dạng JSON:

```json
{
  "flagged": [true|false],
  "suggestion": "[nội dung đã chỉnh sửa nếu flagged=true, hoặc null nếu flagged=false]"
}
    ```

    Văn bản cần kiểm duyệt:
    """{{request.Input}}"""
"""";

            var body = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            requestMessage.Content = content;
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(requestMessage);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException($"❌ OpenAI API lỗi {response.StatusCode}: {responseJson}");
            }

            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var contentString = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                using var resultJson = JsonDocument.Parse(contentString);
                var flagged = resultJson.RootElement.GetProperty("flagged").GetBoolean();
                var suggestion = resultJson.RootElement.GetProperty("suggestion").GetString();

                return (new ModerationResult
                {
                    IsFlagged = flagged,
                    Violations = flagged ? new List<string> { "Vi phạm nội dung" } : new List<string>(),
                    
                });
            }
            catch (Exception ex)
            {
                 throw new BadRequestException($"❌ Lỗi xử lý kết quả từ OpenAI: {ex.Message}");
            }
        }
    }
    public class ModerationRequest
    {
        public string Input { get; set; }
    }
    public class ModerationResult
    {
        public bool IsFlagged { get; set; }
        public List<string> Violations { get; set; }
    }
}
