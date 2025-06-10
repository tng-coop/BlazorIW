namespace BlazorIW.Data;

public class HtmlContent : IContent
{
    public Guid Id { get; init; } = Guid.NewGuid();
}

public class HtmlContentRevision : HtmlContent, IHtmlContent
{
    public int Revision { get; set; } = 1;
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsReviewRequested { get; set; }
    public bool IsPublished { get; set; }
}
