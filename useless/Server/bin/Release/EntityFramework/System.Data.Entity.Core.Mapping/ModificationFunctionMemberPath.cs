using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Core.Mapping;

public sealed class ModificationFunctionMemberPath : MappingItem
{
	private readonly ReadOnlyCollection<EdmMember> _members;

	private readonly AssociationSetEnd _associationSetEnd;

	public ReadOnlyCollection<EdmMember> Members => _members;

	public AssociationSetEnd AssociationSetEnd => _associationSetEnd;

	public ModificationFunctionMemberPath(IEnumerable<EdmMember> members, AssociationSet associationSet)
	{
		Check.NotNull(members, "members");
		_members = new ReadOnlyCollection<EdmMember>(new List<EdmMember>(members));
		if (associationSet != null)
		{
			_associationSetEnd = associationSet.AssociationSetEnds[Members[1].Name];
		}
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[2]
		{
			(AssociationSetEnd == null) ? string.Empty : ("[" + AssociationSetEnd.ParentAssociationSet?.ToString() + "]"),
			StringUtil.BuildDelimitedList(Members, null, ".")
		});
	}
}
