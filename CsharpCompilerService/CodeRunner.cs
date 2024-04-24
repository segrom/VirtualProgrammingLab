using System.Diagnostics;
using Common.QueueStructures;

namespace CsharpCompilerService;

public class CodeRunner
{
    public static CodeRunner Instance => _instance ??= new CodeRunner();
    private static CodeRunner? _instance;

    private const string Username = "runner";
    private const string Homedir = "/home/runner/";
    private const int Timeout = 10000;

    public CodeRunner()
    {
        Console.WriteLine(RunCommand("dotnet", $"--list-sdks"));
        Console.WriteLine("_____ run user inited _____");
    }

    private async Task<(string, string)> RunCommand(string file, string args, string workdir = null, CancellationToken cancellationToken = default)
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
                StartInfo = desc,
            };

            process.Start();

            errors = await process.StandardError.ReadToEndAsync(); 
            output = await process.StandardOutput.ReadToEndAsync();
            
            await process.WaitForExitAsync(cancellationToken);
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
    
    private async Task Timer(int timeout, CancellationToken cancellationToken = default) => await Task.Delay(timeout, cancellationToken);
    
    public async Task<QueueCompileResult> RunCode(QueueCompileRequest request)
    {
        var solutionFilepath = Path.Join(Homedir, "exercise/Solution.cs");
        var testsFilepath = Path.Join(Homedir, "exercise/Tests.cs");
        var solutionPath = Path.Join(Homedir, "exercise/Exercise.csproj");
        await File.WriteAllTextAsync(solutionFilepath, request.Solution);
        await File.WriteAllTextAsync(testsFilepath, request.Tests);

        string envErrors = "";
        var ct = new CancellationTokenSource();
        
        Console.WriteLine("[START WITH RUNNER USER PROJ]");
        var sw = Stopwatch.StartNew();
        Task<(string, string)> solutionTask = RunCommand("unshare", $"-n runuser -u {Username} -- dotnet run --project {solutionPath}", 
            cancellationToken: ct.Token);
        var timerTask = Timer(Timeout, ct.Token);

        await Task.WhenAny(solutionTask, timerTask);
        
        
        if (solutionTask.Status == TaskStatus.Running)
        {
            envErrors = $"Timeout error: program running more than {Timeout}ms \n";
            Console.WriteLine("[TIMEOUT]");
        }
        ct.Cancel();
        var duration = sw.Elapsed;
        Console.WriteLine("[STOP PROJ]");

        var (output, errors) = solutionTask.Result;
        errors = envErrors + errors;
        
        return new QueueCompileResult(request.ServiceId, request.CompileRequestId, output, errors, DateTimeOffset.Now, duration);

    }
}