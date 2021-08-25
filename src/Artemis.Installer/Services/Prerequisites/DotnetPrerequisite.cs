using System;
using System.Threading.Tasks;
using Artemis.Installer.Utilities;
using DotNetWindowsRegistry;
using Microsoft.Win32;

namespace Artemis.Installer.Services.Prerequisites
{
    public class DotnetPrerequisite : IPrerequisite
    {
        private readonly IRegistry _registry;

        public DotnetPrerequisite(IRegistry registry)
        {
            _registry = registry;
        }
        
        protected virtual void OnDownloadProgressUpdated()
        {
            DownloadProgressUpdated?.Invoke(this, EventArgs.Empty);
        }

        public string Title => ".NET 5 runtime x64";
        public string Description => "The .NET 5 runtime is required for Artemis to run, the download is about 50 MB";
        public string DownloadUrl => "https://download.visualstudio.microsoft.com/download/pr/8bc41df1-cbb4-4da6-944f-6652378e9196/1014aacedc80bbcc030dabb168d2532f/windowsdesktop-runtime-5.0.9-win-x64.exe";

        public bool IsDownloading { get; set; }
        public bool IsInstalling { get; set; }

        public long DownloadCurrentBytes { get; private set; }
        public long DownloadTotalBytes { get; private set; }
        public float DownloadPercentage { get; private set; }

        public bool IsMet()
        {
            IRegistryKey registryKey = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            IRegistryKey key = registryKey.OpenSubKey(@"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost");
            object versionValue = key?.GetValue("Version");
            if (versionValue == null)
                return false;

            if (SemanticVersion.TryParse(versionValue.ToString(), out SemanticVersion dotnetVersion))
            {
                return dotnetVersion.Version.Major >= 5;
            }

            return false;
        }

        public async Task Install(string file)
        {
            await ProcessUtilities.RunProcessAsync(file, "-passive");
        }

        public void ReportProgress(long currentBytes, long totalBytes, float percentage)
        {
            DownloadCurrentBytes = currentBytes;
            DownloadTotalBytes = totalBytes;
            DownloadPercentage = percentage;
            OnDownloadProgressUpdated();
        }

        public event EventHandler DownloadProgressUpdated;
    }
}
