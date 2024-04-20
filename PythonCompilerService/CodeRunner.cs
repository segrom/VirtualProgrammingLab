using System.Diagnostics;
using Common.QueueStructures;

namespace PythonCompilerService;

public class CodeRunner
{
    public static CodeRunner Instance => _instance ??= new CodeRunner();
    private static CodeRunner? _instance;

    private const string Username = "runner";
    private const string Homedir = "/home/runner/";

    public CodeRunner()
    {
        Console.WriteLine(RunCommand("python3", $"--version"));
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
    
    public async Task<QueueCompileResult> RunCode(QueueCompileRequest request)
    {
        var solutionFilepath = Path.Join(Homedir, "exercise/solution.py");
        var testsFilepath = Path.Join(Homedir, "exercise/tests.py");
        var mainFilepath = Path.Join(Homedir, "exercise/main.py");
        await File.WriteAllTextAsync(solutionFilepath, request.Solution);
        await File.WriteAllTextAsync(testsFilepath, request.Tests);
        
        Console.WriteLine("[START WITH RUNNER USER PROJ]");
        var sw = Stopwatch.StartNew();
        var (output, errors) = await RunCommand("unshare", $"-n runuser -u {Username} -- python3 {mainFilepath}");
        var duration = sw.Elapsed;
        Console.WriteLine("[STOP PROJ]");

        return new QueueCompileResult(request.ServiceId, request.CompileRequestId, output, errors, DateTimeOffset.Now, duration);

    }
}