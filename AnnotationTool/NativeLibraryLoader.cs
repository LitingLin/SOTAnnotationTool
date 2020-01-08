using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AnnotationTool
{
    class NativeLibraryLoader
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryW(string librayName);
        public static void Load()
        {
            var applicationDirectoryName =
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (applicationDirectoryName == null)
                throw new ApplicationException("System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) return null");
            if (Environment.Is64BitProcess)
            {
                if (!SetDllDirectory(System.IO.Path.Combine(applicationDirectoryName, "x86-64")))
                    throw new ApplicationException("SetDllDirectory failed");
            }
            else
            {
                if (!SetDllDirectory(System.IO.Path.Combine(applicationDirectoryName, "x86")))
                    throw new ApplicationException("SetDllDirectory failed");
            }

            string path = applicationDirectoryName;

            if (Environment.Is64BitProcess)
            {
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\ucrtbase.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\vcruntime140.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-console-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-datetime-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-debug-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-errorhandling-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-file-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-file-l1-2-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-file-l2-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-handle-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-heap-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-interlocked-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-libraryloader-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-localization-l1-2-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-memory-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-namedpipe-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-processenvironment-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-processthreads-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-processthreads-l1-1-1.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-profile-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-rtlsupport-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-string-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-synch-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-synch-l1-2-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-sysinfo-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-timezone-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-core-util-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-conio-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-convert-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-environment-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-filesystem-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-heap-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-locale-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-math-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-multibyte-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-private-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-process-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-runtime-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-stdio-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-string-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-time-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\api-ms-win-crt-utility-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\msvcp120.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\msvcp140.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\msvcr120.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\tbb.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\tbbmalloc.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\boost_chrono-vc140-mt-1_56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\boost_date_time-vc140-mt-1_56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\boost_filesystem-vc140-mt-1_56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\boost_log-vc140-mt-1_56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\boost_regex-vc140-mt-1_56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\boost_serialization-vc140-mt-1_56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\boost_system-vc140-mt-1_56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\boost_thread-vc140-mt-1_56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\libexpat.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\libmat.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\libmwfl.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\libmwfoundation_usm.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\libmwi18n.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\libmwresource_core.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\libmx.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\libut.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\opencv_core341.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\opencv_imgproc341.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\zlib1.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\hdf5.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\icudt56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\icuin56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\icuio56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\icuuc56.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\kcf.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86-64\\annotation-record-operator.dll"));
            }
            else
            {
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\ucrtbase.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\vcruntime140.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-file-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-file-l1-2-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-file-l2-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-handle-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-heap-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-interlocked-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-libraryloader-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-localization-l1-2-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-memory-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-namedpipe-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-processenvironment-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-processthreads-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-processthreads-l1-1-1.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-profile-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-rtlsupport-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-string-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-synch-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-synch-l1-2-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-sysinfo-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-timezone-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-util-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\API-MS-Win-core-xstate-l2-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-conio-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-convert-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-environment-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-filesystem-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-heap-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-locale-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-math-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-multibyte-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-private-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-process-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-runtime-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-stdio-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-string-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-time-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-crt-utility-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-console-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-datetime-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-debug-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\api-ms-win-core-errorhandling-l1-1-0.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\msvcp110.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\msvcp140.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\msvcr110.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\tbb.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\tbbmalloc.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\zlib1.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\boost_date_time-vc110-mt-1_49.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\boost_filesystem-vc110-mt-1_49.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\boost_log-vc110-mt-1_49.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\boost_regex-vc110-mt-1_49.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\boost_serialization-vc110-mt-1_49.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\boost_signals-vc110-mt-1_49.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\boost_system-vc110-mt-1_49.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\boost_thread-vc110-mt-1_49.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\hdf5.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\icudt54.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\icuin54.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\icuio54.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\icuuc54.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\libexpat.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\libmat.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\libmwfl.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\libmwi18n.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\libmwresource_core.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\libmx.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\libut.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\opencv_core341.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\opencv_imgproc341.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\kcf.dll"));
                LoadLibraryW(System.IO.Path.Combine(path, ".\\x86\\annotation-record-operator.dll"));
            }
        }
    }
}
