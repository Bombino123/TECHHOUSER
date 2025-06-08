using System;
using System.Collections.Generic;
using dnlib.DotNet;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Rename
{
	// Token: 0x02000006 RID: 6
	internal class Renamer
	{
		// Token: 0x06000017 RID: 23 RVA: 0x00002DC8 File Offset: 0x00000FC8
		public static void Execute(ModuleDefMD module)
		{
			Renamer.RenameTypes = 0;
			Renamer.RenameFields = 0;
			Renamer.RenameMethods = 0;
			Renamer.RenameNameSpaces = 1;
			string s = Methods.GenerateString();
			foreach (TypeDef typeDef in module.Types)
			{
				if (Methods.GenerateBool(4))
				{
					s = Methods.GenerateString();
					Renamer.RenameNameSpaces++;
				}
				bool flag;
				if (Renamer.TypeRename.TryGetValue(typeDef, out flag))
				{
					if (flag)
					{
						Renamer.InternalRename(typeDef);
					}
				}
				else
				{
					Renamer.InternalRename(typeDef);
				}
				typeDef.Namespace = s;
				foreach (TypeDef typeDef2 in typeDef.NestedTypes)
				{
					bool flag2;
					if (Renamer.TypeRename.TryGetValue(typeDef2, out flag2))
					{
						if (flag2)
						{
							Renamer.InternalRename(typeDef2);
						}
					}
					else
					{
						Renamer.InternalRename(typeDef2);
					}
					foreach (MethodDef methodDef in typeDef2.Methods)
					{
						foreach (ParamDef paramDef in methodDef.ParamDefs)
						{
							paramDef.Name = Methods.GenerateString(16);
						}
					}
				}
				foreach (MethodDef methodDef2 in typeDef.Methods)
				{
					bool flag3;
					if (Renamer.MethodRename.TryGetValue(methodDef2, out flag3))
					{
						if (flag3 && !methodDef2.IsConstructor && !methodDef2.IsSpecialName)
						{
							Renamer.InternalRename(methodDef2);
						}
					}
					else if (!methodDef2.IsConstructor && !methodDef2.IsSpecialName)
					{
						Renamer.InternalRename(methodDef2);
					}
				}
				Renamer.MethodNewName.Clear();
				foreach (FieldDef fieldDef in typeDef.Fields)
				{
					bool flag4;
					if (Renamer.FieldRename.TryGetValue(fieldDef, out flag4))
					{
						if (flag4)
						{
							Renamer.InternalRename(fieldDef);
						}
					}
					else
					{
						Renamer.InternalRename(fieldDef);
					}
				}
				Renamer.FieldNewName.Clear();
			}
			Renamer.TypeRename.Clear();
			Renamer.MethodRename.Clear();
			Renamer.FieldRename.Clear();
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000030BC File Offset: 0x000012BC
		public static string Info()
		{
			return string.Format("Rename Obfuscator [Types: [{0}]  Namespaces: [{1}]  Methods: [{2}]  Fields: [{3}]]", new object[]
			{
				Renamer.RenameTypes,
				Renamer.RenameNameSpaces,
				Renamer.RenameMethods,
				Renamer.RenameFields
			});
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00003110 File Offset: 0x00001310
		private static void InternalRename(TypeDef type)
		{
			string text = Methods.GenerateString();
			while (Renamer.TypeNewName.Contains(text))
			{
				text = Methods.GenerateString();
			}
			Renamer.TypeNewName.Add(text);
			type.Name = text;
			Renamer.RenameTypes++;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x0000315C File Offset: 0x0000135C
		private static void InternalRename(MethodDef method)
		{
			string text = Methods.GenerateString();
			while (Renamer.MethodNewName.Contains(text))
			{
				text = Methods.GenerateString();
			}
			Renamer.MethodNewName.Add(text);
			method.Name = text;
			Renamer.RenameMethods++;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000031A8 File Offset: 0x000013A8
		private static void InternalRename(FieldDef field)
		{
			string text = Methods.GenerateString();
			while (Renamer.FieldNewName.Contains(text))
			{
				text = Methods.GenerateString();
			}
			Renamer.FieldNewName.Add(text);
			field.Name = text;
			Renamer.RenameFields++;
		}

		// Token: 0x04000004 RID: 4
		private static readonly Dictionary<TypeDef, bool> TypeRename = new Dictionary<TypeDef, bool>();

		// Token: 0x04000005 RID: 5
		private static readonly List<string> TypeNewName = new List<string>();

		// Token: 0x04000006 RID: 6
		private static readonly Dictionary<MethodDef, bool> MethodRename = new Dictionary<MethodDef, bool>();

		// Token: 0x04000007 RID: 7
		private static readonly List<string> MethodNewName = new List<string>();

		// Token: 0x04000008 RID: 8
		private static readonly Dictionary<FieldDef, bool> FieldRename = new Dictionary<FieldDef, bool>();

		// Token: 0x04000009 RID: 9
		private static readonly List<string> FieldNewName = new List<string>();

		// Token: 0x0400000A RID: 10
		private static int RenameTypes;

		// Token: 0x0400000B RID: 11
		private static int RenameFields;

		// Token: 0x0400000C RID: 12
		private static int RenameMethods;

		// Token: 0x0400000D RID: 13
		private static int RenameNameSpaces;
	}
}
