using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PathfinderBuildTasks
{
    public class MoveDir : Task, ICancelableTask
    {
        private bool _canceling;

        [Required]
        public ITaskItem[] SourceDirectories { get; set; }

        public ITaskItem DestinationFolder { get; set; }

        public ITaskItem[] DestinationDirectories { get; set; }

        public bool OverwriteReadOnlyFiles { get; set; }

        public bool UseSymlinkOrJunction { get; set; }

        [Output]
        public ITaskItem[] MovedDirectories { get; set; }

        public void Cancel()
        {
            _canceling = true;
        }

        public override bool Execute()
        {
            bool success = true;

            if (SourceDirectories == null || SourceDirectories.Length == 0)
            {
                SourceDirectories = new ITaskItem[0];
                return true;
            }

            if (DestinationDirectories == null && DestinationFolder == null)
            {
                Log.LogError($"${nameof(SourceDirectories)} needs a {nameof(DestinationDirectories)} or {nameof(DestinationFolder)}");
                return false;
            }

            if (DestinationDirectories != null && DestinationDirectories.Length != SourceDirectories.Length)
            {
                Log.LogErrorWithCodeFromResources("General.TwoVectorsMustHaveSameLength", DestinationDirectories.Length, SourceDirectories.Length, nameof(DestinationDirectories), nameof(SourceDirectories));
                return false;
            }

            if (DestinationDirectories == null)
            {
                DestinationDirectories = new ITaskItem[SourceDirectories.Length];

                for (int i = 0; i < SourceDirectories.Length; ++i)
                {
                    // Build the correct path.
                    string destinationDir;
                    try
                    {
                        destinationDir = Path.Combine(DestinationFolder.ItemSpec, Path.GetFileName(SourceDirectories[i].ItemSpec));
                    }
                    catch (ArgumentException e)
                    {
                        Log.LogError($"Could not move {SourceDirectories[i].ItemSpec} to {DestinationFolder.ItemSpec}: {e.Message}");

                        // Clear the outputs.
                        DestinationDirectories = new ITaskItem[0];
                        return false;
                    }

                    // Initialize the DestinationFolder item.
                    DestinationDirectories[i] = new TaskItem(destinationDir);
                }
            }

            // Build up the sucessfully moved subset
            var DestinationDirectoriesSuccessfullyMoved = new List<ITaskItem>();

            // Now that we have a list of DestinationFolder files, move from source to DestinationFolder.
            for (int i = 0; i < SourceDirectories.Length && !_canceling; ++i)
            {
                string sourceDir = SourceDirectories[i].ItemSpec;
                string destinationDir = DestinationDirectories[i].ItemSpec;

                try
                {
                    if (UseSymlinkOrJunction
                        ? CreateSymlinkOrJunctionWithLogging(sourceDir, destinationDir)
                        : MoveDirectoryWithLogging(sourceDir, destinationDir))
                    {
                        SourceDirectories[i].CopyMetadataTo(DestinationDirectories[i]);
                        DestinationDirectoriesSuccessfullyMoved.Add(DestinationDirectories[i]);
                    }
                    else
                    {
                        success = false;
                    }
                }
                catch (Exception e) when (
                    e is UnauthorizedAccessException
                        || e is NotSupportedException
                        || (e is ArgumentException && !(e is ArgumentNullException))
                        || e is SecurityException
                        || e is IOException)
                {
                    Log.LogError($"Could not move {sourceDir} to {destinationDir}: {e.Message}");
                    success = false;

                    // Continue with the rest of the list
                }
            }

            MovedDirectories = DestinationDirectoriesSuccessfullyMoved.ToArray();

            return success && !_canceling;
        }

        private static void MakeWriteableIfReadOnly(string directory)
        {
            var info = new DirectoryInfo(directory);
            if ((info.Attributes & FileAttributes.ReadOnly) != 0)
            {
                info.Attributes &= ~FileAttributes.ReadOnly;
            }
        }

        private bool MoveDirectoryWithLogging(
            string sourceDirectory,
            string destinationDirectory
        )
        {
            if (Directory.Exists(destinationDirectory))
            {
                Log.LogError($"Destination {destinationDirectory} already exists, could not move {sourceDirectory}");
                return false;
            }

            // Check the source exists.
            if (!Directory.Exists(sourceDirectory))
            {
                Log.LogError($"Source {sourceDirectory} does not exist");
                return false;
            }

            if (OverwriteReadOnlyFiles && File.Exists(destinationDirectory))
            {
                MakeWriteableIfReadOnly(destinationDirectory);
            }

            string destinationFolder = Path.GetDirectoryName(destinationDirectory);

            if (!string.IsNullOrEmpty(destinationFolder) && !Directory.Exists(destinationFolder))
            {
                Log.LogMessage(MessageImportance.Normal, $"Creating directory {destinationFolder}");
                Directory.CreateDirectory(destinationFolder);
            }

            // Do not log a fake command line as well, as it's superfluous, and also potentially expensive
            Log.LogMessage(MessageImportance.Normal, $"Moving directory {sourceDirectory} to {destinationDirectory}");

            Directory.Move(sourceDirectory, destinationDirectory);
            var result = Directory.Exists(destinationDirectory);

            if (!result)
            {
                // It failed so we need a nice error message. Unfortunately
                // Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error()); and
                // throw new IOException((new Win32Exception(error)).Message)
                // do not produce great error messages (eg., "The operation succeeded" (!)).
                // For this reason the BCL has is own mapping in System.IO.__Error.WinIOError
                // which is unfortunately internal.
                // So try to get a nice message by using the BCL Move(), which will likely fail
                // and throw. Otherwise use the "correct" method.

                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            if (Directory.Exists(destinationDirectory))
            {
                // Make it writable
                MakeWriteableIfReadOnly(destinationDirectory);
            }

            return true;
        }

        private bool CreateSymlinkOrJunctionWithLogging(
            string sourceDirectory,
            string destinationDirectory
        )
        {
            if (Directory.Exists(destinationDirectory))
            {
                Log.LogError($"Destination {destinationDirectory} already exists, could not link {sourceDirectory}");
                return false;
            }

            // Check the source exists.
            if (!Directory.Exists(sourceDirectory))
            {
                Log.LogError($"Source {sourceDirectory} does not exist");
                return false;
            }

            if (OverwriteReadOnlyFiles && File.Exists(destinationDirectory))
            {
                MakeWriteableIfReadOnly(destinationDirectory);
            }

            string destinationFolder = Path.GetDirectoryName(destinationDirectory);

            if (!string.IsNullOrEmpty(destinationFolder) && !Directory.Exists(destinationFolder))
            {
                Log.LogMessage(MessageImportance.Normal, $"Creating directory {destinationFolder}");
                Directory.CreateDirectory(destinationFolder);
            }

            // Do not log a fake command line as well, as it's superfluous, and also potentially expensive
            Log.LogMessage(MessageImportance.Normal, $"Linking directory {sourceDirectory} to {destinationDirectory}");

            CreateSymlinkOrJunction(sourceDirectory, destinationDirectory);
            // Directory.Move(sourceDirectory, destinationDirectory);
            var result = Directory.Exists(destinationDirectory);

            // if (!result)
            // {
            //     // It failed so we need a nice error message. Unfortunately
            //     // Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error()); and
            //     // throw new IOException((new Win32Exception(error)).Message)
            //     // do not produce great error messages (eg., "The operation succeeded" (!)).
            //     // For this reason the BCL has is own mapping in System.IO.__Error.WinIOError
            //     // which is unfortunately internal.
            //     // So try to get a nice message by using the BCL Move(), which will likely fail
            //     // and throw. Otherwise use the "correct" method.

            //     Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            // }

            if (Directory.Exists(destinationDirectory))
            {
                // Make it writable
                MakeWriteableIfReadOnly(destinationDirectory);
            }

            return true;
        }

        private void CreateSymlinkOrJunction(string source, string dest)
        {
            ProcessStartInfo info = new ProcessStartInfo { UseShellExecute = false };
            switch(GetCurrentPlatform(out var osType))
            {
                case Platform.Linux:
                case Platform.MacOs:
                    info.FileName = "ln";
                    info.Arguments = $"-srd \"{source}\" \"{dest}\"";
                    break;
                case Platform.Windows:
                    info.FileName = "mklink";
                    info.Arguments = $"/J \"{source}\" \"{dest}\"";
                    break;
                default: throw new PlatformNotSupportedException(osType);
            }
            var process = new Process() { StartInfo = info };
            process.Start();
            process.WaitForExit();
        }

        public enum Platform
        {
            Unknown,
            Windows,
            Linux,
            MacOs
        }

        private static Platform GetCurrentPlatform() => GetCurrentPlatform(out var osType);

        private static Platform GetCurrentPlatform(out string osType)
        {
            osType = "";
            string windir = Environment.GetEnvironmentVariable("windir");
            if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
            {
                return Platform.Windows;
            }
            else if (File.Exists(@"/proc/sys/kernel/ostype"))
            {
                osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
                if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
                {
                    // Note: Android gets here too
                    return Platform.Linux;
                }
                else
                {
                    return Platform.Unknown;
                }
            }
            else if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
            {
                // Note: iOS gets here too
                return Platform.MacOs;
            }
            else
            {
                return Platform.Unknown;
            }
        }
    }
}