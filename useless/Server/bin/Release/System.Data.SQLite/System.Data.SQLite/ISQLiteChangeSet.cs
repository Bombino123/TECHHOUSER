using System.Collections;
using System.Collections.Generic;

namespace System.Data.SQLite;

public interface ISQLiteChangeSet : IEnumerable<ISQLiteChangeSetMetadataItem>, IEnumerable, IDisposable
{
	ISQLiteChangeSet Invert();

	ISQLiteChangeSet CombineWith(ISQLiteChangeSet changeSet);

	void Apply(SessionConflictCallback conflictCallback, object clientData);

	void Apply(SessionConflictCallback conflictCallback, SessionTableFilterCallback tableFilterCallback, object clientData);
}
