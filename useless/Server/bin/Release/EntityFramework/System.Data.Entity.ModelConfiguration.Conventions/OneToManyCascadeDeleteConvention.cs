using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class OneToManyCascadeDeleteConvention : IConceptualModelConvention<AssociationType>, IConvention
{
	public virtual void Apply(AssociationType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		if (!item.IsSelfReferencing() && !(item.GetConfiguration() is NavigationPropertyConfiguration { DeleteAction: not null }))
		{
			AssociationEndMember associationEndMember = null;
			if (item.IsRequiredToMany())
			{
				associationEndMember = item.SourceEnd;
			}
			else if (item.IsManyToRequired())
			{
				associationEndMember = item.TargetEnd;
			}
			if (associationEndMember != null)
			{
				associationEndMember.DeleteBehavior = OperationAction.Cascade;
			}
		}
	}
}
