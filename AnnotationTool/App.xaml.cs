using System.Windows;
using System.Windows.Threading;

namespace AnnotationTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        readonly ExceptionHandler _exceptionHandler= new ExceptionHandler();
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _exceptionHandler.UnexpectedExceptionHanlder(e.Exception);
            
            e.Handled = true;
            Current.Shutdown();
        }
    }
}
