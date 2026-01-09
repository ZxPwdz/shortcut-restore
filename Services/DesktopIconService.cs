using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using ShortcutRestore.Interop;
using ShortcutRestore.Models;

namespace ShortcutRestore.Services
{
    public class DesktopIconService
    {
        // Win32 Constants
        private const int LVM_FIRST = 0x1000;
        private const int LVM_GETITEMCOUNT = LVM_FIRST + 4;
        private const int LVM_GETITEMPOSITION = LVM_FIRST + 16;
        private const int LVM_SETITEMPOSITION = LVM_FIRST + 15;
        private const int LVM_GETITEMTEXT = LVM_FIRST + 45;
        private const int LVM_GETITEMTEXTW = LVM_FIRST + 115;

        private const int LVIF_TEXT = 0x0001;

        private const uint PROCESS_VM_OPERATION = 0x0008;
        private const uint PROCESS_VM_READ = 0x0010;
        private const uint PROCESS_VM_WRITE = 0x0020;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RELEASE = 0x8000;
        private const uint PAGE_READWRITE = 0x04;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out POINT lpBuffer, int nSize, out int lpNumberOfBytesRead);

        [StructLayout(LayoutKind.Sequential)]
        private struct LVITEM
        {
            public uint mask;
            public int iItem;
            public int iSubItem;
            public uint state;
            public uint stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
        }

        public List<IconPosition> GetAllIconPositions()
        {
            var positions = new List<IconPosition>();

            try
            {
                IntPtr desktopHandle = GetDesktopListViewHandle();
                if (desktopHandle == IntPtr.Zero)
                {
                    Debug.WriteLine("Failed to get desktop ListView handle");
                    return positions;
                }

                // Get the item count
                int itemCount = (int)SendMessage(desktopHandle, LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);
                Debug.WriteLine($"Found {itemCount} desktop icons");

                if (itemCount == 0)
                    return positions;

                // Get the process ID of the desktop
                GetWindowThreadProcessId(desktopHandle, out uint processId);

                // Open the process for memory operations
                IntPtr processHandle = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE, false, processId);
                if (processHandle == IntPtr.Zero)
                {
                    Debug.WriteLine($"Failed to open process. Error: {Marshal.GetLastWin32Error()}");
                    return positions;
                }

                try
                {
                    // Allocate memory in the target process for POINT structure
                    IntPtr pointPtr = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)Marshal.SizeOf<POINT>(), MEM_COMMIT, PAGE_READWRITE);
                    if (pointPtr == IntPtr.Zero)
                    {
                        Debug.WriteLine("Failed to allocate memory for POINT");
                        return positions;
                    }

