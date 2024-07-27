using System.Diagnostics;

namespace SCICore.util;

public static class ArchiveUtils
{
    public static string? RarPath { get; set; }


    private static Process CreateProcess(string filepath, string args)
    {
        var processStartInfo = new ProcessStartInfo()
        {
            FileName = filepath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var process = new Process();
        process.StartInfo = processStartInfo;
        return process;
    }

    private static async Task<(bool, string, string, Exception?)> RunProcess(Process process)
    {
        string output = null!, error = null!;
        try
        {
            process.Start();

            // Read the output asynchronously
            output = await process.StandardOutput.ReadToEndAsync();
            error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            return (process.ExitCode == 0, output, error, null);
        }
        catch (Exception ex)
        {
            return (false, output, error, ex);
        }
    }

    public static async Task<bool> CompressRar(
        List<string> sources,
        string target,
        string pwd = "",
        int compressRate = 3,
        int? recoveryRate = 10,
        bool solidArchive = false,
        string dictSize = "32"
    )
    {
        if (!File.Exists(RarPath))
        {
            throw new FileNotFoundException($"'{RarPath}' is not a valid path for rar");
        }

        if (File.Exists(target) || Directory.Exists(target))
        {
            throw new Exception($"target already existed: {target}");
        }

        if (target.Contains('"'))
        {
            throw new Exception("No \" in target");
        }


        var filesStr = "";
        foreach (var f in sources)
        {
            var file = f.Trim();
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"File does not exist: {file}");
            }

            filesStr += $" \"{file}\" ";
        }

        pwd = pwd.Trim();
        if (pwd != "")
        {
            if (pwd.Length > 126)
            {
                throw new Exception("len(pwd) <= 126");
            }

            if (pwd.Contains('"'))
            {
                throw new Exception("No \" in pwd");
            }

            // -hp Encrypt file and file name
            pwd = $"-hp\"{pwd}\"";
        }

        if (!(compressRate is >= 0 and <= 5))
        {
            throw new Exception("compressRate should be between 0 and 5");
        }

        var recoveryRateStr = "";
        if (recoveryRate.HasValue)
        {
            if (!(recoveryRate.Value >= 1))
            {
                throw new Exception("recoveryRate should be greater than 0");
            }

            recoveryRateStr = $"-rr{recoveryRate}%";
        }

        // Solid archive
        var solidArchiveStr = solidArchive ? "-s" : "";

        // Dictionary size
        dictSize = $"-md{dictSize}";

        // -o- Skip existing files. If output_path already exists, the return code of the program is 10
        // -ep1 Preserve the path of the compressed folder
        // -r0 Recursively compress folders, but do not recurse into the directory when specifying a file
        var argument =
            $" a {solidArchiveStr} {dictSize} -ep1 -o- -m{compressRate} {pwd} {recoveryRateStr} -r0 \"{target}\" {filesStr}";


        using var process = CreateProcess(RarPath, argument);
        var (res, output, error, exception) = await RunProcess(process);
        return res;
    }


    public static async Task<bool> ExtractRar(string archive, string outputDir, string pwd="")
    {
        if (!File.Exists(RarPath))
        {
            throw new FileNotFoundException($"'{RarPath}' is not a valid path for rar");
        }

        if (!File.Exists(archive))
        {
            throw new FileNotFoundException($"'{RarPath}' is not a valid path for an archive");
        }

        outputDir = outputDir.Trim();

        // dir path must be ended with \
        if (!outputDir.EndsWith("\\"))
        {
            outputDir += "\\";
        }
        
        pwd = pwd.Trim();
        if (pwd != "")
        {
            if (pwd.Length > 126)
            {
                throw new Exception("len(pwd) <= 126");
            }

            if (pwd.Contains('"'))
            {
                throw new Exception("No \" in pwd");
            }

            // -hp Encrypt file and file name
            pwd = $"-hp\"{pwd}\"";
        }

        var argument = $" x {pwd} \"{archive}\" \"{outputDir}\"";

        using var process = CreateProcess(RarPath, argument);
        var (res, output, error, exception) = await RunProcess(process);
        return res;
    }
}