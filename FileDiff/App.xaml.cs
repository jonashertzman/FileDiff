using System.Windows;

namespace FileDiff;

public partial class App : Application
{
	public App()
	{
		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
		{
			Log.DisplayException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
		};

		DispatcherUnhandledException += (s, e) =>
		{
			Log.DisplayException(e.Exception, "Application.Current.DispatcherUnhandledException");
			e.Handled = true;
		};

		TaskScheduler.UnobservedTaskException += (s, e) =>
		{
			Log.DisplayException(e.Exception, "TaskScheduler.UnobservedTaskException");
			e.SetObserved();
		};
	}
}
