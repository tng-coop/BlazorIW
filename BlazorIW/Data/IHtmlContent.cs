namespace BlazorIW.Data;

public interface IHtmlContent
{
    DateTime Date { get; set; }
    string Title { get; set; }
    string Excerpt { get; set; }
    string Content { get; set; }
    bool IsReviewRequested { get; set; }
    bool IsPublished { get; set; }
}
