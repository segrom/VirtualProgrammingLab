using System.Diagnostics;
using System.Text;
using Common.QueueStructures;

namespace CsharpCompilerService;

public class CodeRunner
{
    public static CodeRunner Instance => _instance ??= new CodeRunner();
    private static CodeRunner? _instance;

    private const string Username = "runner";
    private const string Homedir = "/home/runner/";
    private const int Timeout = 5000;

    private const string SandboxTestsMock = @"
    namespace Exercise;

public class Tests
{
    // use ExerciseException
    public void Run(Solution solution)
    {
        solution.Run();
    }
}
";
    
    public CodeRunner()
    {
        var sdks = RunCommand("dotnet", $"--list-sdks");
        sdks.Wait();
        Console.WriteLine(sdks.Result.Item1);
        Console.WriteLine("_____ run user inited _____");
    }

    private async Task<(string, string)> RunCommand(string file, string args, string workdir = null, CancellationToken cancellationToken = default)
    {
        var output = new StringBuilder();
        var errors = new StringBuilder();
        Process process = null;
        Task outputTask = null, errorsTask = null;
        var cr = new CancellationTokenSource();
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

            process = new Process
            {
                StartInfo = desc,
            };

            process.Start();

            errorsTask = Task.Run(() =>
            {
                while (!cr.Token.IsCancellationRequested)
                {
                    var l = process.StandardError.ReadLine();
                    if(string.IsNullOrEmpty(l)) continue;
                    errors.AppendLine(process.StandardError.ReadLine());
                }
            }, cr.Token);
            
            outputTask = Task.Run(() =>
            {
                while (!cr.Token.IsCancellationRequested)
                {
                    var l = process.StandardOutput.ReadLine();
                    if(string.IsNullOrEmpty(l)) continue;
                    if(output.Length > 2000) return;
                    output.AppendLine(l);
                }
            }, cr.Token);

            await process.WaitForExitAsync(cancellationToken);
            cr.Cancel();
        }
        catch (OperationCanceledException canceledException)
        {
            Console.WriteLine($"[OPERATION CANCELLED] Killing process {(process?.Id.ToString() ?? "No process")}");
            process?.Kill();

            while (!process?.HasExited ?? false)
            {
                Console.WriteLine($"\twait process exit {process?.Id}");
                await Task.Yield();
            }

            cr.Cancel();
            return (output.ToString(), errors.AppendLine($"Timeout error: program running more than {Timeout}ms \n").ToString());
        }
        catch (Exception e)
        {
            LogError(e, 0);
            return (output.ToString(), errors.AppendLine(e.Message).ToString());
        }
        return (output.ToString(), errors.ToString());
    }

    private void LogError(Exception e, int deep)
    {
        Console.WriteLine($"[ERROR {deep}]: {e.Message} \n {e.StackTrace}");
        if(e.InnerException != null) LogError(e.InnerException, ++deep);
    }
    
    private async Task Timer(int timeout, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(timeout, cancellationToken);
        }
        catch (OperationCanceledException _) { }
    }
    
    public async Task<QueueCompileResult> RunCode(QueueCompileRequest request)
    {
        var binFolder = Path.Join(Homedir, "exercise/bin");
        var objFolder = Path.Join(Homedir, "exercise/obj");
        
        if(Directory.Exists(binFolder))
        {
            Directory.Delete(binFolder, true);
            Console.WriteLine($"Delete {binFolder}");
        }
        if(Directory.Exists(objFolder))
        {
            Directory.Delete(objFolder, true);
            Console.WriteLine($"Delete {objFolder}");
        }
        
        var solutionFilepath = Path.Join(Homedir, "exercise/Solution.cs");
        var testsFilepath = Path.Join(Homedir, "exercise/Tests.cs");
        var solutionPath = Path.Join(Homedir, "exercise/Exercise.csproj");
        await File.WriteAllTextAsync(solutionFilepath, request.Solution);
        await File.WriteAllTextAsync(testsFilepath, request.IsExercise 
            ? request.Tests
            : SandboxTestsMock);

        string envErrors = "";
        var ct = new CancellationTokenSource();
        
        Console.WriteLine("[START WITH RUNNER USER PROJ]");
        Console.WriteLine($"Request [{request.CompileRequestId}:{request.ServiceId}] with code: \n {request.Solution}");
        var sw = Stopwatch.StartNew();
        Task<(string, string)> solutionTask = RunCommand("unshare", $"-n runuser -u {Username} -- dotnet run --project {solutionPath}", 
            cancellationToken: ct.Token);
        Task timerTask = Timer(Timeout, ct.Token);
        
        //Console.WriteLine($"awaiting tasks solutionStatus:{solutionTask.Status} timerStatus:{timerTask.Status}");
        await Task.WhenAny(solutionTask, timerTask);
        //Console.WriteLine($"some task finished solutionStatus:{solutionTask.Status} timerStatus:{timerTask.Status}");
        
        ct.Cancel();
        var duration = sw.Elapsed;
        Console.WriteLine("[STOP PROJ]");

        var (output, errors) = solutionTask.Result;
        errors = envErrors + errors;
        
        Console.WriteLine($"duration [{duration:g}] out: {(output.Length > 1000? $"(output too long ({output.Length}))" : output)} \n err: {errors} \n");
        
        return new QueueCompileResult(request.ServiceId, request.CompileRequestId, output, errors, DateTimeOffset.Now, duration);

    }
}