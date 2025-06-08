using System.Collections;
using System.Data.Entity.Core.Metadata.Edm;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Objects.DataClasses;

public interface IRelatedEnd
{
	bool IsLoaded { get; set; }

	string RelationshipName { get; }

	string SourceRoleName { get; }

	string TargetRoleName { get; }

	RelationshipSet RelationshipSet { get; }

	void Load();

	Task LoadAsync(CancellationToken cancellationToken);

	void Load(MergeOption mergeOption);

	Task LoadAsync(MergeOption mergeOption, CancellationToken cancellationToken);

	void Add(IEntityWithRelationships entity);

	void Add(object entity);

	bool Remove(IEntityWithRelationships entity);

	bool Remove(object entity);

	void Attach(IEntityWithRelationships entity);

	void Attach(object entity);

	IEnumerable CreateSourceQuery();

	IEnumerator GetEnumerator();
}
