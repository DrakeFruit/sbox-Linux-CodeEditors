using System;
using System.IO;
using System.Text;
using Sandbox;
using Editor;

namespace Editor.CodeEditors;

[Title( "VS Code (Linux Bridge)" )]
public class VSCodeLinux : ICodeEditor
{
    private const string Bridge = "/usr/bin/flatpak-spawn";
    private const string VsCodePath = "/usr/bin/visual-studio-code-electron";

    public bool IsInstalled() => true; 

    public void OpenFile( string path, int? line = null, int? column = null )
    {
        var sln = CodeEditor.FindSolutionFromPath( System.IO.Path.GetDirectoryName( path ) ).Replace( "Z:\\", "/" ).Replace( "\\", "/" );
        var rootPath = Path.GetDirectoryName( sln ).Replace( "Z:\\", "/" ).Replace( "\\", "/" );
        path = path.Replace( "Z:\\", "/" ).Replace( "\\", "/" );

        var args = $"--host {VsCodePath} -g \"{path}:{line}:{column}\" \"{rootPath}\"";
        Launch( args );
    }

    public void OpenSolution()
    {
        var linuxPath = $"\"{Project.Current.GetRootPath()}\"".Replace( "Z:\\", "/" ).Replace( "\\", "/" );
        Launch( $"--host {VsCodePath} \"{linuxPath}\"" );
    }

    public void OpenAddon( Project addon )
    {
        var projectPath = (addon != null) ? addon.GetRootPath() : "";
        projectPath = projectPath.Replace( "Z:\\", "/" ).Replace( "\\", "/" );

		Launch( $"\"{projectPath}\"" );
    }

    private static void Launch( string arguments )
    {
        Log.Info( $"VS Code Linux: Spawning on host: {arguments}" );

        try 
        {
            // never heard of flatpak-spawn but gemini told me to and it works so fuck it
            System.Diagnostics.Process.Start( new System.Diagnostics.ProcessStartInfo
            {
                FileName = Bridge,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            } );
        }
        catch ( Exception e )
        {
            //never actually tried this, ai slop code here
            Log.Warning( $"Bridge failed: {e.Message}. Trying direct shell..." );
            System.Diagnostics.Process.Start( "/bin/sh", $"-c \"{VsCodePath} {arguments.Replace("--host " + VsCodePath, "")}\"" );
        }
    }
}