                    // Allocate memory for LVITEM structure and text buffer
                    int lvItemSize = Marshal.SizeOf<LVITEM>();
                    int textBufferSize = 512;
                    IntPtr lvItemPtr = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)(lvItemSize + textBufferSize), MEM_COMMIT, PAGE_READWRITE);
                    if (lvItemPtr == IntPtr.Zero)
                    {
                        VirtualFreeEx(processHandle, pointPtr, 0, MEM_RELEASE);
                        Debug.WriteLine("Failed to allocate memory for LVITEM");
                        return positions;
                    }

                    IntPtr textPtr = IntPtr.Add(lvItemPtr, lvItemSize);

                    try
                    {
                        for (int i = 0; i < itemCount; i++)
                        {
                            // Get item position
                            SendMessage(desktopHandle, LVM_GETITEMPOSITION, (IntPtr)i, pointPtr);

                            // Read the position
                            byte[] pointBuffer = new byte[Marshal.SizeOf<POINT>()];
                            ReadProcessMemory(processHandle, pointPtr, pointBuffer, pointBuffer.Length, out _);

                            int x = BitConverter.ToInt32(pointBuffer, 0);
                            int y = BitConverter.ToInt32(pointBuffer, 4);

                            // Get item text
                            LVITEM lvItem = new LVITEM
                            {
                                mask = LVIF_TEXT,
                                iItem = i,
                                iSubItem = 0,
                                pszText = textPtr,
                                cchTextMax = textBufferSize / 2 // Unicode characters
                            };

                            // Write LVITEM to target process
                            byte[] lvItemBuffer = new byte[lvItemSize];
                            IntPtr lvItemLocal = Marshal.AllocHGlobal(lvItemSize);
                            try
                            {
                                Marshal.StructureToPtr(lvItem, lvItemLocal, false);
                                Marshal.Copy(lvItemLocal, lvItemBuffer, 0, lvItemSize);
                            }
                            finally
                            {
                                Marshal.FreeHGlobal(lvItemLocal);
                            }

                            WriteProcessMemory(processHandle, lvItemPtr, lvItemBuffer, lvItemSize, out _);

                            // Send message to get item text
                            SendMessage(desktopHandle, LVM_GETITEMTEXTW, (IntPtr)i, lvItemPtr);

                            // Read the text
                            byte[] textBuffer = new byte[textBufferSize];
                            ReadProcessMemory(processHandle, textPtr, textBuffer, textBufferSize, out _);

                            string name = Encoding.Unicode.GetString(textBuffer).TrimEnd('\0');
                            int nullIndex = name.IndexOf('\0');
                            if (nullIndex >= 0)
                                name = name.Substring(0, nullIndex);

                            if (!string.IsNullOrEmpty(name))
                            {
                                positions.Add(new IconPosition(name, x, y));
                                Debug.WriteLine($"Icon: {name} at ({x}, {y})");
                            }
                        }
                    }
                    finally
                    {
                        VirtualFreeEx(processHandle, pointPtr, 0, MEM_RELEASE);
                        VirtualFreeEx(processHandle, lvItemPtr, 0, MEM_RELEASE);
                    }
                }
                finally
                {
                    CloseHandle(processHandle);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting icon positions: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return positions;
        }

        public bool RestoreIconPositions(List<IconPosition> savedPositions)
        {
            if (savedPositions == null || savedPositions.Count == 0)
                return false;

            try
            {
                IntPtr desktopHandle = GetDesktopListViewHandle();
                if (desktopHandle == IntPtr.Zero)
                {
                    Debug.WriteLine("Failed to get desktop ListView handle for restore");
                    return false;
                }

                // Build lookup of current positions by getting icon names first
                var currentIcons = GetAllIconPositions();
                var savedByName = new Dictionary<string, IconPosition>(StringComparer.OrdinalIgnoreCase);
                foreach (var pos in savedPositions)
                {
                    savedByName[pos.Name] = pos;
                }

                // Get process info for setting positions
                GetWindowThreadProcessId(desktopHandle, out uint processId);
                IntPtr processHandle = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE, false, processId);
                if (processHandle == IntPtr.Zero)
                    return false;

                try
                {
                    int itemCount = (int)SendMessage(desktopHandle, LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);

                    // Allocate memory for text retrieval
                    int lvItemSize = Marshal.SizeOf<LVITEM>();
                    int textBufferSize = 512;
                    IntPtr lvItemPtr = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)(lvItemSize + textBufferSize), MEM_COMMIT, PAGE_READWRITE);
                    if (lvItemPtr == IntPtr.Zero)
                        return false;

                    IntPtr textPtr = IntPtr.Add(lvItemPtr, lvItemSize);

                    try
                    {
                        for (int i = 0; i < itemCount; i++)
                        {
                            // Get item text to match with saved positions
                            LVITEM lvItem = new LVITEM
                            {
                                mask = LVIF_TEXT,
                                iItem = i,
                                iSubItem = 0,
                                pszText = textPtr,
                                cchTextMax = textBufferSize / 2
                            };

                            byte[] lvItemBuffer = new byte[lvItemSize];
                            IntPtr lvItemLocal = Marshal.AllocHGlobal(lvItemSize);
                            try
                            {
                                Marshal.StructureToPtr(lvItem, lvItemLocal, false);
                                Marshal.Copy(lvItemLocal, lvItemBuffer, 0, lvItemSize);
                            }
                            finally
                            {
                                Marshal.FreeHGlobal(lvItemLocal);
                            }

                            WriteProcessMemory(processHandle, lvItemPtr, lvItemBuffer, lvItemSize, out _);
                            SendMessage(desktopHandle, LVM_GETITEMTEXTW, (IntPtr)i, lvItemPtr);

                            byte[] textBuffer = new byte[textBufferSize];
                            ReadProcessMemory(processHandle, textPtr, textBuffer, textBufferSize, out _);

                            string name = Encoding.Unicode.GetString(textBuffer).TrimEnd('\0');
                            int nullIndex = name.IndexOf('\0');
                            if (nullIndex >= 0)
                                name = name.Substring(0, nullIndex);

                            // If we have a saved position for this icon, restore it
                            if (savedByName.TryGetValue(name, out var savedPos))
                            {
                                // Pack x and y into lParam (MAKELPARAM)
                                IntPtr lParam = (IntPtr)((savedPos.Y << 16) | (savedPos.X & 0xFFFF));
                                SendMessage(desktopHandle, LVM_SETITEMPOSITION, (IntPtr)i, lParam);
                                Debug.WriteLine($"Restored {name} to ({savedPos.X}, {savedPos.Y})");
                            }
                        }

                        return true;
                    }
                    finally
                    {
                        VirtualFreeEx(processHandle, lvItemPtr, 0, MEM_RELEASE);
                    }
                }
                finally
                {
                    CloseHandle(processHandle);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error restoring icon positions: {ex.Message}");
                return false;
            }
        }

        private IntPtr GetDesktopListViewHandle()
        {
            // The desktop icons are in a SysListView32 control
            // The hierarchy is: Progman -> SHELLDLL_DefView -> SysListView32
            // Or: WorkerW -> SHELLDLL_DefView -> SysListView32

            IntPtr progman = FindWindow("Progman", "Program Manager");
            if (progman == IntPtr.Zero)
            {
                Debug.WriteLine("Could not find Progman window");
                return IntPtr.Zero;
            }

            // First try to find SHELLDLL_DefView under Progman
            IntPtr shellDefView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);

            if (shellDefView == IntPtr.Zero)
            {
                // If not found, it might be under a WorkerW window (when wallpaper slideshow is active)
                IntPtr workerW = IntPtr.Zero;
                do
                {
                    workerW = FindWindowEx(IntPtr.Zero, workerW, "WorkerW", null);
                    if (workerW != IntPtr.Zero)
                    {
                        shellDefView = FindWindowEx(workerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                        if (shellDefView != IntPtr.Zero)
                            break;
                    }
                } while (workerW != IntPtr.Zero);
            }

            if (shellDefView == IntPtr.Zero)
            {
                Debug.WriteLine("Could not find SHELLDLL_DefView window");
                return IntPtr.Zero;
            }

            // Find the SysListView32 control
            IntPtr listView = FindWindowEx(shellDefView, IntPtr.Zero, "SysListView32", "FolderView");
            if (listView == IntPtr.Zero)
            {
                Debug.WriteLine("Could not find SysListView32 window");
            }

            return listView;
        }
    }
}
