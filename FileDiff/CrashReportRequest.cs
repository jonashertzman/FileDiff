namespace FileDiff;

public class CrashReportRequest
{

	public string ApplicationName { get; set; }

	public string BuildNumber { get; set; }

	public string ClientId { get; set; }

	public string ExceptionType { get; set; }

	public string ExceptionMessage { get; set; }

	public string Source { get; set; }

	public string StackTrace { get; set; }

}
