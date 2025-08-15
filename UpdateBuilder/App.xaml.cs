using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using UpdateBuilder.ViewModels;
using UpdateBuilder.Views;

namespace UpdateBuilder
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Registrar o CodePagesEncodingProvider para suportar IBM437 e outros encodings
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            Dispatcher.UnhandledException += DispatcherOnUnhandledException;
            
            var mainWindow = new MainWindow();
            var mainWindowViewModel = new MainWindowViewModel();
            mainWindow.DataContext = mainWindowViewModel;
            
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        
        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = false;
            ShowUnhandledException(e);
        }

        private void ShowUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            var innerExceptionMessage = e.Exception.InnerException?.Message ?? string.Empty;
            var messageBoxText = $"An application error occurred.\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.\n\nError: {e.Exception.Message}\n{innerExceptionMessage}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)";
            
            if (MessageBox.Show(messageBoxText, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Hand) == MessageBoxResult.No && 
                MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                Shutdown();
            }
        }

        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
