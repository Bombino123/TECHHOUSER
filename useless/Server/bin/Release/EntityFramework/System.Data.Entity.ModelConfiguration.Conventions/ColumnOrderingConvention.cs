using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class ColumnOrderingConvention : IStoreModelConvention<EntityType>, IConvention
{
	public virtual void Apply(EntityType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		ValidateColumns(item, model.StoreModel.GetEntitySet(item).Table);
		OrderColumns(item.Properties).Each(delegate(EdmProperty c)
		{
			bool isPrimaryKeyColumn = c.IsPrimaryKeyColumn;
			item.RemoveMember(c);
			item.AddMember(c);
			if (isPrimaryKeyColumn)
			{
				item.AddKeyMember(c);
			}
		});
		item.ForeignKeyBuilders.Each((ForeignKeyBuilder fk) => fk.DependentColumns = OrderColumns(fk.DependentColumns));
	}

	protected virtual void ValidateColumns(EntityType table, string tableName)
	{
	}

	private static IEnumerable<EdmProperty> OrderColumns(IEnumerable<EdmProperty> columns)
	{
		return (from c in columns
			select new
			{
				Column = c,
				Order = (c.GetOrder() ?? int.MaxValue)
			} into c
			orderby c.Order
			select c.Column).ToList();
	}
}
