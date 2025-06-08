using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.SchemaObjectModel;
using System.Data.Entity.Resources;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace System.Data.Entity.Core.Mapping;

internal class MappingItemLoader
{
	private class ModificationFunctionMappingLoader
	{
		private readonly MappingItemLoader m_parentLoader;

		private EdmFunction m_function;

		private readonly EntitySet m_entitySet;

		private readonly AssociationSet m_associationSet;

		private readonly System.Data.Entity.Core.Metadata.Edm.EntityContainer m_modelContainer;

		private readonly EdmItemCollection m_edmItemCollection;

		private readonly StoreItemCollection m_storeItemCollection;

		private bool m_allowCurrentVersion;

		private bool m_allowOriginalVersion;

		private readonly Set<FunctionParameter> m_seenParameters;

		private readonly Stack<EdmMember> m_members;

		private AssociationSet m_associationSetNavigation;

		internal ModificationFunctionMappingLoader(MappingItemLoader parentLoader, EntitySetBase extent)
		{
			m_parentLoader = parentLoader;
			m_modelContainer = extent.EntityContainer;
			m_edmItemCollection = parentLoader.EdmItemCollection;
			m_storeItemCollection = parentLoader.StoreItemCollection;
			m_entitySet = extent as EntitySet;
			if (m_entitySet == null)
			{
				m_associationSet = (AssociationSet)extent;
			}
			m_seenParameters = new Set<FunctionParameter>();
			m_members = new Stack<EdmMember>();
		}

		internal ModificationFunctionMapping LoadEntityTypeModificationFunctionMapping(XPathNavigator nav, EntitySetBase entitySet, bool allowCurrentVersion, bool allowOriginalVersion, EntityType entityType)
		{
			m_function = LoadAndValidateFunctionMetadata(nav.Clone(), out var rowsAffectedParameter);
			if (m_function == null)
			{
				return null;
			}
			m_allowCurrentVersion = allowCurrentVersion;
			m_allowOriginalVersion = allowOriginalVersion;
			IEnumerable<ModificationFunctionParameterBinding> parameterBindings = LoadParameterBindings(nav.Clone(), entityType);
			IEnumerable<ModificationFunctionResultBinding> resultBindings = LoadResultBindings(nav.Clone(), entityType);
			return new ModificationFunctionMapping(entitySet, entityType, m_function, parameterBindings, rowsAffectedParameter, resultBindings);
		}

		internal ModificationFunctionMapping LoadAssociationSetModificationFunctionMapping(XPathNavigator nav, EntitySetBase entitySet, bool isInsert)
		{
			m_function = LoadAndValidateFunctionMetadata(nav.Clone(), out var rowsAffectedParameter);
			if (m_function == null)
			{
				return null;
			}
			if (isInsert)
			{
				m_allowCurrentVersion = true;
				m_allowOriginalVersion = false;
			}
			else
			{
				m_allowCurrentVersion = false;
				m_allowOriginalVersion = true;
			}
			IEnumerable<ModificationFunctionParameterBinding> parameterBindings = LoadParameterBindings(nav.Clone(), m_associationSet.ElementType);
			return new ModificationFunctionMapping(entitySet, entitySet.ElementType, m_function, parameterBindings, rowsAffectedParameter, null);
		}

