using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using AForge.Video.DirectShow.Internals;

namespace AForge.Video.DirectShow;

public class FilterInfo : IComparable
{
	public string Name { get; private set; }

	public string MonikerString { get; private set; }

	public FilterInfo(string monikerString)
	{
		MonikerString = monikerString;
		Name = GetName(monikerString);
	}

	internal FilterInfo(IMoniker moniker)
	{
		MonikerString = GetMonikerString(moniker);
		Name = GetName(moniker);
	}

	public int CompareTo(object value)
	{
		FilterInfo filterInfo = (FilterInfo)value;
		if (filterInfo == null)
		{
			return 1;
		}
		return Name.CompareTo(filterInfo.Name);
	}

	public static object CreateFilter(string filterMoniker)
	{
		object ppvResult = null;
		IBindCtx ppbc = null;
		IMoniker ppmk = null;
		int pchEaten = 0;
		if (Win32.CreateBindCtx(0, out ppbc) == 0)
		{
			if (Win32.MkParseDisplayName(ppbc, filterMoniker, ref pchEaten, out ppmk) == 0)
			{
				Guid riidResult = typeof(IBaseFilter).GUID;
				ppmk.BindToObject(null, null, ref riidResult, out ppvResult);
				Marshal.ReleaseComObject(ppmk);
			}
			Marshal.ReleaseComObject(ppbc);
		}
		return ppvResult;
	}

	private string GetMonikerString(IMoniker moniker)
	{
		moniker.GetDisplayName(null, null, out string ppszDisplayName);
		return ppszDisplayName;
	}

	private string GetName(IMoniker moniker)
	{
		object ppvObj = null;
		IPropertyBag propertyBag = null;
		try
		{
			Guid riid = typeof(IPropertyBag).GUID;
			moniker.BindToStorage(null, null, ref riid, out ppvObj);
			propertyBag = (IPropertyBag)ppvObj;
			object pVar = "";
			int num = propertyBag.Read("FriendlyName", ref pVar, IntPtr.Zero);
			if (num != 0)
			{
				Marshal.ThrowExceptionForHR(num);
			}
			string text = (string)pVar;
			if (text == null || text.Length < 1)
			{
				throw new ApplicationException();
			}
			return text;
		}
		catch (Exception)
		{
			return "";
		}
		finally
		{
			propertyBag = null;
			if (ppvObj != null)
			{
				Marshal.ReleaseComObject(ppvObj);
				ppvObj = null;
			}
		}
	}

	private string GetName(string monikerString)
	{
		IBindCtx ppbc = null;
		IMoniker ppmk = null;
		string result = "";
		int pchEaten = 0;
		if (Win32.CreateBindCtx(0, out ppbc) == 0)
		{
			if (Win32.MkParseDisplayName(ppbc, monikerString, ref pchEaten, out ppmk) == 0)
			{
				result = GetName(ppmk);
				Marshal.ReleaseComObject(ppmk);
				ppmk = null;
			}
			Marshal.ReleaseComObject(ppbc);
			ppbc = null;
		}
		return result;
	}
}
