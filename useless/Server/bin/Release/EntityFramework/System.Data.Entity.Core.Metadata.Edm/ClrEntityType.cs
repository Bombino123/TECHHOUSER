using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class ClrEntityType : EntityType
{
	private readonly Type _type;

	private Func<object> _constructor;

	private readonly string _cspaceTypeName;

	private readonly string _cspaceNamespaceName;

	private string _hash;

	internal Func<object> Constructor
	{
		get
		{
			return _constructor;
		}
		set
		{
			Interlocked.CompareExchange(ref _constructor, value, null);
		}
	}

	internal override Type ClrType => _type;

	internal string CSpaceTypeName => _cspaceTypeName;

	internal string CSpaceNamespaceName => _cspaceNamespaceName;

	internal string HashedDescription
	{
		get
		{
			if (_hash == null)
			{
				Interlocked.CompareExchange(ref _hash, BuildEntityTypeHash(), null);
			}
			return _hash;
		}
	}

	internal ClrEntityType(Type type, string cspaceNamespaceName, string cspaceTypeName)
		: base(Check.NotNull(type, "type").Name, type.NestingNamespace() ?? string.Empty, DataSpace.OSpace)
	{
		_type = type;
		_cspaceNamespaceName = cspaceNamespaceName;
		_cspaceTypeName = cspaceNamespaceName + "." + cspaceTypeName;
		base.Abstract = type.IsAbstract();
	}

	private string BuildEntityTypeHash()
	{
		using SHA256 sHA = MetadataHelper.CreateSHA256HashAlgorithm();
		byte[] array = sHA.ComputeHash(Encoding.ASCII.GetBytes(BuildEntityTypeDescription()));
		StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
		}
		return stringBuilder.ToString();
	}

	private string BuildEntityTypeDescription()
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		stringBuilder.Append("CLR:").Append(ClrType.FullName);
		stringBuilder.Append("Conceptual:").Append(CSpaceTypeName);
		SortedSet<string> sortedSet = new SortedSet<string>();
		foreach (NavigationProperty navigationProperty in base.NavigationProperties)
		{
			sortedSet.Add(navigationProperty.Name + "*" + navigationProperty.FromEndMember.Name + "*" + navigationProperty.FromEndMember.RelationshipMultiplicity.ToString() + "*" + navigationProperty.ToEndMember.Name + "*" + navigationProperty.ToEndMember.RelationshipMultiplicity.ToString() + "*");
		}
		stringBuilder.Append("NavProps:");
		foreach (string item2 in sortedSet)
		{
			stringBuilder.Append(item2);
		}
		SortedSet<string> sortedSet2 = new SortedSet<string>();
		string[] keyMemberNames = KeyMemberNames;
		foreach (string item in keyMemberNames)
		{
			sortedSet2.Add(item);
		}
		stringBuilder.Append("Keys:");
		foreach (string item3 in sortedSet2)
		{
			stringBuilder.Append(item3 + "*");
		}
		SortedSet<string> sortedSet3 = new SortedSet<string>();
		foreach (EdmMember member in base.Members)
		{
			if (!sortedSet2.Contains(member.Name))
			{
				sortedSet3.Add(member.Name + "*");
			}
		}
		stringBuilder.Append("Scalars:");
		foreach (string item4 in sortedSet3)
		{
			stringBuilder.Append(item4 + "*");
		}
		return stringBuilder.ToString();
	}
}
