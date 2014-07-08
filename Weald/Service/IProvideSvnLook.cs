using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Weald.Service
{
    public interface IProvideSvnLook
    {
        int GetHeadRevision(string repoPath);
        DateTime GetRevisionTimestamp(string repoPath, int revision);
        string GetRevisionUsername(string repoPath, int revision);
    }

    public class SvnLookProvider : IProvideSvnLook
    {
        private readonly string _svnLookExePath;

        public SvnLookProvider(IProvideVisualSvnServerInfo serverInfoProvider)
        {
            _svnLookExePath = serverInfoProvider.SvnLookExePath;
        }

        public int GetHeadRevision(string repoPath)
        {
            return Convert.ToInt32(ExecuteSvnLookCommand(string.Format("youngest \"{0}\"", repoPath)));
        }

        public DateTime GetRevisionTimestamp(string repoPath, int revision)
        {
            var output = ExecuteSvnLookCommand(string.Format("date --revision {0} \"{1}\"", revision, repoPath));
            return DateTime.Parse(Regex.Replace(output, @"\s*\(.*\)", "").Trim());
        }

        public string GetRevisionUsername(string repoPath, int revision)
        {
            return ExecuteSvnLookCommand(string.Format("author --revision {0} \"{1}\"", revision, repoPath));
        }

        private string ExecuteSvnLookCommand(string commandArgs)
        {
            if (string.IsNullOrEmpty(_svnLookExePath))
            {
                throw new ArgumentException("Invalid svnlook.exe path provided!");
            }

            var lines = new List<string>();

            var startInfo = new ProcessStartInfo
                {
                    Arguments = commandArgs,
                    CreateNoWindow = true,
                    FileName = _svnLookExePath,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                };

            using (var process = new Process {StartInfo = startInfo})
            {
                process.Start();

                var stream = process.StandardOutput;
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    lines.Add(line);
                }

                stream = process.StandardError;
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    lines.Add(line);
                }

                // Totally hardcoded, arbitrary timeout value
                process.WaitForExit(5000);
            }

            return string.Join("", lines).Trim();
        }
    }
}