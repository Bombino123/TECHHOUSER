using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.Core.Objects.Internal;

internal class TransactionManager
{
	private MergeOption? _originalMergeOption;

	private int _graphUpdateCount;

	internal Dictionary<RelatedEnd, IList<IEntityWrapper>> PromotedRelationships { get; private set; }

	internal Dictionary<object, EntityEntry> PromotedKeyEntries { get; private set; }

	internal HashSet<EntityReference> PopulatedEntityReferences { get; private set; }

	internal HashSet<EntityReference> AlignedEntityReferences { get; private set; }

	internal MergeOption? OriginalMergeOption
	{
		get
		{
			return _originalMergeOption;
		}
		set
		{
			_originalMergeOption = value;
		}
	}

	internal HashSet<IEntityWrapper> ProcessedEntities { get; private set; }

	internal Dictionary<object, IEntityWrapper> WrappedEntities { get; private set; }

	internal bool TrackProcessedEntities { get; private set; }

	internal bool IsAddTracking { get; private set; }

	internal bool IsAttachTracking { get; private set; }

	internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>> AddedRelationshipsByGraph { get; private set; }

	internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>> DeletedRelationshipsByGraph { get; private set; }

	internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> AddedRelationshipsByForeignKey { get; private set; }

	internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> AddedRelationshipsByPrincipalKey { get; private set; }

	internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> DeletedRelationshipsByForeignKey { get; private set; }

	internal Dictionary<IEntityWrapper, HashSet<RelatedEnd>> ChangedForeignKeys { get; private set; }

	internal bool IsDetectChanges { get; private set; }

	internal bool IsAlignChanges { get; private set; }

	internal bool IsLocalPublicAPI { get; private set; }

	internal bool IsOriginalValuesGetter { get; private set; }

	internal bool IsForeignKeyUpdate { get; private set; }

	internal bool IsRelatedEndAdd { get; private set; }

	internal bool IsGraphUpdate => _graphUpdateCount != 0;

	internal object EntityBeingReparented { get; set; }

	internal bool IsDetaching { get; private set; }

	internal EntityReference RelationshipBeingUpdated { get; private set; }

	internal bool IsFixupByReference { get; private set; }

	internal void BeginAddTracking()
	{
		IsAddTracking = true;
		PopulatedEntityReferences = new HashSet<EntityReference>();
		AlignedEntityReferences = new HashSet<EntityReference>();
		PromotedRelationships = new Dictionary<RelatedEnd, IList<IEntityWrapper>>();
		if (!IsDetectChanges)
		{
			TrackProcessedEntities = true;
			ProcessedEntities = new HashSet<IEntityWrapper>();
			WrappedEntities = new Dictionary<object, IEntityWrapper>(ObjectReferenceEqualityComparer.Default);
		}
	}

	internal void EndAddTracking()
	{
		IsAddTracking = false;
		PopulatedEntityReferences = null;
		AlignedEntityReferences = null;
		PromotedRelationships = null;
		if (!IsDetectChanges)
		{
			TrackProcessedEntities = false;
			ProcessedEntities = null;
			WrappedEntities = null;
		}
	}

	internal void BeginAttachTracking()
	{
		IsAttachTracking = true;
		PromotedRelationships = new Dictionary<RelatedEnd, IList<IEntityWrapper>>();
		PromotedKeyEntries = new Dictionary<object, EntityEntry>(ObjectReferenceEqualityComparer.Default);
		PopulatedEntityReferences = new HashSet<EntityReference>();
		AlignedEntityReferences = new HashSet<EntityReference>();
		TrackProcessedEntities = true;
		ProcessedEntities = new HashSet<IEntityWrapper>();
		WrappedEntities = new Dictionary<object, IEntityWrapper>(ObjectReferenceEqualityComparer.Default);
		OriginalMergeOption = null;
	}

	internal void EndAttachTracking()
	{
		IsAttachTracking = false;
		PromotedRelationships = null;
		PromotedKeyEntries = null;
		PopulatedEntityReferences = null;
		AlignedEntityReferences = null;
		TrackProcessedEntities = false;
		ProcessedEntities = null;
		WrappedEntities = null;
		OriginalMergeOption = null;
	}

	internal bool BeginDetectChanges()
	{
		if (IsDetectChanges)
		{
			return false;
		}
		IsDetectChanges = true;
		TrackProcessedEntities = true;
		ProcessedEntities = new HashSet<IEntityWrapper>();
		WrappedEntities = new Dictionary<object, IEntityWrapper>(ObjectReferenceEqualityComparer.Default);
		DeletedRelationshipsByGraph = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>>();
		AddedRelationshipsByGraph = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>>();
		DeletedRelationshipsByForeignKey = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>>();
		AddedRelationshipsByForeignKey = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>>();
		AddedRelationshipsByPrincipalKey = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>>();
		ChangedForeignKeys = new Dictionary<IEntityWrapper, HashSet<RelatedEnd>>();
		return true;
	}

	internal void EndDetectChanges()
	{
		IsDetectChanges = false;
		TrackProcessedEntities = false;
		ProcessedEntities = null;
		WrappedEntities = null;
		DeletedRelationshipsByGraph = null;
		AddedRelationshipsByGraph = null;
		DeletedRelationshipsByForeignKey = null;
		AddedRelationshipsByForeignKey = null;
		AddedRelationshipsByPrincipalKey = null;
		ChangedForeignKeys = null;
	}

	internal void BeginAlignChanges()
	{
		IsAlignChanges = true;
	}

	internal void EndAlignChanges()
	{
		IsAlignChanges = false;
	}

	internal void ResetProcessedEntities()
	{
		ProcessedEntities.Clear();
	}

	internal void BeginLocalPublicAPI()
	{
		IsLocalPublicAPI = true;
	}

	internal void EndLocalPublicAPI()
	{
		IsLocalPublicAPI = false;
	}

	internal void BeginOriginalValuesGetter()
	{
		IsOriginalValuesGetter = true;
	}

	internal void EndOriginalValuesGetter()
	{
		IsOriginalValuesGetter = false;
	}

	internal void BeginForeignKeyUpdate(EntityReference relationship)
	{
		RelationshipBeingUpdated = relationship;
		IsForeignKeyUpdate = true;
	}

	internal void EndForeignKeyUpdate()
	{
		RelationshipBeingUpdated = null;
		IsForeignKeyUpdate = false;
	}

	internal void BeginRelatedEndAdd()
	{
		IsRelatedEndAdd = true;
	}

	internal void EndRelatedEndAdd()
	{
		IsRelatedEndAdd = false;
	}

	internal void BeginGraphUpdate()
	{
		_graphUpdateCount++;
	}

	internal void EndGraphUpdate()
	{
		_graphUpdateCount--;
	}

	internal void BeginDetaching()
	{
		IsDetaching = true;
	}

	internal void EndDetaching()
	{
		IsDetaching = false;
	}

	internal void BeginFixupKeysByReference()
	{
		IsFixupByReference = true;
	}

	internal void EndFixupKeysByReference()
	{
		IsFixupByReference = false;
	}
}
