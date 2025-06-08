using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.ImportProtect
{
	// Token: 0x02000027 RID: 39
	internal class ImportProtect
	{
		// Token: 0x060000A1 RID: 161 RVA: 0x00008358 File Offset: 0x00006558
		public static FieldDefUser CreateField(FieldSig sig)
		{
			return new FieldDefUser(Methods.GenerateString(), sig, FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.Static);
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x0000836C File Offset: 0x0000656C
		public static void Execute(ModuleDef module)
		{
			Dictionary<IMethod, MethodDef> dictionary = new Dictionary<IMethod, MethodDef>();
			Dictionary<IMethod, TypeDef> dictionary2 = new Dictionary<IMethod, TypeDef>();
			FieldDefUser fieldDefUser = ImportProtect.CreateField(new FieldSig(module.ImportAsTypeSig(typeof(object[]))));
			MethodDef methodDef = module.GlobalType.FindOrCreateStaticConstructor();
			foreach (TypeDef typeDef in module.GetTypes().ToArray<TypeDef>())
			{
				if (!typeDef.IsDelegate && !typeDef.IsGlobalModuleType && !(typeDef.Namespace == "Costura"))
				{
					foreach (MethodDef methodDef2 in typeDef.Methods.ToArray<MethodDef>())
					{
						if (methodDef2.HasBody && methodDef2.Body.HasInstructions && !methodDef2.IsConstructor)
						{
							IList<Instruction> instructions = methodDef2.Body.Instructions;
							for (int k = 0; k < instructions.Count; k++)
							{
								if (instructions[k].OpCode == OpCodes.Call || instructions[k].OpCode != OpCodes.Callvirt)
								{
									IMethod method = instructions[k].Operand as IMethod;
									if (method != null && method.IsMethodDef)
									{
										MethodDef methodDef3 = method.ResolveMethodDef();
										if (methodDef3 != null && !methodDef3.HasThis)
										{
											if (dictionary.ContainsKey(method))
											{
												instructions[k].OpCode = OpCodes.Call;
												instructions[k].Operand = dictionary[method];
											}
											else
											{
												MethodSig methodSig = ImportProtect.CreateProxySignature(module, methodDef3);
												TypeDef typeDef2 = ImportProtect.CreateDelegateType(module, methodSig);
												module.Types.Add(typeDef2);
												MethodImplAttributes implFlags = MethodImplAttributes.IL;
												MethodAttributes flags = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig;
												MethodDefUser methodDefUser = new MethodDefUser(Methods.GenerateString(), methodSig, implFlags, flags);
												methodDefUser.Body = new CilBody();
												methodDefUser.Body.Instructions.Add(OpCodes.Ldsfld.ToInstruction(fieldDefUser));
												methodDefUser.Body.Instructions.Add(OpCodes.Ldc_I4.ToInstruction(dictionary2.Count));
												methodDefUser.Body.Instructions.Add(OpCodes.Ldelem_Ref.ToInstruction());
												foreach (Parameter parameter in methodDefUser.Parameters)
												{
													parameter.Name = Methods.GenerateString();
													methodDefUser.Body.Instructions.Add(OpCodes.Ldarg.ToInstruction(parameter));
												}
												methodDefUser.Body.Instructions.Add(OpCodes.Call.ToInstruction(typeDef2.Methods[1]));
												methodDefUser.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
												typeDef2.Methods.Add(methodDefUser);
												instructions[k].OpCode = OpCodes.Call;
												instructions[k].Operand = methodDefUser;
												if (method.IsMethodDef)
												{
													dictionary2.Add(methodDef3, typeDef2);
												}
												else if (method.IsMemberRef)
												{
													dictionary2.Add(method as MemberRef, typeDef2);
												}
												dictionary.Add(method, methodDefUser);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			module.GlobalType.Fields.Add(fieldDefUser);
			List<Instruction> list = new List<Instruction>();
			List<Instruction> list2 = methodDef.Body.Instructions.ToList<Instruction>();
			methodDef.Body.Instructions.Clear();
			list.Add(OpCodes.Ldc_I4.ToInstruction(dictionary2.Count));
			list.Add(OpCodes.Newarr.ToInstruction(module.CorLibTypes.Object));
			list.Add(OpCodes.Dup.ToInstruction());
			int num = 0;
			foreach (KeyValuePair<IMethod, TypeDef> keyValuePair in dictionary2)
			{
				list.Add(OpCodes.Ldc_I4.ToInstruction(num));
				list.Add(OpCodes.Ldnull.ToInstruction());
				list.Add(OpCodes.Ldftn.ToInstruction(keyValuePair.Key));
				list.Add(OpCodes.Newobj.ToInstruction(keyValuePair.Value.Methods[0]));
				list.Add(OpCodes.Stelem_Ref.ToInstruction());
				list.Add(OpCodes.Dup.ToInstruction());
				num++;
			}
			list.Add(OpCodes.Pop.ToInstruction());
			list.Add(OpCodes.Stsfld.ToInstruction(fieldDefUser));
			foreach (Instruction item in list)
			{
				methodDef.Body.Instructions.Add(item);
			}
			foreach (Instruction item2 in list2)
			{
				methodDef.Body.Instructions.Add(item2);
			}
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x000088EC File Offset: 0x00006AEC
		public static TypeDef CreateDelegateType(ModuleDef module, MethodSig sig)
		{
			TypeDefUser typeDefUser = new TypeDefUser(Methods.GenerateString(), module.CorLibTypes.GetTypeRef("System", "MulticastDelegate"));
			typeDefUser.Attributes = (TypeAttributes.Public | TypeAttributes.Sealed);
			MethodDefUser methodDefUser = new MethodDefUser(".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void, module.CorLibTypes.Object, module.CorLibTypes.IntPtr));
			methodDefUser.Attributes = (MethodAttributes.Private | MethodAttributes.FamANDAssem | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
			methodDefUser.ImplAttributes = MethodImplAttributes.CodeTypeMask;
			typeDefUser.Methods.Add(methodDefUser);
			MethodDefUser methodDefUser2 = new MethodDefUser("Invoke", sig.Clone());
			methodDefUser2.MethodSig.HasThis = true;
			methodDefUser2.Attributes = (MethodAttributes.Private | MethodAttributes.FamANDAssem | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask);
			methodDefUser2.ImplAttributes = MethodImplAttributes.CodeTypeMask;
			typeDefUser.Methods.Add(methodDefUser2);
			return typeDefUser;
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x000089BC File Offset: 0x00006BBC
		public static MethodSig CreateProxySignature(ModuleDef module, IMethod method)
		{
			IEnumerable<TypeSig> enumerable = method.MethodSig.Params.Select(delegate(TypeSig type)
			{
				if (type.IsClassSig && method.MethodSig.HasThis)
				{
					return module.CorLibTypes.Object;
				}
				return type;
			});
			if (method.MethodSig.HasThis && !method.MethodSig.ExplicitThis)
			{
				TypeDef typeDef = method.DeclaringType.ResolveTypeDefThrow();
				if (!typeDef.IsValueType)
				{
					enumerable = new CorLibTypeSig[]
					{
						module.CorLibTypes.Object
					}.Concat(enumerable);
				}
				else
				{
					enumerable = new TypeSig[]
					{
						typeDef.ToTypeSig(true)
					}.Concat(enumerable);
				}
			}
			TypeSig typeSig = method.MethodSig.RetType;
			if (typeSig.IsClassSig)
			{
				typeSig = module.CorLibTypes.Object;
			}
			return MethodSig.CreateStatic(typeSig, enumerable.ToArray<TypeSig>());
		}
	}
}
