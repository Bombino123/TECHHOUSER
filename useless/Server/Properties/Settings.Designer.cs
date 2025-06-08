using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace Server.Properties
{
	// Token: 0x0200003C RID: 60
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.9.0.0")]
	internal sealed partial class Settings : ApplicationSettingsBase
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x060001A8 RID: 424 RVA: 0x0001B5D2 File Offset: 0x000197D2
		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}

		// Token: 0x0400014E RID: 334
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
	}
}
