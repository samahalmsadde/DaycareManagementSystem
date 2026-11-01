namespace DaycareAPI2026.Models;

public sealed class Child
{
    public string Id { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Dob { get; set; } = "";
    public string Gender { get; set; } = "";
    public string Group { get; set; } = "";
    public string ParentName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Allergies { get; set; } = "";
    public string Notes { get; set; } = "";
}

public sealed class StaffMember
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
}

public sealed class AttendanceRecord
{
    public string Id { get; set; } = "";
    public string ChildId { get; set; } = "";
    public string Date { get; set; } = "";
    public string Status { get; set; } = "";
}

public sealed class DailyCareRecord
{
    public string Id { get; set; } = "";
    public string ChildId { get; set; } = "";
    public string Date { get; set; } = "";
    public string Meals { get; set; } = "";
    public string Nap { get; set; } = "";
    public string Activities { get; set; } = "";
    public string Incidents { get; set; } = "";
    public string Notes { get; set; } = "";
}

public sealed class Payment
{
    public string Id { get; set; } = "";
    public string ChildId { get; set; } = "";
    public string Date { get; set; } = "";
    public decimal Amount { get; set; }
    public string Method { get; set; } = "";
    public string Notes { get; set; } = "";
}

public sealed class DaycareDatabase
{
    public List<Child> Children { get; set; } = new();
    public List<StaffMember> Staff { get; set; } = new();
    public List<AttendanceRecord> Attendance { get; set; } = new();
    public List<DailyCareRecord> DailyCare { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
}
