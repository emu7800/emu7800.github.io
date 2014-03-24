using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Browser;
using EMU7800.SL.View;

namespace EMU7800.SL
{
    public partial class App
    {
        public App()
        {
            Startup += Application_Startup;
            Exit += Application_Exit;
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
        }

        void Application_Startup(object sender, StartupEventArgs e)
        {
            RootVisual = new MainPage();
        }

        static void Application_Exit(object sender, EventArgs e)
        {
        }

        static void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using the browser's exception mechanism.
            // On IE this will display it a yellow alert icon in the status bar and Firefox will display a script error.
            if (!Debugger.IsAttached)
            {
                // This will allow the application to continue running after an exception has been thrown but not handled.
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(() => ReportErrorToDOM(e));
            }
        }

        static void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            var message = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
            message = message.Replace('"', '\'').Replace("\r\n", @"\n");
            var code = string.Format("throw new Error(\"Unhandled Error in Silverlight Application EMU7800.SL: {0}\");", message);
            try
            {
                HtmlPage.Window.Eval(code);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
