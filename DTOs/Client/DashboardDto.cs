namespace BankOs.DTOs.Dashboard;

public class DashboardDto
{
    public DashboardUserDto User { get; set; } = null!;
    public DashboardAccountDto? Account { get; set; }  // null si el usuario no tiene cuenta aún
    public List<DashboardActivityDto> RecentActivity { get; set; } = [];
}

public class DashboardUserDto
{
    public string Email { get; set; } = null!;
}

public class DashboardAccountDto
{
    public Guid Id { get; set; }
    public string MaskedNumber { get; set; } = null!;  // "••••4290"
    public decimal Balance { get; set; }
    public string Currency { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public class DashboardActivityDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;       // "debit" | "credit"
    public decimal Amount { get; set; }             // negativo si es débito
    public string Status { get; set; } = null!;
    public DateTime Date { get; set; }
}