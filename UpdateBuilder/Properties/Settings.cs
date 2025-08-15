using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UpdateBuilder.Properties
{
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.5.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default => defaultInstance;

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("")]
		public string PatchPath
		{
			get
			{
				return (string)this["PatchPath"];
			}
			set
			{
				this["PatchPath"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("")]
		public string OutPath
		{
			get
			{
				return (string)this["OutPath"];
			}
			set
			{
				this["OutPath"] = value;
			}
		}
	}
}
