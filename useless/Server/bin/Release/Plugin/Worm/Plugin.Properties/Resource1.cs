using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Plugin.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resource1
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Plugin.Properties.Resource1", typeof(Resource1).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static byte[] Dropper => (byte[])ResourceManager.GetObject("Dropper", resourceCulture);

	internal static byte[] excel => (byte[])ResourceManager.GetObject("excel", resourceCulture);

	internal static byte[] photo => (byte[])ResourceManager.GetObject("photo", resourceCulture);

	internal static byte[] powerpoint => (byte[])ResourceManager.GetObject("powerpoint", resourceCulture);

	internal static byte[] txt => (byte[])ResourceManager.GetObject("txt", resourceCulture);

	internal static byte[] video => (byte[])ResourceManager.GetObject("video", resourceCulture);

	internal static byte[] word => (byte[])ResourceManager.GetObject("word", resourceCulture);

	internal Resource1()
	{
	}
}
