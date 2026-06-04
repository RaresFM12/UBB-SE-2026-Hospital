using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        string slnDir = @"d:\facultate\UBB-cs\sem4\iss\UBB-SE-2026-Hospital\UBB-SE-2026-Hospital";
        string servicesProj = Path.Combine(slnDir, @"Hospital.Services\Hospital.Services.csproj");
        string sharedServicesDir = Path.Combine(slnDir, @"Hospital.Shared\Services");

        while (True)
        {
            Console.WriteLine("Running dotnet build...");
            var psi = new ProcessStartInfo
            {
                FileName = @"C:\Program Files\dotnet\dotnet.exe",
                Arguments = $"build \"{servicesProj}\" /nodeReuse:false",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var errorRegex = new Regex(@"^(.*\.cs)\(\d+,\d+\): error CS0535: '[^']+' does not implement interface member '([^.]+)\.([^']+)'", RegexOptions.Multiline);
            var matches = errorRegex.Matches(output);

            if (matches.Count == 0)
            {
                Console.WriteLine("No more CS0535 errors!");
                break;
            }

            Console.WriteLine($"Found {matches.Count} CS0535 errors.");
            bool fixedAny = false;

            var uniqueErrors = matches.Cast<Match>().Select(m => new {
                File = m.Groups[1].Value,
                Interface = m.Groups[2].Value,
                MemberStr = m.Groups[3].Value
            }).Distinct().ToList();

            foreach (var err in uniqueErrors)
            {
                string interfacePath = Directory.GetFiles(sharedServicesDir, $"{err.Interface}.cs", SearchOption.AllDirectories).FirstOrDefault();
                if (interfacePath == null)
                {
                    Console.WriteLine($"Could not find interface {err.Interface}.cs");
                    continue;
                }

                string interfaceContent = File.ReadAllText(interfacePath);
                string newMethod = null;

                if (err.MemberStr.Contains("("))
                {
                    string methodName = err.MemberStr.Substring(0, err.MemberStr.IndexOf("(")).Trim();
                    var sigRegex = new Regex($@"((?:[\w<>,\[\]\?]+\s+)+{Regex.Escape(methodName)}\s*\([^)]*\))\s*;");
                    var sigMatch = sigRegex.Match(interfaceContent);
                    if (sigMatch.Success)
                    {
                        newMethod = $"public {sigMatch.Groups[1].Value.Trim()} {{ throw new System.NotImplementedException(); }}";
                    }
                }
                else
                {
                    string propName = err.MemberStr.Trim();
                    var propRegex = new Regex($@"((?:[\w<>,\[\]\?]+\s+)+{Regex.Escape(propName)}\s*\{{[^}}]*get[^}}]*\}})");
                    var propMatch = propRegex.Match(interfaceContent);
                    if (propMatch.Success)
                    {
                        string sig = propMatch.Groups[1].Value.Trim();
                        // just replace { get; set; } with => throw
                        var typeRegex = new Regex($@"([\w<>,\[\]\?]+)\s+{Regex.Escape(propName)}");
                        var tMatch = typeRegex.Match(sig);
                        if (tMatch.Success)
                        {
                            newMethod = $"public {tMatch.Groups[1].Value.Trim()} {propName} => throw new System.NotImplementedException();";
                        }
                    }
                }

                if (newMethod != null)
                {
                    Console.WriteLine($"Patching {err.File} with {newMethod}");
                    var lines = File.ReadAllLines(err.File).ToList();
                    for (int i = lines.Count - 1; i >= 0; i--)
                    {
                        if (lines[i].Contains("}"))
                        {
                            lines.Insert(i, $"    {newMethod}");
                            break;
                        }
                    }
                    bool written = false;
                    for (int retry = 0; retry < 5; retry++)
                    {
                        try
                        {
                            File.WriteAllLines(err.File, lines);
                            written = true;
                            break;
                        }
                        catch (IOException)
                        {
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                    if (written) fixedAny = true;
                }
                else
                {
                    Console.WriteLine($"Could not extract signature for {err.MemberStr} in {err.Interface}.cs");
                }
            }

            if (!fixedAny)
            {
                Console.WriteLine("Failed to fix any errors in this pass. Aborting.");
                break;
            }
        }
    }

    private static bool True => true;
}
