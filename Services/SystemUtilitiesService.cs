using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ShortcutRestore.Interop;

namespace ShortcutRestore.Services
{
    public class SystemUtilitiesService
    {
        /// <summary>
        /// Clears the DNS resolver cache using ipconfig /flushdns
        /// </summary>
        public async Task<(bool Success, string Message)> ClearDnsCacheAsync()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/flushdns",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    return (false, "Failed to start ipconfig process");
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    return (true, "DNS cache cleared successfully");
                }
                else
                {
                    return (false, string.IsNullOrEmpty(error) ? output : error);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Empties the Windows Recycle Bin
        /// </summary>
        public Task<(bool Success, string Message)> EmptyRecycleBinAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    uint flags = NativeMethods.SHERB_NOCONFIRMATION |
                                 NativeMethods.SHERB_NOPROGRESSUI |
                                 NativeMethods.SHERB_NOSOUND;

                    int result = NativeMethods.SHEmptyRecycleBin(IntPtr.Zero, null, flags);

                    // S_OK = 0, S_FALSE = 1 (empty recycle bin)
                    if (result == 0 || result == 1)
                    {
                        return (true, "Recycle Bin emptied successfully");
                    }
                    else
                    {
                        // -2147418113 = 0x8000FFFF = E_UNEXPECTED (often means already empty)
                        if (result == -2147418113)
                        {
                            return (true, "Recycle Bin is already empty");
                        }
                        return (false, $"Failed with error code: {result}");
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"Error: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Clears Windows Recent Files/Most Recently Used list
        /// </summary>
        public Task<(bool Success, string Message)> ClearRecentFilesAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // Get Recent folder path
                    string recentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Recent);

                    int deletedCount = 0;
                    int errorCount = 0;

                    if (Directory.Exists(recentFolder))
                    {
                        // Delete all files in Recent folder
                        foreach (var file in Directory.GetFiles(recentFolder))
                        {
                            try
                            {
                                File.Delete(file);
                                deletedCount++;
                            }
                            catch
                            {
                                errorCount++;
                            }
                        }

                        // Also clear subdirectories (AutomaticDestinations, CustomDestinations)
                        foreach (var dir in Directory.GetDirectories(recentFolder))
                        {
                            try
                            {
                                foreach (var file in Directory.GetFiles(dir))
                                {
                                    try
                                    {
                                        File.Delete(file);
                                        deletedCount++;
                                    }
                                    catch
                                    {
                                        errorCount++;
                                    }
                                }
                            }
                            catch
                            {
                                errorCount++;
                            }
                        }
                    }

                    if (deletedCount > 0 || errorCount == 0)
                    {
                        return (true, $"Cleared {deletedCount} recent items");
                    }
                    else
                    {
                        return (false, $"Failed to clear some items ({errorCount} errors)");
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"Error: {ex.Message}");
                }
            });
        }
    }
}
