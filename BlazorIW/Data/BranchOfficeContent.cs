namespace BlazorIW.Data;

public class BranchOfficeContent : IContent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int Revision { get; set; } = 1;
    public DateTime Date { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string TelephoneNumber { get; set; } = string.Empty;
    public string FaxNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsReviewRequested { get; set; }
    public bool IsPublished { get; set; }
}
