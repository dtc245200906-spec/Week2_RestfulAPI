
using System.Net;
using System.Net.Http.Json;

string baseUrl = args.Length > 0
    ? args[0]
    : "http://localhost:5000";

using var client = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};

Console.WriteLine("===== EDUSUBMIT CLIENT =====");

// 1. GET danh sách bài tập
Console.WriteLine("\n1. DANH SACH BAI TAP");

var assignments = await client.GetFromJsonAsync<List<AssignmentDto>>(
    "/api/assignments");

if (assignments != null)
{
    foreach (var assignment in assignments)
    {
        Console.WriteLine($"{assignment.Id}. {assignment.Title}");
    }
}

// 2. GET chi tiết bài tập
Console.WriteLine("\n2. CHI TIET BAI TAP");

var assignmentDetail = await client.GetFromJsonAsync<AssignmentDto>(
    "/api/assignments/1");

if (assignmentDetail != null)
{
    Console.WriteLine($"Ten: {assignmentDetail.Title}");
    Console.WriteLine($"Mo ta: {assignmentDetail.Description}");
    Console.WriteLine($"Trang thai: {assignmentDetail.IsOpen}");
}

// 3. POST nộp bài
Console.WriteLine("\n3. NOP BAI TAP");

var request = new CreateSubmissionRequest(
    "SV001",
    "https://github.com/student/baitap"
);

var response = await client.PostAsJsonAsync(
    "/api/assignments/1/submissions",
    request
);

Console.WriteLine($"HTTP Status: {(int)response.StatusCode} {response.StatusCode}");

if (response.StatusCode == HttpStatusCode.Created)
{
    var submission = await response.Content.ReadFromJsonAsync<SubmissionDto>();

    Console.WriteLine("Nop bai thanh cong!");

    // 4. GET thông tin bài đã nộp
    Console.WriteLine("\n4. THONG TIN BAI DA NOP");

    if (submission != null)
    {
        var savedSubmission = await client.GetFromJsonAsync<SubmissionDto>(
            $"/api/submissions/{submission.Id}");

        Console.WriteLine($"Ma bai nop: {savedSubmission?.Id}");
        Console.WriteLine($"Ma sinh vien: {savedSubmission?.StudentCode}");
        Console.WriteLine($"Repository: {savedSubmission?.RepositoryUrl}");
    }
}
else
{
    var error = await response.Content.ReadFromJsonAsync<ApiError>();
    Console.WriteLine($"Loi: {error?.Message}");
}

// 5. Kiểm tra lỗi 400
Console.WriteLine("\n5. KIEM TRA DU LIEU KHONG HOP LE");

var invalidRequest = new CreateSubmissionRequest("", "");

var errorResponse = await client.PostAsJsonAsync(
    "/api/assignments/1/submissions",
    invalidRequest
);

Console.WriteLine($"HTTP Status: {(int)errorResponse.StatusCode} {errorResponse.StatusCode}");

if (errorResponse.StatusCode == HttpStatusCode.BadRequest)
{
    var error = await errorResponse.Content.ReadFromJsonAsync<ApiError>();
    Console.WriteLine($"Thong bao: {error?.Message}");
}

// 6. Kiểm tra bài tập không tồn tại (404)
Console.WriteLine("\n6. KIEM TRA BAI TAP KHONG TON TAI");

var notFoundResponse = await client.GetAsync("/api/assignments/999");

Console.WriteLine($"HTTP Status: {(int)notFoundResponse.StatusCode} {notFoundResponse.StatusCode}");

if (notFoundResponse.StatusCode == HttpStatusCode.NotFound)
{
    Console.WriteLine("Khong tim thay bai tap - 404 NotFound");
}
Console.WriteLine("\n===== KET THUC =====");

public record AssignmentDto(
    int Id,
    string Title,
    string Description,
    bool IsOpen
);

public record CreateSubmissionRequest(
    string StudentCode,
    string RepositoryUrl
);

public record SubmissionDto(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl,
    DateTime SubmittedAt
);

public class ApiError
{
    public string? Message { get; set; }
}
