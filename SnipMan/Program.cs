using System;
using System.Windows.Forms;
using System.Reflection;

namespace SnipMan
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            Application.Run(new Form1());
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string resourceName = "SnipMan.Resources." + new AssemblyName(args.Name).Name + ".dll";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            }

            return null;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //if (e.ExceptionObject is System.SystemException)
            //{
            //    new SnipMan.Core.ErrorHandler(((Exception)(e.ExceptionObject)).Message + "\n\nRuns in a 32-bit OS Only", ((Exception)(e.ExceptionObject)).StackTrace);
            //}
            new Core.ErrorHandler(((Exception)(e.ExceptionObject)).Message, ((Exception)(e.ExceptionObject)).StackTrace);
            //throw new NotImplementedException();
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            new Core.ErrorHandler(((Exception)(e.Exception)).Message, ((Exception)(e.Exception)).StackTrace);
            //throw new NotImplementedException();
        }
    }
}
