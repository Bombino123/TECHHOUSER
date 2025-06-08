using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Server.Properties
{
	// Token: 0x0200003B RID: 59
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x060001A4 RID: 420 RVA: 0x0001B58F File Offset: 0x0001978F
		internal Resources()
		{
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x060001A5 RID: 421 RVA: 0x0001B597 File Offset: 0x00019797
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("Server.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x060001A6 RID: 422 RVA: 0x0001B5C3 File Offset: 0x000197C3
		// (set) Token: 0x060001A7 RID: 423 RVA: 0x0001B5CA File Offset: 0x000197CA
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x0400014C RID: 332
		private static ResourceManager resourceMan;

		// Token: 0x0400014D RID: 333
		private static CultureInfo resourceCulture;
	}
}
