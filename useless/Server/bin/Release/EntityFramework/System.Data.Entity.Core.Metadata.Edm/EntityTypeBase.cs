using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class EntityTypeBase : StructuralType
{
	private readonly ReadOnlyMetadataCollection<EdmMember> _keyMembers;

	private readonly object _keyPropertiesSync = new object();

	private ReadOnlyMetadataCollection<EdmProperty> _keyProperties;

	private string[] _keyMemberNames;

	[MetadataProperty(BuiltInTypeKind.EdmMember, true)]
	public virtual ReadOnlyMetadataCollection<EdmMember> KeyMembers
	{
		get
		{
			if (BaseType != null && ((EntityTypeBase)BaseType).KeyMembers.Count != 0)
			{
				return ((EntityTypeBase)BaseType).KeyMembers;
			}
			return _keyMembers;
		}
	}

	public virtual ReadOnlyMetadataCollection<EdmProperty> KeyProperties
	{
		get
		{
			ReadOnlyMetadataCollection<EdmProperty> keyProperties = _keyProperties;
			if (keyProperties == null)
			{
				lock (_keyPropertiesSync)
				{
					if (_keyProperties == null)
					{
						KeyMembers.SourceAccessed += KeyMembersSourceAccessedEventHandler;
						_keyProperties = new ReadOnlyMetadataCollection<EdmProperty>(KeyMembers.Cast<EdmProperty>().ToList());
					}
					keyProperties = _keyProperties;
				}
			}
			return keyProperties;
		}
	}

	internal virtual string[] KeyMemberNames
	{
		get
		{
			string[] keyMemberNames = _keyMemberNames;
			if (keyMemberNames == null)
			{
				keyMemberNames = new string[KeyMembers.Count];
				for (int i = 0; i < keyMemberNames.Length; i++)
				{
					keyMemberNames[i] = KeyMembers[i].Name;
				}
				_keyMemberNames = keyMemberNames;
			}
			return _keyMemberNames;
		}
	}

	internal EntityTypeBase(string name, string namespaceName, DataSpace dataSpace)
		: base(name, namespaceName, dataSpace)
	{
		_keyMembers = new ReadOnlyMetadataCollection<EdmMember>(new MetadataCollection<EdmMember>());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ResetKeyPropertiesCache()
	{
		if (_keyProperties == null)
		{
			return;
		}
		lock (_keyPropertiesSync)
		{
			if (_keyProperties != null)
			{
				_keyProperties = null;
				KeyMembers.SourceAccessed -= KeyMembersSourceAccessedEventHandler;
			}
		}
	}

	private void KeyMembersSourceAccessedEventHandler(object sender, EventArgs e)
	{
		ResetKeyPropertiesCache();
	}

	public void AddKeyMember(EdmMember member)
	{
		Check.NotNull(member, "member");
		Util.ThrowIfReadOnly(this);
		if (!base.Members.Contains(member))
		{
			AddMember(member);
		}
		_keyMembers.Source.Add(member);
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			_keyMembers.Source.SetReadOnly();
			base.SetReadOnly();
		}
	}

	internal static void CheckAndAddMembers(IEnumerable<EdmMember> members, EntityType entityType)
	{
		foreach (EdmMember member in members)
		{
			if (member == null)
			{
				throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("members"));
			}
			entityType.AddMember(member);
		}
	}

	internal void CheckAndAddKeyMembers(IEnumerable<string> keyMembers)
	{
		foreach (string keyMember in keyMembers)
		{
			if (keyMember == null)
			{
				throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("keyMembers"));
			}
			if (!base.Members.TryGetValue(keyMember, ignoreCase: false, out var item))
			{
				throw new ArgumentException(Strings.InvalidKeyMember(keyMember));
			}
			AddKeyMember(item);
		}
	}

	public override void RemoveMember(EdmMember member)
	{
		Check.NotNull(member, "member");
		Util.ThrowIfReadOnly(this);
		if (_keyMembers.Contains(member))
		{
			_keyMembers.Source.Remove(member);
		}
		base.RemoveMember(member);
	}

	internal override void NotifyItemIdentityChanged(EdmMember item, string initialIdentity)
	{
		base.NotifyItemIdentityChanged(item, initialIdentity);
		_keyMembers.Source.HandleIdentityChange(item, initialIdentity);
	}
}
