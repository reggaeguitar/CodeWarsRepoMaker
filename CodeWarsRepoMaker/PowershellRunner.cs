using System.Collections.ObjectModel;
using System.Management.Automation;

namespace CodeWarsRepoMaker
{
    class PowershellRunner
    {
        public void RunCommandViaPS(string directory, string command)
        {
            using PowerShell powershell = PowerShell.Create();
            powershell.AddScript($"cd {directory}");
            powershell.AddScript(command);
            Collection<PSObject> results = powershell.Invoke();
        }
    }
}
