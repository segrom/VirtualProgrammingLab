using System.Diagnostics;

namespace CompilerService;

public class RunUser
{
    public static RunUser? Instance;

    private const string Username = "runner";
    private const string Homedir = "/home/runner/";
    
    public RunUser()
    {
        if(Instance != null) return;
        
            Console.WriteLine(RunCommand("adduser", $"{Username} --disabled-password --gecos \"\""));
            Console.WriteLine(RunCommand("apt-get", $"update"));
            Console.WriteLine(RunCommand("apt-get", $"install wget unzip zip -y"));
            Console.WriteLine(RunCommand("wget", $"https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb", Homedir));
            Console.WriteLine(RunCommand("dpkg", $" -i packages-microsoft-prod.deb", Homedir));
            Console.WriteLine(RunCommand("rm", $"packages-microsoft-prod.deb", Homedir));
            Console.WriteLine(RunCommand("apt-get", $"update"));
            Console.WriteLine(RunCommand("apt-get", $"install -y dotnet-sdk-8.0"));

        
        Instance = this;
    }

    private string RunCommand(string file, string args, string workdir = null)
    {
        string output = "";
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = file,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (!string.IsNullOrEmpty(workdir)) psi.WorkingDirectory = workdir;

            Process proc = new Process
            {
                StartInfo = psi
            };

            proc.Start();

            string error = proc.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(error))
                throw new Exception("error: " + error);

            output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return output;
    }

    public void RunCode(string code)
    {
        var path = Path.Join(Homedir, "Solution.cs");
        File.WriteAllText(path, code);
        Console.WriteLine(RunCommand("dotnet build", $" {path} -o {Path.Join(Homedir, "build")}", Homedir));
    }
}