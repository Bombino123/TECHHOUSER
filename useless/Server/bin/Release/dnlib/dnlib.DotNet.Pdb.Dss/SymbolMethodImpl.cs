using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Threading;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Pdb.Symbols;

namespace dnlib.DotNet.Pdb.Dss;

internal sealed class SymbolMethodImpl : SymbolMethod
{
	private readonly SymbolReaderImpl reader;

	private readonly ISymUnmanagedMethod method;

	private readonly ISymUnmanagedAsyncMethod asyncMethod;

	private volatile SymbolScope rootScope;

	private volatile SymbolSequencePoint[] sequencePoints;

	private volatile SymbolAsyncStepInfo[] asyncStepInfos;

	public override int Token
	{
		get
		{
			method.GetToken(out var pToken);
			return (int)pToken;
		}
	}

	public override SymbolScope RootScope
	{
		get
		{
			if (rootScope == null)
			{
				method.GetRootScope(out var pRetVal);
				Interlocked.CompareExchange(ref rootScope, (pRetVal == null) ? null : new SymbolScopeImpl(pRetVal, this, null), null);
			}
			return rootScope;
		}
	}

	public override IList<SymbolSequencePoint> SequencePoints
	{
		get
		{
			if (sequencePoints == null)
			{
				method.GetSequencePointCount(out var pRetVal);
				SymbolSequencePoint[] array = new SymbolSequencePoint[pRetVal];
				int[] array2 = new int[array.Length];
				_ = new ISymbolDocument[array.Length];
				int[] array3 = new int[array.Length];
				int[] array4 = new int[array.Length];
				int[] array5 = new int[array.Length];
				int[] array6 = new int[array.Length];
				ISymUnmanagedDocument[] array7 = new ISymUnmanagedDocument[array.Length];
				if (array.Length != 0)
				{
					method.GetSequencePoints((uint)array.Length, out var _, array2, array7, array3, array4, array5, array6);
				}
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new SymbolSequencePoint
					{
						Offset = array2[i],
						Document = new SymbolDocumentImpl(array7[i]),
						Line = array3[i],
						Column = array4[i],
						EndLine = array5[i],
						EndColumn = array6[i]
					};
				}
				sequencePoints = array;
			}
			return sequencePoints;
		}
	}

	public int AsyncKickoffMethod
	{
		get
		{
			if (asyncMethod == null || !asyncMethod.IsAsyncMethod())
			{
				return 0;
			}
			return (int)asyncMethod.GetKickoffMethod();
		}
	}

	public uint? AsyncCatchHandlerILOffset
	{
		get
		{
			if (asyncMethod == null || !asyncMethod.IsAsyncMethod())
			{
				return null;
			}
			if (!asyncMethod.HasCatchHandlerILOffset())
			{
				return null;
			}
			return asyncMethod.GetCatchHandlerILOffset();
		}
	}

	public IList<SymbolAsyncStepInfo> AsyncStepInfos
	{
		get
		{
			if (asyncMethod == null || !asyncMethod.IsAsyncMethod())
			{
				return null;
			}
			if (asyncStepInfos == null)
			{
				uint pcStepInfo = asyncMethod.GetAsyncStepInfoCount();
				uint[] array = new uint[pcStepInfo];
				uint[] array2 = new uint[pcStepInfo];
				uint[] array3 = new uint[pcStepInfo];
				asyncMethod.GetAsyncStepInfo(pcStepInfo, out pcStepInfo, array, array2, array3);
				SymbolAsyncStepInfo[] array4 = new SymbolAsyncStepInfo[pcStepInfo];
				for (int i = 0; i < array4.Length; i++)
				{
					array4[i] = new SymbolAsyncStepInfo(array[i], array2[i], array3[i]);
				}
				asyncStepInfos = array4;
			}
			return asyncStepInfos;
		}
	}

	public SymbolMethodImpl(SymbolReaderImpl reader, ISymUnmanagedMethod method)
	{
		this.reader = reader;
		this.method = method;
		asyncMethod = method as ISymUnmanagedAsyncMethod;
	}

	public override void GetCustomDebugInfos(MethodDef method, CilBody body, IList<PdbCustomDebugInfo> result)
	{
		reader.GetCustomDebugInfos(this, method, body, result);
	}
}
