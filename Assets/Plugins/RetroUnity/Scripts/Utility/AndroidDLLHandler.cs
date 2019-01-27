using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RetroUnity.Utility
{
    /// <summary>
    /// Android specific implementation for handling DLL loading.
    /// </summary>
    public sealed class AndroidDLLHandler : IDLLHandler
    {

        //POSIX standard call to load lib/DLL
        [DllImport("libdl.so", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr dlopen(string dllToLoad);

        //POSIX standard call to get proc of loaded lib/DLL
        [DllImport("libdl.so")]
        private static extern IntPtr dlsym(IntPtr hModule, string procedureName);

        //POSIX standard call to unload the lib/DLL
        [DllImport("libdl.so")]
        private static extern bool dlclose(IntPtr hModule);

        private static IntPtr _dllPointer = IntPtr.Zero;

        // Prevent warning on other platforms.
#pragma warning disable 0414
        private static readonly AndroidDLLHandler _instance = new AndroidDLLHandler();
#pragma warning restore 0414

        /// <summary>
        /// Prevent 'new' keyword.
        /// </summary>
        private AndroidDLLHandler()
        {
        }

        /// <summary>
        /// Gets the current instance (singleton).
        /// </summary>
        public static AndroidDLLHandler Instance
        {
            get
            {
#if UNITY_ANDROID
                return _instance;
#else
                Debug.LogError("This DLL handler is only compatible with Android.");
                return null;
#endif
            }
        }

        public bool LoadCore(string dllPath)
        {
            _dllPointer = dlopen(dllPath);

            if (_dllPointer == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                Debug.LogErrorFormat("Failed to load library (ErrorCode: {0})", errorCode);
                return false;
            }

            return true;
        }

        public void UnloadCore()
        {
            dlclose(_dllPointer);
        }

        public T GetMethod<T>(string functionName) where T : class
        {
            if (_dllPointer == IntPtr.Zero)
            {
                Debug.LogError("Lib not found, cannot get method '" + functionName + "'");
                return default(T);
            }

            IntPtr pAddressOfFunctionToCall = dlsym(_dllPointer, functionName);

            if (pAddressOfFunctionToCall == IntPtr.Zero)
            {
                Debug.LogError("Address for function " + functionName + " not found.");
                return default(T);
            }

            return Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(T)) as T;
        }
    }
}
