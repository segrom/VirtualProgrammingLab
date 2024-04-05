using System.Diagnostics;
using Common.Structures;

namespace CompilerService;

public class CodeRunner
{
    public static CodeRunner Instance => _instance ??= new CodeRunner();
    private static CodeRunner? _instance;

    private const string Username = "runner";
    private const string Homedir = "/home/runner/";

    public CodeRunner()
    {
        Console.WriteLine(RunCommand("dotnet", $"--list-sdks"));
        Console.WriteLine("_____ run user inited _____");
    }

    private async Task<(string, string)> RunCommand(string file, string args, string workdir = null)
    {
        var output = "";
        var errors = "";
        try
        {
            var desc = new ProcessStartInfo
            {
                FileName = file,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (!string.IsNullOrEmpty(workdir)) desc.WorkingDirectory = workdir;

            var process = new Process
            {
                StartInfo = desc
            };

            process.Start();

            errors = await process.StandardError.ReadToEndAsync(); 
            output = await process.StandardOutput.ReadToEndAsync();
            
            await process.WaitForExitAsync();
        }
        catch (Exception e)
        {
            LogError(e, 0);
            return (output, errors + "\n" + e.Message);
        }
        return (output, errors);
    }

    private void LogError(Exception e, int deep)
    {
        Console.WriteLine($"[ERROR {deep}]: {e.Message} \n {e.StackTrace}");
        if(e.InnerException != null) LogError(e.InnerException, ++deep);
    }
    
    public async Task<CompileResult> RunCode(CompileRequest request)
    {
        var filepath = Path.Join(Homedir, "exercise/Solution.cs");
        var solutionPath = Path.Join(Homedir, "exercise/Exercise.csproj");
        await File.WriteAllTextAsync(filepath, request.Code);
        
        Console.WriteLine("[START WITH RUNNER USER PROJ]");
        var (output, errors) = await RunCommand("unshare", $"-n runuser -u {Username} -- dotnet run --project {solutionPath}");
        Console.WriteLine("[STOP PROJ]");

        return new CompileResult(request.ServiceId, request.SourceId, output, errors);

    }
}