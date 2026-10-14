using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PA
{
    internal class LibraryLoader
    {
        // Forzamos a Windows a buscar DLLs nativas en nuestra carpeta temporal personalizada
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        private static string _TempRouteApp = "";

        public static void Load()
        {
            // 1. Crear el entorno aislado en %TEMP%
            string tempFolder = Path.GetTempPath();
            _TempRouteApp = Path.Combine(tempFolder, "convex_runtime_V2");

            if (!Directory.Exists(_TempRouteApp))
            {
                Directory.CreateDirectory(_TempRouteApp);
            }

            // Apuntar el buscador de Windows a nuestra carpeta temporal
            SetDllDirectory(_TempRouteApp);

            // ========================================================
            // DETECCIÓN EN CALIENTE DE LA ARQUITECTURA (x86 vs x64)
            // ========================================================
            bool is64Bit = Environment.Is64BitProcess;

            // 2. SELECCIÓN Y CONFIGURACIÓN DE PC DE BYTES
            byte[] rawSqliteInterop;
            byte[] rawWebviewLoader;

            if (is64Bit)
            {
                // La computadora corre a 64 bits: Extraemos los rehenes de 64
                rawSqliteInterop = Properties.LibraryResources.SQLite_Interop_x64;
                rawWebviewLoader = Properties.LibraryResources.WebView2Loader_x64;
            }
            else
            {
                // La computadora es una tostadora de 32 bits: Extraemos los rehenes de 32
                rawSqliteInterop = Properties.LibraryResources.SQLite_Interop_x86;
                rawWebviewLoader = Properties.LibraryResources.WebView2Loader_x86;
            }

            // 3. MATERIALIZACIÓN CRIPTOGRÁFICA EN EL %TEMP%
            // Aunque vengan de recursos diferentes, en el disco los guardamos con el nombre estándar
            _write_dll_in_temp("SQLite.Interop.dll", rawSqliteInterop);
            _write_dll_in_temp("WebView2Loader.dll", rawWebviewLoader);

            // Desactivar el escaneo por defecto de carpetas x86/x64 secundarias de SQLite
            Environment.SetEnvironmentVariable("PreLoadSQLite_RuntimePlatform", "false");

            // 4. INTERCEPCIÓN DE ENSAMBLADOS EN LA RAM (Para el System.Data.SQLite.dll de C#)
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string cleanName = new AssemblyName(args.Name).Name;

                if (cleanName == "System.Data.SQLite")
                {
                    return Assembly.Load(Properties.LibraryResources.System_Data_SQLite);
                }
                if (cleanName == "Microsoft.Web.WebView2.WinForms")
                {
                    return Assembly.Load(Properties.LibraryResources.WebView2_WinForms);
                }
                if (cleanName == "Microsoft.Web.WebView2.Core")
                {
                    return Assembly.Load(Properties.LibraryResources.WebView2_Core);
                }
                if (cleanName == "Microsoft.Web.WebView2.Wpf")
                {
                    return Assembly.Load(Properties.LibraryResources.WebView2_Wpf);
                }
                return null;
            };
        }

        private static void _write_dll_in_temp(string nameFile, byte[] resourceByte)
        {
            string output_DIR = Path.Combine(_TempRouteApp, nameFile);
            if (!File.Exists(output_DIR))
            {
                try
                {
                    File.WriteAllBytes(output_DIR, resourceByte);
                }
                catch
                {
                    // Si el archivo ya existe y está bloqueado por una instancia previa,
                    // significa que la librería ya está operativa en el entorno.
                }
            }
        }

        public static void Unload()
        {
            // Restablecemos el directorio de DLLs de Windows por seguridad
            SetDllDirectory(null);

            if (Directory.Exists(_TempRouteApp))
            {
                try
                {
                    // Ruta de los dos polizones nativos
                    string sqliteInterop = Path.Combine(_TempRouteApp, "SQLite.Interop.dll");
                    string webviewLoader = Path.Combine(_TempRouteApp, "WebView2Loader.dll");

                    // Si existen, les metemos guillotina
                    if (File.Exists(sqliteInterop)) File.Delete(sqliteInterop);
                    if (File.Exists(webviewLoader)) File.Delete(webviewLoader);

                    // Intentamos borrar la carpeta contenedora si ya quedó vacía
                    Directory.Delete(_TempRouteApp);
                }
                catch
                {
                    // Si Windows retiene el archivo por un milisegundo extra, el sistema operativo
                    // limpiará la carpeta temporal en su próximo ciclo de mantenimiento automático.
                }
            }
        }
    }
}
