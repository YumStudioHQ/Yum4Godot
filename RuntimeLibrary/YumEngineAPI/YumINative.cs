using System;
using System.Runtime.InteropServices;

namespace Yum4Godot.RuntimeLibrary.YumEngineAPI
{
    internal static class Native
    {
#if WINDOWS
        private const string DllName = "yum.dll";
#elif LINUX
        private const string DllName = "libyum.so";
#elif OSX
        private const string DllName = "libyum.dylib";
#else
        private const string DllName = "yum"; // fallback
#endif

        // -------- Variant --------
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr YumVariant_new();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YumVariant_delete(IntPtr var);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YumVariant_setInt(IntPtr var, long v);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YumVariant_setFloat(IntPtr var, double v);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YumVariant_setBool(IntPtr var, int v);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YumVariant_setString(IntPtr var,
            [MarshalAs(UnmanagedType.LPStr)] string str);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern long YumVariant_asInt(IntPtr var);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double YumVariant_asFloat(IntPtr var);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumVariant_asBool(IntPtr var);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr YumVariant_asString(IntPtr var);

        public static string YumVariant_asStringSafe(IntPtr var) =>
            Marshal.PtrToStringAnsi(YumVariant_asString(var)) ?? string.Empty;

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumVariant_isInt(IntPtr var);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumVariant_isFloat(IntPtr var);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumVariant_isBool(IntPtr var);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumVariant_isString(IntPtr var);

        // -------- Vector --------
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr YumVector_new();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YumVector_delete(IntPtr vec);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YumVector_append(IntPtr vec, IntPtr value);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YumVector_pop(IntPtr vec);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YumVector_clear(IntPtr vec);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern long YumVector_size(IntPtr vec);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr YumVector_at(IntPtr vec, long index);

        // -------- Subsystem --------
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr YumSubsystem_new();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YumSubsystem_delete(IntPtr subsystem);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong YumSubsystem_newState(IntPtr subsystem, int lstdlibs);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YumSubsystem_deleteState(IntPtr subsystem, ulong uid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumSubsystem_isValidUID(IntPtr subsystem, ulong uid);

        // -------- LuaSubsystem --------
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumLuaSubsystem_load(
            IntPtr subsystem, ulong uid,
            [MarshalAs(UnmanagedType.LPStr)] string src,
            [MarshalAs(UnmanagedType.I1)] bool isFile);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool YumLuaSubsystem_good(IntPtr subsystem, ulong uid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr YumLuaSubsystem_call(
            IntPtr subsystem, ulong uid,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            IntPtr args);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void YumCallback(IntPtr inVec, IntPtr outVec);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumLuaSubsystem_pushCallback(
            IntPtr subsystem,
            ulong uid,
            string name,
            YumCallback cb,
            string ns);



        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr YumEngineInfo_studioName();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr YumEngineInfo_name();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr YumEngineInfo_branch();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumEngineInfo_versionMajor();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumEngineInfo_versionMinor();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumEngineInfo_versionPatch();
        public static string SafeIt(IntPtr i) => Marshal.PtrToStringAnsi(i) ?? string.Empty;

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int YumLuaSubsystem_hasMethod(IntPtr s, ulong uid, string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong YumEngineInfo_longVersion();
    }

}