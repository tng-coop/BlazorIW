using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace BlazorIW.Services;

public record WebRootFileInfo(string Path, string Attributes);

public class WebRootFileService(IWebHostEnvironment env)
{
    private readonly IWebHostEnvironment _env = env;

    public IEnumerable<WebRootFileInfo> GetFiles(string path = "")
    {
        foreach (var entry in _env.WebRootFileProvider.GetDirectoryContents(path))
        {
            if (entry.IsDirectory)
            {
                var subPath = Path.Combine(path, entry.Name);
                foreach (var nested in GetFiles(subPath))
                {
                    yield return nested;
                }
            }
            else
            {
                var filePath = Path.Combine(path, entry.Name).Replace("\\", "/");
                var attributes = "Unavailable";
                if (!string.IsNullOrEmpty(entry.PhysicalPath))
                {
                    try
                    {
                        attributes = File.GetAttributes(entry.PhysicalPath).ToString();
                    }
                    catch
                    {
                        attributes = "Unavailable";
                    }
                }
                yield return new WebRootFileInfo(filePath, attributes);
            }
        }
    }
}
