using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using dnlib.Threading;
using dnlib.Utils;

namespace dnlib.DotNet;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(ParameterList_CollectionDebugView))]
[ComVisible(true)]
public sealed class ParameterList : IList<Parameter>, ICollection<Parameter>, IEnumerable<Parameter>, IEnumerable
{
	public struct Enumerator : IEnumerator<Parameter>, IDisposable, IEnumerator
	{
		private readonly ParameterList list;

		private List<Parameter>.Enumerator listEnumerator;

		private Parameter current;

		public Parameter Current => current;

		Parameter IEnumerator<Parameter>.Current => current;

		object IEnumerator.Current => current;

		internal Enumerator(ParameterList list)
		{
			this.list = list;
			current = null;
			list.theLock.EnterReadLock();
			try
			{
				listEnumerator = list.parameters.GetEnumerator();
			}
			finally
			{
				list.theLock.ExitReadLock();
			}
		}

		public bool MoveNext()
		{
			list.theLock.EnterWriteLock();
			try
			{
				bool result = listEnumerator.MoveNext();
				current = listEnumerator.Current;
				return result;
			}
			finally
			{
				list.theLock.ExitWriteLock();
			}
		}

		public void Dispose()
		{
			listEnumerator.Dispose();
		}

		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private readonly MethodDef method;

	private readonly List<Parameter> parameters;

	private readonly Parameter hiddenThisParameter;

	private ParamDef hiddenThisParamDef;

	private readonly Parameter returnParameter;

	private int methodSigIndexBase;

	private readonly Lock theLock = Lock.Create();

	public MethodDef Method => method;

	public int Count
	{
		get
		{
			theLock.EnterReadLock();
			try
			{
				return parameters.Count;
			}
			finally
			{
				theLock.ExitReadLock();
			}
		}
	}

	public int MethodSigIndexBase
	{
		get
		{
			theLock.EnterReadLock();
			try
			{
				return (methodSigIndexBase == 1) ? 1 : 0;
			}
			finally
			{
				theLock.ExitReadLock();
			}
		}
	}

