using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;
using Serilog;
using System.IO;

namespace PhishingPortal.OutlookAddInVSTO
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string logsDirectory = Path.Combine(workingDirectory, "Logs");

            System.IO.Directory.CreateDirectory(logsDirectory);

            // Configure Serilog to write logs to the "Logs" folder
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(System.IO.Path.Combine(logsDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Outlook Add-in started.");
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            Log.CloseAndFlush();
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion

        protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new Ribbon1();
        }
    }

}
