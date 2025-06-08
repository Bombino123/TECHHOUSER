using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Common.EntitySql;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Data.Entity.Core.Query.PlanCompiler;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal sealed class GeneratedView : InternalBase
{
	private readonly EntitySetBase m_extent;

	private readonly EdmType m_type;

	private DbQueryCommandTree m_commandTree;

	private readonly string m_eSQL;

	private Node m_internalTreeNode;

	private DiscriminatorMap m_discriminatorMap;

	private readonly StorageMappingItemCollection m_mappingItemCollection;

	private readonly ConfigViewGenerator m_config;

	internal string eSQL => m_eSQL;

	internal static GeneratedView CreateGeneratedView(EntitySetBase extent, EdmType type, DbQueryCommandTree commandTree, string eSQL, StorageMappingItemCollection mappingItemCollection, ConfigViewGenerator config)
	{
		DiscriminatorMap discriminatorMap = null;
		if (commandTree != null)
		{
			commandTree = ViewSimplifier.SimplifyView(extent, commandTree);
			if (extent.BuiltInTypeKind == BuiltInTypeKind.EntitySet)
			{
				DiscriminatorMap.TryCreateDiscriminatorMap((EntitySet)extent, commandTree.Query, out discriminatorMap);
			}
		}
		return new GeneratedView(extent, type, commandTree, eSQL, discriminatorMap, mappingItemCollection, config);
	}

	internal static GeneratedView CreateGeneratedViewForFKAssociationSet(EntitySetBase extent, EdmType type, DbQueryCommandTree commandTree, StorageMappingItemCollection mappingItemCollection, ConfigViewGenerator config)
	{
		return new GeneratedView(extent, type, commandTree, null, null, mappingItemCollection, config);
	}

	internal static bool TryParseUserSpecifiedView(EntitySetBaseMapping setMapping, EntityTypeBase type, string eSQL, bool includeSubtypes, StorageMappingItemCollection mappingItemCollection, ConfigViewGenerator config, IList<EdmSchemaError> errors, out GeneratedView generatedView)
	{
		bool flag = false;
		if (!TryParseView(eSQL, isUserSpecified: true, setMapping.Set, mappingItemCollection, config, out var commandTree, out var discriminatorMap, out var parserException))
		{
			EdmSchemaError item = new EdmSchemaError(Strings.Mapping_Invalid_QueryView2(setMapping.Set.Name, parserException.Message), 2068, EdmSchemaErrorSeverity.Error, setMapping.EntityContainerMapping.SourceLocation, setMapping.StartLineNumber, setMapping.StartLinePosition, parserException);
			errors.Add(item);
			flag = true;
		}
		else
		{
			foreach (EdmSchemaError item3 in ViewValidator.ValidateQueryView(commandTree, setMapping, type, includeSubtypes))
			{
				errors.Add(item3);
				flag = true;
			}
			if (!(commandTree.Query.ResultType.EdmType is CollectionType collectionType) || !setMapping.Set.ElementType.IsAssignableFrom(collectionType.TypeUsage.EdmType))
			{
				EdmSchemaError item2 = new EdmSchemaError(Strings.Mapping_Invalid_QueryView_Type(setMapping.Set.Name), 2069, EdmSchemaErrorSeverity.Error, setMapping.EntityContainerMapping.SourceLocation, setMapping.StartLineNumber, setMapping.StartLinePosition);
				errors.Add(item2);
				flag = true;
			}
		}
		if (!flag)
		{
			generatedView = new GeneratedView(setMapping.Set, type, commandTree, eSQL, discriminatorMap, mappingItemCollection, config);
			return true;
		}
		generatedView = null;
		return false;
	}

	private GeneratedView(EntitySetBase extent, EdmType type, DbQueryCommandTree commandTree, string eSQL, DiscriminatorMap discriminatorMap, StorageMappingItemCollection mappingItemCollection, ConfigViewGenerator config)
	{
		m_extent = extent;
		m_type = type;
		m_commandTree = commandTree;
		m_eSQL = eSQL;
		m_discriminatorMap = discriminatorMap;
		m_mappingItemCollection = mappingItemCollection;
		m_config = config;
		if (m_config.IsViewTracing)
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			ToCompactString(stringBuilder);
			Helpers.FormatTraceLine("CQL view for {0}", stringBuilder.ToString());
		}
	}

	internal DbQueryCommandTree GetCommandTree()
	{
		if (m_commandTree == null)
		{
			if (TryParseView(m_eSQL, isUserSpecified: false, m_extent, m_mappingItemCollection, m_config, out m_commandTree, out m_discriminatorMap, out var parserException))
			{
				return m_commandTree;
			}
			throw new MappingException(Strings.Mapping_Invalid_QueryView(m_extent.Name, parserException.Message));
		}
		return m_commandTree;
	}

	internal Node GetInternalTree(Command targetIqtCommand)
	{
		if (m_internalTreeNode == null)
		{
			Command command = ITreeGenerator.Generate(GetCommandTree(), m_discriminatorMap);
			PlanCompiler.Assert(command.Root.Op.OpType == OpType.PhysicalProject, "Expected a physical projectOp at the root of the tree - found " + command.Root.Op.OpType);
			command.DisableVarVecEnumCaching();
			m_internalTreeNode = command.Root.Child0;
		}
		return OpCopier.Copy(targetIqtCommand, m_internalTreeNode);
	}

	private static bool TryParseView(string eSQL, bool isUserSpecified, EntitySetBase extent, StorageMappingItemCollection mappingItemCollection, ConfigViewGenerator config, out DbQueryCommandTree commandTree, out DiscriminatorMap discriminatorMap, out Exception parserException)
	{
		commandTree = null;
		discriminatorMap = null;
		parserException = null;
		config.StartSingleWatch(PerfType.ViewParsing);
		try
		{
			ParserOptions.CompilationMode compilationMode = ParserOptions.CompilationMode.RestrictedViewGenerationMode;
			if (isUserSpecified)
			{
				compilationMode = ParserOptions.CompilationMode.UserViewGenerationMode;
			}
			commandTree = (DbQueryCommandTree)ExternalCalls.CompileView(eSQL, mappingItemCollection, compilationMode);
			commandTree = ViewSimplifier.SimplifyView(extent, commandTree);
			if (extent.BuiltInTypeKind == BuiltInTypeKind.EntitySet)
			{
				DiscriminatorMap.TryCreateDiscriminatorMap((EntitySet)extent, commandTree.Query, out discriminatorMap);
			}
		}
		catch (Exception ex)
		{
			if (!ex.IsCatchableExceptionType())
			{
				throw;
			}
			parserException = ex;
		}
		finally
		{
			config.StopSingleWatch(PerfType.ViewParsing);
		}
		return parserException == null;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		bool num = m_type != m_extent.ElementType;
		if (num)
		{
			builder.Append("OFTYPE(");
		}
		builder.AppendFormat("{0}.{1}", m_extent.EntityContainer.Name, m_extent.Name);
		if (num)
		{
			builder.Append(", ").Append(m_type.Name).Append(')');
		}
		builder.AppendLine(" = ");
		if (!string.IsNullOrEmpty(m_eSQL))
		{
			builder.Append(m_eSQL);
		}
		else
		{
			builder.Append(m_commandTree.Print());
		}
	}
}
