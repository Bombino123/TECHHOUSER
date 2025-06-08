namespace System.Data.Entity.Infrastructure.Design;

public interface IReportHandler
{
	void OnError(string message);

	void OnWarning(string message);

	void OnInformation(string message);

	void OnVerbose(string message);
}
