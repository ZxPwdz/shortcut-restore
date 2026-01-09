using System;
using System.Runtime.InteropServices;

namespace ShortcutRestore.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [ComImport]
    [Guid("000214E6-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellFolder
    {
        void ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
            ref uint pchEaten, out IntPtr ppidl, ref uint pdwAttributes);
        void EnumObjects(IntPtr hwnd, uint grfFlags, out IntPtr ppenumIDList);
        void BindToObject(IntPtr pidl, IntPtr pbc, [In] ref Guid riid, out IntPtr ppv);
        void BindToStorage(IntPtr pidl, IntPtr pbc, [In] ref Guid riid, out IntPtr ppv);
        [PreserveSig]
        int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);
        void CreateViewObject(IntPtr hwndOwner, [In] ref Guid riid, out IntPtr ppv);
        void GetAttributesOf(uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref uint rgfInOut);
        void GetUIObjectOf(IntPtr hwndOwner, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl,
            [In] ref Guid riid, ref uint rgfReserved, out IntPtr ppv);
        void GetDisplayNameOf(IntPtr pidl, uint uFlags, out STRRET pName);
        void SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName,
            uint uFlags, out IntPtr ppidlOut);
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct STRRET
    {
        [FieldOffset(0)]
        public uint uType;
        [FieldOffset(4)]
        public IntPtr pOleStr;
        [FieldOffset(4)]
        public uint uOffset;
        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public byte[] cStr;
    }

    [ComImport]
    [Guid("000214F2-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumIDList
    {
        [PreserveSig]
        int Next(uint celt, out IntPtr rgelt, out uint pceltFetched);
        void Skip(uint celt);
        void Reset();
        void Clone(out IEnumIDList ppenum);
    }

    [ComImport]
    [Guid("cde725b0-ccc9-4519-917e-325d72fab4ce")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFolderView
    {
        void _OnMouse(); // OnDestroy placeholder
        void GetCurrentViewMode(out uint pViewMode);
        void SetCurrentViewMode(uint ViewMode);
        void GetFolder([In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        void Item(int iItemIndex, out IntPtr ppidl);
        void ItemCount(uint uFlags, out int pcItems);
        void Items(uint uFlags, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        void GetSelectionMarkedItem(out int piItem);
        void GetFocusedItem(out int piItem);
        void GetItemPosition(IntPtr pidl, out POINT ppt);
        void GetSpacing(out POINT ppt);
        void GetDefaultSpacing(out POINT ppt);
        void GetAutoArrange();
        void SelectItem(int iItem, uint dwFlags);
        void SelectAndPositionItems(uint cidl,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] apidl,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] POINT[] apt,
            uint dwFlags);
    }

    [ComImport]
    [Guid("1AF3A467-214F-4298-908E-06B03E0B39F9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFolderView2 : IFolderView
    {
        // IFolderView methods
        new void _OnMouse();
        new void GetCurrentViewMode(out uint pViewMode);
        new void SetCurrentViewMode(uint ViewMode);
        new void GetFolder([In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        new void Item(int iItemIndex, out IntPtr ppidl);
        new void ItemCount(uint uFlags, out int pcItems);
        new void Items(uint uFlags, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        new void GetSelectionMarkedItem(out int piItem);
        new void GetFocusedItem(out int piItem);
        new void GetItemPosition(IntPtr pidl, out POINT ppt);
        new void GetSpacing(out POINT ppt);
        new void GetDefaultSpacing(out POINT ppt);
        new void GetAutoArrange();
        new void SelectItem(int iItem, uint dwFlags);
        new void SelectAndPositionItems(uint cidl,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] apidl,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] POINT[] apt,
            uint dwFlags);

        // IFolderView2 additional methods
        void SetGroupBy(IntPtr key, bool fAscending);
        void GetGroupBy(out IntPtr key, out bool fAscending);
        void SetViewProperty(IntPtr pidl, IntPtr propkey, IntPtr propvar);
        void GetViewProperty(IntPtr pidl, IntPtr propkey, out IntPtr propvar);
        void SetTileViewProperties(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszPropList);
        void SetExtendedTileViewProperties(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszPropList);
        void SetText(int iType, [MarshalAs(UnmanagedType.LPWStr)] string pszText);
        void SetCurrentFolderFlags(uint dwMask, uint dwFlags);
        void GetCurrentFolderFlags(out uint pdwFlags);
        void GetSortColumnCount(out int pcColumns);
        void SetSortColumns(IntPtr rgSortColumns, int cColumns);
        void GetSortColumns(out IntPtr rgSortColumns, int cColumns);
        void GetItem(int iItem, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        void GetVisibleItem(int iStart, bool fPrevious, out int piItem);
        void GetSelectedItem(int iStart, out int piItem);
        void GetSelection(bool fNoneImpliesFolder, [MarshalAs(UnmanagedType.IUnknown)] out object ppsia);
        void GetSelectionState(IntPtr pidl, out uint pdwFlags);
        void InvokeVerbOnSelection([MarshalAs(UnmanagedType.LPWStr)] string pszVerb);
        void SetViewModeAndIconSize(int uViewMode, int iImageSize);
        void GetViewModeAndIconSize(out int puViewMode, out int piImageSize);
        void SetGroupSubsetCount(uint cVisibleRows);
        void GetGroupSubsetCount(out uint pcVisibleRows);
        void SetRedraw(bool fRedrawOn);
        void IsMoveInSameFolder();
        void DoRename();
    }

    [ComImport]
    [Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IServiceProvider
    {
        void QueryService([In] ref Guid guidService, [In] ref Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);
    }

    [ComImport]
    [Guid("000214E2-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellBrowser
    {
        void _QueryService(); // Placeholder for QueryService
        void _SetStatusTextSB();
        void _EnableModelessSB();
        void _TranslateAcceleratorSB();
        void _BrowseObject();
        void _GetViewStateStream();
        void _GetControlWindow();
        void _SendControlMsg();
        void QueryActiveShellView([MarshalAs(UnmanagedType.IUnknown)] out object ppshv);
        void _OnViewWindowActive();
        void _SetToolbarItems();
    }

    [ComImport]
    [Guid("85CB6900-4D95-11CF-960C-0080C7F4EE85")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IShellWindows
    {
    }

    [ComImport]
    [Guid("9BA05972-F6A8-11CF-A442-00A0C90A8F39")]
    public class ShellWindows
    {
    }

    public static class ShellConstants
    {
        public const uint SVGIO_BACKGROUND = 0x00000000;
        public const uint SVGIO_SELECTION = 0x00000001;
        public const uint SVGIO_ALLVIEW = 0x00000002;
        public const uint SVGIO_CHECKED = 0x00000003;

        public const uint SVSIF_SELECT = 0x00000001;
        public const uint SVSIF_POSITIONITEM = 0x00000080;

        public const uint SHGDN_NORMAL = 0x0000;
        public const uint SHGDN_INFOLDER = 0x0001;
        public const uint SHGDN_FOREDITING = 0x1000;
        public const uint SHGDN_FORADDRESSBAR = 0x4000;
        public const uint SHGDN_FORPARSING = 0x8000;

        public const int SWC_DESKTOP = 0x00000008;
        public const int SWFO_NEEDDISPATCH = 0x00000001;
    }

    public static class NativeMethods
    {
        [DllImport("shell32.dll")]
        public static extern int SHGetDesktopFolder(out IShellFolder ppshf);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken,
            uint dwFlags, [Out] System.Text.StringBuilder pszPath);

        [DllImport("shell32.dll")]
        public static extern int SHEmptyRecycleBin(IntPtr hwnd,
            [MarshalAs(UnmanagedType.LPWStr)] string? pszRootPath, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter,
            string lpszClass, string? lpszWindow);

        [DllImport("ole32.dll")]
        public static extern int CoCreateInstance([In] ref Guid rclsid, IntPtr pUnkOuter,
            uint dwClsContext, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrRetToBuf(ref STRRET pstr, IntPtr pidl,
            System.Text.StringBuilder pszBuf, uint cchBuf);

        public const uint SHERB_NOCONFIRMATION = 0x00000001;
        public const uint SHERB_NOPROGRESSUI = 0x00000002;
        public const uint SHERB_NOSOUND = 0x00000004;

        public const int CSIDL_DESKTOP = 0x0000;
        public const int CSIDL_RECENT = 0x0008;

        public const uint CLSCTX_LOCAL_SERVER = 0x4;
        public const uint CLSCTX_INPROC_SERVER = 0x1;
    }
}
