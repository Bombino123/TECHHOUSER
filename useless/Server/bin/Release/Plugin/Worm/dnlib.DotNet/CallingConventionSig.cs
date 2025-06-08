using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class CallingConventionSig : IContainsGenericParameter
{
	protected CallingConvention callingConvention;

	private byte[] extraData;

	public byte[] ExtraData
	{
		get
		{
			return extraData;
		}
		set
		{
			extraData = value;
		}
	}

	public bool IsDefault => (callingConvention & CallingConvention.Mask) == 0;

	public bool IsC => (callingConvention & CallingConvention.Mask) == CallingConvention.C;

	public bool IsStdCall => (callingConvention & CallingConvention.Mask) == CallingConvention.StdCall;

	public bool IsThisCall => (callingConvention & CallingConvention.Mask) == CallingConvention.ThisCall;

	public bool IsFastCall => (callingConvention & CallingConvention.Mask) == CallingConvention.FastCall;

	public bool IsVarArg => (callingConvention & CallingConvention.Mask) == CallingConvention.VarArg;

	public bool IsField => (callingConvention & CallingConvention.Mask) == CallingConvention.Field;

	public bool IsLocalSig => (callingConvention & CallingConvention.Mask) == CallingConvention.LocalSig;

	public bool IsProperty => (callingConvention & CallingConvention.Mask) == CallingConvention.Property;

	public bool IsUnmanaged => (callingConvention & CallingConvention.Mask) == CallingConvention.Unmanaged;

	public bool IsGenericInst => (callingConvention & CallingConvention.Mask) == CallingConvention.GenericInst;

	public bool IsNativeVarArg => (callingConvention & CallingConvention.Mask) == CallingConvention.NativeVarArg;

	public bool Generic
	{
		get
		{
			return (callingConvention & CallingConvention.Generic) != 0;
		}
		set
		{
			if (value)
			{
				callingConvention |= CallingConvention.Generic;
			}
			else
			{
				callingConvention &= ~CallingConvention.Generic;
			}
		}
	}

	public bool HasThis
	{
		get
		{
			return (callingConvention & CallingConvention.HasThis) != 0;
		}
		set
		{
			if (value)
			{
				callingConvention |= CallingConvention.HasThis;
			}
			else
			{
				callingConvention &= ~CallingConvention.HasThis;
			}
		}
	}

	public bool ExplicitThis
	{
		get
		{
			return (callingConvention & CallingConvention.ExplicitThis) != 0;
		}
		set
		{
			if (value)
			{
				callingConvention |= CallingConvention.ExplicitThis;
			}
			else
			{
				callingConvention &= ~CallingConvention.ExplicitThis;
			}
		}
	}

	public bool ReservedByCLR
	{
		get
		{
			return (callingConvention & CallingConvention.ReservedByCLR) != 0;
		}
		set
		{
			if (value)
			{
				callingConvention |= CallingConvention.ReservedByCLR;
			}
			else
			{
				callingConvention &= ~CallingConvention.ReservedByCLR;
			}
		}
	}

	public bool ImplicitThis
	{
		get
		{
			if (HasThis)
			{
				return !ExplicitThis;
			}
			return false;
		}
	}

	public bool ContainsGenericParameter => TypeHelper.ContainsGenericParameter(this);

	protected CallingConventionSig()
	{
	}

	protected CallingConventionSig(CallingConvention callingConvention)
	{
		this.callingConvention = callingConvention;
	}

	public CallingConvention GetCallingConvention()
	{
		return callingConvention;
	}
}
