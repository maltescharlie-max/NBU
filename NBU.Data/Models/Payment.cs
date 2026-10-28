namespace NBU.Data.Models;

public sealed class Payment
{
    public int Id { get; set; }
    public int ScholarshipApplicationId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly DepositDate { get; set; }
    public string Period { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";

    public ScholarshipApplication ScholarshipApplication { get; set; } = null!;
}
