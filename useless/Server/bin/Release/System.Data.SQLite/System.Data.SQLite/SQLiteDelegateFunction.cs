using System.Globalization;

namespace System.Data.SQLite;

public class SQLiteDelegateFunction : SQLiteFunction
{
	private const string NoCallbackError = "No \"{0}\" callback is set.";

	private const string ResultInt32Error = "\"{0}\" result must be Int32.";

	private Delegate callback1;

	private Delegate callback2;

	public virtual Delegate Callback1
	{
		get
		{
			return callback1;
		}
		set
		{
			callback1 = value;
		}
	}

	public virtual Delegate Callback2
	{
		get
		{
			return callback2;
		}
		set
		{
			callback2 = value;
		}
	}

	public SQLiteDelegateFunction()
		: this(null, null)
	{
	}

	public SQLiteDelegateFunction(Delegate callback1, Delegate callback2)
	{
		this.callback1 = callback1;
		this.callback2 = callback2;
	}

	protected virtual object[] GetInvokeArgs(object[] args, bool earlyBound)
	{
		object[] array = new object[2] { "Invoke", args };
		if (!earlyBound)
		{
			array = new object[1] { array };
		}
		return array;
	}

	protected virtual object[] GetStepArgs(object[] args, int stepNumber, object contextData, bool earlyBound)
	{
		object[] array = new object[4] { "Step", args, stepNumber, contextData };
		if (!earlyBound)
		{
			array = new object[1] { array };
		}
		return array;
	}

	protected virtual void UpdateStepArgs(object[] args, ref object contextData, bool earlyBound)
	{
		object[] array = ((!earlyBound) ? (args[0] as object[]) : args);
		if (array != null)
		{
			contextData = array[^1];
		}
	}

	protected virtual object[] GetFinalArgs(object contextData, bool earlyBound)
	{
		object[] array = new object[2] { "Final", contextData };
		if (!earlyBound)
		{
			array = new object[1] { array };
		}
		return array;
	}

	protected virtual object[] GetCompareArgs(string param1, string param2, bool earlyBound)
	{
		object[] array = new object[3] { "Compare", param1, param2 };
		if (!earlyBound)
		{
			array = new object[1] { array };
		}
		return array;
	}

	public override object Invoke(object[] args)
	{
		if ((object)callback1 == null)
		{
			throw new InvalidOperationException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "No \"{0}\" callback is set.", "Invoke"));
		}
		if (callback1 is SQLiteInvokeDelegate sQLiteInvokeDelegate)
		{
			return sQLiteInvokeDelegate("Invoke", args);
		}
		return callback1.DynamicInvoke(GetInvokeArgs(args, earlyBound: false));
	}

	public override void Step(object[] args, int stepNumber, ref object contextData)
	{
		if ((object)callback1 == null)
		{
			throw new InvalidOperationException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "No \"{0}\" callback is set.", "Step"));
		}
		if (callback1 is SQLiteStepDelegate sQLiteStepDelegate)
		{
			sQLiteStepDelegate("Step", args, stepNumber, ref contextData);
			return;
		}
		object[] stepArgs = GetStepArgs(args, stepNumber, contextData, earlyBound: false);
		callback1.DynamicInvoke(stepArgs);
		UpdateStepArgs(stepArgs, ref contextData, earlyBound: false);
	}

	public override object Final(object contextData)
	{
		if ((object)callback2 == null)
		{
			throw new InvalidOperationException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "No \"{0}\" callback is set.", "Final"));
		}
		if (callback2 is SQLiteFinalDelegate sQLiteFinalDelegate)
		{
			return sQLiteFinalDelegate("Final", contextData);
		}
		return callback1.DynamicInvoke(GetFinalArgs(contextData, earlyBound: false));
	}

	public override int Compare(string param1, string param2)
	{
		if ((object)callback1 == null)
		{
			throw new InvalidOperationException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "No \"{0}\" callback is set.", "Compare"));
		}
		if (callback1 is SQLiteCompareDelegate sQLiteCompareDelegate)
		{
			return sQLiteCompareDelegate("Compare", param1, param2);
		}
		object obj = callback1.DynamicInvoke(GetCompareArgs(param1, param2, earlyBound: false));
		if (obj is int)
		{
			return (int)obj;
		}
		throw new InvalidOperationException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "\"{0}\" result must be Int32.", "Compare"));
	}
}
