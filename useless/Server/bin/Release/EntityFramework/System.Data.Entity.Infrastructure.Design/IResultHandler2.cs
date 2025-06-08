namespace System.Data.Entity.Infrastructure.Design;

public interface IResultHandler2 : IResultHandler
{
	void SetError(string type, string message, string stackTrace);
}
