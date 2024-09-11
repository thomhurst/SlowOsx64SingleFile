using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using CliWrap;

namespace Tests.Benchmark;

[MarkdownExporterAttribute.GitHub]
public class Benchmarks
{
    private Stream _outputStream;
    
    private static readonly string Path = GetProjectPath();

    [GlobalSetup]
    public void GetOutputStream()
    {
        _outputStream = Console.OpenStandardOutput();
    }

    [GlobalCleanup]
    public async Task FlushStream()
    {
        await _outputStream.FlushAsync();
    }
    
    [Benchmark]
    public async Task SingleFile()
    {
        await Cli.Wrap(System.IO.Path.Combine(Path, "singlefile-publish", GetExecutableFileName()))
            .WithStandardOutputPipe(PipeTarget.ToStream(_outputStream))
            .ExecuteAsync();
    }

    [Benchmark]
    public async Task DotnetRun()
    {
        await Cli.Wrap("dotnet")
            .WithArguments(["run", "--no-build", "-c", "Release"])
            .WithWorkingDirectory(Path)
            .WithStandardOutputPipe(PipeTarget.ToStream(_outputStream))
            .ExecuteAsync();
    }

    private static string GetProjectPath()
    {
        var folder = new DirectoryInfo(Environment.CurrentDirectory);

        while (folder.Name != "SlowOsx64SingleFile")
        {
            folder = folder.Parent!;
        }
        
        return System.IO.Path.Combine(folder.FullName, "SlowOsx64SingleFile");
    }

    private string GetPlatformFolder()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "win-x64";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "linux-x64";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "osx-x64";
        }
        
        throw new NotImplementedException();
    }

    private string GetExecutableFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "SlowOsx64SingleFile.exe";
        }

        return "SlowOsx64SingleFile";
    }
}