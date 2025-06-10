using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace BlazorIW.Services;

public record WebRootFileInfo(string Path, string Owner, string Permissions);

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
                var owner = "Unavailable";
                var permissions = "Unavailable";
                if (!string.IsNullOrEmpty(entry.PhysicalPath))
                {
                    try
                    {
                        var mode = File.GetUnixFileMode(entry.PhysicalPath);
                        permissions = UnixModeToString(mode);
                    }
                    catch
                    {
                        permissions = "Unavailable";
                    }
                }
                yield return new WebRootFileInfo(filePath, owner, permissions);
            }
        }
    }

    private static string UnixModeToString(UnixFileMode mode)
    {
        Span<char> chars = stackalloc char[9];
        chars[0] = (mode & UnixFileMode.UserRead) != 0 ? 'r' : '-';
        chars[1] = (mode & UnixFileMode.UserWrite) != 0 ? 'w' : '-';
        chars[2] = (mode & UnixFileMode.UserExecute) != 0 ? 'x' : '-';
        chars[3] = (mode & UnixFileMode.GroupRead) != 0 ? 'r' : '-';
        chars[4] = (mode & UnixFileMode.GroupWrite) != 0 ? 'w' : '-';
        chars[5] = (mode & UnixFileMode.GroupExecute) != 0 ? 'x' : '-';
        chars[6] = (mode & UnixFileMode.OtherRead) != 0 ? 'r' : '-';
        chars[7] = (mode & UnixFileMode.OtherWrite) != 0 ? 'w' : '-';
        chars[8] = (mode & UnixFileMode.OtherExecute) != 0 ? 'x' : '-';
        return new string(chars);
    }
}
