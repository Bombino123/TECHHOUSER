using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;

namespace System.Data.Entity.Core.Mapping;

internal class CompressingHashBuilder : StringHashBuilder
{
	private const int HashCharacterCompressionThreshold = 2048;

	private const int SpacesPerIndent = 4;

	private int _indent;

	private static readonly Dictionary<Type, string> _legacyTypeNames = InitializeLegacyTypeNames();

	internal CompressingHashBuilder(HashAlgorithm hashAlgorithm)
		: base(hashAlgorithm, 6144)
	{
	}

	internal override void Append(string content)
	{
		base.Append(string.Empty.PadLeft(4 * _indent, ' '));
		base.Append(content);
		CompressHash();
	}

	internal override void AppendLine(string content)
	{
		base.Append(string.Empty.PadLeft(4 * _indent, ' '));
		base.AppendLine(content);
		CompressHash();
	}

	private static Dictionary<Type, string> InitializeLegacyTypeNames()
	{
		return new Dictionary<Type, string>
		{
			{
				typeof(AssociationSetMapping),
				"System.Data.Entity.Core.Mapping.StorageAssociationSetMapping"
			},
			{
				typeof(AssociationSetModificationFunctionMapping),
				"System.Data.Entity.Core.Mapping.StorageAssociationSetModificationFunctionMapping"
			},
			{
				typeof(AssociationTypeMapping),
				"System.Data.Entity.Core.Mapping.StorageAssociationTypeMapping"
			},
			{
				typeof(ComplexPropertyMapping),
				"System.Data.Entity.Core.Mapping.StorageComplexPropertyMapping"
			},
			{
				typeof(ComplexTypeMapping),
				"System.Data.Entity.Core.Mapping.StorageComplexTypeMapping"
			},
			{
				typeof(ConditionPropertyMapping),
				"System.Data.Entity.Core.Mapping.StorageConditionPropertyMapping"
			},
			{
				typeof(EndPropertyMapping),
				"System.Data.Entity.Core.Mapping.StorageEndPropertyMapping"
			},
			{
				typeof(EntityContainerMapping),
				"System.Data.Entity.Core.Mapping.StorageEntityContainerMapping"
			},
			{
				typeof(EntitySetMapping),
				"System.Data.Entity.Core.Mapping.StorageEntitySetMapping"
			},
			{
				typeof(EntityTypeMapping),
				"System.Data.Entity.Core.Mapping.StorageEntityTypeMapping"
			},
			{
				typeof(EntityTypeModificationFunctionMapping),
				"System.Data.Entity.Core.Mapping.StorageEntityTypeModificationFunctionMapping"
			},
			{
				typeof(MappingFragment),
				"System.Data.Entity.Core.Mapping.StorageMappingFragment"
			},
			{
				typeof(ModificationFunctionMapping),
				"System.Data.Entity.Core.Mapping.StorageModificationFunctionMapping"
			},
			{
				typeof(ModificationFunctionMemberPath),
				"System.Data.Entity.Core.Mapping.StorageModificationFunctionMemberPath"
			},
			{
				typeof(ModificationFunctionParameterBinding),
				"System.Data.Entity.Core.Mapping.StorageModificationFunctionParameterBinding"
			},
			{
				typeof(ModificationFunctionResultBinding),
				"System.Data.Entity.Core.Mapping.StorageModificationFunctionResultBinding"
			},
			{
				typeof(PropertyMapping),
				"System.Data.Entity.Core.Mapping.StoragePropertyMapping"
			},
			{
				typeof(ScalarPropertyMapping),
				"System.Data.Entity.Core.Mapping.StorageScalarPropertyMapping"
			},
			{
				typeof(EntitySetBaseMapping),
				"System.Data.Entity.Core.Mapping.StorageSetMapping"
			},
			{
				typeof(TypeMapping),
				"System.Data.Entity.Core.Mapping.StorageTypeMapping"
			}
		};
	}

	internal void AppendObjectStartDump(object o, int objectIndex)
	{
		base.Append(string.Empty.PadLeft(4 * _indent, ' '));
		if (!_legacyTypeNames.TryGetValue(o.GetType(), out var value))
		{
			value = o.GetType().ToString();
		}
		base.Append(value);
		base.Append(" Instance#");
		base.AppendLine(objectIndex.ToString(CultureInfo.InvariantCulture));
		CompressHash();
		_indent++;
	}

	internal void AppendObjectEndDump()
	{
		_indent--;
	}

	private void CompressHash()
	{
		if (base.CharCount >= 2048)
		{
			string s = ComputeHash();
			Clear();
			base.Append(s);
		}
	}
}
