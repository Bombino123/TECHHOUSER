using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace AntdUI;

public static class EventHub
{
	private static ConcurrentDictionary<Control, IEventListener> dic = new ConcurrentDictionary<Control, IEventListener>();

	public static void AddListener(this Control control)
	{
		Control control2 = control;
		if (control2 is IEventListener value && dic.TryAdd(control2, value))
		{
			((Component)(object)control2).Disposed += delegate
			{
				dic.TryRemove(control2, out IEventListener _);
			};
		}
	}

	public static void Dispatch(EventType id, object? tag = null)
	{
		foreach (KeyValuePair<Control, IEventListener> item in dic)
		{
			item.Value.HandleEvent(id, tag);
		}
	}
}
