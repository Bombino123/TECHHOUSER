using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.EntityClient.Internal;

internal interface IEntityAdapter
{
	DbConnection Connection { get; set; }

	bool AcceptChangesDuringUpdate { get; set; }

	int? CommandTimeout { get; set; }

	int Update();

	Task<int> UpdateAsync(CancellationToken cancellationToken);
}
