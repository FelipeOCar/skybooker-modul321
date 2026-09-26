using Microsoft.Extensions.Configuration;

namespace SkyBooker.Shared;

/// <summary>Loads generated local credentials without relying on the shell environment of Visual Studio.</summary>
public static class LocalConfiguration
{
    public static void Load(IConfigurationBuilder builder)
    {
        foreach (string start in new[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
        {
            for (DirectoryInfo? directory = new(start); directory is not null; directory = directory.Parent)
            {
                if (!File.Exists(Path.Combine(directory.FullName, "SkyBooker.sln"))) continue;
                string path = Path.Combine(directory.FullName, "skybooker.local.json");
                if (File.Exists(path)) builder.AddJsonFile(path, optional: false, reloadOnChange: false);
                // Docker, CI and explicit environment variables take precedence over the file.
                builder.AddEnvironmentVariables();
                return;
            }
        }
        builder.AddEnvironmentVariables();
    }
}
