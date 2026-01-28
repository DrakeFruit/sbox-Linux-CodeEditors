using System;
using System.IO;
using System.Text;
using Sandbox;
using Editor;

namespace Editor.CodeEditors;

[Title("VS Code - Linux")]
public class VSCodeLinux : ICodeEditor
{
    private string VsCodePath = "/usr/bin/code";

    public bool IsInstalled() => true;

    public void OpenFile(string path, int? line = null, int? column = null)
    {
        var sln = CodeEditor.FindSolutionFromPath(System.IO.Path.GetDirectoryName(path));
        var rootPath = ToRelativeLinuxPath(Path.GetDirectoryName(sln));
        path = ToRelativeLinuxPath(path, fixCodeCasing: true);
        var args = $"--host {VsCodePath} -g \"{path}:{line}:{column}\" \"{rootPath}\"";
        Launch(args);
    }

    public void OpenSolution()
    {
        var linuxPath = ToRelativeLinuxPath(Project.Current.GetRootPath());
        Launch($"--host {VsCodePath} \"{linuxPath}\"");
    }

    public void OpenAddon(Project addon)
    {
        var projectPath = (addon != null) ? addon.GetRootPath() : "";
        projectPath = ToRelativeLinuxPath(projectPath);

        Launch($"\"{projectPath}\"");
    }

    private static void Launch(string arguments)
    {
        // never heard of flatpak-spawn but gemini told me to and it works so fuck it
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "/usr/bin/flatpak-spawn",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true
        });
    }

    // Converts a Windows path to a relative Linux path
    private static string ToRelativeLinuxPath(string path, bool fixCodeCasing = false)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        // Convert Windows path to Linux path
        path = path.Replace("Z:\\", "/").Replace("\\", "/");

        // Fix case-sensitive directory name if needed (e.g., "code" -> "Code")
        if (fixCodeCasing)
        {
            var directory = Path.GetDirectoryName(path)?.Replace("\\", "/");
            var fileName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                directory = directory.Replace("/code/", "/Code/");
                path = directory + "/" + fileName;
            }
        }

        // Make it relative to current directory
        var currentDir = Directory.GetCurrentDirectory().Replace("\\", "/");
        if (path.StartsWith(currentDir))
        {
            path = path.Substring(currentDir.Length).TrimStart('/');
            if (string.IsNullOrEmpty(path))
                path = ".";
        }
        else if (path.StartsWith("/"))
        {
            // If it's still an absolute path, try to make it relative
            path = Path.GetRelativePath(currentDir, path).Replace("\\", "/");
        }

        return path;
    }
}
