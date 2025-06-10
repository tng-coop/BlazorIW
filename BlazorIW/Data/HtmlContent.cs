namespace BlazorIW.Data;

public class HtmlContent : IContent
{
    public Guid Id { get; init; } = Guid.NewGuid();
}

public class HtmlContentRevision : HtmlContent, IHtmlContent
{
    public int Revision { get; set; } = 1;
    public string Html { get; set; } = string.Empty;
}
