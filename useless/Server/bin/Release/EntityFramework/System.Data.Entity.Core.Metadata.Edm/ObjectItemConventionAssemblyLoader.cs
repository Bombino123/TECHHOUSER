using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class ObjectItemConventionAssemblyLoader : ObjectItemAssemblyLoader
{
	internal class ConventionOSpaceTypeFactory : OSpaceTypeFactory
	{
		private readonly ObjectItemConventionAssemblyLoader _loader;

		public override List<Action> ReferenceResolutions => _loader._referenceResolutions;

		public override Dictionary<EdmType, EdmType> CspaceToOspace => _loader.SessionData.CspaceToOspace;

		public override Dictionary<string, EdmType> LoadedTypes => _loader.SessionData.TypesInLoading;

		public ConventionOSpaceTypeFactory(ObjectItemConventionAssemblyLoader loader)
		{
			_loader = loader;
		}

		public override void LogLoadMessage(string message, EdmType relatedType)
		{
			_loader.SessionData.LoadMessageLogger.LogLoadMessage(message, relatedType);
		}

		public override void LogError(string errorMessage, EdmType relatedType)
		{
			string message = _loader.SessionData.LoadMessageLogger.CreateErrorMessageWithTypeSpecificLoadLogs(errorMessage, relatedType);
			_loader.SessionData.EdmItemErrors.Add(new EdmItemError(message));
		}

		public override void TrackClosure(Type type)
		{
			_loader.TrackClosure(type);
		}

		public override void AddToTypesInAssembly(EdmType type)
		{
			_loader.CacheEntry.TypesInAssembly.Add(type);
		}
	}

	private readonly List<Action> _referenceResolutions = new List<Action>();

	private readonly ConventionOSpaceTypeFactory _factory;

	public new virtual MutableAssemblyCacheEntry CacheEntry => (MutableAssemblyCacheEntry)base.CacheEntry;

	internal ObjectItemConventionAssemblyLoader(Assembly assembly, ObjectItemLoadingSessionData sessionData)
		: base(assembly, new MutableAssemblyCacheEntry(), sessionData)
	{
		base.SessionData.RegisterForLevel1PostSessionProcessing(this);
		_factory = new ConventionOSpaceTypeFactory(this);
	}

	protected override void LoadTypesFromAssembly()
	{
		foreach (Type accessibleType in base.SourceAssembly.GetAccessibleTypes())
		{
			if (!TryGetCSpaceTypeMatch(accessibleType, out var cspaceType))
			{
				continue;
			}
			if (accessibleType.IsValueType() && !accessibleType.IsEnum())
			{
				base.SessionData.LoadMessageLogger.LogLoadMessage(Strings.Validator_OSpace_Convention_Struct(cspaceType.FullName, accessibleType.FullName), cspaceType);
				continue;
			}
			EdmType edmType = _factory.TryCreateType(accessibleType, cspaceType);
			if (edmType != null)
			{
				CacheEntry.TypesInAssembly.Add(edmType);
				if (!base.SessionData.CspaceToOspace.ContainsKey(cspaceType))
				{
					base.SessionData.CspaceToOspace.Add(cspaceType, edmType);
					continue;
				}
				EdmType edmType2 = base.SessionData.CspaceToOspace[cspaceType];
				base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_OSpace_Convention_AmbiguousClrType(cspaceType.Name, edmType2.ClrType.FullName, accessibleType.FullName)));
			}
		}
		if (base.SessionData.TypesInLoading.Count == 0)
		{
			base.SessionData.ObjectItemAssemblyLoaderFactory = null;
		}
	}

	protected override void AddToAssembliesLoaded()
	{
		base.SessionData.AssembliesLoaded.Add(base.SourceAssembly, CacheEntry);
	}

	private bool TryGetCSpaceTypeMatch(Type type, out EdmType cspaceType)
	{
		if (base.SessionData.ConventionCSpaceTypeNames.TryGetValue(type.Name, out var value))
		{
			if (value.Value == 1)
			{
				cspaceType = value.Key;
				return true;
			}
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_OSpace_Convention_MultipleTypesWithSameName(type.Name)));
		}
		cspaceType = null;
		return false;
	}

	internal override void OnLevel1SessionProcessing()
	{
		CreateRelationships();
		foreach (Action referenceResolution in _referenceResolutions)
		{
			referenceResolution();
		}
		base.OnLevel1SessionProcessing();
	}

	internal virtual void TrackClosure(Type type)
	{
		if (base.SourceAssembly != type.Assembly() && !CacheEntry.ClosureAssemblies.Contains(type.Assembly()) && (!type.IsGenericType() || (!EntityUtil.IsAnICollection(type) && !(type.GetGenericTypeDefinition() == typeof(EntityReference<>)) && !(type.GetGenericTypeDefinition() == typeof(Nullable<>)))))
		{
			CacheEntry.ClosureAssemblies.Add(type.Assembly());
		}
		if (type.IsGenericType())
		{
			Type[] genericArguments = type.GetGenericArguments();
			foreach (Type type2 in genericArguments)
			{
				TrackClosure(type2);
			}
		}
	}

	private void CreateRelationships()
	{
		if (!base.SessionData.ConventionBasedRelationshipsAreLoaded)
		{
			base.SessionData.ConventionBasedRelationshipsAreLoaded = true;
			_factory.CreateRelationships(base.SessionData.EdmItemCollection);
		}
	}

	internal static bool SessionContainsConventionParameters(ObjectItemLoadingSessionData sessionData)
	{
		return sessionData.EdmItemCollection != null;
	}

	internal static ObjectItemAssemblyLoader Create(Assembly assembly, ObjectItemLoadingSessionData sessionData)
	{
		if (!ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent(assembly))
		{
			return new ObjectItemConventionAssemblyLoader(assembly, sessionData);
		}
		sessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_OSpace_Convention_AttributeAssemblyReferenced(assembly.FullName)));
		return new ObjectItemNoOpAssemblyLoader(assembly, sessionData);
	}
}
