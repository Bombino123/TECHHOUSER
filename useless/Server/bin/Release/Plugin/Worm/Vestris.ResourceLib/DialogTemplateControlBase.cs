using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public abstract class DialogTemplateControlBase
{
	private ResourceId _captionId;

	private ResourceId _controlClassId;

	private byte[] _creationData;

	public abstract short x { get; set; }

	public abstract short y { get; set; }

	public abstract short cx { get; set; }

	public abstract short cy { get; set; }

	public abstract uint Style { get; set; }

	public abstract uint ExtendedStyle { get; set; }

	public ResourceId CaptionId
	{
		get
		{
			return _captionId;
		}
		set
		{
			_captionId = value;
		}
	}

	public ResourceId ControlClassId
	{
		get
		{
			return _controlClassId;
		}
		set
		{
			_controlClassId = value;
		}
	}

	public User32.DialogItemClass ControlClass => (User32.DialogItemClass)(int)ControlClassId.Id;

	public byte[] CreationData
	{
		get
		{
			return _creationData;
		}
		set
		{
			_creationData = value;
		}
	}

	internal virtual IntPtr Read(IntPtr lpRes)
	{
		lpRes = DialogTemplateUtil.ReadResourceId(lpRes, out _controlClassId);
		lpRes = DialogTemplateUtil.ReadResourceId(lpRes, out _captionId);
		if ((ushort)Marshal.ReadInt16(lpRes) == 0)
		{
			lpRes = new IntPtr(lpRes.ToInt64() + 2);
		}
		else
		{
			ushort num = (ushort)Marshal.ReadInt16(lpRes);
			_creationData = new byte[num];
			Marshal.Copy(lpRes, _creationData, 0, _creationData.Length);
			lpRes = new IntPtr(lpRes.ToInt64() + num);
		}
		return lpRes;
	}

	public virtual void Write(BinaryWriter w)
	{
		DialogTemplateUtil.WriteResourceId(w, _controlClassId);
		DialogTemplateUtil.WriteResourceId(w, _captionId);
		if (_creationData == null)
		{
			w.Write((ushort)0);
			return;
		}
		ResourceUtil.PadToWORD(w);
		w.Write((ushort)_creationData.Length);
		w.Write(_creationData);
	}
}
