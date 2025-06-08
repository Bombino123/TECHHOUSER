using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public delegate void EventHandler2<TEventArgs>(object sender, TEventArgs e);
