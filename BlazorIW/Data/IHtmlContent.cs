namespace BlazorIW.Data;

public interface IHtmlContent
{
    string Html { get; set; }
    bool IsReviewRequested { get; set; }
    bool IsPublished { get; set; }
}
