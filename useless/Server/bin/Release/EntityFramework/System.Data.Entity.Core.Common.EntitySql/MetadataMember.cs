using System.Collections.Generic;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql;

internal abstract class MetadataMember : ExpressionResolution
{
	private sealed class MetadataMemberNameEqualityComparer : IEqualityComparer<MetadataMember>
	{
		private readonly StringComparer _stringComparer;

		internal MetadataMemberNameEqualityComparer(StringComparer stringComparer)
		{
			_stringComparer = stringComparer;
		}

		bool IEqualityComparer<MetadataMember>.Equals(MetadataMember x, MetadataMember y)
		{
			return _stringComparer.Equals(x.Name, y.Name);
		}

		int IEqualityComparer<MetadataMember>.GetHashCode(MetadataMember obj)
		{
			return _stringComparer.GetHashCode(obj.Name);
		}
	}

	internal readonly MetadataMemberClass MetadataMemberClass;

	internal readonly string Name;

	internal override string ExpressionClassName => MetadataMemberExpressionClassName;

	internal static string MetadataMemberExpressionClassName => Strings.LocalizedMetadataMemberExpression;

	internal abstract string MetadataMemberClassName { get; }

	protected MetadataMember(MetadataMemberClass @class, string name)
		: base(ExpressionResolutionClass.MetadataMember)
	{
		MetadataMemberClass = @class;
		Name = name;
	}

	internal static IEqualityComparer<MetadataMember> CreateMetadataMemberNameEqualityComparer(StringComparer stringComparer)
	{
		return new MetadataMemberNameEqualityComparer(stringComparer);
	}
}
