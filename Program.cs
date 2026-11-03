using DaycareAPI2026.Data;
using DaycareAPI2026.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DataStore>();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "DaycareAPI2026" }));

app.MapGet("/api/children", (DataStore store) =>
    store.Read(db => db.Children.OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToList()));
app.MapPost("/api/children", (Child item, DataStore store) =>
{
    if (string.IsNullOrWhiteSpace(item.FirstName) || string.IsNullOrWhiteSpace(item.LastName))
        return Results.BadRequest(new { error = "First and last names are required." });
    item.Id = Guid.NewGuid().ToString("N");
    return store.Write(db => { db.Children.Add(item); return Results.Created($"/api/children/{item.Id}", item); });
});
app.MapPut("/api/children/{id}", (string id, Child item, DataStore store) =>
{
    if (string.IsNullOrWhiteSpace(item.FirstName) || string.IsNullOrWhiteSpace(item.LastName))
        return Results.BadRequest(new { error = "First and last names are required." });
    return store.Write(db =>
    {
        int i = db.Children.FindIndex(x => x.Id == id);
        if (i < 0) return Results.NotFound();
        item.Id = id; db.Children[i] = item; return Results.Ok(item);
    });
});
app.MapDelete("/api/children/{id}", (string id, DataStore store) => store.Write(db =>
{
    if (db.Children.RemoveAll(x => x.Id == id) == 0) return Results.NotFound();
    db.Attendance.RemoveAll(x => x.ChildId == id);
    db.DailyCare.RemoveAll(x => x.ChildId == id);
    db.Payments.RemoveAll(x => x.ChildId == id);
    return Results.NoContent();
}));

app.MapGet("/api/staff", (DataStore store) => store.Read(db => db.Staff.ToList()));
app.MapPost("/api/staff", (StaffMember item, DataStore store) =>
{
    if (string.IsNullOrWhiteSpace(item.Name)) return Results.BadRequest(new { error = "Staff name is required." });
    item.Id = Guid.NewGuid().ToString("N");
    return store.Write(db => { db.Staff.Add(item); return Results.Created($"/api/staff/{item.Id}", item); });
});
app.MapPut("/api/staff/{id}", (string id, StaffMember item, DataStore store) =>
{
    if (string.IsNullOrWhiteSpace(item.Name)) return Results.BadRequest(new { error = "Staff name is required." });
    return store.Write(db =>
    {
        int i = db.Staff.FindIndex(x => x.Id == id);
        if (i < 0) return Results.NotFound();
        item.Id = id; db.Staff[i] = item; return Results.Ok(item);
    });
});
app.MapDelete("/api/staff/{id}", (string id, DataStore store) => store.Write(db =>
    db.Staff.RemoveAll(x => x.Id == id) > 0 ? Results.NoContent() : Results.NotFound()));

app.MapGet("/api/attendance", (string? date, DataStore store) => store.Read(db =>
    db.Attendance.Where(x => date == null || x.Date == date).ToList()));
app.MapPut("/api/attendance", (AttendanceRecord item, DataStore store) =>
{
    if (string.IsNullOrWhiteSpace(item.ChildId) || !DateOnly.TryParse(item.Date, out _) ||
        item.Status is not ("Present" or "Absent"))
        return Results.BadRequest(new { error = "Valid child, date and status are required." });
    return store.Write(db =>
    {
        if (!db.Children.Any(x => x.Id == item.ChildId)) return Results.NotFound(new { error = "Child not found." });
        var existing = db.Attendance.FirstOrDefault(x => x.ChildId == item.ChildId && x.Date == item.Date);
        if (existing != null) { existing.Status = item.Status; return Results.Ok(existing); }
        item.Id = Guid.NewGuid().ToString("N");
        db.Attendance.Add(item); return Results.Ok(item);
    });
});

app.MapDelete("/api/attendance", (string childId, string date, DataStore store) => store.Write(db =>
{
    db.Attendance.RemoveAll(x => x.ChildId == childId && x.Date == date);
    return Results.NoContent();
}));

