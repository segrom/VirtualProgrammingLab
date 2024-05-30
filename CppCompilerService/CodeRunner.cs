using System.Diagnostics;
using System.Text;
using Common.QueueStructures;

namespace PythonCompilerService;

public class CodeRunner
{
    public static CodeRunner Instance => _instance ??= new CodeRunner();
    private static CodeRunner? _instance;

    private const string Username = "runner";
    private const string Homedir = "/home/runner/";
    private const int Timeout = 5000;

    private const string SandboxTestsMock = @"#include ""Tests.h""
#include ""ExerciseException.h""

void Tests::Run(Solution* solution) {
    solution->Run();
}
";
    
    public CodeRunner()
    {
        var pyv = RunCommand("g++", $"--version");
        pyv.Wait();
        Console.WriteLine(pyv.Result.Item1);
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
                StartInfo = desc
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
        var solutionFilepath = Path.Join(Homedir, "exercise/Solution.h");
        var testsFilepath = Path.Join(Homedir, "exercise/Tests.cpp");
        var mainFilepath = Path.Join(Homedir, "exercise/main.cpp");
        var executableFilepath = Path.Join(Homedir, "exercise/solution");
        await File.WriteAllTextAsync(solutionFilepath, request.Solution);
        await File.WriteAllTextAsync(testsFilepath, request.IsExercise
            ? request.Tests
            : SandboxTestsMock);
        
        string envErrors = "";
        var ct = new CancellationTokenSource();
        
        Console.WriteLine($"Request [{request.CompileRequestId}:{request.ServiceId}] with code: \n {request.Solution}");

        Console.WriteLine("[START BUILD]");
        
        Task<(string, string)> buildTask = RunCommand("g++", $"{mainFilepath} -o {executableFilepath}",
            cancellationToken: ct.Token);
        await buildTask;
        var (buildOutput, buildErrors) = buildTask.Result;
        Console.WriteLine(buildOutput);
        if (!string.IsNullOrEmpty(buildErrors))
        {
            Console.WriteLine(buildErrors);
            return new QueueCompileResult(request.ServiceId, request.CompileRequestId, buildOutput, buildErrors, DateTimeOffset.Now, new TimeSpan(0));
        }
        
        Console.WriteLine("[BUILD COMPLETED]");
        
        Console.WriteLine("[START WITH RUNNER USER PROJ]");
        var sw = Stopwatch.StartNew();
        Task<(string, string)> solutionTask = RunCommand("unshare", $"-n runuser -u {Username} -- {executableFilepath}",
            cancellationToken: ct.Token);
        var timerTask = Timer(Timeout, ct.Token);
        
        await Task.WhenAny(solutionTask, timerTask);
        
        ct.Cancel();
        var duration = sw.Elapsed;
        Console.WriteLine("[STOP PROJ]");

        var (output, errors) = solutionTask.Result;
        errors = envErrors + errors;
        Console.WriteLine($"duration [{duration:g}] out: {(output.Length > 1000? $"(output too long ({output.Length}))" : output)} \n err: {errors} \n");
        
        return new QueueCompileResult(request.ServiceId, request.CompileRequestId, output, errors, DateTimeOffset.Now, duration);

    }
}