using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace QuanLyNhanSu.API.Services
{
    public class AiService
    {
        private readonly ThongKeService _thongKeService;
        private readonly HttpClient _httpClient;

        public AiService(
            ThongKeService thongKeService,
            HttpClient httpClient)
        {
            _thongKeService = thongKeService;
            _httpClient = httpClient;
        }

        public async Task<string> AskAsync(string question)
        {
            // 1. Kiểm tra câu hỏi
            if (string.IsNullOrWhiteSpace(question))
            {
                return "Vui lòng nhập câu hỏi.";
            }

            // 2. Lấy Gemini API Key từ Environment
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "Chưa cấu hình GEMINI_API_KEY.";
            }

            // 3. Backend xác định tháng / năm.
            // Giữ Regex hiện tại để tránh phụ thuộc hoàn toàn vào AI.
            var thang = DateTime.Today.Month;
            var nam = DateTime.Today.Year;

            var match = Regex.Match(
                question,
                @"tháng\s*(\d{1,2})(?:\s*(?:/|năm)\s*(\d{4}))?",
                RegexOptions.IgnoreCase
            );

            if (match.Success)
            {
                thang = int.Parse(match.Groups[1].Value);

                if (match.Groups[2].Success)
                {
                    nam = int.Parse(match.Groups[2].Value);
                }
            }

            if (thang < 1 || thang > 12)
            {
                return "Tháng không hợp lệ. Vui lòng nhập tháng từ 1 đến 12.";
            }

            if (nam < 2000)
            {
                return "Năm không hợp lệ.";
            }

            // =========================================================
            // GEMINI REQUEST #1: PHÂN TÍCH Ý ĐỊNH
            // =========================================================

            var intentPrompt = $"""
            Bạn có nhiệm vụ phân tích ý định câu hỏi trong hệ thống quản lý nhân sự.

            Hãy xác định câu hỏi thuộc MỘT trong các nhóm sau:

            OVERVIEW    : tổng quan nhân sự, số lượng nhân viên, thông tin tổng hợp
            ATTENDANCE  : chấm công, ngày công, giờ làm
            LEAVE       : nghỉ phép
            PAYROLL     : lương, bảng lương, quỹ lương
            CONTRACT    : hợp đồng, hợp đồng sắp hết hạn
            DEPARTMENT  : phòng ban, nhân viên theo phòng ban
            POSITION    : chức vụ, nhân viên theo chức vụ
            STATUS      : trạng thái làm việc của nhân viên
            UNKNOWN     : không thuộc phạm vi dữ liệu trên

            Chỉ trả về đúng MỘT từ trong danh sách:
            OVERVIEW
            ATTENDANCE
            LEAVE
            PAYROLL
            CONTRACT
            DEPARTMENT
            POSITION
            STATUS
            UNKNOWN

            Không giải thích.
            Không trả JSON.
            Không thêm ký tự khác.

            Câu hỏi:
            {question}
            """;

            var intentResult = await SendToGeminiAsync(
                intentPrompt,
                apiKey
            );

            // Nếu request Gemini đầu tiên gặp lỗi thật sự thì dừng,
            // tránh tiếp tục gọi request thứ hai không cần thiết.
            if (IsGeminiError(intentResult))
            {
                return intentResult;
            }

            var intent = NormalizeIntent(intentResult);

            // =========================================================
            // 4. BACKEND QUYẾT ĐỊNH DỮ LIỆU CẦN TRUY XUẤT
            // Gemini chỉ phân tích ý định.
            // Backend mới quyết định Service nào được gọi.
            // =========================================================

            object? duLieu;

            switch (intent)
            {
                case "OVERVIEW":
                    duLieu = await _thongKeService
                        .GetOverviewAsync();
                    break;

                case "ATTENDANCE":
                    duLieu = await _thongKeService
                        .GetAttendanceByMonthAsync(thang, nam);
                    break;

                case "LEAVE":
                    duLieu = await _thongKeService
                        .GetLeaveByMonthAsync(thang, nam);
                    break;

                case "PAYROLL":
                    duLieu = await _thongKeService
                        .GetPayrollByMonthAsync(thang, nam);
                    break;

                case "CONTRACT":
                    duLieu = await _thongKeService
                        .GetExpiringContractsAsync();
                    break;

                case "DEPARTMENT":
                    duLieu = await _thongKeService
                        .GetEmployeesByDepartmentAsync();
                    break;

                case "POSITION":
                    duLieu = await _thongKeService
                        .GetEmployeesByPositionAsync();
                    break;

                case "STATUS":
                    duLieu = await _thongKeService
                        .GetEmployeesByStatusAsync();
                    break;

                default:
                    return "Câu hỏi nằm ngoài phạm vi dữ liệu nhân sự mà hệ thống hiện hỗ trợ.";
            }

            // 5. Chuyển dữ liệu Backend vừa truy xuất thành JSON
            var duLieuNhanSu = JsonSerializer.Serialize(duLieu);

            // =========================================================
            // GEMINI REQUEST #2: TẠO CÂU TRẢ LỜI
            // =========================================================

            var answerPrompt = $"""
            Bạn là trợ lý AI của hệ thống quản lý nhân sự.

            Hãy trả lời câu hỏi của người dùng dựa trên dữ liệu thực tế
            do Backend cung cấp bên dưới.

            Quy tắc bắt buộc:
            - Chỉ sử dụng dữ liệu được cung cấp.
            - Không tự bịa số liệu hoặc thông tin.
            - Nếu dữ liệu không đủ để trả lời, hãy nói rõ dữ liệu hệ thống hiện chưa đủ.
            - Không yêu cầu người dùng phải nhập đúng từ khóa.
            - Trả lời ngắn gọn, rõ ràng bằng tiếng Việt.
            - Không tự động đưa ra quyết định nhân sự, khen thưởng,
              kỷ luật hoặc đánh giá hiệu suất.
            - Không được giả định rằng bạn có quyền truy cập trực tiếp cơ sở dữ liệu.

            Ý định đã xác định:
            {intent}

            Kỳ dữ liệu:
            Tháng: {thang}
            Năm: {nam}

            Dữ liệu thực tế do Backend cung cấp:
            {duLieuNhanSu}

            Câu hỏi của người dùng:
            {question}
            """;

            return await SendToGeminiAsync(
                answerPrompt,
                apiKey
            );
        }

        // Chuẩn hóa kết quả request #1.
        // Nếu Gemini lỡ trả thêm khoảng trắng, dấu ``` hoặc chữ thường,
        // Backend vẫn cố gắng xử lý an toàn.
        private string NormalizeIntent(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return "UNKNOWN";
            }

            var value = result
                .Trim()
                .Replace("```", "")
                .Replace("\"", "")
                .Trim()
                .ToUpperInvariant();

            string[] validIntents =
            {
                "OVERVIEW",
                "ATTENDANCE",
                "LEAVE",
                "PAYROLL",
                "CONTRACT",
                "DEPARTMENT",
                "POSITION",
                "STATUS",
                "UNKNOWN"
            };

            // Trường hợp lý tưởng: Gemini trả đúng 1 từ.
            if (validIntents.Contains(value))
            {
                return value;
            }

            // Fallback nếu Gemini vẫn trả thêm một ít nội dung.
            foreach (var intent in validIntents)
            {
                if (Regex.IsMatch(
                    value,
                    $@"\b{Regex.Escape(intent)}\b",
                    RegexOptions.IgnoreCase))
                {
                    return intent;
                }
            }

            return "UNKNOWN";
        }

        // Phân biệt lỗi gọi Gemini với intent UNKNOWN.
        private bool IsGeminiError(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return true;
            }

            return result.StartsWith(
                       "Gemini hiện đã đạt giới hạn sử dụng",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   result.StartsWith(
                       "Không thể kết nối Gemini",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   result.StartsWith(
                       "Có lỗi xảy ra khi xử lý câu hỏi bằng Gemini",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   result.StartsWith(
                       "Gemini không trả về nội dung",
                       StringComparison.OrdinalIgnoreCase);
        }

        // Hàm dùng chung cho cả 2 request Gemini
        private async Task<string> SendToGeminiAsync(
            string prompt,
            string apiKey)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                },

                generationConfig = new
                {
                    temperature = 0.2
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent"
            );

            request.Headers.Add(
                "x-goog-api-key",
                apiKey
            );

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await _httpClient.SendAsync(request);

                var responseContent =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"Gemini error: {response.StatusCode} - {responseContent}"
                    );

                    if ((int)response.StatusCode == 429)
                    {
                        return "Gemini hiện đã đạt giới hạn sử dụng. Vui lòng thử lại sau.";
                    }

                    return $"Không thể kết nối Gemini. Mã lỗi: {response.StatusCode}.";
                }

                using var document =
                    JsonDocument.Parse(responseContent);

                var root = document.RootElement;

                // Kiểm tra cấu trúc response trước khi đọc
                // để tránh exception nếu Gemini trả response bất thường.
                if (!root.TryGetProperty("candidates", out var candidates)
                    || candidates.GetArrayLength() == 0)
                {
                    return "Gemini không trả về nội dung.";
                }

                var candidate = candidates[0];

                if (!candidate.TryGetProperty("content", out var content)
                    || !content.TryGetProperty("parts", out var parts)
                    || parts.GetArrayLength() == 0)
                {
                    return "Gemini không trả về nội dung.";
                }

                var part = parts[0];

                if (!part.TryGetProperty("text", out var textElement))
                {
                    return "Gemini không trả về nội dung.";
                }

                var text = textElement.GetString();

                return string.IsNullOrWhiteSpace(text)
                    ? "Gemini không trả về nội dung."
                    : text;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Gemini HTTP exception: {ex.Message}"
                );

                return "Có lỗi xảy ra khi kết nối Gemini.";
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Gemini JSON exception: {ex.Message}"
                );

                return "Có lỗi xảy ra khi xử lý phản hồi từ Gemini.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Gemini exception: {ex.Message}"
                );

                return "Có lỗi xảy ra khi xử lý câu hỏi bằng Gemini.";
            }
        }
    }
}