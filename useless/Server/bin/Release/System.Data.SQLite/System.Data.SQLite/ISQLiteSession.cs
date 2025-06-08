using System.IO;

namespace System.Data.SQLite;

public interface ISQLiteSession : IDisposable
{
	bool IsEnabled();

	void SetToEnabled();

	void SetToDisabled();

	bool IsIndirect();

	void SetToIndirect();

	void SetToDirect();

	bool IsEmpty();

	long GetMemoryBytesInUse();

	void AttachTable(string name);

	void SetTableFilter(SessionTableFilterCallback callback, object clientData);

	void CreateChangeSet(ref byte[] rawData);

	void CreateChangeSet(Stream stream);

	void CreatePatchSet(ref byte[] rawData);

	void CreatePatchSet(Stream stream);

	void LoadDifferencesFromTable(string fromDatabaseName, string tableName);
}
