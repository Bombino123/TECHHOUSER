using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure;

public interface IDbAsyncQueryProvider : IQueryProvider
{
	Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken);

	Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken);
}
