using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbFunctionCommandTree : DbCommandTree
{
	private readonly EdmFunction _edmFunction;

	private readonly TypeUsage _resultType;

	private readonly ReadOnlyCollection<string> _parameterNames;

	private readonly ReadOnlyCollection<TypeUsage> _parameterTypes;

	public EdmFunction EdmFunction => _edmFunction;

	public TypeUsage ResultType => _resultType;

	public override DbCommandTreeKind CommandTreeKind => DbCommandTreeKind.Function;

	public DbFunctionCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, EdmFunction edmFunction, TypeUsage resultType, IEnumerable<KeyValuePair<string, TypeUsage>> parameters)
		: base(metadata, dataSpace)
	{
		Check.NotNull(edmFunction, "edmFunction");
		_edmFunction = edmFunction;
		_resultType = resultType;
		List<string> list = new List<string>();
		List<TypeUsage> list2 = new List<TypeUsage>();
		if (parameters != null)
		{
			foreach (KeyValuePair<string, TypeUsage> parameter in parameters)
			{
				list.Add(parameter.Key);
				list2.Add(parameter.Value);
			}
		}
		_parameterNames = new ReadOnlyCollection<string>(list);
		_parameterTypes = new ReadOnlyCollection<TypeUsage>(list2);
	}

	internal override IEnumerable<KeyValuePair<string, TypeUsage>> GetParameters()
	{
		for (int idx = 0; idx < _parameterNames.Count; idx++)
		{
			yield return new KeyValuePair<string, TypeUsage>(_parameterNames[idx], _parameterTypes[idx]);
		}
	}

	internal override void DumpStructure(ExpressionDumper dumper)
	{
		if (EdmFunction != null)
		{
			dumper.Dump(EdmFunction);
		}
	}

	internal override string PrintTree(ExpressionPrinter printer)
	{
		return printer.Print(this);
	}
}
