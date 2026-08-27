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
            if (string.IsNullOrWhiteSpace(question))
            {
                return "Vui lòng nhập câu hỏi.";
            }
            var apiKey = Environment.GetEnvironmentVariable(
                "GEMINI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "Chưa cấu hình GEMINI_API_KEY.";
            }

            var cauHoi = question.ToLower();

            var thang = DateTime.Today.Month;
            var nam = DateTime.Today.Year;

            var match = Regex.Match(
                question,
                @"tháng\s*(\d{1,2})(?:\s*/\s*(\d{4}))?",
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

            object duLieu;

            if (cauHoi.Contains("chấm công") ||
                cauHoi.Contains("đi trễ") ||
                cauHoi.Contains("về sớm"))
            {
                duLieu = await _thongKeService
                    .GetAttendanceByMonthAsync(thang, nam);
            }
            else if (cauHoi.Contains("nghỉ phép") ||
                    cauHoi.Contains("đơn nghỉ"))
            {
                duLieu = await _thongKeService
                    .GetLeaveByMonthAsync(thang, nam);
            }
            else if (cauHoi.Contains("lương") ||
                    cauHoi.Contains("quỹ lương"))
            {
                duLieu = await _thongKeService
                    .GetPayrollByMonthAsync(thang, nam);
            }
            else if (cauHoi.Contains("hợp đồng") &&
                    (cauHoi.Contains("hết hạn") ||
                    cauHoi.Contains("sắp hết")))
            {
                duLieu = await _thongKeService
                    .GetExpiringContractsAsync();
            }
            else if (cauHoi.Contains("phòng ban"))
            {
                duLieu = await _thongKeService
                    .GetEmployeesByDepartmentAsync();
            }
            else if (cauHoi.Contains("chức vụ"))
            {
                duLieu = await _thongKeService
                    .GetEmployeesByPositionAsync();
            }
            else if (cauHoi.Contains("trạng thái"))
            {
                duLieu = await _thongKeService
                    .GetEmployeesByStatusAsync();
            }
            else if (cauHoi.Contains("báo cáo") ||
                    cauHoi.Contains("tổng hợp") ||
                    cauHoi.Contains("tình hình nhân sự"))
            {
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

                duLieu = new
                {
                    KyThongKe = new
                    {
                        Thang = thang,
                        Nam = nam
                    },
                    Overview = overview,
                    ChamCong = chamCong,
                    NghiPhep = nghiPhep,
                    QuyLuong = quyLuong,
                    HopDongSapHetHan = hopDong
                };
            }
            else
            {
                return "Tôi chưa xác định được nội dung cần tra cứu. "
                    + "Bạn có thể hỏi về nhân viên, phòng ban, chức vụ, "
                    + "chấm công, nghỉ phép, lương, hợp đồng hoặc báo cáo nhân sự.";
            }

            var duLieuNhanSu = JsonSerializer.Serialize(duLieu);

            var prompt = $"""
            Bạn là trợ lý AI của hệ thống quản lý nhân sự.

            Chỉ trả lời dựa trên dữ liệu hệ thống được cung cấp.
            Không tự bịa dữ liệu.
            Nếu dữ liệu không đủ, hãy nói rõ chưa đủ dữ liệu.
            Trả lời ngắn gọn, dễ hiểu bằng tiếng Việt.
            Nếu câu hỏi liên quan đến tháng/năm,
            hãy sử dụng đúng kỳ thống kê được cung cấp.

            Dữ liệu hệ thống:
            {duLieuNhanSu}

            Câu hỏi người dùng:
            {question}
            """;

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
                Console.WriteLine(ex.Message);
                return "Có lỗi xảy ra khi xử lý câu hỏi bằng Gemini.";
            }
        }
    }
}