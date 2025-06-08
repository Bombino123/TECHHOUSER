using System.Data.Common;
using System.Data.Entity.Core.Common;

namespace System.Data.Entity.Core;

public interface IExtendedDataRecord : IDataRecord
{
	DataRecordInfo DataRecordInfo { get; }

	DbDataRecord GetDataRecord(int i);

	DbDataReader GetDataReader(int i);
}
