
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var assignments = new List<Assignment>
{
    new(1, "Bài tập RESTful API", "Xây dựng API quản lý bài tập", true),
    new(2, "Bài tập C# nâng cao", "Thực hành lập trình C#", true),
    new(3, "Bài tập đã đóng", "Bài tập không còn nhận bài", false)
};

var submissions = new List<Submission>();

app.MapGet("/", () => "EduSubmit RESTful API is running!");

app.MapGet("/api/assignments", () =>
{
    return Results.Ok(assignments);
});

app.MapGet("/api/assignments/{assignmentId:int}", (int assignmentId) =>
{
    var assignment = assignments.FirstOrDefault(a => a.Id == assignmentId);

    if (assignment == null)
        return Results.NotFound(new { message = "Không tìm thấy bài tập." });

    return Results.Ok(assignment);
});

app.MapPost("/api/assignments/{assignmentId:int}/submissions",
    (int assignmentId, CreateSubmissionRequest request) =>
{
    var assignment = assignments.FirstOrDefault(a => a.Id == assignmentId);

    if (assignment == null)
        return Results.NotFound(new { message = "Bài tập không tồn tại." });

    if (!assignment.IsOpen)
        return Results.BadRequest(new { message = "Bài tập đã đóng." });

    if (string.IsNullOrWhiteSpace(request.StudentCode) ||
        string.IsNullOrWhiteSpace(request.RepositoryUrl))
    {
        return Results.BadRequest(new { message = "Mã sinh viên và Repository URL không được để trống." });
    }

    var submission = new Submission(
        Guid.NewGuid(),
        assignmentId,
        request.StudentCode,
        request.RepositoryUrl,
        DateTime.UtcNow
    );

    submissions.Add(submission);

    return Results.Created(
        $"/api/submissions/{submission.Id}",
        submission
    );
});

app.MapGet("/api/submissions/{submissionId:guid}", (Guid submissionId) =>
{
    var submission = submissions.FirstOrDefault(s => s.Id == submissionId);

    if (submission == null)
        return Results.NotFound(new { message = "Không tìm thấy bài nộp." });

    return Results.Ok(submission);
});

app.Run();

record Assignment(
    int Id,
    string Title,
    string Description,
    bool IsOpen
);

record CreateSubmissionRequest(
    string StudentCode,
    string RepositoryUrl
);

record Submission(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl,
    DateTime SubmittedAt
);
