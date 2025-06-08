using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public class InfoQueueFilterDescription
{
	internal struct __Native
	{
		public int CategorieCount;

		public IntPtr PCategoryList;

		public int SeveritieCount;

		public IntPtr PSeverityList;

		public int IDCount;

		public IntPtr PIDList;

		internal void __MarshalFree()
		{
			if (PCategoryList != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(PCategoryList);
			}
			if (PSeverityList != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(PSeverityList);
			}
			if (PIDList != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(PIDList);
			}
		}
	}

	internal int CategorieCount;

	internal IntPtr PCategoryList;

	internal int SeveritieCount;

	internal IntPtr PSeverityList;

	internal int IDCount;

	internal IntPtr PIDList;

	public MessageCategory[] Categories { get; set; }

	public MessageSeverity[] Severities { get; set; }

	public MessageId[] Ids { get; set; }

	internal void __MarshalFree(ref __Native @ref)
	{
		@ref.__MarshalFree();
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Categories = new MessageCategory[@ref.CategorieCount];
		if (@ref.CategorieCount > 0)
		{
			Utilities.Read(@ref.PCategoryList, Categories, 0, @ref.CategorieCount);
		}
		Severities = new MessageSeverity[@ref.SeveritieCount];
		if (@ref.SeveritieCount > 0)
		{
			Utilities.Read(@ref.PSeverityList, Severities, 0, @ref.SeveritieCount);
		}
		Ids = new MessageId[@ref.IDCount];
		if (@ref.IDCount > 0)
		{
			Utilities.Read(@ref.PIDList, Ids, 0, @ref.IDCount);
		}
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.CategorieCount = ((Categories != null) ? Categories.Length : 0);
		if (@ref.CategorieCount > 0)
		{
			@ref.PCategoryList = Marshal.AllocHGlobal(4 * @ref.CategorieCount);
			Utilities.Write(@ref.PCategoryList, Categories, 0, @ref.CategorieCount);
		}
		@ref.SeveritieCount = ((Severities != null) ? Severities.Length : 0);
		if (@ref.SeveritieCount > 0)
		{
			@ref.PSeverityList = Marshal.AllocHGlobal(4 * @ref.SeveritieCount);
			Utilities.Write(@ref.PSeverityList, Severities, 0, @ref.SeveritieCount);
		}
		@ref.IDCount = ((Ids != null) ? Ids.Length : 0);
		if (@ref.IDCount > 0)
		{
			@ref.PIDList = Marshal.AllocHGlobal(4 * @ref.IDCount);
			Utilities.Write(@ref.PIDList, Ids, 0, @ref.IDCount);
		}
	}
}
