using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public abstract class DbCommandTree
{
	private readonly MetadataWorkspace _metadata;

	private readonly DataSpace _dataSpace;

	private readonly bool _useDatabaseNullSemantics;

	private readonly bool _disableFilterOverProjectionSimplificationForCustomFunctions;

	public bool UseDatabaseNullSemantics => _useDatabaseNullSemantics;

	public bool DisableFilterOverProjectionSimplificationForCustomFunctions => _disableFilterOverProjectionSimplificationForCustomFunctions;

	public IEnumerable<KeyValuePair<string, TypeUsage>> Parameters => GetParameters();

	public abstract DbCommandTreeKind CommandTreeKind { get; }

	public virtual MetadataWorkspace MetadataWorkspace => _metadata;

	public virtual DataSpace DataSpace => _dataSpace;

	internal DbCommandTree()
	{
		_useDatabaseNullSemantics = true;
	}

	internal DbCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, bool useDatabaseNullSemantics = true, bool disableFilterOverProjectionSimplificationForCustomFunctions = false)
	{
		if (!IsValidDataSpace(dataSpace))
		{
			throw new ArgumentException(Strings.Cqt_CommandTree_InvalidDataSpace, "dataSpace");
		}
		_metadata = metadata;
		_dataSpace = dataSpace;
		_useDatabaseNullSemantics = useDatabaseNullSemantics;
		_disableFilterOverProjectionSimplificationForCustomFunctions = disableFilterOverProjectionSimplificationForCustomFunctions;
	}

	internal abstract IEnumerable<KeyValuePair<string, TypeUsage>> GetParameters();

	internal void Dump(ExpressionDumper dumper)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("DataSpace", DataSpace);
		dumper.Begin(GetType().Name, dictionary);
		dumper.Begin("Parameters", null);
		foreach (KeyValuePair<string, TypeUsage> parameter in Parameters)
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2.Add("Name", parameter.Key);
			dumper.Begin("Parameter", dictionary2);
			dumper.Dump(parameter.Value, "ParameterType");
			dumper.End("Parameter");
		}
		dumper.End("Parameters");
		DumpStructure(dumper);
		dumper.End(GetType().Name);
	}

	internal abstract void DumpStructure(ExpressionDumper dumper);

	public override string ToString()
	{
		return Print();
	}

	internal string Print()
	{
		return PrintTree(new ExpressionPrinter());
	}

	internal abstract string PrintTree(ExpressionPrinter printer);

	internal static bool IsValidDataSpace(DataSpace dataSpace)
	{
		if (dataSpace != 0 && DataSpace.CSpace != dataSpace)
		{
			return DataSpace.SSpace == dataSpace;
		}
		return true;
	}

	internal static bool IsValidParameterName(string name)
	{
		if (!string.IsNullOrWhiteSpace(name))
		{
			return name.IsValidUndottedName();
		}
		return false;
	}
}
