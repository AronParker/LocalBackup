using System;
using System.Runtime.InteropServices;
using System.Security;

namespace LocalBackup
{
    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        [Flags]
        public enum FOS : uint
        {
            OVERWRITEPROMPT = 0x2,
            STRICTFILETYPES = 0x4,
            NOCHANGEDIR = 0x8,
            PICKFOLDERS = 0x20,
            FORCEFILESYSTEM = 0x40,
            ALLNONSTORAGEITEMS = 0x80,
            NOVALIDATE = 0x100,
            ALLOWMULTISELECT = 0x200,
            PATHMUSTEXIST = 0x800,
            FILEMUSTEXIST = 0x1000,
            CREATEPROMPT = 0x2000,
            SHAREAWARE = 0x4000,
            NOREADONLYRETURN = 0x8000,
            NOTESTFILECREATE = 0x10000,
            HIDEMRUPLACES = 0x20000,
            HIDEPINNEDPLACES = 0x40000,
            NODEREFERENCELINKS = 0x100000,
            OKBUTTONNEEDSINTERACTION = 0x200000,
            DONTADDTORECENT = 0x2000000,
            FORCESHOWHIDDEN = 0x10000000,
            DEFAULTNOMINIMODE = 0x20000000,
            FORCEPREVIEWPANEON = 0x40000000,
            SUPPORTSTREAMABLEITEMS = 0x80000000
        }

        public enum SIGDN : uint
        {
            NORMALDISPLAY = 0,
            PARENTRELATIVEPARSING = 0x80018001,
            DESKTOPABSOLUTEPARSING = 0x80028000,
            PARENTRELATIVEEDITING = 0x80031001,
            DESKTOPABSOLUTEEDITING = 0x8004c000,
            FILESYSPATH = 0x80058000,
            URL = 0x80068000,
            PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
            PARENTRELATIVE = 0x80080001,
            PARENTRELATIVEFORUI = 0x80094001
        }

        [ComImport, Guid("d57c7288-d4ad-4768-be02-9d969532d960"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), CoClass(typeof(FileOpenDialog))]
        public interface IFileOpenDialog
        {
            [PreserveSig]
            int Show(IntPtr parent);
            void SetFileTypes(uint cFileTypes, IntPtr rgFilterSpec);
            void SetFileTypeIndex(uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise(IntPtr pfde, out uint pdwCookie);
            void Unadvise(uint dwCookie);
            void SetOptions(FOS fos);
            void GetOptions(out FOS pfos);
            void SetDefaultFolder(IShellItem psi);
            void SetFolder(IShellItem psi);
            void GetFolder(out IShellItem ppsi);
            void GetCurrentSelection(out IShellItem ppsi);
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult(out IShellItem ppsi);
            void AddPlace(IShellItem psi, int fdcp);
            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close(int hr);
            void SetClientGuid(IntPtr guid);
            void ClearClientData();
            void SetFilter(IntPtr pFilter);
            void GetResults(out IShellItemArray ppenum);
            void GetSelectedItems(out IShellItemArray ppsai);
        }

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellItem
        {
            void BindToHandler(IntPtr pbc, IntPtr bhid, IntPtr riid, out IntPtr ppv);
            void GetParent(out IShellItem ppsi);
            void GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(IShellItem psi, uint hint, out int piOrder);
        }

        [ComImport, Guid("B63EA76D-1F85-456F-A19C-48159EFA858B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellItemArray
        {
            void BindToHandler(IntPtr pbc, IntPtr bhid, IntPtr riid, IntPtr ppvOut);
            void GetPropertyStore(int Flags, IntPtr riid, IntPtr ppv);
            void GetPropertyDescriptionList(IntPtr keyType, IntPtr riid, IntPtr ppv);
            void GetAttributes(int AttribFlags, uint sfgaoMask, out uint psfgaoAttribs);
            void GetCount(out uint pdwNumItems);
            void GetItemAt(uint dwIndex, out IShellItem ppsi);
            void EnumItems(IntPtr ppenumShellItems);
        }

        [DllImport("Shell32.dll", ExactSpelling = true)]
        public static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath,
                                                             IntPtr pbc,
                                                             ref Guid riid,
                                                             out IShellItem ppv);

        [DllImport("UxTheme.dll", ExactSpelling = true)]
        public static extern int SetWindowTheme(IntPtr hWnd,
                                                [MarshalAs(UnmanagedType.LPWStr)] string appName,
                                                [MarshalAs(UnmanagedType.LPWStr)] string partList);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int memcmp(byte[] buf1, byte[] buf2, UIntPtr count);

        [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        public class FileOpenDialog
        {
        }
    }
}
