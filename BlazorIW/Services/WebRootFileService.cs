using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace BlazorIW.Services;

public class WebRootFileService(IWebHostEnvironment env)
{
    private readonly IWebHostEnvironment _env = env;

    public IEnumerable<string> GetFiles(string path = "")
    {
        foreach (var entry in _env.WebRootFileProvider.GetDirectoryContents(path))
        {
            if (entry.IsDirectory)
            {
                var subPath = Path.Combine(path, entry.Name);
                foreach (var nested in GetFiles(subPath))
                {
                    yield return nested.Replace("\\", "/");
                }
            }
            else
            {
                yield return Path.Combine(path, entry.Name).Replace("\\", "/");
            }
        }
    }
}
