using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq.Expressions;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal class RecordStateFactory
{
	internal readonly int StateSlotNumber;

	internal readonly int ColumnCount;

	internal readonly DataRecordInfo DataRecordInfo;

	internal readonly Func<Shaper, bool> GatherData;

	internal readonly ReadOnlyCollection<RecordStateFactory> NestedRecordStateFactories;

	internal readonly ReadOnlyCollection<string> ColumnNames;

	internal readonly ReadOnlyCollection<TypeUsage> TypeUsages;

	internal readonly ReadOnlyCollection<bool> IsColumnNested;

	internal readonly bool HasNestedColumns;

	internal readonly FieldNameLookup FieldNameLookup;

	private readonly string Description;

	public RecordStateFactory(int stateSlotNumber, int columnCount, RecordStateFactory[] nestedRecordStateFactories, DataRecordInfo dataRecordInfo, Expression<Func<Shaper, bool>> gatherData, string[] propertyNames, TypeUsage[] typeUsages, bool[] isColumnNested)
	{
		StateSlotNumber = stateSlotNumber;
		ColumnCount = columnCount;
		NestedRecordStateFactories = new ReadOnlyCollection<RecordStateFactory>(nestedRecordStateFactories);
		DataRecordInfo = dataRecordInfo;
		GatherData = gatherData.Compile();
		Description = gatherData.ToString();
		ColumnNames = new ReadOnlyCollection<string>(propertyNames);
		TypeUsages = new ReadOnlyCollection<TypeUsage>(typeUsages);
		FieldNameLookup = new FieldNameLookup(ColumnNames);
		if (isColumnNested == null)
		{
			isColumnNested = new bool[columnCount];
			for (int i = 0; i < columnCount; i++)
			{
				switch (typeUsages[i].EdmType.BuiltInTypeKind)
				{
				case BuiltInTypeKind.CollectionType:
				case BuiltInTypeKind.ComplexType:
				case BuiltInTypeKind.EntityType:
				case BuiltInTypeKind.RowType:
					isColumnNested[i] = true;
					HasNestedColumns = true;
					break;
				default:
					isColumnNested[i] = false;
					break;
				}
			}
		}
		IsColumnNested = new ReadOnlyCollection<bool>(isColumnNested);
	}

	public RecordStateFactory(int stateSlotNumber, int columnCount, RecordStateFactory[] nestedRecordStateFactories, DataRecordInfo dataRecordInfo, Expression gatherData, string[] propertyNames, TypeUsage[] typeUsages)
		: this(stateSlotNumber, columnCount, nestedRecordStateFactories, dataRecordInfo, CodeGenEmitter.BuildShaperLambda<bool>(gatherData), propertyNames, typeUsages, null)
	{
	}

	internal RecordState Create(CoordinatorFactory coordinatorFactory)
	{
		return new RecordState(this, coordinatorFactory);
	}
}
