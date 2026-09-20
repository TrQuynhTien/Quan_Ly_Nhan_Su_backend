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

            // 2. Lấy Gemini API Key
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "Chưa cấu hình GEMINI_API_KEY.";
            }

            // 3. Xác định tháng / năm nếu người dùng có nhập
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

            // 4. Backend lấy gói dữ liệu nhân sự
            // Không cần Gemini phân loại intent nữa

            var overview = await _thongKeService
                .GetOverviewAsync();

            var chamCong = await _thongKeService
                .GetAttendanceByMonthAsync(thang, nam);

            var nghiPhep = await _thongKeService
                .GetLeaveByMonthAsync(thang, nam);

            var quyLuong = await _thongKeService
                .GetPayrollByMonthAsync(thang, nam);

            var hopDong = await _thongKeService
                .GetExpiringContractsAsync();

            var phongBan = await _thongKeService
                .GetEmployeesByDepartmentAsync();

            var chucVu = await _thongKeService
                .GetEmployeesByPositionAsync();

            var trangThai = await _thongKeService
                .GetEmployeesByStatusAsync();

            var duLieu = new
            {
                KyThongKe = new
                {
                    Thang = thang,
                    Nam = nam
                },

                TongQuan = overview,
                ChamCong = chamCong,
                NghiPhep = nghiPhep,
                QuyLuong = quyLuong,
                HopDongSapHetHan = hopDong,
                NhanVienTheoPhongBan = phongBan,
                NhanVienTheoChucVu = chucVu,
                NhanVienTheoTrangThai = trangThai
            };

            // 5. Chuyển dữ liệu thành JSON
            var duLieuNhanSu = JsonSerializer.Serialize(duLieu);

            // 6. Gemini tự đọc câu hỏi + dữ liệu và trả lời
            var prompt = $"""
            Bạn là trợ lý AI của hệ thống quản lý nhân sự.

            Người dùng có thể đặt câu hỏi tự do bằng ngôn ngữ tự nhiên.
            Hãy tự hiểu ý nghĩa của câu hỏi và trả lời dựa trên dữ liệu hệ thống được cung cấp.

            Quy tắc bắt buộc:
            - Chỉ sử dụng dữ liệu được cung cấp bên dưới.
            - Không tự bịa số liệu hoặc thông tin.
            - Nếu dữ liệu không đủ để trả lời, hãy nói rõ rằng dữ liệu hệ thống hiện chưa đủ.
            - Không yêu cầu người dùng phải nhập đúng từ khóa.
            - Có thể hiểu các cách diễn đạt tương đương về nhân sự, nhân viên, phòng ban,
              chức vụ, chấm công, nghỉ phép, lương, hợp đồng và trạng thái làm việc.
            - Nếu câu hỏi không liên quan đến dữ liệu nhân sự mà hệ thống hỗ trợ,
              hãy trả lời rằng câu hỏi nằm ngoài phạm vi hỗ trợ của hệ thống.
            - Trả lời ngắn gọn, rõ ràng bằng tiếng Việt.
            - Không tự động đưa ra quyết định nhân sự, khen thưởng, kỷ luật hoặc đánh giá hiệu suất.

            Kỳ dữ liệu đang được cung cấp:
            Tháng: {thang}
            Năm: {nam}

            Dữ liệu hệ thống:
            {duLieuNhanSu}

            Câu hỏi của người dùng:
            {question}
            """;

            return await SendToGeminiAsync(prompt, apiKey);
        }

        // Chỉ còn 1 lần gọi Gemini cho mỗi câu hỏi
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

            request.Headers.Add("x-goog-api-key", apiKey);

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

                var text = document.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "Gemini không trả về nội dung.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gemini exception: {ex.Message}");

                return "Có lỗi xảy ra khi xử lý câu hỏi bằng Gemini.";
            }
        }
    }
}