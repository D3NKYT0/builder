using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using UpdateBuilder.ViewModels;
using UpdateBuilder.Views;

namespace UpdateBuilder
{
    public partial class App : Application
    {
        //private bool _contentLoaded;
        /*
        // Конструктор базового класса App
        public App()
        {
            this.InitializeComponent();
            //this.Suspending += OnSuspending;
            base.Startup += App_OnStartup;
            //base.Startup += App_OnStartup;
            ///Uri resourceLocator = new Uri("/UpdateBuilder;component/UpdateBuilder/App.xaml", UriKind.Relative);
            ///Application.LoadComponent(this, resourceLocator);
        }*/
        
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            //Startup="App_OnStartup"
            //StartupUri = "Views/MainWindow.xaml"

            base.Dispatcher.UnhandledException += DispatcherOnUnhandledException;
            MainWindow mainWindow = new MainWindow();
            MainWindowViewModel mainWindowViewModel2 = (MainWindowViewModel)(mainWindow.DataContext = new MainWindowViewModel());
            Application.Current.MainWindow = mainWindow;
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
            string messageBoxText = string.Format("An application error occurred.\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.\n\nError: {0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)", e.Exception.Message + ((e.Exception.InnerException != null) ? ("\n" + e.Exception.InnerException.Message) : null));
            if (MessageBox.Show(messageBoxText, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Hand) == MessageBoxResult.No && MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        

        /*
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			if (!_contentLoaded)
			{
				_contentLoaded = true;
				base.Startup += App_OnStartup;
				Uri resourceLocator = new Uri("/UpdateBuilder;component/app.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
		}*/

       

        
		[STAThread]
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public static void Main()
		{
			//string messageBoxText = string.Format("TEST");
			//if (MessageBox.Show(messageBoxText, "UpdateBuilder", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
				App app = new App();
				app.InitializeComponent();
                //app.Startup += app.App_OnStartup;
                app.Run();
                //InitializeComponent();
            }


        }
    }
}
