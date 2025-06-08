using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.EntityClient.Internal;

internal sealed class EntityProviderServices : DbProviderServices
{
	internal static readonly EntityProviderServices Instance = new EntityProviderServices();

	protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
	{
		Check.NotNull(providerManifest, "providerManifest");
		Check.NotNull(commandTree, "commandTree");
		return CreateDbCommandDefinition(providerManifest, commandTree, new DbInterceptionContext());
	}

	internal static EntityCommandDefinition CreateCommandDefinition(DbProviderFactory storeProviderFactory, DbCommandTree commandTree, DbInterceptionContext interceptionContext, IDbDependencyResolver resolver = null)
	{
		return new EntityCommandDefinition(storeProviderFactory, commandTree, interceptionContext, resolver);
	}

	internal override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree, DbInterceptionContext interceptionContext)
	{
		return CreateCommandDefinition(((StoreItemCollection)commandTree.MetadataWorkspace.GetItemCollection(DataSpace.SSpace)).ProviderFactory, commandTree, interceptionContext);
	}

	internal override void ValidateDataSpace(DbCommandTree commandTree)
	{
		if (commandTree.DataSpace != DataSpace.CSpace)
		{
			throw new ProviderIncompatibleException(Strings.EntityClient_RequiresNonStoreCommandTree);
		}
	}

	public override DbCommandDefinition CreateCommandDefinition(DbCommand prototype)
	{
		Check.NotNull(prototype, "prototype");
		return ((EntityCommand)prototype).GetCommandDefinition();
	}

	protected override string GetDbProviderManifestToken(DbConnection connection)
	{
		Check.NotNull(connection, "connection");
		if (connection.GetType() != typeof(EntityConnection))
		{
			throw new ArgumentException(Strings.Mapping_Provider_WrongConnectionType(typeof(EntityConnection)));
		}
		return MetadataItem.EdmProviderManifest.Token;
	}

	protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
	{
		Check.NotNull(manifestToken, "manifestToken");
		return MetadataItem.EdmProviderManifest;
	}
}
