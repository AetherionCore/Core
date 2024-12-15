using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// Class which houses the build information of the currently running build of the Server.
    /// </summary>
    public static class ServerContext
    {
        public static string ExecutingDirectory => Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location
        );

        public static string BuildDateString => (
            Attribute.GetCustomAttribute(
                Assembly.GetExecutingAssembly(),
                typeof(BuildDateTimeAttribute)
            ) as BuildDateTimeAttribute
        ).Date;

        public static string GitCommitHash => GetGitCommitHash();

        private static string GetGitCommitHash()
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse HEAD",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processStartInfo))
                {
                    using (var reader = process.StandardOutput)
                    {
                        string commitHash = reader.ReadToEnd().Trim();
                        return commitHash;
                    }
                }
            }
            catch (Exception)
            {
                return "Unknown Commit";
            }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class BuildDateTimeAttribute : Attribute
    {
        public string Date { get; private set; }
        public BuildDateTimeAttribute(string date)
        {
            Date = date;
        }
    }
}