		private IEnumerable<ModificationFunctionResultBinding> LoadResultBindings(XPathNavigator nav, EntityType entityType)
		{
			List<ModificationFunctionResultBinding> list = new List<ModificationFunctionResultBinding>();
			IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
			if (nav.MoveToChild(XPathNodeType.Element))
			{
				do
				{
					if (nav.LocalName == "ResultBinding")
					{
						string aliasResolvedAttributeValue = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "Name");
						string aliasResolvedAttributeValue2 = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "ColumnName");
						EdmProperty item = null;
						if (aliasResolvedAttributeValue == null || !entityType.Properties.TryGetValue(aliasResolvedAttributeValue, ignoreCase: false, out item))
						{
							AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_ModificationFunction_PropertyNotFound, aliasResolvedAttributeValue, entityType.Name, MappingErrorCode.InvalidEdmMember, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
							return new List<ModificationFunctionResultBinding>();
						}
						ModificationFunctionResultBinding item2 = new ModificationFunctionResultBinding(aliasResolvedAttributeValue2, item);
						list.Add(item2);
					}
				}
				while (nav.MoveToNext(XPathNodeType.Element));
			}
			KeyToListMap<EdmProperty, string> keyToListMap = new KeyToListMap<EdmProperty, string>(EqualityComparer<EdmProperty>.Default);
			foreach (ModificationFunctionResultBinding item3 in list)
			{
				keyToListMap.Add(item3.Property, item3.ColumnName);
			}
			foreach (EdmProperty key in keyToListMap.Keys)
			{
				ReadOnlyCollection<string> readOnlyCollection = keyToListMap.ListForKey(key);
				if (1 < readOnlyCollection.Count)
				{
					AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_ModificationFunction_AmbiguousResultBinding, key.Name, StringUtil.ToCommaSeparatedString(readOnlyCollection), MappingErrorCode.AmbiguousResultBindingInModificationFunctionMapping, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
					return new List<ModificationFunctionResultBinding>();
				}
			}
			return list;
		}

		private IEnumerable<ModificationFunctionParameterBinding> LoadParameterBindings(XPathNavigator nav, StructuralType type)
		{
			List<ModificationFunctionParameterBinding> result = new List<ModificationFunctionParameterBinding>(LoadParameterBindings(nav.Clone(), type, restrictToKeyMembers: false));
			Set<FunctionParameter> set = new Set<FunctionParameter>(m_function.Parameters);
			set.Subtract(m_seenParameters);
			if (set.Count != 0)
			{
				AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_ModificationFunction_MissingParameter, m_function.FullName, StringUtil.ToCommaSeparatedString(set), MappingErrorCode.InvalidParameterInModificationFunctionMapping, m_parentLoader.m_sourceLocation, (IXmlLineInfo)nav, m_parentLoader.m_parsingErrors);
				return new List<ModificationFunctionParameterBinding>();
			}
			return result;
		}

		private IEnumerable<ModificationFunctionParameterBinding> LoadParameterBindings(XPathNavigator nav, StructuralType type, bool restrictToKeyMembers)
		{
			if (!nav.MoveToChild(XPathNodeType.Element))
			{
				yield break;
			}
			do
			{
				switch (nav.LocalName)
				{
				case "ScalarProperty":
				{
					ModificationFunctionParameterBinding modificationFunctionParameterBinding = LoadScalarPropertyParameterBinding(nav.Clone(), type, restrictToKeyMembers);
					if (modificationFunctionParameterBinding != null)
					{
						yield return modificationFunctionParameterBinding;
						break;
					}
					yield break;
				}
				case "ComplexProperty":
				{
					ComplexType complexType;
					EdmMember edmMember = LoadComplexTypeProperty(nav.Clone(), type, out complexType);
					if (edmMember == null)
					{
						break;
					}
					m_members.Push(edmMember);
					foreach (ModificationFunctionParameterBinding item in LoadParameterBindings(nav.Clone(), complexType, restrictToKeyMembers))
					{
						yield return item;
					}
					m_members.Pop();
					break;
				}
				case "AssociationEnd":
				{
					AssociationSetEnd associationSetEnd2 = LoadAssociationEnd(nav.Clone());
					if (associationSetEnd2 == null)
					{
						break;
					}
					m_members.Push(associationSetEnd2.CorrespondingAssociationEndMember);
					m_associationSetNavigation = associationSetEnd2.ParentAssociationSet;
					foreach (ModificationFunctionParameterBinding item2 in LoadParameterBindings(nav.Clone(), associationSetEnd2.EntitySet.ElementType, restrictToKeyMembers: true))
					{
						yield return item2;
					}
					m_associationSetNavigation = null;
					m_members.Pop();
					break;
				}
				case "EndProperty":
				{
					AssociationSetEnd associationSetEnd = LoadEndProperty(nav.Clone());
					if (associationSetEnd == null)
					{
						break;
					}
					m_members.Push(associationSetEnd.CorrespondingAssociationEndMember);
					foreach (ModificationFunctionParameterBinding item3 in LoadParameterBindings(nav.Clone(), associationSetEnd.EntitySet.ElementType, restrictToKeyMembers: true))
					{
						yield return item3;
					}
					m_members.Pop();
					break;
				}
				}
			}
			while (nav.MoveToNext(XPathNodeType.Element));
		}

		private AssociationSetEnd LoadAssociationEnd(XPathNavigator nav)
		{
			IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
			string aliasResolvedAttributeValue = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "AssociationSet");
			string aliasResolvedAttributeValue2 = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "From");
			string aliasResolvedAttributeValue3 = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "To");
			RelationshipSet relationshipSet = null;
			if (aliasResolvedAttributeValue == null || !m_modelContainer.TryGetRelationshipSetByName(aliasResolvedAttributeValue, ignoreCase: false, out relationshipSet) || BuiltInTypeKind.AssociationSet != relationshipSet.BuiltInTypeKind)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_AssociationSetDoesNotExist, aliasResolvedAttributeValue, MappingErrorCode.InvalidAssociationSet, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			AssociationSet associationSet = (AssociationSet)relationshipSet;
			AssociationSetEnd item = null;
			if (aliasResolvedAttributeValue2 == null || !associationSet.AssociationSetEnds.TryGetValue(aliasResolvedAttributeValue2, ignoreCase: false, out item))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_AssociationSetRoleDoesNotExist, aliasResolvedAttributeValue2, MappingErrorCode.InvalidAssociationSetRoleInModificationFunctionMapping, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			AssociationSetEnd item2 = null;
			if (aliasResolvedAttributeValue3 == null || !associationSet.AssociationSetEnds.TryGetValue(aliasResolvedAttributeValue3, ignoreCase: false, out item2))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_AssociationSetRoleDoesNotExist, aliasResolvedAttributeValue3, MappingErrorCode.InvalidAssociationSetRoleInModificationFunctionMapping, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			if (!item.EntitySet.Equals(m_entitySet))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_AssociationSetFromRoleIsNotEntitySet, aliasResolvedAttributeValue2, MappingErrorCode.InvalidAssociationSetRoleInModificationFunctionMapping, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			if (item2.CorrespondingAssociationEndMember.RelationshipMultiplicity != RelationshipMultiplicity.One && item2.CorrespondingAssociationEndMember.RelationshipMultiplicity != 0)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_AssociationSetCardinality, aliasResolvedAttributeValue3, MappingErrorCode.InvalidAssociationSetCardinalityInModificationFunctionMapping, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			if (associationSet.ElementType.IsForeignKey)
			{
				System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint referentialConstraint = associationSet.ElementType.ReferentialConstraints.Single();
				EdmSchemaError edmSchemaError = AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_AssociationEndMappingForeignKeyAssociation, aliasResolvedAttributeValue3, MappingErrorCode.InvalidModificationFunctionMappingAssociationEndForeignKey, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				if (item.CorrespondingAssociationEndMember != referentialConstraint.ToRole || !referentialConstraint.ToProperties.All((EdmProperty p) => m_entitySet.ElementType.KeyMembers.Contains(p)))
				{
					return null;
				}
				edmSchemaError.Severity = EdmSchemaErrorSeverity.Warning;
			}
			return item2;
		}

		private AssociationSetEnd LoadEndProperty(XPathNavigator nav)
		{
			string aliasResolvedAttributeValue = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "Name");
			AssociationSetEnd item = null;
			if (aliasResolvedAttributeValue == null || !m_associationSet.AssociationSetEnds.TryGetValue(aliasResolvedAttributeValue, ignoreCase: false, out item))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_AssociationSetRoleDoesNotExist, aliasResolvedAttributeValue, MappingErrorCode.InvalidAssociationSetRoleInModificationFunctionMapping, m_parentLoader.m_sourceLocation, (IXmlLineInfo)nav, m_parentLoader.m_parsingErrors);
				return null;
			}
			return item;
		}

		private EdmMember LoadComplexTypeProperty(XPathNavigator nav, StructuralType type, out ComplexType complexType)
		{
			IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
			string aliasResolvedAttributeValue = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "Name");
			string aliasResolvedAttributeValue2 = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "TypeName");
			EdmMember item = null;
			if (aliasResolvedAttributeValue == null || !type.Members.TryGetValue(aliasResolvedAttributeValue, ignoreCase: false, out item))
			{
				AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_ModificationFunction_PropertyNotFound, aliasResolvedAttributeValue, type.Name, MappingErrorCode.InvalidEdmMember, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				complexType = null;
				return null;
			}
			complexType = null;
			if (aliasResolvedAttributeValue2 == null || !m_edmItemCollection.TryGetItem<ComplexType>(aliasResolvedAttributeValue2, out complexType))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_ComplexTypeNotFound, aliasResolvedAttributeValue2, MappingErrorCode.InvalidComplexType, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			if (!item.TypeUsage.EdmType.Equals(complexType) && !Helper.IsSubtypeOf(item.TypeUsage.EdmType, complexType))
			{
				AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_ModificationFunction_WrongComplexType, aliasResolvedAttributeValue2, item.Name, MappingErrorCode.InvalidComplexType, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			return item;
		}

		private ModificationFunctionParameterBinding LoadScalarPropertyParameterBinding(XPathNavigator nav, StructuralType type, bool restrictToKeyMembers)
		{
			IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
			string aliasResolvedAttributeValue = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "ParameterName");
			string aliasResolvedAttributeValue2 = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "Name");
			string aliasResolvedAttributeValue3 = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "Version");
			bool flag = false;
			if (aliasResolvedAttributeValue3 == null)
			{
				if (!m_allowOriginalVersion)
				{
					flag = true;
				}
				else
				{
					if (m_allowCurrentVersion)
					{
						AddToSchemaErrors(Strings.Mapping_ModificationFunction_MissingVersion, MappingErrorCode.MissingVersionInModificationFunctionMapping, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
						return null;
					}
					flag = false;
				}
			}
			else
			{
				flag = aliasResolvedAttributeValue3 == "Current";
			}
			if (flag && !m_allowCurrentVersion)
			{
				AddToSchemaErrors(Strings.Mapping_ModificationFunction_VersionMustBeOriginal, MappingErrorCode.InvalidVersionInModificationFunctionMapping, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			if (!flag && !m_allowOriginalVersion)
			{
				AddToSchemaErrors(Strings.Mapping_ModificationFunction_VersionMustBeCurrent, MappingErrorCode.InvalidVersionInModificationFunctionMapping, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			FunctionParameter item = null;
			if (aliasResolvedAttributeValue == null || !m_function.Parameters.TryGetValue(aliasResolvedAttributeValue, ignoreCase: false, out item))
			{
				AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_ModificationFunction_ParameterNotFound, aliasResolvedAttributeValue, m_function.Name, MappingErrorCode.InvalidParameterInModificationFunctionMapping, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			EdmMember item2 = null;
			if (restrictToKeyMembers)
			{
				if (aliasResolvedAttributeValue2 == null || !((EntityType)type).KeyMembers.TryGetValue(aliasResolvedAttributeValue2, ignoreCase: false, out item2))
				{
					AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_ModificationFunction_PropertyNotKey, aliasResolvedAttributeValue2, type.Name, MappingErrorCode.InvalidEdmMember, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
					return null;
				}
			}
			else if (aliasResolvedAttributeValue2 == null || !type.Members.TryGetValue(aliasResolvedAttributeValue2, ignoreCase: false, out item2))
			{
				AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_ModificationFunction_PropertyNotFound, aliasResolvedAttributeValue2, type.Name, MappingErrorCode.InvalidEdmMember, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			if (m_seenParameters.Contains(item))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_ParameterBoundTwice, aliasResolvedAttributeValue, MappingErrorCode.ParameterBoundTwiceInModificationFunctionMapping, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			int count = m_parentLoader.m_parsingErrors.Count;
			if (Helper.ValidateAndConvertTypeUsage(item2.TypeUsage, item.TypeUsage) == null && count == m_parentLoader.m_parsingErrors.Count)
			{
				AddToSchemaErrorWithMessage(Strings.Mapping_ModificationFunction_PropertyParameterTypeMismatch(item2.TypeUsage.EdmType, item2.Name, item2.DeclaringType.FullName, item.TypeUsage.EdmType, item.Name, m_function.FullName), MappingErrorCode.InvalidModificationFunctionMappingPropertyParameterTypeMismatch, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
			}
			m_members.Push(item2);
			IEnumerable<EdmMember> members = m_members;
			AssociationSet associationSet = m_associationSetNavigation;
			if (m_members.Last().BuiltInTypeKind == BuiltInTypeKind.AssociationEndMember)
			{
				AssociationEndMember associationEndMember = (AssociationEndMember)m_members.Last();
				AssociationType associationType = (AssociationType)associationEndMember.DeclaringType;
				if (associationType.IsForeignKey)
				{
					System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint referentialConstraint = associationType.ReferentialConstraints.Single();
					if (referentialConstraint.FromRole == associationEndMember)
					{
						int index = referentialConstraint.FromProperties.IndexOf((EdmProperty)m_members.First());
						members = new EdmMember[1] { referentialConstraint.ToProperties[index] };
						associationSet = null;
					}
				}
			}
			ModificationFunctionParameterBinding result = new ModificationFunctionParameterBinding(item, new ModificationFunctionMemberPath(members, associationSet), flag);
			m_members.Pop();
			m_seenParameters.Add(item);
			return result;
		}

		private EdmFunction LoadAndValidateFunctionMetadata(XPathNavigator nav, out FunctionParameter rowsAffectedParameter)
		{
			IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
			m_seenParameters.Clear();
			string aliasResolvedAttributeValue = m_parentLoader.GetAliasResolvedAttributeValue(nav.Clone(), "FunctionName");
			rowsAffectedParameter = null;
			ReadOnlyCollection<EdmFunction> functions = m_storeItemCollection.GetFunctions(aliasResolvedAttributeValue);
			if (functions.Count == 0)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_UnknownFunction, aliasResolvedAttributeValue, MappingErrorCode.InvalidModificationFunctionMappingUnknownFunction, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			if (1 < functions.Count)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_AmbiguousFunction, aliasResolvedAttributeValue, MappingErrorCode.InvalidModificationFunctionMappingAmbiguousFunction, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			EdmFunction edmFunction = functions[0];
			if (MetadataHelper.IsComposable(edmFunction))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_NotValidFunction, aliasResolvedAttributeValue, MappingErrorCode.InvalidModificationFunctionMappingNotValidFunction, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
				return null;
			}
			string attributeValue = GetAttributeValue(nav, "RowsAffectedParameter");
			if (!string.IsNullOrEmpty(attributeValue))
			{
				if (!edmFunction.Parameters.TryGetValue(attributeValue, ignoreCase: false, out rowsAffectedParameter))
				{
					AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_RowsAffectedParameterDoesNotExist(attributeValue, edmFunction.FullName), MappingErrorCode.MappingFunctionImportRowsAffectedParameterDoesNotExist, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
					return null;
				}
				if (ParameterMode.Out != rowsAffectedParameter.Mode && ParameterMode.InOut != rowsAffectedParameter.Mode)
				{
					AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_RowsAffectedParameterHasWrongMode(attributeValue, rowsAffectedParameter.Mode, ParameterMode.Out, ParameterMode.InOut), MappingErrorCode.MappingFunctionImportRowsAffectedParameterHasWrongMode, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
					return null;
				}
				PrimitiveType primitiveType = (PrimitiveType)rowsAffectedParameter.TypeUsage.EdmType;
				if (!TypeSemantics.IsIntegerNumericType(rowsAffectedParameter.TypeUsage))
				{
					AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_RowsAffectedParameterHasWrongType(attributeValue, primitiveType.PrimitiveTypeKind), MappingErrorCode.MappingFunctionImportRowsAffectedParameterHasWrongType, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
					return null;
				}
				m_seenParameters.Add(rowsAffectedParameter);
			}
			foreach (FunctionParameter parameter in edmFunction.Parameters)
			{
				if (parameter.Mode != 0 && attributeValue != parameter.Name)
				{
					AddToSchemaErrorWithMessage(Strings.Mapping_ModificationFunction_NotValidFunctionParameter(aliasResolvedAttributeValue, parameter.Name, "RowsAffectedParameter"), MappingErrorCode.InvalidModificationFunctionMappingNotValidFunctionParameter, m_parentLoader.m_sourceLocation, lineInfo, m_parentLoader.m_parsingErrors);
					return null;
				}
			}
			return edmFunction;
		}
	}

	private readonly Dictionary<string, string> m_alias;

	private readonly StorageMappingItemCollection m_storageMappingItemCollection;

	private readonly string m_sourceLocation;

	private readonly List<EdmSchemaError> m_parsingErrors;

	private readonly Dictionary<EdmMember, KeyValuePair<TypeUsage, TypeUsage>> m_scalarMemberMappings;

	private bool m_hasQueryViews;

	private string m_currentNamespaceUri;

	private readonly EntityContainerMapping m_containerMapping;

	private readonly double m_version;

	private static XmlSchemaSet s_mappingXmlSchema;

	internal double MappingVersion => m_version;

	internal IList<EdmSchemaError> ParsingErrors => m_parsingErrors;

	internal bool HasQueryViews => m_hasQueryViews;

	internal EntityContainerMapping ContainerMapping => m_containerMapping;

	private EdmItemCollection EdmItemCollection => m_storageMappingItemCollection.EdmItemCollection;

	private StoreItemCollection StoreItemCollection => m_storageMappingItemCollection.StoreItemCollection;

	internal MappingItemLoader(XmlReader reader, StorageMappingItemCollection storageMappingItemCollection, string fileName, Dictionary<EdmMember, KeyValuePair<TypeUsage, TypeUsage>> scalarMemberMappings)
	{
		m_storageMappingItemCollection = storageMappingItemCollection;
		m_alias = new Dictionary<string, string>(StringComparer.Ordinal);
		if (fileName != null)
		{
			m_sourceLocation = fileName;
		}
		else
		{
			m_sourceLocation = null;
		}
		m_parsingErrors = new List<EdmSchemaError>();
		m_scalarMemberMappings = scalarMemberMappings;
		m_containerMapping = LoadMappingItems(reader);
		if (m_currentNamespaceUri != null)
		{
			if (m_currentNamespaceUri == "urn:schemas-microsoft-com:windows:storage:mapping:CS")
			{
				m_version = 1.0;
			}
			else if (m_currentNamespaceUri == "http://schemas.microsoft.com/ado/2008/09/mapping/cs")
			{
				m_version = 2.0;
			}
			else
			{
				m_version = 3.0;
			}
		}
	}

	private EntityContainerMapping LoadMappingItems(XmlReader innerReader)
	{
		XmlReader schemaValidatingReader = GetSchemaValidatingReader(innerReader);
		try
		{
			XPathDocument xPathDocument = new XPathDocument(schemaValidatingReader);
			if (m_parsingErrors.Count != 0 && !MetadataHelper.CheckIfAllErrorsAreWarnings(m_parsingErrors))
			{
				return null;
			}
			XPathNavigator xPathNavigator = xPathDocument.CreateNavigator();
			return LoadMappingItems(xPathNavigator.Clone());
		}
		catch (XmlException ex)
		{
			EdmSchemaError item = new EdmSchemaError(Strings.Mapping_InvalidMappingSchema_Parsing(ex.Message), 2024, EdmSchemaErrorSeverity.Error, m_sourceLocation, ex.LineNumber, ex.LinePosition);
			m_parsingErrors.Add(item);
		}
		return null;
	}

	private EntityContainerMapping LoadMappingItems(XPathNavigator nav)
	{
		if (!MoveToRootElement(nav) || nav.NodeType != XPathNodeType.Element)
		{
			AddToSchemaErrors(Strings.Mapping_Invalid_CSRootElementMissing("urn:schemas-microsoft-com:windows:storage:mapping:CS", "http://schemas.microsoft.com/ado/2008/09/mapping/cs", "http://schemas.microsoft.com/ado/2009/11/mapping/cs"), MappingErrorCode.RootMappingElementMissing, m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
			return null;
		}
		EntityContainerMapping result = LoadMappingChildNodes(nav.Clone());
		if (m_parsingErrors.Count != 0 && !MetadataHelper.CheckIfAllErrorsAreWarnings(m_parsingErrors))
		{
			result = null;
		}
		return result;
	}

	private bool MoveToRootElement(XPathNavigator nav)
	{
		if (nav.MoveToChild("Mapping", "http://schemas.microsoft.com/ado/2009/11/mapping/cs"))
		{
			m_currentNamespaceUri = "http://schemas.microsoft.com/ado/2009/11/mapping/cs";
			return true;
		}
		if (nav.MoveToChild("Mapping", "http://schemas.microsoft.com/ado/2008/09/mapping/cs"))
		{
			m_currentNamespaceUri = "http://schemas.microsoft.com/ado/2008/09/mapping/cs";
			return true;
		}
		if (nav.MoveToChild("Mapping", "urn:schemas-microsoft-com:windows:storage:mapping:CS"))
		{
			m_currentNamespaceUri = "urn:schemas-microsoft-com:windows:storage:mapping:CS";
			return true;
		}
		return false;
	}

	private EntityContainerMapping LoadMappingChildNodes(XPathNavigator nav)
	{
		bool flag;
		if (nav.MoveToChild("Alias", m_currentNamespaceUri))
		{
			do
			{
				m_alias.Add(GetAttributeValue(nav.Clone(), "Key"), GetAttributeValue(nav.Clone(), "Value"));
			}
			while (nav.MoveToNext("Alias", m_currentNamespaceUri));
			flag = nav.MoveToNext(XPathNodeType.Element);
		}
		else
		{
			flag = nav.MoveToChild(XPathNodeType.Element);
		}
		if (!flag)
		{
			return null;
		}
		return LoadEntityContainerMapping(nav.Clone());
	}

	private EntityContainerMapping LoadEntityContainerMapping(XPathNavigator nav)
	{
		IXmlLineInfo xmlLineInfo = (IXmlLineInfo)nav;
		string attributeValue = GetAttributeValue(nav.Clone(), "CdmEntityContainer");
		string attributeValue2 = GetAttributeValue(nav.Clone(), "StorageEntityContainer");
		bool boolAttributeValue = GetBoolAttributeValue(nav.Clone(), "GenerateUpdateViews", defaultValue: true);
		System.Data.Entity.Core.Metadata.Edm.EntityContainer entityContainer;
		if (m_storageMappingItemCollection.TryGetItem<EntityContainerMapping>(attributeValue, out var item))
		{
			System.Data.Entity.Core.Metadata.Edm.EntityContainer edmEntityContainer = item.EdmEntityContainer;
			entityContainer = item.StorageEntityContainer;
			if (attributeValue2 != entityContainer.Name)
			{
				AddToSchemaErrors(Strings.StorageEntityContainerNameMismatchWhileSpecifyingPartialMapping(attributeValue2, entityContainer.Name, edmEntityContainer.Name), MappingErrorCode.StorageEntityContainerNameMismatchWhileSpecifyingPartialMapping, m_sourceLocation, xmlLineInfo, m_parsingErrors);
				return null;
			}
		}
		else
		{
			if (m_storageMappingItemCollection.ContainsStorageEntityContainer(attributeValue2))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_AlreadyMapped_StorageEntityContainer, attributeValue2, MappingErrorCode.AlreadyMappedStorageEntityContainer, m_sourceLocation, xmlLineInfo, m_parsingErrors);
				return null;
			}
			EdmItemCollection.TryGetEntityContainer(attributeValue, out var edmEntityContainer);
			if (edmEntityContainer == null)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_EntityContainer, attributeValue, MappingErrorCode.InvalidEntityContainer, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			}
			StoreItemCollection.TryGetEntityContainer(attributeValue2, out entityContainer);
			if (entityContainer == null)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_StorageEntityContainer, attributeValue2, MappingErrorCode.InvalidEntityContainer, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			}
			if (edmEntityContainer == null || entityContainer == null)
			{
				return null;
			}
			item = new EntityContainerMapping(edmEntityContainer, entityContainer, m_storageMappingItemCollection, boolAttributeValue, boolAttributeValue);
			item.StartLineNumber = xmlLineInfo.LineNumber;
			item.StartLinePosition = xmlLineInfo.LinePosition;
		}
		LoadEntityContainerMappingChildNodes(nav.Clone(), item, entityContainer);
		return item;
	}

	private void LoadEntityContainerMappingChildNodes(XPathNavigator nav, EntityContainerMapping entityContainerMapping, System.Data.Entity.Core.Metadata.Edm.EntityContainer storageEntityContainerType)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		bool flag = false;
		if (nav.MoveToChild(XPathNodeType.Element))
		{
			do
			{
				switch (nav.LocalName)
				{
				case "EntitySetMapping":
					LoadEntitySetMapping(nav.Clone(), entityContainerMapping, storageEntityContainerType);
					flag = true;
					break;
				case "AssociationSetMapping":
					LoadAssociationSetMapping(nav.Clone(), entityContainerMapping, storageEntityContainerType);
					break;
				case "FunctionImportMapping":
					LoadFunctionImportMapping(nav.Clone(), entityContainerMapping);
					break;
				default:
					AddToSchemaErrors(Strings.Mapping_InvalidContent_Container_SubElement, MappingErrorCode.SetMappingExpected, m_sourceLocation, lineInfo, m_parsingErrors);
					break;
				}
			}
			while (nav.MoveToNext(XPathNodeType.Element));
		}
		if (entityContainerMapping.EdmEntityContainer.BaseEntitySets.Count != 0 && !flag)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.ViewGen_Missing_Sets_Mapping, entityContainerMapping.EdmEntityContainer.Name, MappingErrorCode.EmptyContainerMapping, m_sourceLocation, lineInfo, m_parsingErrors);
			return;
		}
		ValidateFunctionAssociationFunctionMappingUnique(nav.Clone(), entityContainerMapping);
		ValidateModificationFunctionMappingConsistentForAssociations(nav.Clone(), entityContainerMapping);
		ValidateQueryViewsClosure(nav.Clone(), entityContainerMapping);
		ValidateEntitySetFunctionMappingClosure(nav.Clone(), entityContainerMapping);
		entityContainerMapping.SourceLocation = m_sourceLocation;
	}

	private void ValidateModificationFunctionMappingConsistentForAssociations(XPathNavigator nav, EntityContainerMapping entityContainerMapping)
	{
		foreach (EntitySetMapping entitySetMap in entityContainerMapping.EntitySetMaps)
		{
			if (entitySetMap.ModificationFunctionMappings.Count <= 0)
			{
				continue;
			}
			Set<AssociationSetEnd> expectedEnds = new Set<AssociationSetEnd>(entitySetMap.ImplicitlyMappedAssociationSetEnds).MakeReadOnly();
			foreach (EntityTypeModificationFunctionMapping modificationFunctionMapping in entitySetMap.ModificationFunctionMappings)
			{
				if (modificationFunctionMapping.DeleteFunctionMapping != null)
				{
					ValidateModificationFunctionMappingConsistentForAssociations(nav, entitySetMap, modificationFunctionMapping, modificationFunctionMapping.DeleteFunctionMapping, expectedEnds, "DeleteFunction");
				}
				if (modificationFunctionMapping.InsertFunctionMapping != null)
				{
					ValidateModificationFunctionMappingConsistentForAssociations(nav, entitySetMap, modificationFunctionMapping, modificationFunctionMapping.InsertFunctionMapping, expectedEnds, "InsertFunction");
				}
				if (modificationFunctionMapping.UpdateFunctionMapping != null)
				{
					ValidateModificationFunctionMappingConsistentForAssociations(nav, entitySetMap, modificationFunctionMapping, modificationFunctionMapping.UpdateFunctionMapping, expectedEnds, "UpdateFunction");
				}
			}
		}
	}

	private void ValidateModificationFunctionMappingConsistentForAssociations(XPathNavigator nav, EntitySetMapping entitySetMapping, EntityTypeModificationFunctionMapping entityTypeMapping, ModificationFunctionMapping functionMapping, Set<AssociationSetEnd> expectedEnds, string elementName)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		Set<AssociationSetEnd> set = new Set<AssociationSetEnd>(functionMapping.CollocatedAssociationSetEnds);
		set.MakeReadOnly();
		foreach (AssociationSetEnd expectedEnd in expectedEnds)
		{
			if (MetadataHelper.IsAssociationValidForEntityType(expectedEnd, entityTypeMapping.EntityType) && !set.Contains(expectedEnd))
			{
				AddToSchemaErrorWithMessage(Strings.Mapping_ModificationFunction_AssociationSetNotMappedForOperation(entitySetMapping.Set.Name, expectedEnd.ParentAssociationSet.Name, elementName, entityTypeMapping.EntityType.FullName), MappingErrorCode.InvalidModificationFunctionMappingAssociationSetNotMappedForOperation, m_sourceLocation, lineInfo, m_parsingErrors);
			}
		}
		foreach (AssociationSetEnd item in set)
		{
			if (!MetadataHelper.IsAssociationValidForEntityType(item, entityTypeMapping.EntityType))
			{
				AddToSchemaErrorWithMessage(Strings.Mapping_ModificationFunction_AssociationEndMappingInvalidForEntityType(entityTypeMapping.EntityType.FullName, item.ParentAssociationSet.Name, MetadataHelper.GetEntityTypeForEnd(MetadataHelper.GetOppositeEnd(item).CorrespondingAssociationEndMember).FullName), MappingErrorCode.InvalidModificationFunctionMappingAssociationEndMappingInvalidForEntityType, m_sourceLocation, lineInfo, m_parsingErrors);
			}
		}
	}

	private void ValidateFunctionAssociationFunctionMappingUnique(XPathNavigator nav, EntityContainerMapping entityContainerMapping)
	{
		Dictionary<EntitySetBase, int> dictionary = new Dictionary<EntitySetBase, int>();
		foreach (EntitySetMapping entitySetMap in entityContainerMapping.EntitySetMaps)
		{
			if (entitySetMap.ModificationFunctionMappings.Count <= 0)
			{
				continue;
			}
			Set<EntitySetBase> set = new Set<EntitySetBase>();
			foreach (AssociationSetEnd implicitlyMappedAssociationSetEnd in entitySetMap.ImplicitlyMappedAssociationSetEnds)
			{
				set.Add(implicitlyMappedAssociationSetEnd.ParentAssociationSet);
			}
			foreach (EntitySetBase item in set)
			{
				IncrementCount(dictionary, item);
			}
		}
		foreach (AssociationSetMapping relationshipSetMap in entityContainerMapping.RelationshipSetMaps)
		{
			if (relationshipSetMap.ModificationFunctionMapping != null)
			{
				IncrementCount(dictionary, relationshipSetMap.Set);
			}
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<EntitySetBase, int> item2 in dictionary)
		{
			if (item2.Value > 1)
			{
				list.Add(item2.Key.Name);
			}
		}
		if (0 < list.Count)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_AssociationSetAmbiguous, StringUtil.ToCommaSeparatedString(list), MappingErrorCode.AmbiguousModificationFunctionMappingForAssociationSet, m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
		}
	}

	private static void IncrementCount<T>(Dictionary<T, int> counts, T key)
	{
		int value = ((!counts.TryGetValue(key, out value)) ? 1 : (value + 1));
		counts[key] = value;
	}

	private void ValidateEntitySetFunctionMappingClosure(XPathNavigator nav, EntityContainerMapping entityContainerMapping)
	{
		KeyToListMap<EntitySet, EntitySetBaseMapping> keyToListMap = new KeyToListMap<EntitySet, EntitySetBaseMapping>(EqualityComparer<EntitySet>.Default);
		foreach (EntitySetBaseMapping allSetMap in entityContainerMapping.AllSetMaps)
		{
			foreach (TypeMapping typeMapping in allSetMap.TypeMappings)
			{
				foreach (MappingFragment mappingFragment in typeMapping.MappingFragments)
				{
					keyToListMap.Add(mappingFragment.TableSet, allSetMap);
				}
			}
		}
		Set<EntitySetBase> implicitMappedAssociationSets = new Set<EntitySetBase>();
		foreach (EntitySetMapping entitySetMap in entityContainerMapping.EntitySetMaps)
		{
			if (entitySetMap.ModificationFunctionMappings.Count <= 0)
			{
				continue;
			}
			foreach (AssociationSetEnd implicitlyMappedAssociationSetEnd in entitySetMap.ImplicitlyMappedAssociationSetEnds)
			{
				implicitMappedAssociationSets.Add(implicitlyMappedAssociationSetEnd.ParentAssociationSet);
			}
		}
		foreach (EntitySet key in keyToListMap.Keys)
		{
			if (keyToListMap.ListForKey(key).Any((EntitySetBaseMapping s) => s.HasModificationFunctionMapping || implicitMappedAssociationSets.Any((EntitySetBase aset) => aset == s.Set)) && keyToListMap.ListForKey(key).Any((EntitySetBaseMapping s) => !s.HasModificationFunctionMapping && !implicitMappedAssociationSets.Any((EntitySetBase aset) => aset == s.Set)))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_MissingSetClosure, StringUtil.ToCommaSeparatedString(from s in keyToListMap.ListForKey(key)
					where !s.HasModificationFunctionMapping
					select s.Set.Name), MappingErrorCode.MissingSetClosureInModificationFunctionMapping, m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
			}
		}
	}

	private static void ValidateClosureAmongSets(EntityContainerMapping entityContainerMapping, Set<EntitySetBase> sets, Set<EntitySetBase> additionalSetsInClosure)
	{
		bool flag;
		do
		{
			flag = false;
			List<EntitySetBase> list = new List<EntitySetBase>();
			foreach (EntitySetBase item in additionalSetsInClosure)
			{
				if (!(item is AssociationSet associationSet) || associationSet.ElementType.IsForeignKey)
				{
					continue;
				}
				foreach (AssociationSetEnd associationSetEnd in associationSet.AssociationSetEnds)
				{
					if (!additionalSetsInClosure.Contains(associationSetEnd.EntitySet))
					{
						list.Add(associationSetEnd.EntitySet);
					}
				}
			}
			foreach (EntitySetBase baseEntitySet in entityContainerMapping.EdmEntityContainer.BaseEntitySets)
			{
				if (!(baseEntitySet is AssociationSet associationSet2) || associationSet2.ElementType.IsForeignKey || additionalSetsInClosure.Contains(associationSet2))
				{
					continue;
				}
				foreach (AssociationSetEnd associationSetEnd2 in associationSet2.AssociationSetEnds)
				{
					if (additionalSetsInClosure.Contains(associationSetEnd2.EntitySet))
					{
						list.Add(associationSet2);
						break;
					}
				}
			}
			if (0 < list.Count)
			{
				flag = true;
				additionalSetsInClosure.AddRange(list);
			}
		}
		while (flag);
		additionalSetsInClosure.Subtract(sets);
	}

	private void ValidateQueryViewsClosure(XPathNavigator nav, EntityContainerMapping entityContainerMapping)
	{
		if (!m_hasQueryViews)
		{
			return;
		}
		Set<EntitySetBase> set = new Set<EntitySetBase>();
		Set<EntitySetBase> set2 = new Set<EntitySetBase>();
		foreach (EntitySetBaseMapping allSetMap in entityContainerMapping.AllSetMaps)
		{
			if (allSetMap.QueryView != null)
			{
				set.Add(allSetMap.Set);
			}
		}
		set2.AddRange(set);
		ValidateClosureAmongSets(entityContainerMapping, set, set2);
		if (0 < set2.Count)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_Invalid_Query_Views_MissingSetClosure, StringUtil.ToCommaSeparatedString(set2), MappingErrorCode.MissingSetClosureInQueryViews, m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
		}
	}

	private void LoadEntitySetMapping(XPathNavigator nav, EntityContainerMapping entityContainerMapping, System.Data.Entity.Core.Metadata.Edm.EntityContainer storageEntityContainerType)
	{
		string aliasResolvedAttributeValue = GetAliasResolvedAttributeValue(nav.Clone(), "Name");
		string attributeValue = GetAttributeValue(nav.Clone(), "TypeName");
		string aliasResolvedAttributeValue2 = GetAliasResolvedAttributeValue(nav.Clone(), "StoreEntitySet");
		bool boolAttributeValue = GetBoolAttributeValue(nav.Clone(), "MakeColumnsDistinct", defaultValue: false);
		EntitySetMapping entitySetMapping = (EntitySetMapping)entityContainerMapping.GetEntitySetMapping(aliasResolvedAttributeValue);
		IXmlLineInfo xmlLineInfo = (IXmlLineInfo)nav;
		EntitySet entitySet;
		if (entitySetMapping == null)
		{
			if (!entityContainerMapping.EdmEntityContainer.TryGetEntitySetByName(aliasResolvedAttributeValue, ignoreCase: false, out entitySet))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Entity_Set, aliasResolvedAttributeValue, MappingErrorCode.InvalidEntitySet, m_sourceLocation, xmlLineInfo, m_parsingErrors);
				return;
			}
			entitySetMapping = new EntitySetMapping(entitySet, entityContainerMapping);
		}
		else
		{
			entitySet = (EntitySet)entitySetMapping.Set;
		}
		entitySetMapping.StartLineNumber = xmlLineInfo.LineNumber;
		entitySetMapping.StartLinePosition = xmlLineInfo.LinePosition;
		entityContainerMapping.AddSetMapping(entitySetMapping);
		if (string.IsNullOrEmpty(attributeValue))
		{
			if (nav.MoveToChild(XPathNodeType.Element))
			{
				do
				{
					switch (nav.LocalName)
					{
					case "EntityTypeMapping":
						aliasResolvedAttributeValue2 = GetAliasResolvedAttributeValue(nav.Clone(), "StoreEntitySet");
						LoadEntityTypeMapping(nav.Clone(), entitySetMapping, aliasResolvedAttributeValue2, storageEntityContainerType, distinctFlagAboveType: false, entityContainerMapping.GenerateUpdateViews);
						break;
					case "QueryView":
						if (!string.IsNullOrEmpty(aliasResolvedAttributeValue2))
						{
							AddToSchemaErrorsWithMemberInfo(Strings.Mapping_TableName_QueryView, aliasResolvedAttributeValue, MappingErrorCode.TableNameAttributeWithQueryView, m_sourceLocation, xmlLineInfo, m_parsingErrors);
							return;
						}
						if (!LoadQueryView(nav.Clone(), entitySetMapping))
						{
							return;
						}
						break;
					default:
						AddToSchemaErrors(Strings.Mapping_InvalidContent_TypeMapping_QueryView, MappingErrorCode.InvalidContent, m_sourceLocation, xmlLineInfo, m_parsingErrors);
						break;
					}
				}
				while (nav.MoveToNext(XPathNodeType.Element));
			}
		}
		else
		{
			LoadEntityTypeMapping(nav.Clone(), entitySetMapping, aliasResolvedAttributeValue2, storageEntityContainerType, boolAttributeValue, entityContainerMapping.GenerateUpdateViews);
		}
		ValidateAllEntityTypesHaveFunctionMapping(nav.Clone(), entitySetMapping);
		if (entitySetMapping.HasNoContent)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Emtpty_SetMap, entitySet.Name, MappingErrorCode.EmptySetMapping, m_sourceLocation, xmlLineInfo, m_parsingErrors);
		}
	}

	private void ValidateAllEntityTypesHaveFunctionMapping(XPathNavigator nav, EntitySetMapping setMapping)
	{
		Set<EdmType> set = new Set<EdmType>();
		foreach (EntityTypeModificationFunctionMapping modificationFunctionMapping in setMapping.ModificationFunctionMappings)
		{
			set.Add(modificationFunctionMapping.EntityType);
		}
		if (0 >= set.Count)
		{
			return;
		}
		Set<EdmType> set2 = new Set<EdmType>(MetadataHelper.GetTypeAndSubtypesOf(setMapping.Set.ElementType, EdmItemCollection, includeAbstractTypes: false));
		set2.Subtract(set);
		Set<EdmType> set3 = new Set<EdmType>();
		foreach (EntityType item in set2)
		{
			if (item.Abstract)
			{
				set3.Add(item);
			}
		}
		set2.Subtract(set3);
		if (0 < set2.Count)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_MissingEntityType, StringUtil.ToCommaSeparatedString(set2), MappingErrorCode.MissingModificationFunctionMappingForEntityType, m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
		}
	}

	private bool TryParseEntityTypeAttribute(XPathNavigator nav, EntityType rootEntityType, Func<EntityType, string> typeNotAssignableMessage, out Set<EntityType> isOfTypeEntityTypes, out Set<EntityType> entityTypes)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		string attributeValue = GetAttributeValue(nav.Clone(), "TypeName");
		isOfTypeEntityTypes = new Set<EntityType>();
		entityTypes = new Set<EntityType>();
		foreach (string item2 in from s in attributeValue.Split(new char[1] { ';' })
			select s.Trim())
		{
			bool flag = item2.StartsWith("IsTypeOf(", StringComparison.Ordinal);
			string text;
			if (flag)
			{
				if (!item2.EndsWith(")", StringComparison.Ordinal))
				{
					AddToSchemaErrorWithMessage(Strings.Mapping_InvalidContent_IsTypeOfNotTerminated, MappingErrorCode.InvalidEntityType, m_sourceLocation, lineInfo, m_parsingErrors);
					return false;
				}
				text = item2.Substring("IsTypeOf(".Length);
				text = text.Substring(0, text.Length - ")".Length).Trim();
			}
			else
			{
				text = item2;
			}
			text = GetAliasResolvedValue(text);
			if (!EdmItemCollection.TryGetItem<EntityType>(text, out var item))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Entity_Type, text, MappingErrorCode.InvalidEntityType, m_sourceLocation, lineInfo, m_parsingErrors);
				return false;
			}
			if (!Helper.IsAssignableFrom(rootEntityType, item))
			{
				AddToSchemaErrorWithMessage(typeNotAssignableMessage(item), MappingErrorCode.InvalidEntityType, m_sourceLocation, lineInfo, m_parsingErrors);
				return false;
			}
			if (item.Abstract)
			{
				if (!flag)
				{
					AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_AbstractEntity_Type, item.FullName, MappingErrorCode.MappingOfAbstractType, m_sourceLocation, lineInfo, m_parsingErrors);
					return false;
				}
				if (!MetadataHelper.GetTypeAndSubtypesOf(item, EdmItemCollection, includeAbstractTypes: false).GetEnumerator().MoveNext())
				{
					AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_AbstractEntity_IsOfType, item.FullName, MappingErrorCode.MappingOfAbstractType, m_sourceLocation, lineInfo, m_parsingErrors);
					return false;
				}
			}
			if (flag)
			{
				isOfTypeEntityTypes.Add(item);
			}
			else
			{
				entityTypes.Add(item);
			}
		}
		return true;
	}

	private void LoadEntityTypeMapping(XPathNavigator nav, EntitySetMapping entitySetMapping, string tableName, System.Data.Entity.Core.Metadata.Edm.EntityContainer storageEntityContainerType, bool distinctFlagAboveType, bool generateUpdateViews)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		EntityTypeMapping entityTypeMapping = new EntityTypeMapping(entitySetMapping);
		EntityType rootEntityType = (EntityType)entitySetMapping.Set.ElementType;
		if (!TryParseEntityTypeAttribute(nav.Clone(), rootEntityType, (EntityType e) => Strings.Mapping_InvalidContent_Entity_Type_For_Entity_Set(e.FullName, rootEntityType.FullName, entitySetMapping.Set.Name), out var isOfTypeEntityTypes, out var entityTypes))
		{
			return;
		}
		foreach (EntityType item in entityTypes)
		{
			entityTypeMapping.AddType(item);
		}
		foreach (EntityType item2 in isOfTypeEntityTypes)
		{
			entityTypeMapping.AddIsOfType(item2);
		}
		if (string.IsNullOrEmpty(tableName))
		{
			if (!nav.MoveToChild(XPathNodeType.Element))
			{
				return;
			}
			do
			{
				if (nav.LocalName == "ModificationFunctionMapping")
				{
					entitySetMapping.HasModificationFunctionMapping = true;
					LoadEntityTypeModificationFunctionMapping(nav.Clone(), entitySetMapping, entityTypeMapping);
					continue;
				}
				if (nav.LocalName != "MappingFragment")
				{
					AddToSchemaErrors(Strings.Mapping_InvalidContent_Table_Expected, MappingErrorCode.TableMappingFragmentExpected, m_sourceLocation, lineInfo, m_parsingErrors);
					continue;
				}
				bool boolAttributeValue = GetBoolAttributeValue(nav.Clone(), "MakeColumnsDistinct", defaultValue: false);
				if (generateUpdateViews && boolAttributeValue)
				{
					AddToSchemaErrors(Strings.Mapping_DistinctFlagInReadWriteContainer, MappingErrorCode.DistinctFragmentInReadWriteContainer, m_sourceLocation, lineInfo, m_parsingErrors);
				}
				tableName = GetAliasResolvedAttributeValue(nav.Clone(), "StoreEntitySet");
				MappingFragment mappingFragment = LoadMappingFragment(nav.Clone(), entityTypeMapping, tableName, storageEntityContainerType, boolAttributeValue);
				if (mappingFragment != null)
				{
					entityTypeMapping.AddFragment(mappingFragment);
				}
			}
			while (nav.MoveToNext(XPathNodeType.Element));
		}
		else
		{
			if (nav.LocalName == "ModificationFunctionMapping")
			{
				AddToSchemaErrors(Strings.Mapping_ModificationFunction_In_Table_Context, MappingErrorCode.InvalidTableNameAttributeWithModificationFunctionMapping, m_sourceLocation, lineInfo, m_parsingErrors);
			}
			if (generateUpdateViews && distinctFlagAboveType)
			{
				AddToSchemaErrors(Strings.Mapping_DistinctFlagInReadWriteContainer, MappingErrorCode.DistinctFragmentInReadWriteContainer, m_sourceLocation, lineInfo, m_parsingErrors);
			}
			MappingFragment mappingFragment2 = LoadMappingFragment(nav.Clone(), entityTypeMapping, tableName, storageEntityContainerType, distinctFlagAboveType);
			if (mappingFragment2 != null)
			{
				entityTypeMapping.AddFragment(mappingFragment2);
			}
		}
		entitySetMapping.AddTypeMapping(entityTypeMapping);
	}

	private void LoadEntityTypeModificationFunctionMapping(XPathNavigator nav, EntitySetMapping entitySetMapping, EntityTypeMapping entityTypeMapping)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		if (entityTypeMapping.IsOfTypes.Count != 0 || entityTypeMapping.Types.Count != 1)
		{
			AddToSchemaErrors(Strings.Mapping_ModificationFunction_Multiple_Types, MappingErrorCode.InvalidModificationFunctionMappingForMultipleTypes, m_sourceLocation, lineInfo, m_parsingErrors);
			return;
		}
		EntityType entityType = (EntityType)entityTypeMapping.Types[0];
		if (entityType.Abstract)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_AbstractEntity_FunctionMapping, entityType.FullName, MappingErrorCode.MappingOfAbstractType, m_sourceLocation, lineInfo, m_parsingErrors);
			return;
		}
		foreach (EntityTypeModificationFunctionMapping modificationFunctionMapping5 in entitySetMapping.ModificationFunctionMappings)
		{
			if (modificationFunctionMapping5.EntityType.Equals(entityType))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_ModificationFunction_RedundantEntityTypeMapping, entityType.Name, MappingErrorCode.RedundantEntityTypeMappingInModificationFunctionMapping, m_sourceLocation, lineInfo, m_parsingErrors);
				return;
			}
		}
		ModificationFunctionMappingLoader modificationFunctionMappingLoader = new ModificationFunctionMappingLoader(this, entitySetMapping.Set);
		ModificationFunctionMapping modificationFunctionMapping = null;
		ModificationFunctionMapping modificationFunctionMapping2 = null;
		ModificationFunctionMapping modificationFunctionMapping3 = null;
		if (nav.MoveToChild(XPathNodeType.Element))
		{
			do
			{
				switch (nav.LocalName)
				{
				case "DeleteFunction":
					modificationFunctionMapping = modificationFunctionMappingLoader.LoadEntityTypeModificationFunctionMapping(nav.Clone(), entitySetMapping.Set, allowCurrentVersion: false, allowOriginalVersion: true, entityType);
					break;
				case "InsertFunction":
					modificationFunctionMapping2 = modificationFunctionMappingLoader.LoadEntityTypeModificationFunctionMapping(nav.Clone(), entitySetMapping.Set, allowCurrentVersion: true, allowOriginalVersion: false, entityType);
					break;
				case "UpdateFunction":
					modificationFunctionMapping3 = modificationFunctionMappingLoader.LoadEntityTypeModificationFunctionMapping(nav.Clone(), entitySetMapping.Set, allowCurrentVersion: true, allowOriginalVersion: true, entityType);
					break;
				}
			}
			while (nav.MoveToNext(XPathNodeType.Element));
		}
		IEnumerable<ModificationFunctionParameterBinding> enumerable = new List<ModificationFunctionParameterBinding>();
		if (modificationFunctionMapping != null)
		{
			enumerable = Helper.Concat<ModificationFunctionParameterBinding>(enumerable, modificationFunctionMapping.ParameterBindings);
		}
		if (modificationFunctionMapping2 != null)
		{
			enumerable = Helper.Concat<ModificationFunctionParameterBinding>(enumerable, modificationFunctionMapping2.ParameterBindings);
		}
		if (modificationFunctionMapping3 != null)
		{
			enumerable = Helper.Concat<ModificationFunctionParameterBinding>(enumerable, modificationFunctionMapping3.ParameterBindings);
		}
		Dictionary<AssociationSet, AssociationEndMember> dictionary = new Dictionary<AssociationSet, AssociationEndMember>();
		foreach (ModificationFunctionParameterBinding item in enumerable)
		{
			if (item.MemberPath.AssociationSetEnd != null)
			{
				AssociationSet parentAssociationSet = item.MemberPath.AssociationSetEnd.ParentAssociationSet;
				AssociationEndMember correspondingAssociationEndMember = item.MemberPath.AssociationSetEnd.CorrespondingAssociationEndMember;
				if (dictionary.TryGetValue(parentAssociationSet, out var value) && value != correspondingAssociationEndMember)
				{
					AddToSchemaErrorWithMessage(Strings.Mapping_ModificationFunction_MultipleEndsOfAssociationMapped(correspondingAssociationEndMember.Name, value.Name, parentAssociationSet.Name), MappingErrorCode.InvalidModificationFunctionMappingMultipleEndsOfAssociationMapped, m_sourceLocation, lineInfo, m_parsingErrors);
					return;
				}
				dictionary[parentAssociationSet] = correspondingAssociationEndMember;
			}
		}
		EntityTypeModificationFunctionMapping modificationFunctionMapping4 = new EntityTypeModificationFunctionMapping(entityType, modificationFunctionMapping, modificationFunctionMapping2, modificationFunctionMapping3);
		entitySetMapping.AddModificationFunctionMapping(modificationFunctionMapping4);
	}

	private bool LoadQueryView(XPathNavigator nav, EntitySetBaseMapping setMapping)
	{
		string value = nav.Value;
		bool flag = false;
		string text = GetAttributeValue(nav.Clone(), "TypeName");
		if (text != null)
		{
			text = text.Trim();
		}
		IXmlLineInfo xmlLineInfo = nav as IXmlLineInfo;
		if (setMapping.QueryView == null)
		{
			if (text != null)
			{
				AddToSchemaErrorsWithMemberInfo((object val) => Strings.Mapping_TypeName_For_First_QueryView, setMapping.Set.Name, MappingErrorCode.TypeNameForFirstQueryView, m_sourceLocation, xmlLineInfo, m_parsingErrors);
				return false;
			}
			if (string.IsNullOrEmpty(value))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_Empty_QueryView, setMapping.Set.Name, MappingErrorCode.EmptyQueryView, m_sourceLocation, xmlLineInfo, m_parsingErrors);
				return false;
			}
			setMapping.QueryView = value;
			m_hasQueryViews = true;
			return true;
		}
		if (text == null || text.Trim().Length == 0)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_QueryView_TypeName_Not_Defined, setMapping.Set.Name, MappingErrorCode.NoTypeNameForTypeSpecificQueryView, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			return false;
		}
		EntityType rootEntityType = (EntityType)setMapping.Set.ElementType;
		if (!TryParseEntityTypeAttribute(nav.Clone(), rootEntityType, (EntityType e) => Strings.Mapping_InvalidContent_Entity_Type_For_Entity_Set(e.FullName, rootEntityType.FullName, setMapping.Set.Name), out var isOfTypeEntityTypes, out var entityTypes))
		{
			return false;
		}
		EntityType entityType;
		if (isOfTypeEntityTypes.Count == 1)
		{
			entityType = isOfTypeEntityTypes.First();
			flag = true;
		}
		else
		{
			if (entityTypes.Count != 1)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_QueryViewMultipleTypeInTypeName, setMapping.Set.ToString(), MappingErrorCode.TypeNameContainsMultipleTypesForQueryView, m_sourceLocation, xmlLineInfo, m_parsingErrors);
				return false;
			}
			entityType = entityTypes.First();
			flag = false;
		}
		if (flag && setMapping.Set.ElementType.EdmEquals(entityType))
		{
			AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_QueryView_For_Base_Type, entityType.ToString(), setMapping.Set.ToString(), MappingErrorCode.IsTypeOfQueryViewForBaseType, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			return false;
		}
		if (string.IsNullOrEmpty(value))
		{
			if (flag)
			{
				AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_Empty_QueryView_OfType, entityType.Name, setMapping.Set.Name, MappingErrorCode.EmptyQueryView, m_sourceLocation, xmlLineInfo, m_parsingErrors);
				return false;
			}
			AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_Empty_QueryView_OfTypeOnly, setMapping.Set.Name, entityType.Name, MappingErrorCode.EmptyQueryView, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			return false;
		}
		Pair<EntitySetBase, Pair<EntityTypeBase, bool>> key = new Pair<EntitySetBase, Pair<EntityTypeBase, bool>>(setMapping.Set, new Pair<EntityTypeBase, bool>(entityType, flag));
		if (setMapping.ContainsTypeSpecificQueryView(key))
		{
			EdmSchemaError edmSchemaError = null;
			edmSchemaError = ((!flag) ? new EdmSchemaError(Strings.Mapping_QueryView_Duplicate_OfTypeOnly(setMapping.Set, entityType), 2082, EdmSchemaErrorSeverity.Error, m_sourceLocation, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition) : new EdmSchemaError(Strings.Mapping_QueryView_Duplicate_OfType(setMapping.Set, entityType), 2082, EdmSchemaErrorSeverity.Error, m_sourceLocation, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition));
			m_parsingErrors.Add(edmSchemaError);
			return false;
		}
		setMapping.AddTypeSpecificQueryView(key, value);
		return true;
	}

	private void LoadAssociationSetMapping(XPathNavigator nav, EntityContainerMapping entityContainerMapping, System.Data.Entity.Core.Metadata.Edm.EntityContainer storageEntityContainerType)
	{
		IXmlLineInfo xmlLineInfo = (IXmlLineInfo)nav;
		string aliasResolvedAttributeValue = GetAliasResolvedAttributeValue(nav.Clone(), "Name");
		string aliasResolvedAttributeValue2 = GetAliasResolvedAttributeValue(nav.Clone(), "TypeName");
		string aliasResolvedAttributeValue3 = GetAliasResolvedAttributeValue(nav.Clone(), "StoreEntitySet");
		entityContainerMapping.EdmEntityContainer.TryGetRelationshipSetByName(aliasResolvedAttributeValue, ignoreCase: false, out var relationshipSet);
		if (!(relationshipSet is AssociationSet associationSet))
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Association_Set, aliasResolvedAttributeValue, MappingErrorCode.InvalidAssociationSet, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			return;
		}
		if (associationSet.ElementType.IsForeignKey)
		{
			System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint referentialConstraint = associationSet.ElementType.ReferentialConstraints.Single();
			IEnumerable<EdmMember> dependentKeys = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)referentialConstraint.ToRole).KeyMembers;
			if (associationSet.ElementType.ReferentialConstraints.Single().ToProperties.All((EdmProperty p) => dependentKeys.Contains(p)))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_ForeignKey_Association_Set_PKtoPK, aliasResolvedAttributeValue, MappingErrorCode.InvalidAssociationSet, m_sourceLocation, xmlLineInfo, m_parsingErrors).Severity = EdmSchemaErrorSeverity.Warning;
			}
			else
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_ForeignKey_Association_Set, aliasResolvedAttributeValue, MappingErrorCode.InvalidAssociationSet, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			}
			return;
		}
		if (entityContainerMapping.ContainsAssociationSetMapping(associationSet))
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_Duplicate_CdmAssociationSet_StorageMap, aliasResolvedAttributeValue, MappingErrorCode.DuplicateSetMapping, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			return;
		}
		AssociationSetMapping associationSetMapping = new AssociationSetMapping(associationSet, entityContainerMapping);
		associationSetMapping.StartLineNumber = xmlLineInfo.LineNumber;
		associationSetMapping.StartLinePosition = xmlLineInfo.LinePosition;
		if (!nav.MoveToChild(XPathNodeType.Element))
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Emtpty_SetMap, associationSet.Name, MappingErrorCode.EmptySetMapping, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			return;
		}
		entityContainerMapping.AddSetMapping(associationSetMapping);
		if (nav.LocalName == "QueryView")
		{
			if (!string.IsNullOrEmpty(aliasResolvedAttributeValue3))
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_TableName_QueryView, aliasResolvedAttributeValue, MappingErrorCode.TableNameAttributeWithQueryView, m_sourceLocation, xmlLineInfo, m_parsingErrors);
				return;
			}
			if (!LoadQueryView(nav.Clone(), associationSetMapping) || !nav.MoveToNext(XPathNodeType.Element))
			{
				return;
			}
		}
		if (nav.LocalName == "EndProperty" || nav.LocalName == "ModificationFunctionMapping")
		{
			if (string.IsNullOrEmpty(aliasResolvedAttributeValue2))
			{
				AddToSchemaErrors(Strings.Mapping_InvalidContent_Association_Type_Empty, MappingErrorCode.InvalidAssociationType, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			}
			else
			{
				LoadAssociationTypeMapping(nav.Clone(), associationSetMapping, aliasResolvedAttributeValue2, aliasResolvedAttributeValue3, storageEntityContainerType);
			}
		}
		else if (nav.LocalName == "Condition")
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_AssociationSet_Condition, aliasResolvedAttributeValue, MappingErrorCode.InvalidContent, m_sourceLocation, xmlLineInfo, m_parsingErrors);
		}
	}

	private void LoadFunctionImportMapping(XPathNavigator nav, EntityContainerMapping entityContainerMapping)
	{
		IXmlLineInfo xmlLineInfo = (IXmlLineInfo)nav.Clone();
		if (!TryGetFunctionImportStoreFunction(nav, out var targetFunction) || !TryGetFunctionImportModelFunction(nav, entityContainerMapping, out var functionImport))
		{
			return;
		}
		if (!functionImport.IsComposableAttribute && targetFunction.IsComposableAttribute)
		{
			AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_TargetFunctionMustBeNonComposable(functionImport.FullName, targetFunction.FullName), MappingErrorCode.MappingFunctionImportTargetFunctionMustBeNonComposable, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			return;
		}
		if (functionImport.IsComposableAttribute && !targetFunction.IsComposableAttribute)
		{
			AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_TargetFunctionMustBeComposable(functionImport.FullName, targetFunction.FullName), MappingErrorCode.MappingFunctionImportTargetFunctionMustBeComposable, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			return;
		}
		ValidateFunctionImportMappingParameters(nav, targetFunction, functionImport);
		List<List<FunctionImportStructuralTypeMapping>> list = new List<List<FunctionImportStructuralTypeMapping>>();
		if (nav.MoveToChild(XPathNodeType.Element))
		{
			int num = 0;
			do
			{
				if (nav.LocalName == "ResultMapping")
				{
					List<FunctionImportStructuralTypeMapping> functionImportMappingResultMapping = GetFunctionImportMappingResultMapping(nav.Clone(), xmlLineInfo, functionImport, num);
					list.Add(functionImportMappingResultMapping);
				}
				num++;
			}
			while (nav.MoveToNext(XPathNodeType.Element));
		}
		if (list.Count > 0 && list.Count != functionImport.ReturnParameters.Count)
		{
			AddToSchemaErrors(Strings.Mapping_FunctionImport_ResultMappingCountDoesNotMatchResultCount(functionImport.Identity), MappingErrorCode.FunctionResultMappingCountMismatch, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			return;
		}
		if (functionImport.IsComposableAttribute)
		{
			EdmFunction edmFunction = StoreItemCollection.ConvertToCTypeFunction(targetFunction);
			RowType tvfReturnType = TypeHelpers.GetTvfReturnType(edmFunction);
			RowType tvfReturnType2 = TypeHelpers.GetTvfReturnType(targetFunction);
			if (tvfReturnType == null)
			{
				AddToSchemaErrors(Strings.Mapping_FunctionImport_ResultMapping_InvalidSType(functionImport.Identity), MappingErrorCode.MappingFunctionImportTVFExpected, m_sourceLocation, xmlLineInfo, m_parsingErrors);
				return;
			}
			List<FunctionImportStructuralTypeMapping> typeMappings = ((list.Count > 0) ? list[0] : new List<FunctionImportStructuralTypeMapping>());
			FunctionImportMappingComposable mapping = null;
			if (MetadataHelper.TryGetFunctionImportReturnType<EdmType>(functionImport, 0, out var returnType))
			{
				FunctionImportMappingComposableHelper functionImportMappingComposableHelper = new FunctionImportMappingComposableHelper(entityContainerMapping, m_sourceLocation, m_parsingErrors);
				if (Helper.IsStructuralType(returnType))
				{
					if (!functionImportMappingComposableHelper.TryCreateFunctionImportMappingComposableWithStructuralResult(functionImport, edmFunction, typeMappings, tvfReturnType, tvfReturnType2, xmlLineInfo, out mapping))
					{
						return;
					}
				}
				else if (!functionImportMappingComposableHelper.TryCreateFunctionImportMappingComposableWithScalarResult(functionImport, edmFunction, targetFunction, returnType, tvfReturnType, xmlLineInfo, out mapping))
				{
					return;
				}
			}
			entityContainerMapping.AddFunctionImportMapping(mapping);
			return;
		}
		FunctionImportMappingNonComposable functionImportMappingNonComposable = new FunctionImportMappingNonComposable(functionImport, targetFunction, list, EdmItemCollection);
		foreach (FunctionImportStructuralTypeMappingKB internalResultMapping in functionImportMappingNonComposable.InternalResultMappings)
		{
			internalResultMapping.ValidateTypeConditions(validateAmbiguity: false, m_parsingErrors, m_sourceLocation);
		}
		for (int i = 0; i < functionImportMappingNonComposable.InternalResultMappings.Count; i++)
		{
			if (MetadataHelper.TryGetFunctionImportReturnType<EntityType>(functionImport, i, out var returnType2) && returnType2.Abstract && functionImportMappingNonComposable.GetResultMapping(i).NormalizedEntityTypeMappings.Count == 0)
			{
				AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_FunctionImport_ImplicitMappingForAbstractReturnType, returnType2.FullName, functionImport.Identity, MappingErrorCode.MappingOfAbstractType, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			}
		}
		entityContainerMapping.AddFunctionImportMapping(functionImportMappingNonComposable);
	}

	private bool TryGetFunctionImportStoreFunction(XPathNavigator nav, out EdmFunction targetFunction)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		targetFunction = null;
		string aliasResolvedAttributeValue = GetAliasResolvedAttributeValue(nav.Clone(), "FunctionName");
		ReadOnlyCollection<EdmFunction> functions = StoreItemCollection.GetFunctions(aliasResolvedAttributeValue);
		if (functions.Count == 0)
		{
			AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_StoreFunctionDoesNotExist(aliasResolvedAttributeValue), MappingErrorCode.MappingFunctionImportStoreFunctionDoesNotExist, m_sourceLocation, lineInfo, m_parsingErrors);
			return false;
		}
		if (functions.Count > 1)
		{
			AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_FunctionAmbiguous(aliasResolvedAttributeValue), MappingErrorCode.MappingFunctionImportStoreFunctionAmbiguous, m_sourceLocation, lineInfo, m_parsingErrors);
			return false;
		}
		targetFunction = functions.Single();
		return true;
	}

	private bool TryGetFunctionImportModelFunction(XPathNavigator nav, EntityContainerMapping entityContainerMapping, out EdmFunction functionImport)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		string aliasResolvedAttributeValue = GetAliasResolvedAttributeValue(nav.Clone(), "FunctionImportName");
		System.Data.Entity.Core.Metadata.Edm.EntityContainer edmEntityContainer = entityContainerMapping.EdmEntityContainer;
		functionImport = null;
		foreach (EdmFunction functionImport2 in edmEntityContainer.FunctionImports)
		{
			if (functionImport2.Name == aliasResolvedAttributeValue)
			{
				functionImport = functionImport2;
				break;
			}
		}
		if (functionImport == null)
		{
			AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_FunctionImportDoesNotExist(aliasResolvedAttributeValue, entityContainerMapping.EdmEntityContainer.Name), MappingErrorCode.MappingFunctionImportFunctionImportDoesNotExist, m_sourceLocation, lineInfo, m_parsingErrors);
			return false;
		}
		if (entityContainerMapping.TryGetFunctionImportMapping(functionImport, out var _))
		{
			AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_FunctionImportMappedMultipleTimes(aliasResolvedAttributeValue), MappingErrorCode.MappingFunctionImportFunctionImportMappedMultipleTimes, m_sourceLocation, lineInfo, m_parsingErrors);
			return false;
		}
		return true;
	}

	private void ValidateFunctionImportMappingParameters(XPathNavigator nav, EdmFunction targetFunction, EdmFunction functionImport)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		foreach (FunctionParameter parameter in targetFunction.Parameters)
		{
			if (!functionImport.Parameters.TryGetValue(parameter.Name, ignoreCase: false, out var item))
			{
				AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_TargetParameterHasNoCorrespondingImportParameter(parameter.Name), MappingErrorCode.MappingFunctionImportTargetParameterHasNoCorrespondingImportParameter, m_sourceLocation, lineInfo, m_parsingErrors);
				continue;
			}
			if (parameter.Mode != item.Mode)
			{
				AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_IncompatibleParameterMode(parameter.Name, parameter.Mode, item.Mode), MappingErrorCode.MappingFunctionImportIncompatibleParameterMode, m_sourceLocation, lineInfo, m_parsingErrors);
			}
			PrimitiveType primitiveType = Helper.AsPrimitive(item.TypeUsage.EdmType);
			if (Helper.IsSpatialType(primitiveType))
			{
				primitiveType = Helper.GetSpatialNormalizedPrimitiveType(primitiveType);
			}
			PrimitiveType primitiveType2 = (PrimitiveType)StoreItemCollection.ProviderManifest.GetEdmType(parameter.TypeUsage).EdmType;
			if (primitiveType2 == null)
			{
				AddToSchemaErrorWithMessage(Strings.Mapping_ProviderReturnsNullType(parameter.Name), MappingErrorCode.MappingStoreProviderReturnsNullEdmType, m_sourceLocation, lineInfo, m_parsingErrors);
				return;
			}
			if (primitiveType2.PrimitiveTypeKind != primitiveType.PrimitiveTypeKind)
			{
				AddToSchemaErrorWithMessage(Helper.IsEnumType(item.TypeUsage.EdmType) ? Strings.Mapping_FunctionImport_IncompatibleEnumParameterType(parameter.Name, primitiveType2.Name, item.TypeUsage.EdmType.FullName, Helper.GetUnderlyingEdmTypeForEnumType(item.TypeUsage.EdmType).Name) : Strings.Mapping_FunctionImport_IncompatibleParameterType(parameter.Name, primitiveType2.Name, primitiveType.Name), MappingErrorCode.MappingFunctionImportIncompatibleParameterType, m_sourceLocation, lineInfo, m_parsingErrors);
			}
		}
		foreach (FunctionParameter parameter2 in functionImport.Parameters)
		{
			if (!targetFunction.Parameters.TryGetValue(parameter2.Name, ignoreCase: false, out var _))
			{
				AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_ImportParameterHasNoCorrespondingTargetParameter(parameter2.Name), MappingErrorCode.MappingFunctionImportImportParameterHasNoCorrespondingTargetParameter, m_sourceLocation, lineInfo, m_parsingErrors);
			}
		}
	}

	private List<FunctionImportStructuralTypeMapping> GetFunctionImportMappingResultMapping(XPathNavigator nav, IXmlLineInfo functionImportMappingLineInfo, EdmFunction functionImport, int resultSetIndex)
	{
		List<FunctionImportStructuralTypeMapping> list = new List<FunctionImportStructuralTypeMapping>();
		if (nav.MoveToChild(XPathNodeType.Element))
		{
			do
			{
				EntitySet entitySet = ((functionImport.EntitySets.Count > resultSetIndex) ? functionImport.EntitySets[resultSetIndex] : null);
				if (nav.LocalName == "EntityTypeMapping")
				{
					if (MetadataHelper.TryGetFunctionImportReturnType<EntityType>(functionImport, resultSetIndex, out var resultEntityType))
					{
						if (entitySet == null)
						{
							AddToSchemaErrors(Strings.Mapping_FunctionImport_EntityTypeMappingForFunctionNotReturningEntitySet("EntityTypeMapping", functionImport.Identity), MappingErrorCode.MappingFunctionImportEntityTypeMappingForFunctionNotReturningEntitySet, m_sourceLocation, functionImportMappingLineInfo, m_parsingErrors);
						}
						if (TryLoadFunctionImportEntityTypeMapping(nav.Clone(), resultEntityType, (EntityType e) => Strings.Mapping_FunctionImport_InvalidContentEntityTypeForEntitySet(e.FullName, resultEntityType.FullName, entitySet.Name, functionImport.Identity), out var typeMapping))
						{
							list.Add(typeMapping);
						}
					}
					else
					{
						AddToSchemaErrors(Strings.Mapping_FunctionImport_ResultMapping_InvalidCTypeETExpected(functionImport.Identity), MappingErrorCode.MappingFunctionImportUnexpectedEntityTypeMapping, m_sourceLocation, functionImportMappingLineInfo, m_parsingErrors);
					}
				}
				else
				{
					if (!(nav.LocalName == "ComplexTypeMapping"))
					{
						continue;
					}
					if (MetadataHelper.TryGetFunctionImportReturnType<ComplexType>(functionImport, resultSetIndex, out var returnType))
					{
						if (TryLoadFunctionImportComplexTypeMapping(nav.Clone(), returnType, functionImport, out var typeMapping2))
						{
							list.Add(typeMapping2);
						}
					}
					else
					{
						AddToSchemaErrors(Strings.Mapping_FunctionImport_ResultMapping_InvalidCTypeCTExpected(functionImport.Identity), MappingErrorCode.MappingFunctionImportUnexpectedComplexTypeMapping, m_sourceLocation, functionImportMappingLineInfo, m_parsingErrors);
					}
				}
			}
			while (nav.MoveToNext(XPathNodeType.Element));
		}
		return list;
	}

	private bool TryLoadFunctionImportComplexTypeMapping(XPathNavigator nav, ComplexType resultComplexType, EdmFunction functionImport, out FunctionImportComplexTypeMapping typeMapping)
	{
		typeMapping = null;
		LineInfo lineInfo = new LineInfo(nav);
		if (!TryParseComplexTypeAttribute(nav, resultComplexType, functionImport, out var complexType))
		{
			return false;
		}
		Collection<FunctionImportReturnTypePropertyMapping> collection = new Collection<FunctionImportReturnTypePropertyMapping>();
		if (!LoadFunctionImportStructuralType(nav.Clone(), new List<StructuralType> { complexType }, collection, null))
		{
			return false;
		}
		typeMapping = new FunctionImportComplexTypeMapping(complexType, collection, lineInfo);
		return true;
	}

	private bool TryParseComplexTypeAttribute(XPathNavigator nav, ComplexType resultComplexType, EdmFunction functionImport, out ComplexType complexType)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		string attributeValue = GetAttributeValue(nav.Clone(), "TypeName");
		attributeValue = GetAliasResolvedValue(attributeValue);
		if (!EdmItemCollection.TryGetItem<ComplexType>(attributeValue, out complexType))
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Complex_Type, attributeValue, MappingErrorCode.InvalidComplexType, m_sourceLocation, lineInfo, m_parsingErrors);
			return false;
		}
		if (!Helper.IsAssignableFrom(resultComplexType, complexType))
		{
			AddToSchemaErrorWithMessage(Strings.Mapping_FunctionImport_ResultMapping_MappedTypeDoesNotMatchReturnType(functionImport.Identity, complexType.FullName), MappingErrorCode.InvalidComplexType, m_sourceLocation, lineInfo, m_parsingErrors);
			return false;
		}
		return true;
	}

	private bool TryLoadFunctionImportEntityTypeMapping(XPathNavigator nav, EntityType resultEntityType, Func<EntityType, string> registerEntityTypeMismatchError, out FunctionImportEntityTypeMapping typeMapping)
	{
		typeMapping = null;
		LineInfo lineInfo = new LineInfo(nav);
		GetAttributeValue(nav.Clone(), "TypeName");
		if (!TryParseEntityTypeAttribute(nav.Clone(), resultEntityType, registerEntityTypeMismatchError, out var isOfTypeEntityTypes, out var entityTypes))
		{
			return false;
		}
		IEnumerable<StructuralType> currentTypes = isOfTypeEntityTypes.Concat(entityTypes).Distinct().OfType<StructuralType>();
		Collection<FunctionImportReturnTypePropertyMapping> collection = new Collection<FunctionImportReturnTypePropertyMapping>();
		List<FunctionImportEntityTypeMappingCondition> conditions = new List<FunctionImportEntityTypeMappingCondition>();
		if (!LoadFunctionImportStructuralType(nav.Clone(), currentTypes, collection, conditions))
		{
			return false;
		}
		typeMapping = new FunctionImportEntityTypeMapping(isOfTypeEntityTypes, entityTypes, conditions, collection, lineInfo);
		return true;
	}

	private bool LoadFunctionImportStructuralType(XPathNavigator nav, IEnumerable<StructuralType> currentTypes, Collection<FunctionImportReturnTypePropertyMapping> columnRenameMappings, List<FunctionImportEntityTypeMappingCondition> conditions)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav.Clone();
		if (nav.MoveToChild(XPathNodeType.Element))
		{
			do
			{
				if (nav.LocalName == "ScalarProperty")
				{
					LoadFunctionImportStructuralTypeMappingScalarProperty(nav, columnRenameMappings, currentTypes);
				}
				if (nav.LocalName == "Condition")
				{
					LoadFunctionImportEntityTypeMappingCondition(nav, conditions);
				}
			}
			while (nav.MoveToNext(XPathNodeType.Element));
		}
		bool flag = false;
		if (conditions != null)
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (FunctionImportEntityTypeMappingCondition condition in conditions)
			{
				if (!hashSet.Add(condition.ColumnName))
				{
					AddToSchemaErrorWithMessage(Strings.Mapping_InvalidContent_Duplicate_Condition_Member(condition.ColumnName), MappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
					flag = true;
				}
			}
		}
		return !flag;
	}

	private void LoadFunctionImportStructuralTypeMappingScalarProperty(XPathNavigator nav, Collection<FunctionImportReturnTypePropertyMapping> columnRenameMappings, IEnumerable<StructuralType> currentTypes)
	{
		LineInfo lineInfo = new LineInfo(nav);
		string memberName = GetAliasResolvedAttributeValue(nav.Clone(), "Name");
		string aliasResolvedAttributeValue = GetAliasResolvedAttributeValue(nav.Clone(), "ColumnName");
		if (!currentTypes.All((StructuralType t) => t.Members.Contains(memberName)))
		{
			AddToSchemaErrorWithMessage(Strings.Mapping_InvalidContent_Cdm_Member(memberName), MappingErrorCode.InvalidEdmMember, m_sourceLocation, lineInfo, m_parsingErrors);
		}
		if (columnRenameMappings.Any((FunctionImportReturnTypePropertyMapping m) => m.CMember == memberName))
		{
			AddToSchemaErrorWithMessage(Strings.Mapping_InvalidContent_Duplicate_Cdm_Member(memberName), MappingErrorCode.DuplicateMemberMapping, m_sourceLocation, lineInfo, m_parsingErrors);
		}
		else
		{
			columnRenameMappings.Add(new FunctionImportReturnTypeScalarPropertyMapping(memberName, aliasResolvedAttributeValue, lineInfo));
		}
	}

	private void LoadFunctionImportEntityTypeMappingCondition(XPathNavigator nav, List<FunctionImportEntityTypeMappingCondition> conditions)
	{
		LineInfo lineInfo = new LineInfo(nav);
		string aliasResolvedAttributeValue = GetAliasResolvedAttributeValue(nav.Clone(), "ColumnName");
		string aliasResolvedAttributeValue2 = GetAliasResolvedAttributeValue(nav.Clone(), "Value");
		string aliasResolvedAttributeValue3 = GetAliasResolvedAttributeValue(nav.Clone(), "IsNull");
		if (aliasResolvedAttributeValue3 != null && aliasResolvedAttributeValue2 != null)
		{
			AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Both_Values, MappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
		}
		else if (aliasResolvedAttributeValue3 == null && aliasResolvedAttributeValue2 == null)
		{
			AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Either_Values, MappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
		}
		else if (aliasResolvedAttributeValue3 != null)
		{
			bool isNull = Convert.ToBoolean(aliasResolvedAttributeValue3, CultureInfo.InvariantCulture);
			conditions.Add(new FunctionImportEntityTypeMappingConditionIsNull(aliasResolvedAttributeValue, isNull, lineInfo));
		}
		else
		{
			XPathNavigator xPathNavigator = nav.Clone();
			xPathNavigator.MoveToAttribute("Value", string.Empty);
			conditions.Add(new FunctionImportEntityTypeMappingConditionValue(aliasResolvedAttributeValue, xPathNavigator, lineInfo));
		}
	}

	private void LoadAssociationTypeMapping(XPathNavigator nav, AssociationSetMapping associationSetMapping, string associationTypeName, string tableName, System.Data.Entity.Core.Metadata.Edm.EntityContainer storageEntityContainerType)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		EdmItemCollection.TryGetItem<AssociationType>(associationTypeName, out var item);
		if (item == null)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Association_Type, associationTypeName, MappingErrorCode.InvalidAssociationType, m_sourceLocation, lineInfo, m_parsingErrors);
			return;
		}
		if (!associationSetMapping.Set.ElementType.Equals(item))
		{
			AddToSchemaErrorWithMessage(Strings.Mapping_Invalid_Association_Type_For_Association_Set(associationTypeName, associationSetMapping.Set.ElementType.FullName, associationSetMapping.Set.Name), MappingErrorCode.DuplicateTypeMapping, m_sourceLocation, lineInfo, m_parsingErrors);
			return;
		}
		AssociationTypeMapping associationTypeMapping2 = (associationSetMapping.AssociationTypeMapping = new AssociationTypeMapping(item, associationSetMapping));
		if (string.IsNullOrEmpty(tableName) && associationSetMapping.QueryView == null)
		{
			AddToSchemaErrors(Strings.Mapping_InvalidContent_Table_Expected, MappingErrorCode.InvalidTable, m_sourceLocation, lineInfo, m_parsingErrors);
			return;
		}
		MappingFragment mappingFragment = LoadAssociationMappingFragment(nav.Clone(), associationSetMapping, associationTypeMapping2, tableName, storageEntityContainerType);
		if (mappingFragment != null)
		{
			associationTypeMapping2.MappingFragment = mappingFragment;
		}
	}

	private void LoadAssociationTypeModificationFunctionMapping(XPathNavigator nav, AssociationSetMapping associationSetMapping)
	{
		ModificationFunctionMappingLoader modificationFunctionMappingLoader = new ModificationFunctionMappingLoader(this, associationSetMapping.Set);
		ModificationFunctionMapping deleteFunctionMapping = null;
		ModificationFunctionMapping insertFunctionMapping = null;
		if (nav.MoveToChild(XPathNodeType.Element))
		{
			do
			{
				switch (nav.LocalName)
				{
				case "DeleteFunction":
					deleteFunctionMapping = modificationFunctionMappingLoader.LoadAssociationSetModificationFunctionMapping(nav.Clone(), associationSetMapping.Set, isInsert: false);
					break;
				case "InsertFunction":
					insertFunctionMapping = modificationFunctionMappingLoader.LoadAssociationSetModificationFunctionMapping(nav.Clone(), associationSetMapping.Set, isInsert: true);
					break;
				}
			}
			while (nav.MoveToNext(XPathNodeType.Element));
		}
		associationSetMapping.ModificationFunctionMapping = new AssociationSetModificationFunctionMapping((AssociationSet)associationSetMapping.Set, deleteFunctionMapping, insertFunctionMapping);
	}

	private MappingFragment LoadMappingFragment(XPathNavigator nav, EntityTypeMapping typeMapping, string tableName, System.Data.Entity.Core.Metadata.Edm.EntityContainer storageEntityContainerType, bool distinctFlag)
	{
		IXmlLineInfo navLineInfo = (IXmlLineInfo)nav;
		if (typeMapping.SetMapping.QueryView != null)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_QueryView_PropertyMaps, typeMapping.SetMapping.Set.Name, MappingErrorCode.PropertyMapsWithQueryView, m_sourceLocation, navLineInfo, m_parsingErrors);
			return null;
		}
		storageEntityContainerType.TryGetEntitySetByName(tableName, ignoreCase: false, out var entitySet);
		if (entitySet == null)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Table, tableName, MappingErrorCode.InvalidTable, m_sourceLocation, navLineInfo, m_parsingErrors);
			return null;
		}
		EntityType elementType = entitySet.ElementType;
		MappingFragment mappingFragment = new MappingFragment(entitySet, typeMapping, distinctFlag);
		mappingFragment.StartLineNumber = navLineInfo.LineNumber;
		mappingFragment.StartLinePosition = navLineInfo.LinePosition;
		if (nav.MoveToChild(XPathNodeType.Element))
		{
			do
			{
				EdmType containerType = null;
				string attributeValue = GetAttributeValue(nav.Clone(), "Name");
				if (attributeValue != null)
				{
					containerType = typeMapping.GetContainerType(attributeValue);
				}
				switch (nav.LocalName)
				{
				case "ScalarProperty":
				{
					ScalarPropertyMapping scalarPropertyMapping = LoadScalarPropertyMapping(nav.Clone(), containerType, elementType.Properties);
					if (scalarPropertyMapping != null)
					{
						mappingFragment.AddPropertyMapping(scalarPropertyMapping);
					}
					break;
				}
				case "ComplexProperty":
				{
					ComplexPropertyMapping complexPropertyMapping = LoadComplexPropertyMapping(nav.Clone(), containerType, elementType.Properties);
					if (complexPropertyMapping != null)
					{
						mappingFragment.AddPropertyMapping(complexPropertyMapping);
					}
					break;
				}
				case "Condition":
				{
					ConditionPropertyMapping conditionPropertyMapping = LoadConditionPropertyMapping(nav.Clone(), containerType, elementType.Properties);
					if (conditionPropertyMapping != null)
					{
						mappingFragment.AddConditionProperty(conditionPropertyMapping, delegate(EdmMember member)
						{
							AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Duplicate_Condition_Member, member.Name, MappingErrorCode.ConditionError, m_sourceLocation, navLineInfo, m_parsingErrors);
						});
					}
					break;
				}
				default:
					AddToSchemaErrors(Strings.Mapping_InvalidContent_General, MappingErrorCode.InvalidContent, m_sourceLocation, navLineInfo, m_parsingErrors);
					break;
				}
			}
			while (nav.MoveToNext(XPathNodeType.Element));
		}
		nav.MoveToChild(XPathNodeType.Element);
		return mappingFragment;
	}

	private MappingFragment LoadAssociationMappingFragment(XPathNavigator nav, AssociationSetMapping setMapping, AssociationTypeMapping typeMapping, string tableName, System.Data.Entity.Core.Metadata.Edm.EntityContainer storageEntityContainerType)
	{
		IXmlLineInfo navLineInfo = (IXmlLineInfo)nav;
		MappingFragment mappingFragment = null;
		EntityType entityType = null;
		if (setMapping.QueryView == null)
		{
			storageEntityContainerType.TryGetEntitySetByName(tableName, ignoreCase: false, out var entitySet);
			if (entitySet == null)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Table, tableName, MappingErrorCode.InvalidTable, m_sourceLocation, navLineInfo, m_parsingErrors);
				return null;
			}
			entityType = entitySet.ElementType;
			mappingFragment = new MappingFragment(entitySet, typeMapping, makeColumnsDistinct: false);
			mappingFragment.StartLineNumber = setMapping.StartLineNumber;
			mappingFragment.StartLinePosition = setMapping.StartLinePosition;
		}
		do
		{
			switch (nav.LocalName)
			{
			case "EndProperty":
			{
				if (setMapping.QueryView != null)
				{
					AddToSchemaErrorsWithMemberInfo(Strings.Mapping_QueryView_PropertyMaps, setMapping.Set.Name, MappingErrorCode.PropertyMapsWithQueryView, m_sourceLocation, navLineInfo, m_parsingErrors);
					return null;
				}
				string aliasResolvedAttributeValue = GetAliasResolvedAttributeValue(nav.Clone(), "Name");
				EdmMember item = null;
				typeMapping.AssociationType.Members.TryGetValue(aliasResolvedAttributeValue, ignoreCase: false, out item);
				if (!(item is AssociationEndMember end))
				{
					AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_End, aliasResolvedAttributeValue, MappingErrorCode.InvalidEdmMember, m_sourceLocation, navLineInfo, m_parsingErrors);
				}
				else
				{
					mappingFragment.AddPropertyMapping(LoadEndPropertyMapping(nav.Clone(), end, entityType));
				}
				break;
			}
			case "Condition":
			{
				if (setMapping.QueryView != null)
				{
					AddToSchemaErrorsWithMemberInfo(Strings.Mapping_QueryView_PropertyMaps, setMapping.Set.Name, MappingErrorCode.PropertyMapsWithQueryView, m_sourceLocation, navLineInfo, m_parsingErrors);
					return null;
				}
				ConditionPropertyMapping conditionPropertyMapping = LoadConditionPropertyMapping(nav.Clone(), null, entityType.Properties);
				if (conditionPropertyMapping != null)
				{
					mappingFragment.AddConditionProperty(conditionPropertyMapping, delegate(EdmMember member)
					{
						AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Duplicate_Condition_Member, member.Name, MappingErrorCode.ConditionError, m_sourceLocation, navLineInfo, m_parsingErrors);
					});
				}
				break;
			}
			case "ModificationFunctionMapping":
				setMapping.HasModificationFunctionMapping = true;
				LoadAssociationTypeModificationFunctionMapping(nav.Clone(), setMapping);
				break;
			default:
				AddToSchemaErrors(Strings.Mapping_InvalidContent_General, MappingErrorCode.InvalidContent, m_sourceLocation, navLineInfo, m_parsingErrors);
				break;
			}
		}
		while (nav.MoveToNext(XPathNodeType.Element));
		return mappingFragment;
	}

	private ScalarPropertyMapping LoadScalarPropertyMapping(XPathNavigator nav, EdmType containerType, ReadOnlyMetadataCollection<EdmProperty> tableProperties)
	{
		IXmlLineInfo xmlLineInfo = (IXmlLineInfo)nav;
		string aliasResolvedAttributeValue = GetAliasResolvedAttributeValue(nav.Clone(), "Name");
		EdmProperty item = null;
		if (!string.IsNullOrEmpty(aliasResolvedAttributeValue) && (containerType == null || !Helper.IsCollectionType(containerType)))
		{
			if (containerType != null)
			{
				if (Helper.IsRefType(containerType))
				{
					((EntityType)((RefType)containerType).ElementType).Properties.TryGetValue(aliasResolvedAttributeValue, ignoreCase: false, out item);
				}
				else
				{
					(containerType as StructuralType).Members.TryGetValue(aliasResolvedAttributeValue, ignoreCase: false, out var item2);
					item = item2 as EdmProperty;
				}
			}
			if (item == null)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Cdm_Member, aliasResolvedAttributeValue, MappingErrorCode.InvalidEdmMember, m_sourceLocation, xmlLineInfo, m_parsingErrors);
			}
		}
		string aliasResolvedAttributeValue2 = GetAliasResolvedAttributeValue(nav.Clone(), "ColumnName");
		tableProperties.TryGetValue(aliasResolvedAttributeValue2, ignoreCase: false, out var item3);
		if (item3 == null)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Column, aliasResolvedAttributeValue2, MappingErrorCode.InvalidStorageMember, m_sourceLocation, xmlLineInfo, m_parsingErrors);
		}
		if (item == null || item3 == null)
		{
			return null;
		}
		if (!Helper.IsScalarType(item.TypeUsage.EdmType))
		{
			EdmSchemaError item4 = new EdmSchemaError(Strings.Mapping_Invalid_CSide_ScalarProperty(item.Name), 2085, EdmSchemaErrorSeverity.Error, m_sourceLocation, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
			m_parsingErrors.Add(item4);
			return null;
		}
		ValidateAndUpdateScalarMemberMapping(item, item3, xmlLineInfo);
		return new ScalarPropertyMapping(item, item3);
	}

	private ComplexPropertyMapping LoadComplexPropertyMapping(XPathNavigator nav, EdmType containerType, ReadOnlyMetadataCollection<EdmProperty> tableProperties)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		CollectionType collectionType = containerType as CollectionType;
		string aliasResolvedAttributeValue = GetAliasResolvedAttributeValue(nav.Clone(), "Name");
		EdmProperty edmProperty = null;
		EdmType item = null;
		string aliasResolvedAttributeValue2 = GetAliasResolvedAttributeValue(nav.Clone(), "TypeName");
		StructuralType structuralType = containerType as StructuralType;
		if (string.IsNullOrEmpty(aliasResolvedAttributeValue2))
		{
			if (collectionType == null)
			{
				if (structuralType != null)
				{
					structuralType.Members.TryGetValue(aliasResolvedAttributeValue, ignoreCase: false, out var item2);
					edmProperty = item2 as EdmProperty;
					if (edmProperty == null)
					{
						AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Cdm_Member, aliasResolvedAttributeValue, MappingErrorCode.InvalidEdmMember, m_sourceLocation, lineInfo, m_parsingErrors);
					}
					item = edmProperty.TypeUsage.EdmType;
				}
				else
				{
					AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Cdm_Member, aliasResolvedAttributeValue, MappingErrorCode.InvalidEdmMember, m_sourceLocation, lineInfo, m_parsingErrors);
				}
			}
			else
			{
				item = collectionType.TypeUsage.EdmType;
			}
		}
		else
		{
			if (containerType != null)
			{
				structuralType.Members.TryGetValue(aliasResolvedAttributeValue, ignoreCase: false, out var item3);
				edmProperty = item3 as EdmProperty;
			}
			if (edmProperty == null)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Cdm_Member, aliasResolvedAttributeValue, MappingErrorCode.InvalidEdmMember, m_sourceLocation, lineInfo, m_parsingErrors);
			}
			EdmItemCollection.TryGetItem<EdmType>(aliasResolvedAttributeValue2, out item);
			item = item as ComplexType;
			if (item == null)
			{
				AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Complex_Type, aliasResolvedAttributeValue2, MappingErrorCode.InvalidComplexType, m_sourceLocation, lineInfo, m_parsingErrors);
			}
		}
		ComplexPropertyMapping complexPropertyMapping = new ComplexPropertyMapping(edmProperty);
		XPathNavigator xPathNavigator = nav.Clone();
		bool flag = false;
		if (xPathNavigator.MoveToChild(XPathNodeType.Element) && xPathNavigator.LocalName == "ComplexTypeMapping")
		{
			flag = true;
		}
		if (edmProperty == null || item == null)
		{
			return null;
		}
		if (flag)
		{
			nav.MoveToChild(XPathNodeType.Element);
			do
			{
				complexPropertyMapping.AddTypeMapping(LoadComplexTypeMapping(nav.Clone(), null, tableProperties));
			}
			while (nav.MoveToNext(XPathNodeType.Element));
		}
		else
		{
			complexPropertyMapping.AddTypeMapping(LoadComplexTypeMapping(nav.Clone(), item, tableProperties));
		}
		return complexPropertyMapping;
	}

	private ComplexTypeMapping LoadComplexTypeMapping(XPathNavigator nav, EdmType type, ReadOnlyMetadataCollection<EdmProperty> tableType)
	{
		bool isPartial = false;
		string attributeValue = GetAttributeValue(nav.Clone(), "IsPartial");
		if (!string.IsNullOrEmpty(attributeValue))
		{
			isPartial = Convert.ToBoolean(attributeValue, CultureInfo.InvariantCulture);
		}
		ComplexTypeMapping complexTypeMapping = new ComplexTypeMapping(isPartial);
		if (type != null)
		{
			complexTypeMapping.AddType(type as ComplexType);
		}
		else
		{
			string text = GetAliasResolvedAttributeValue(nav.Clone(), "TypeName");
			int num = text.IndexOf(';');
			string text2 = null;
			do
			{
				if (num != -1)
				{
					text2 = text.Substring(0, num);
					text = text.Substring(num + 1, text.Length - (num + 1));
				}
				else
				{
					text2 = text;
					text = string.Empty;
				}
				int num2 = text2.IndexOf("IsTypeOf(", StringComparison.Ordinal);
				if (num2 == 0)
				{
					text2 = text2.Substring("IsTypeOf(".Length, text2.Length - ("IsTypeOf(".Length + 1));
					text2 = GetAliasResolvedValue(text2);
				}
				else
				{
					text2 = GetAliasResolvedValue(text2);
				}
				EdmItemCollection.TryGetItem<ComplexType>(text2, out var item);
				if (item == null)
				{
					AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Complex_Type, text2, MappingErrorCode.InvalidComplexType, m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
					num = text.IndexOf(';');
					continue;
				}
				if (num2 == 0)
				{
					complexTypeMapping.AddIsOfType(item);
				}
				else
				{
					complexTypeMapping.AddType(item);
				}
				num = text.IndexOf(';');
			}
			while (text.Length != 0);
		}
		if (nav.MoveToChild(XPathNodeType.Element))
		{
			do
			{
				EdmType ownerType = complexTypeMapping.GetOwnerType(GetAttributeValue(nav.Clone(), "Name"));
				switch (nav.LocalName)
				{
				case "ScalarProperty":
				{
					ScalarPropertyMapping scalarPropertyMapping = LoadScalarPropertyMapping(nav.Clone(), ownerType, tableType);
					if (scalarPropertyMapping != null)
					{
						complexTypeMapping.AddPropertyMapping(scalarPropertyMapping);
					}
					break;
				}
				case "ComplexProperty":
				{
					ComplexPropertyMapping complexPropertyMapping = LoadComplexPropertyMapping(nav.Clone(), ownerType, tableType);
					if (complexPropertyMapping != null)
					{
						complexTypeMapping.AddPropertyMapping(complexPropertyMapping);
					}
					break;
				}
				case "Condition":
				{
					ConditionPropertyMapping conditionPropertyMapping = LoadConditionPropertyMapping(nav.Clone(), ownerType, tableType);
					if (conditionPropertyMapping != null)
					{
						complexTypeMapping.AddConditionProperty(conditionPropertyMapping, delegate(EdmMember member)
						{
							AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_Duplicate_Condition_Member, member.Name, MappingErrorCode.ConditionError, m_sourceLocation, (IXmlLineInfo)nav, m_parsingErrors);
						});
					}
					break;
				}
				default:
					throw Error.NotSupported();
				}
			}
			while (nav.MoveToNext(XPathNodeType.Element));
		}
		return complexTypeMapping;
	}

	private EndPropertyMapping LoadEndPropertyMapping(XPathNavigator nav, AssociationEndMember end, EntityType tableType)
	{
		EndPropertyMapping endPropertyMapping = new EndPropertyMapping
		{
			AssociationEnd = end
		};
		nav.MoveToChild(XPathNodeType.Element);
		do
		{
			string localName = nav.LocalName;
			if (localName == null || !(localName == "ScalarProperty"))
			{
				continue;
			}
			EntityTypeBase elementType = (end.TypeUsage.EdmType as RefType).ElementType;
			ScalarPropertyMapping scalarPropertyMapping = LoadScalarPropertyMapping(nav.Clone(), elementType, tableType.Properties);
			if (scalarPropertyMapping != null)
			{
				if (!elementType.KeyMembers.Contains(scalarPropertyMapping.Property))
				{
					IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
					AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_EndProperty, scalarPropertyMapping.Property.Name, MappingErrorCode.InvalidEdmMember, m_sourceLocation, lineInfo, m_parsingErrors);
					return null;
				}
				endPropertyMapping.AddPropertyMapping(scalarPropertyMapping);
			}
		}
		while (nav.MoveToNext(XPathNodeType.Element));
		return endPropertyMapping;
	}

	private ConditionPropertyMapping LoadConditionPropertyMapping(XPathNavigator nav, EdmType containerType, ReadOnlyMetadataCollection<EdmProperty> tableProperties)
	{
		string aliasResolvedAttributeValue = GetAliasResolvedAttributeValue(nav.Clone(), "Name");
		string aliasResolvedAttributeValue2 = GetAliasResolvedAttributeValue(nav.Clone(), "ColumnName");
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		if (aliasResolvedAttributeValue != null && aliasResolvedAttributeValue2 != null)
		{
			AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Both_Members, MappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
			return null;
		}
		if (aliasResolvedAttributeValue == null && aliasResolvedAttributeValue2 == null)
		{
			AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Either_Members, MappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
			return null;
		}
		EdmProperty edmProperty = null;
		if (aliasResolvedAttributeValue != null && containerType != null)
		{
			((StructuralType)containerType).Members.TryGetValue(aliasResolvedAttributeValue, ignoreCase: false, out var item);
			edmProperty = item as EdmProperty;
		}
		EdmProperty item2 = null;
		if (aliasResolvedAttributeValue2 != null)
		{
			tableProperties.TryGetValue(aliasResolvedAttributeValue2, ignoreCase: false, out item2);
		}
		EdmProperty edmProperty2 = ((item2 != null) ? item2 : edmProperty);
		if (edmProperty2 == null)
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_ConditionMapping_InvalidMember, (aliasResolvedAttributeValue2 != null) ? aliasResolvedAttributeValue2 : aliasResolvedAttributeValue, MappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
			return null;
		}
		bool? flag = null;
		object value = null;
		string attributeValue = GetAttributeValue(nav.Clone(), "IsNull");
		EdmType edmType = edmProperty2.TypeUsage.EdmType;
		if (Helper.IsPrimitiveType(edmType))
		{
			TypeUsage typeUsage;
			if (edmProperty2.DeclaringType.DataSpace == DataSpace.SSpace)
			{
				typeUsage = StoreItemCollection.ProviderManifest.GetEdmType(edmProperty2.TypeUsage);
				if (typeUsage == null)
				{
					AddToSchemaErrorWithMessage(Strings.Mapping_ProviderReturnsNullType(edmProperty2.Name), MappingErrorCode.MappingStoreProviderReturnsNullEdmType, m_sourceLocation, lineInfo, m_parsingErrors);
					return null;
				}
			}
			else
			{
				typeUsage = edmProperty2.TypeUsage;
			}
			PrimitiveType obj = (PrimitiveType)typeUsage.EdmType;
			Type clrEquivalentType = obj.ClrEquivalentType;
			PrimitiveTypeKind primitiveTypeKind = obj.PrimitiveTypeKind;
			if (attributeValue == null && !IsTypeSupportedForCondition(primitiveTypeKind))
			{
				AddToSchemaErrorWithMemberAndStructure(Strings.Mapping_InvalidContent_ConditionMapping_InvalidPrimitiveTypeKind, edmProperty2.Name, edmType.FullName, MappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
				return null;
			}
			if (!TryGetTypedAttributeValue(nav.Clone(), "Value", clrEquivalentType, m_sourceLocation, m_parsingErrors, out value))
			{
				return null;
			}
		}
		else
		{
			if (!Helper.IsEnumType(edmType))
			{
				AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_NonScalar, MappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
				return null;
			}
			value = GetEnumAttributeValue(nav.Clone(), "Value", (EnumType)edmType, m_sourceLocation, m_parsingErrors);
		}
		if (attributeValue != null && value != null)
		{
			AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Both_Values, MappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
			return null;
		}
		if (attributeValue == null && value == null)
		{
			AddToSchemaErrors(Strings.Mapping_InvalidContent_ConditionMapping_Either_Values, MappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
			return null;
		}
		if (attributeValue != null)
		{
			flag = Convert.ToBoolean(attributeValue, CultureInfo.InvariantCulture);
		}
		if (item2 != null && (item2.IsStoreGeneratedComputed || item2.IsStoreGeneratedIdentity))
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_InvalidContent_ConditionMapping_Computed, item2.Name, MappingErrorCode.ConditionError, m_sourceLocation, lineInfo, m_parsingErrors);
			return null;
		}
		if (value == null)
		{
			return new IsNullConditionMapping(edmProperty2, flag.Value);
		}
		return new ValueConditionMapping(edmProperty2, value);
	}

	internal static bool IsTypeSupportedForCondition(PrimitiveTypeKind primitiveTypeKind)
	{
		switch (primitiveTypeKind)
		{
		case PrimitiveTypeKind.Boolean:
		case PrimitiveTypeKind.Byte:
		case PrimitiveTypeKind.SByte:
		case PrimitiveTypeKind.Int16:
		case PrimitiveTypeKind.Int32:
		case PrimitiveTypeKind.Int64:
		case PrimitiveTypeKind.String:
			return true;
		case PrimitiveTypeKind.Binary:
		case PrimitiveTypeKind.DateTime:
		case PrimitiveTypeKind.Decimal:
		case PrimitiveTypeKind.Double:
		case PrimitiveTypeKind.Guid:
		case PrimitiveTypeKind.Single:
		case PrimitiveTypeKind.Time:
		case PrimitiveTypeKind.DateTimeOffset:
			return false;
		default:
			return false;
		}
	}

	private static XmlSchemaSet GetOrCreateSchemaSet()
	{
		if (s_mappingXmlSchema == null)
		{
			XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
			AddResourceXsdToSchemaSet(xmlSchemaSet, "System.Data.Resources.CSMSL_1.xsd");
			AddResourceXsdToSchemaSet(xmlSchemaSet, "System.Data.Resources.CSMSL_2.xsd");
			AddResourceXsdToSchemaSet(xmlSchemaSet, "System.Data.Resources.CSMSL_3.xsd");
			Interlocked.CompareExchange(ref s_mappingXmlSchema, xmlSchemaSet, null);
		}
		return s_mappingXmlSchema;
	}

	private static void AddResourceXsdToSchemaSet(XmlSchemaSet set, string resourceName)
	{
		using XmlReader reader = DbProviderServices.GetXmlResource(resourceName);
		XmlSchema schema = XmlSchema.Read(reader, null);
		set.Add(schema);
	}

	internal static void AddToSchemaErrors(string message, MappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
	{
		EdmSchemaError item = new EdmSchemaError(message, (int)errorCode, EdmSchemaErrorSeverity.Error, location, lineInfo.LineNumber, lineInfo.LinePosition);
		parsingErrors.Add(item);
	}

	internal static EdmSchemaError AddToSchemaErrorsWithMemberInfo(Func<object, string> messageFormat, string errorMember, MappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
	{
		EdmSchemaError edmSchemaError = new EdmSchemaError(messageFormat(errorMember), (int)errorCode, EdmSchemaErrorSeverity.Error, location, lineInfo.LineNumber, lineInfo.LinePosition);
		parsingErrors.Add(edmSchemaError);
		return edmSchemaError;
	}

	internal static void AddToSchemaErrorWithMemberAndStructure(Func<object, object, string> messageFormat, string errorMember, string errorStructure, MappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
	{
		EdmSchemaError item = new EdmSchemaError(messageFormat(errorMember, errorStructure), (int)errorCode, EdmSchemaErrorSeverity.Error, location, lineInfo.LineNumber, lineInfo.LinePosition);
		parsingErrors.Add(item);
	}

	private static void AddToSchemaErrorWithMessage(string errorMessage, MappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
	{
		EdmSchemaError item = new EdmSchemaError(errorMessage, (int)errorCode, EdmSchemaErrorSeverity.Error, location, lineInfo.LineNumber, lineInfo.LinePosition);
		parsingErrors.Add(item);
	}

	private string GetAliasResolvedAttributeValue(XPathNavigator nav, string attributeName)
	{
		return GetAliasResolvedValue(GetAttributeValue(nav, attributeName));
	}

	private static bool GetBoolAttributeValue(XPathNavigator nav, string attributeName, bool defaultValue)
	{
		bool result = defaultValue;
		object typedAttributeValue = Helper.GetTypedAttributeValue(nav, attributeName, typeof(bool));
		if (typedAttributeValue != null)
		{
			result = (bool)typedAttributeValue;
		}
		return result;
	}

	private static string GetAttributeValue(XPathNavigator nav, string attributeName)
	{
		return Helper.GetAttributeValue(nav, attributeName);
	}

	private static bool TryGetTypedAttributeValue(XPathNavigator nav, string attributeName, Type clrType, string sourceLocation, IList<EdmSchemaError> parsingErrors, out object value)
	{
		value = null;
		try
		{
			value = Helper.GetTypedAttributeValue(nav, attributeName, clrType);
		}
		catch (FormatException)
		{
			AddToSchemaErrors(Strings.Mapping_ConditionValueTypeMismatch, MappingErrorCode.ConditionError, sourceLocation, (IXmlLineInfo)nav, parsingErrors);
			return false;
		}
		return true;
	}

	private static EnumMember GetEnumAttributeValue(XPathNavigator nav, string attributeName, EnumType enumType, string sourceLocation, IList<EdmSchemaError> parsingErrors)
	{
		IXmlLineInfo lineInfo = (IXmlLineInfo)nav;
		string attributeValue = GetAttributeValue(nav, attributeName);
		if (string.IsNullOrEmpty(attributeValue))
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_Enum_EmptyValue, enumType.FullName, MappingErrorCode.InvalidEnumValue, sourceLocation, lineInfo, parsingErrors);
		}
		if (!enumType.Members.TryGetValue(attributeValue, ignoreCase: false, out var item))
		{
			AddToSchemaErrorsWithMemberInfo(Strings.Mapping_Enum_InvalidValue, attributeValue, MappingErrorCode.InvalidEnumValue, sourceLocation, lineInfo, parsingErrors);
		}
		return item;
	}

	private string GetAliasResolvedValue(string aliasedString)
	{
		if (aliasedString == null || aliasedString.Length == 0)
		{
			return aliasedString;
		}
		int num = aliasedString.LastIndexOf('.');
		if (num == -1)
		{
			return aliasedString;
		}
		string key = aliasedString.Substring(0, num);
		m_alias.TryGetValue(key, out var value);
		if (value != null)
		{
			aliasedString = value + aliasedString.Substring(num);
		}
		return aliasedString;
	}

	private XmlReader GetSchemaValidatingReader(XmlReader innerReader)
	{
		XmlReaderSettings xmlReaderSettings = GetXmlReaderSettings();
		return XmlReader.Create(innerReader, xmlReaderSettings);
	}

	private XmlReaderSettings GetXmlReaderSettings()
	{
		XmlReaderSettings xmlReaderSettings = Schema.CreateEdmStandardXmlReaderSettings();
		xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
		xmlReaderSettings.ValidationEventHandler += XsdValidationCallBack;
		xmlReaderSettings.ValidationType = ValidationType.Schema;
		xmlReaderSettings.Schemas = GetOrCreateSchemaSet();
		return xmlReaderSettings;
	}

	private void XsdValidationCallBack(object sender, ValidationEventArgs args)
	{
		if (args.Severity != XmlSeverityType.Warning)
		{
			string schemaLocation = null;
			if (!string.IsNullOrEmpty(args.Exception.SourceUri))
			{
				schemaLocation = Helper.GetFileNameFromUri(new Uri(args.Exception.SourceUri));
			}
			EdmSchemaErrorSeverity severity = EdmSchemaErrorSeverity.Error;
			if (args.Severity == XmlSeverityType.Warning)
			{
				severity = EdmSchemaErrorSeverity.Warning;
			}
			EdmSchemaError item = new EdmSchemaError(Strings.Mapping_InvalidMappingSchema_validation(args.Exception.Message), 2025, severity, schemaLocation, args.Exception.LineNumber, args.Exception.LinePosition);
			m_parsingErrors.Add(item);
		}
	}

	private void ValidateAndUpdateScalarMemberMapping(EdmProperty member, EdmProperty columnMember, IXmlLineInfo lineInfo)
	{
		if (!m_scalarMemberMappings.TryGetValue(member, out var value))
		{
			int count = m_parsingErrors.Count;
			TypeUsage typeUsage = Helper.ValidateAndConvertTypeUsage(member, columnMember);
			if (typeUsage == null)
			{
				if (count == m_parsingErrors.Count)
				{
					EdmSchemaError item = new EdmSchemaError(GetInvalidMemberMappingErrorMessage(member, columnMember), 2019, EdmSchemaErrorSeverity.Error, m_sourceLocation, lineInfo.LineNumber, lineInfo.LinePosition);
					m_parsingErrors.Add(item);
				}
			}
			else
			{
				m_scalarMemberMappings.Add(member, new KeyValuePair<TypeUsage, TypeUsage>(typeUsage, columnMember.TypeUsage));
			}
			return;
		}
		TypeUsage value2 = value.Value;
		TypeUsage modelTypeUsage = columnMember.TypeUsage.ModelTypeUsage;
		if (columnMember.TypeUsage.EdmType != value2.EdmType)
		{
			EdmSchemaError item2 = new EdmSchemaError(Strings.Mapping_StoreTypeMismatch_ScalarPropertyMapping(member.Name, value2.EdmType.Name), 2039, EdmSchemaErrorSeverity.Error, m_sourceLocation, lineInfo.LineNumber, lineInfo.LinePosition);
			m_parsingErrors.Add(item2);
		}
		else if (!TypeSemantics.IsSubTypeOf(ResolveTypeUsageForEnums(member.TypeUsage), modelTypeUsage))
		{
			EdmSchemaError item3 = new EdmSchemaError(GetInvalidMemberMappingErrorMessage(member, columnMember), 2019, EdmSchemaErrorSeverity.Error, m_sourceLocation, lineInfo.LineNumber, lineInfo.LinePosition);
			m_parsingErrors.Add(item3);
		}
	}

	internal static string GetInvalidMemberMappingErrorMessage(EdmMember cSpaceMember, EdmMember sSpaceMember)
	{
		return Strings.Mapping_Invalid_Member_Mapping(cSpaceMember.TypeUsage.EdmType?.ToString() + GetFacetsForDisplay(cSpaceMember.TypeUsage), cSpaceMember.Name, cSpaceMember.DeclaringType.FullName, sSpaceMember.TypeUsage.EdmType?.ToString() + GetFacetsForDisplay(sSpaceMember.TypeUsage), sSpaceMember.Name, sSpaceMember.DeclaringType.FullName);
	}

	private static string GetFacetsForDisplay(TypeUsage typeUsage)
	{
		ReadOnlyMetadataCollection<Facet> facets = typeUsage.Facets;
		if (facets == null || facets.Count == 0)
		{
			return string.Empty;
		}
		int count = facets.Count;
		StringBuilder stringBuilder = new StringBuilder("[");
		for (int i = 0; i < count - 1; i++)
		{
			stringBuilder.AppendFormat("{0}={1},", facets[i].Name, facets[i].Value ?? string.Empty);
		}
		stringBuilder.AppendFormat("{0}={1}]", facets[count - 1].Name, facets[count - 1].Value ?? string.Empty);
		return stringBuilder.ToString();
	}

	internal static TypeUsage ResolveTypeUsageForEnums(TypeUsage typeUsage)
	{
		if (!Helper.IsEnumType(typeUsage.EdmType))
		{
			return typeUsage;
		}
		return TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(typeUsage.EdmType), typeUsage.Facets);
	}
}
