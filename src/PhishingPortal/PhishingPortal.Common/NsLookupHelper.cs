using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Common
{
    public class NsLookupHelper : INsLookupHelper
    {
        public NsLookupHelper(ILogger<NsLookupHelper> logger)
        {
            Logger = logger;
        }

        public ILogger<NsLookupHelper> Logger { get; }

        /// <summary>
        /// Verify against Dns records
        /// </summary>
        /// <param name="type">TXT, MX, etc</param>
        /// <param name="domain">example.com</param>
        /// <param name="value">any value saved in DNS records</param>
        /// <returns></returns>
        public bool VerifyDnsRecords(string type, string domain, string value)
        {
            Logger.LogInformation($"Verifying Domain - {domain} for value {value}");


            try
            {
                using (var process = new Process())
                {
#if !Linux
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/c nslookup -type={type} {domain}"; // Note the /c command (*)
#else
                // for linux
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c \" " + command + " \"";
#endif
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();
                    //* Read the output (or the error)
                    string output = process.StandardOutput.ReadToEnd();

#if DEBUG
                    Logger.LogWarning("Domain verification being skipped due to DEBUG mode");
                    return true;
#endif

                    if (output.Contains(value))
                    {
                        Logger.LogInformation("Successfully verified");
                        return true;
                    }

                    Logger.LogInformation(output);
                    string err = process.StandardError.ReadToEnd();
                    Logger.LogError(err);
                    process.WaitForExit();

                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
            }

            return false;
        }
    }
}
