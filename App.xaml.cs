using System;
using System.Windows;

namespace Leakage2021
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //*******************************************************************************
        private static System.Reflection.Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            System.Reflection.Assembly executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var executingAssemblyName = executingAssembly.GetName();
            var resName = executingAssemblyName.Name + ".resources";

            System.Reflection.AssemblyName assemblyName = new System.Reflection.AssemblyName(args.Name); string path = "";
            if (resName == assemblyName.Name)
            {
                path = executingAssemblyName.Name + ".g.resources"; ;
            }
            else
            {
                path = assemblyName.Name + ".dll";
                if (assemblyName.CultureInfo.Equals(System.Globalization.CultureInfo.InvariantCulture) == false)
                {
                    path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
                }
            }
            using (System.IO.Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;
                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return System.Reflection.Assembly.Load(assemblyRawBytes);
            }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }
        //*******************************************************************************

        // 内存释放
        //[System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //public static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        //public void FulshMemor()
        //{
        //    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        //        SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
        //}

        //public App()
        //{
        //    //var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMinutes(20)};//20分钟
        //    //var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };//10S
        //    //timer.Tick += (s, e) => FulshMemor();
        //    //timer.Start();
        //}
        //*******************************************************************************

    }
}