	public Parameter this[int index]
	{
		get
		{
			theLock.EnterReadLock();
			try
			{
				return parameters[index];
			}
			finally
			{
				theLock.ExitReadLock();
			}
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public Parameter ReturnParameter
	{
		get
		{
			theLock.EnterReadLock();
			try
			{
				return returnParameter;
			}
			finally
			{
				theLock.ExitReadLock();
			}
		}
	}

	bool ICollection<Parameter>.IsReadOnly => true;

	public ParameterList(MethodDef method, TypeDef declaringType)
	{
		this.method = method;
		parameters = new List<Parameter>();
		methodSigIndexBase = -1;
		hiddenThisParameter = new Parameter(this, 0, -2);
		returnParameter = new Parameter(this, -1, -1);
		UpdateThisParameterType(declaringType);
		UpdateParameterTypes();
	}

	internal void UpdateThisParameterType(TypeDef methodDeclaringType)
	{
		theLock.EnterWriteLock();
		try
		{
			if (methodDeclaringType == null)
			{
				hiddenThisParameter.Type = null;
				return;
			}
			bool isValueType = methodDeclaringType.IsValueType;
			ClassOrValueTypeSig classOrValueTypeSig = ((!isValueType) ? ((ClassOrValueTypeSig)new ClassSig(methodDeclaringType)) : ((ClassOrValueTypeSig)new ValueTypeSig(methodDeclaringType)));
			TypeSig typeSig;
			if (methodDeclaringType.HasGenericParameters)
			{
				int count = methodDeclaringType.GenericParameters.Count;
				List<TypeSig> list = new List<TypeSig>(count);
				for (int i = 0; i < count; i++)
				{
					list.Add(new GenericVar(i, methodDeclaringType));
				}
				typeSig = new GenericInstSig(classOrValueTypeSig, list);
			}
			else
			{
				typeSig = classOrValueTypeSig;
			}
			hiddenThisParameter.Type = (isValueType ? new ByRefSig(typeSig) : typeSig);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	public void UpdateParameterTypes()
	{
		theLock.EnterWriteLock();
		try
		{
			MethodSig methodSig = method.MethodSig;
			if (methodSig == null)
			{
				methodSigIndexBase = -1;
				parameters.Clear();
				return;
			}
			if (UpdateThisParameter_NoLock(methodSig))
			{
				parameters.Clear();
			}
			returnParameter.Type = methodSig.RetType;
			ResizeParameters_NoLock(methodSig.Params.Count + methodSigIndexBase);
			if (methodSigIndexBase > 0)
			{
				parameters[0] = hiddenThisParameter;
			}
			for (int i = 0; i < methodSig.Params.Count; i++)
			{
				parameters[i + methodSigIndexBase].Type = methodSig.Params[i];
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private bool UpdateThisParameter_NoLock(MethodSig methodSig)
	{
		int num = ((methodSig != null) ? (methodSig.ImplicitThis ? 1 : 0) : (-1));
		if (methodSigIndexBase == num)
		{
			return false;
		}
		methodSigIndexBase = num;
		return true;
	}

	private void ResizeParameters_NoLock(int length)
	{
		if (parameters.Count == length)
		{
			return;
		}
		if (parameters.Count < length)
		{
			for (int i = parameters.Count; i < length; i++)
			{
				parameters.Add(new Parameter(this, i, i - methodSigIndexBase));
			}
		}
		else
		{
			while (parameters.Count > length)
			{
				parameters.RemoveAt(parameters.Count - 1);
			}
		}
	}

	internal ParamDef FindParamDef(Parameter param)
	{
		theLock.EnterReadLock();
		try
		{
			return FindParamDef_NoLock(param);
		}
		finally
		{
			theLock.ExitReadLock();
		}
	}

	private ParamDef FindParamDef_NoLock(Parameter param)
	{
		int num;
		if (param.IsReturnTypeParameter)
		{
			num = 0;
		}
		else
		{
			if (!param.IsNormalMethodParameter)
			{
				return hiddenThisParamDef;
			}
			num = param.MethodSigIndex + 1;
		}
		IList<ParamDef> paramDefs = method.ParamDefs;
		int count = paramDefs.Count;
		for (int i = 0; i < count; i++)
		{
			ParamDef paramDef = paramDefs[i];
			if (paramDef != null && paramDef.Sequence == num)
			{
				return paramDef;
			}
		}
		return null;
	}

	internal void TypeUpdated(Parameter param)
	{
		MethodSig methodSig = method.MethodSig;
		if (methodSig != null)
		{
			int methodSigIndex = param.MethodSigIndex;
			if (methodSigIndex == -1)
			{
				methodSig.RetType = param.Type;
			}
			else if (methodSigIndex >= 0)
			{
				methodSig.Params[methodSigIndex] = param.Type;
			}
		}
	}

	internal void CreateParamDef(Parameter param)
	{
		theLock.EnterWriteLock();
		try
		{
			ParamDef paramDef = FindParamDef_NoLock(param);
			if (paramDef == null)
			{
				if (param.IsHiddenThisParameter)
				{
					hiddenThisParamDef = UpdateRowId_NoLock(new ParamDefUser(UTF8String.Empty, ushort.MaxValue, (ParamAttributes)0));
					return;
				}
				int num = ((!param.IsReturnTypeParameter) ? (param.MethodSigIndex + 1) : 0);
				paramDef = UpdateRowId_NoLock(new ParamDefUser(UTF8String.Empty, (ushort)num, (ParamAttributes)0));
				method.ParamDefs.Add(paramDef);
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private ParamDef UpdateRowId_NoLock(ParamDef pd)
	{
		TypeDef declaringType = method.DeclaringType;
		if (declaringType == null)
		{
			return pd;
		}
		ModuleDef module = declaringType.Module;
		if (module == null)
		{
			return pd;
		}
		return module.UpdateRowId(pd);
	}

	public int IndexOf(Parameter item)
	{
		theLock.EnterReadLock();
		try
		{
			return parameters.IndexOf(item);
		}
		finally
		{
			theLock.ExitReadLock();
		}
	}

	void IList<Parameter>.Insert(int index, Parameter item)
	{
		throw new NotSupportedException();
	}

	void IList<Parameter>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	void ICollection<Parameter>.Add(Parameter item)
	{
		throw new NotSupportedException();
	}

	void ICollection<Parameter>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<Parameter>.Contains(Parameter item)
	{
		theLock.EnterReadLock();
		try
		{
			return parameters.Contains(item);
		}
		finally
		{
			theLock.ExitReadLock();
		}
	}

	void ICollection<Parameter>.CopyTo(Parameter[] array, int arrayIndex)
	{
		theLock.EnterReadLock();
		try
		{
			parameters.CopyTo(array, arrayIndex);
		}
		finally
		{
			theLock.ExitReadLock();
		}
	}

	bool ICollection<Parameter>.Remove(Parameter item)
	{
		throw new NotSupportedException();
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<Parameter> IEnumerable<Parameter>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