app.MapGet("/api/daily-care", (string? childId, string? date, DataStore store) => store.Read(db =>
    db.DailyCare.Where(x => (childId == null || x.ChildId == childId) && (date == null || x.Date == date))
        .OrderByDescending(x => x.Date).ToList()));
app.MapPost("/api/daily-care", (DailyCareRecord item, DataStore store) =>
{
    if (string.IsNullOrWhiteSpace(item.ChildId) || !DateOnly.TryParse(item.Date, out _))
        return Results.BadRequest(new { error = "Child and valid date are required." });
    return store.Write(db =>
    {
        if (!db.Children.Any(x => x.Id == item.ChildId)) return Results.NotFound(new { error = "Child not found." });
        item.Id = Guid.NewGuid().ToString("N"); db.DailyCare.Add(item);
        return Results.Created($"/api/daily-care/{item.Id}", item);
    });
});
app.MapPut("/api/daily-care/{id}", (string id, DailyCareRecord item, DataStore store) =>
{
    if (string.IsNullOrWhiteSpace(item.ChildId) || !DateOnly.TryParse(item.Date, out _))
        return Results.BadRequest(new { error = "Child and valid date are required." });
    return store.Write(db =>
    {
        int i = db.DailyCare.FindIndex(x => x.Id == id);
        if (i < 0 || !db.Children.Any(x => x.Id == item.ChildId)) return Results.NotFound();
        item.Id = id; db.DailyCare[i] = item; return Results.Ok(item);
    });
});
app.MapDelete("/api/daily-care/{id}", (string id, DataStore store) => store.Write(db =>
    db.DailyCare.RemoveAll(x => x.Id == id) > 0 ? Results.NoContent() : Results.NotFound()));

app.MapGet("/api/payments", (DataStore store) => store.Read(db =>
    db.Payments.OrderByDescending(x => x.Date).ToList()));
app.MapPost("/api/payments", (Payment item, DataStore store) =>
{
    if (string.IsNullOrWhiteSpace(item.ChildId) || !DateOnly.TryParse(item.Date, out _) || item.Amount <= 0)
        return Results.BadRequest(new { error = "Child, date and positive amount are required." });
    return store.Write(db =>
    {
        if (!db.Children.Any(x => x.Id == item.ChildId)) return Results.NotFound(new { error = "Child not found." });
        item.Id = Guid.NewGuid().ToString("N"); db.Payments.Add(item);
        return Results.Created($"/api/payments/{item.Id}", item);
    });
});
app.MapDelete("/api/payments/{id}", (string id, DataStore store) => store.Write(db =>
    db.Payments.RemoveAll(x => x.Id == id) > 0 ? Results.NoContent() : Results.NotFound()));

app.MapGet("/api/dashboard", (string? date, DataStore store) => store.Read(db =>
{
    string day = date ?? DateTime.Today.ToString("yyyy-MM-dd");
    var records = db.Attendance.Where(x => x.Date == day).ToList();
    return new
    {
        totalChildren = db.Children.Count,
        male = db.Children.Count(x => x.Gender == "Male"),
        female = db.Children.Count(x => x.Gender == "Female"),
        present = records.Count(x => x.Status == "Present"),
        absent = records.Count(x => x.Status == "Absent"),
        unmarked = db.Children.Count - records.Count,
        staff = db.Staff.Count,
        payments = db.Payments.Sum(x => x.Amount)
    };
}));
app.MapGet("/api/export", (DataStore store) =>
    Results.Json(store.Read(db => db)));

app.MapFallbackToFile("index.html");

if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/etl/run", async () =>
    {
        try
        {
            string output =
                await DaycareAPI2026.ETL.DaycareETLPipeline
                    .RunAsync(app.Environment.ContentRootPath);

            string result = await File.ReadAllTextAsync(output);

            return Results.Content(result, "application/json");
        }
        catch (FileNotFoundException)
        {
            return Results.NotFound(new
            {
                error = "ETL source file not found."
            });
        }
        catch (InvalidDataException ex)
        {
            return Results.BadRequest(new
            {
                error = ex.Message
            });
        }
    });
}

app.Run();
