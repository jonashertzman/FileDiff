using System.Windows;

namespace FileDiff;

public partial class App : Application
{
	public App()
	{
		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
		{
			Log.LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
		};

		DispatcherUnhandledException += (s, e) =>
		{
			Log.LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
			e.Handled = true;
		};

		TaskScheduler.UnobservedTaskException += (s, e) =>
		{
			Log.LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
			e.SetObserved();
		};
	}
}
