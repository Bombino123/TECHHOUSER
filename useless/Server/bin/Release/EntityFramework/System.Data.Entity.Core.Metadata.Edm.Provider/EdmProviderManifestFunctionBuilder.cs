using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm.Provider;

internal sealed class EdmProviderManifestFunctionBuilder
{
	private readonly List<EdmFunction> functions = new List<EdmFunction>();

	private readonly TypeUsage[] primitiveTypes;

	internal EdmProviderManifestFunctionBuilder(ReadOnlyCollection<PrimitiveType> edmPrimitiveTypes)
	{
		TypeUsage[] array = new TypeUsage[edmPrimitiveTypes.Count];
		foreach (PrimitiveType edmPrimitiveType in edmPrimitiveTypes)
		{
			array[(int)edmPrimitiveType.PrimitiveTypeKind] = TypeUsage.Create(edmPrimitiveType);
		}
		primitiveTypes = array;
	}

	internal ReadOnlyCollection<EdmFunction> ToFunctionCollection()
	{
		return new ReadOnlyCollection<EdmFunction>(functions);
	}

	internal static void ForAllBasePrimitiveTypes(Action<PrimitiveTypeKind> forEachType)
	{
		for (int i = 0; i < 32; i++)
		{
			PrimitiveTypeKind primitiveTypeKind = (PrimitiveTypeKind)i;
			if (!Helper.IsStrongSpatialTypeKind(primitiveTypeKind))
			{
				forEachType(primitiveTypeKind);
			}
		}
	}

	internal static void ForTypes(IEnumerable<PrimitiveTypeKind> typeKinds, Action<PrimitiveTypeKind> forEachType)
	{
		foreach (PrimitiveTypeKind typeKind in typeKinds)
		{
			forEachType(typeKind);
		}
	}

	internal void AddAggregate(string aggregateFunctionName, PrimitiveTypeKind collectionArgumentElementTypeKind)
	{
		AddAggregate(collectionArgumentElementTypeKind, aggregateFunctionName, collectionArgumentElementTypeKind);
	}

	internal void AddAggregate(PrimitiveTypeKind returnTypeKind, string aggregateFunctionName, PrimitiveTypeKind collectionArgumentElementTypeKind)
	{
		FunctionParameter functionParameter = CreateReturnParameter(returnTypeKind);
		FunctionParameter functionParameter2 = CreateAggregateParameter(collectionArgumentElementTypeKind);
		EdmFunction edmFunction = new EdmFunction(aggregateFunctionName, "Edm", DataSpace.CSpace, new EdmFunctionPayload
		{
			IsAggregate = true,
			IsBuiltIn = true,
			ReturnParameters = new FunctionParameter[1] { functionParameter },
			Parameters = new FunctionParameter[1] { functionParameter2 },
			IsFromProviderManifest = true
		});
		edmFunction.SetReadOnly();
		functions.Add(edmFunction);
	}

	internal void AddFunction(PrimitiveTypeKind returnType, string functionName)
	{
		AddFunction(returnType, functionName, new KeyValuePair<string, PrimitiveTypeKind>[0]);
	}

	internal void AddFunction(PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argumentTypeKind, string argumentName)
	{
		AddFunction(returnType, functionName, new KeyValuePair<string, PrimitiveTypeKind>[1]
		{
			new KeyValuePair<string, PrimitiveTypeKind>(argumentName, argumentTypeKind)
		});
	}

	internal void AddFunction(PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name, PrimitiveTypeKind argument2TypeKind, string argument2Name)
	{
		AddFunction(returnType, functionName, new KeyValuePair<string, PrimitiveTypeKind>[2]
		{
			new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind)
		});
	}

	internal void AddFunction(PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name, PrimitiveTypeKind argument2TypeKind, string argument2Name, PrimitiveTypeKind argument3TypeKind, string argument3Name)
	{
		AddFunction(returnType, functionName, new KeyValuePair<string, PrimitiveTypeKind>[3]
		{
			new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument3Name, argument3TypeKind)
		});
	}

	internal void AddFunction(PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name, PrimitiveTypeKind argument2TypeKind, string argument2Name, PrimitiveTypeKind argument3TypeKind, string argument3Name, PrimitiveTypeKind argument4TypeKind, string argument4Name, PrimitiveTypeKind argument5TypeKind, string argument5Name, PrimitiveTypeKind argument6TypeKind, string argument6Name)
	{
		AddFunction(returnType, functionName, new KeyValuePair<string, PrimitiveTypeKind>[6]
		{
			new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument3Name, argument3TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument4Name, argument4TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument5Name, argument5TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument6Name, argument6TypeKind)
		});
	}

	internal void AddFunction(PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name, PrimitiveTypeKind argument2TypeKind, string argument2Name, PrimitiveTypeKind argument3TypeKind, string argument3Name, PrimitiveTypeKind argument4TypeKind, string argument4Name, PrimitiveTypeKind argument5TypeKind, string argument5Name, PrimitiveTypeKind argument6TypeKind, string argument6Name, PrimitiveTypeKind argument7TypeKind, string argument7Name)
	{
		AddFunction(returnType, functionName, new KeyValuePair<string, PrimitiveTypeKind>[7]
		{
			new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument3Name, argument3TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument4Name, argument4TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument5Name, argument5TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument6Name, argument6TypeKind),
			new KeyValuePair<string, PrimitiveTypeKind>(argument7Name, argument7TypeKind)
		});
	}

	private void AddFunction(PrimitiveTypeKind returnType, string functionName, KeyValuePair<string, PrimitiveTypeKind>[] parameterDefinitions)
	{
		FunctionParameter functionParameter = CreateReturnParameter(returnType);
		FunctionParameter[] parameters = parameterDefinitions.Select((KeyValuePair<string, PrimitiveTypeKind> paramDef) => CreateParameter(paramDef.Value, paramDef.Key)).ToArray();
		EdmFunction edmFunction = new EdmFunction(functionName, "Edm", DataSpace.CSpace, new EdmFunctionPayload
		{
			IsBuiltIn = true,
			ReturnParameters = new FunctionParameter[1] { functionParameter },
			Parameters = parameters,
			IsFromProviderManifest = true
		});
		edmFunction.SetReadOnly();
		functions.Add(edmFunction);
	}

	private FunctionParameter CreateParameter(PrimitiveTypeKind primitiveParameterType, string parameterName)
	{
		return new FunctionParameter(parameterName, primitiveTypes[(int)primitiveParameterType], ParameterMode.In);
	}

	private FunctionParameter CreateAggregateParameter(PrimitiveTypeKind collectionParameterTypeElementTypeKind)
	{
		return new FunctionParameter("collection", TypeUsage.Create(primitiveTypes[(int)collectionParameterTypeElementTypeKind].EdmType.GetCollectionType()), ParameterMode.In);
	}

	private FunctionParameter CreateReturnParameter(PrimitiveTypeKind primitiveReturnType)
	{
		return new FunctionParameter("ReturnType", primitiveTypes[(int)primitiveReturnType], ParameterMode.ReturnValue);
	}
}
