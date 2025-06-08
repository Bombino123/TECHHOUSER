using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Helper
{
	// Token: 0x02000002 RID: 2
	public static class InjectHelper
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		private static TypeDefUser Clone(TypeDef origin)
		{
			TypeDefUser typeDefUser = new TypeDefUser(origin.Namespace, origin.Name)
			{
				Attributes = origin.Attributes
			};
			if (origin.ClassLayout != null)
			{
				typeDefUser.ClassLayout = new ClassLayoutUser(origin.ClassLayout.PackingSize, origin.ClassSize);
			}
			foreach (GenericParam genericParam in origin.GenericParameters)
			{
				typeDefUser.GenericParameters.Add(new GenericParamUser(genericParam.Number, genericParam.Flags, "-"));
			}
			return typeDefUser;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002100 File Offset: 0x00000300
		private static MethodDefUser Clone(MethodDef origin)
		{
			MethodDefUser methodDefUser = new MethodDefUser(origin.Name, null, origin.ImplAttributes, origin.Attributes);
			foreach (GenericParam genericParam in origin.GenericParameters)
			{
				methodDefUser.GenericParameters.Add(new GenericParamUser(genericParam.Number, genericParam.Flags, "-"));
			}
			return methodDefUser;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002188 File Offset: 0x00000388
		private static FieldDefUser Clone(FieldDef origin)
		{
			return new FieldDefUser(origin.Name, null, origin.Attributes);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000219C File Offset: 0x0000039C
		private static TypeDef PopulateContext(TypeDef typeDef, InjectHelper.InjectContext ctx)
		{
			IDnlibDef dnlibDef;
			TypeDef typeDef2;
			if (!ctx.Mep.TryGetValue(typeDef, out dnlibDef))
			{
				typeDef2 = InjectHelper.Clone(typeDef);
				ctx.Mep[typeDef] = typeDef2;
			}
			else
			{
				typeDef2 = (TypeDef)dnlibDef;
			}
			foreach (TypeDef typeDef3 in typeDef.NestedTypes)
			{
				typeDef2.NestedTypes.Add(InjectHelper.PopulateContext(typeDef3, ctx));
			}
			foreach (MethodDef methodDef in typeDef.Methods)
			{
				typeDef2.Methods.Add((MethodDef)(ctx.Mep[methodDef] = InjectHelper.Clone(methodDef)));
			}
			foreach (FieldDef fieldDef in typeDef.Fields)
			{
				typeDef2.Fields.Add((FieldDef)(ctx.Mep[fieldDef] = InjectHelper.Clone(fieldDef)));
			}
			return typeDef2;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000022EC File Offset: 0x000004EC
		private static void CopyTypeDef(TypeDef typeDef, InjectHelper.InjectContext ctx)
		{
			TypeDef typeDef2 = (TypeDef)ctx.Mep[typeDef];
			typeDef2.BaseType = ctx.Importer.Import(typeDef.BaseType);
			foreach (InterfaceImpl interfaceImpl in typeDef.Interfaces)
			{
				typeDef2.Interfaces.Add(new InterfaceImplUser(ctx.Importer.Import(interfaceImpl.Interface)));
			}
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002384 File Offset: 0x00000584
		private static void CopyMethodDef(MethodDef methodDef, InjectHelper.InjectContext ctx)
		{
			MethodDef methodDef2 = (MethodDef)ctx.Mep[methodDef];
			methodDef2.Signature = ctx.Importer.Import(methodDef.Signature);
			methodDef2.Parameters.UpdateParameterTypes();
			if (methodDef.ImplMap != null)
			{
				methodDef2.ImplMap = new ImplMapUser(new ModuleRefUser(ctx.TargetModule, methodDef.ImplMap.Module.Name), methodDef.ImplMap.Name, methodDef.ImplMap.Attributes);
			}
			foreach (CustomAttribute customAttribute in methodDef.CustomAttributes)
			{
				methodDef2.CustomAttributes.Add(new CustomAttribute((ICustomAttributeType)ctx.Importer.Import(customAttribute.Constructor)));
			}
			if (!methodDef.HasBody)
			{
				return;
			}
			methodDef2.Body = new CilBody(methodDef.Body.InitLocals, new List<Instruction>(), new List<ExceptionHandler>(), new List<Local>())
			{
				MaxStack = methodDef.Body.MaxStack
			};
			Dictionary<object, object> bodyMap = new Dictionary<object, object>();
			foreach (Local local in methodDef.Body.Variables)
			{
				Local local2 = new Local(ctx.Importer.Import(local.Type));
				methodDef2.Body.Variables.Add(local2);
				local2.Name = local.Name;
				local2.Attributes = local.Attributes;
				bodyMap[local] = local2;
			}
			foreach (Instruction instruction in methodDef.Body.Instructions)
			{
				Instruction instruction2 = new Instruction(instruction.OpCode, instruction.Operand)
				{
					SequencePoint = instruction.SequencePoint
				};
				object operand = instruction2.Operand;
				IType type = operand as IType;
				if (type == null)
				{
					IMethod method = operand as IMethod;
					if (method == null)
					{
						IField field = operand as IField;
						if (field != null)
						{
							instruction2.Operand = ctx.Importer.Import(field);
						}
					}
					else
					{
						instruction2.Operand = ctx.Importer.Import(method);
					}
				}
				else
				{
					instruction2.Operand = ctx.Importer.Import(type);
				}
				methodDef2.Body.Instructions.Add(instruction2);
				bodyMap[instruction] = instruction2;
			}
			foreach (Instruction instruction3 in methodDef2.Body.Instructions)
			{
				if (instruction3.Operand != null && bodyMap.ContainsKey(instruction3.Operand))
				{
					instruction3.Operand = bodyMap[instruction3.Operand];
				}
				else
				{
					Instruction[] array = instruction3.Operand as Instruction[];
					if (array != null)
					{
						Instruction instruction4 = instruction3;
						Instruction[] result = new Instruction[array.Length];
						for (int i = 0; i < array.Length; i++)
						{
							result[i] = (Instruction)bodyMap[array[i]];
						}
						instruction4.Operand = result;
					}
				}
			}
			foreach (ExceptionHandler exceptionHandler in methodDef.Body.ExceptionHandlers)
			{
				methodDef2.Body.ExceptionHandlers.Add(new ExceptionHandler(exceptionHandler.HandlerType)
				{
					CatchType = ((exceptionHandler.CatchType == null) ? null : ctx.Importer.Import(exceptionHandler.CatchType)),
					TryStart = (Instruction)bodyMap[exceptionHandler.TryStart],
					TryEnd = (Instruction)bodyMap[exceptionHandler.TryEnd],
					HandlerStart = (Instruction)bodyMap[exceptionHandler.HandlerStart],
					HandlerEnd = (Instruction)bodyMap[exceptionHandler.HandlerEnd],
					FilterStart = ((exceptionHandler.FilterStart == null) ? null : ((Instruction)bodyMap[exceptionHandler.FilterStart]))
				});
			}
			methodDef2.Body.SimplifyMacros(methodDef2.Parameters);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002864 File Offset: 0x00000A64
		private static void CopyFieldDef(FieldDef fieldDef, InjectHelper.InjectContext ctx)
		{
			((FieldDef)ctx.Mep[fieldDef]).Signature = ctx.Importer.Import(fieldDef.Signature);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000289C File Offset: 0x00000A9C
		private static void Copy(TypeDef typeDef, InjectHelper.InjectContext ctx, bool copySelf)
		{
			if (copySelf)
			{
				InjectHelper.CopyTypeDef(typeDef, ctx);
			}
			foreach (TypeDef typeDef2 in typeDef.NestedTypes)
			{
				InjectHelper.Copy(typeDef2, ctx, true);
			}
			foreach (MethodDef methodDef in typeDef.Methods)
			{
				InjectHelper.CopyMethodDef(methodDef, ctx);
			}
			foreach (FieldDef fieldDef in typeDef.Fields)
			{
				InjectHelper.CopyFieldDef(fieldDef, ctx);
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002968 File Offset: 0x00000B68
		public static TypeDef Inject(TypeDef typeDef, ModuleDef target)
		{
			InjectHelper.InjectContext injectContext = new InjectHelper.InjectContext(target);
			InjectHelper.PopulateContext(typeDef, injectContext);
			InjectHelper.Copy(typeDef, injectContext, true);
			return (TypeDef)injectContext.Mep[typeDef];
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000029A0 File Offset: 0x00000BA0
		public static MethodDef Inject(MethodDef methodDef, ModuleDef target)
		{
			InjectHelper.InjectContext injectContext = new InjectHelper.InjectContext(target);
			injectContext.Mep[methodDef] = InjectHelper.Clone(methodDef);
			InjectHelper.InjectContext injectContext2 = injectContext;
			InjectHelper.CopyMethodDef(methodDef, injectContext2);
			return (MethodDef)injectContext2.Mep[methodDef];
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000029E0 File Offset: 0x00000BE0
		public static IEnumerable<IDnlibDef> Inject(TypeDef typeDef, TypeDef newType, ModuleDef target)
		{
			InjectHelper.InjectContext injectContext = new InjectHelper.InjectContext(target);
			injectContext.Mep[typeDef] = newType;
			InjectHelper.InjectContext injectContext2 = injectContext;
			InjectHelper.PopulateContext(typeDef, injectContext2);
			InjectHelper.Copy(typeDef, injectContext2, false);
			return injectContext2.Mep.Values.Except(new TypeDef[]
			{
				newType
			});
		}

		// Token: 0x20000D0 RID: 208
		private class InjectContext : ImportMapper
		{
			// Token: 0x0600069A RID: 1690 RVA: 0x0005C7E8 File Offset: 0x0005A9E8
			public InjectContext(ModuleDef target)
			{
				this.TargetModule = target;
				this.Importer = new Importer(target, ImporterOptions.TryToUseTypeDefs, default(GenericParamContext), this);
			}

			// Token: 0x17000077 RID: 119
			// (get) Token: 0x0600069B RID: 1691 RVA: 0x0005C824 File Offset: 0x0005AA24
			public Importer Importer { get; }

			// Token: 0x0600069C RID: 1692 RVA: 0x0005C82C File Offset: 0x0005AA2C
			public override ITypeDefOrRef Map(ITypeDefOrRef typeDefOrRef)
			{
				TypeDef typeDef = typeDefOrRef as TypeDef;
				if (typeDef == null || !this.Mep.ContainsKey(typeDef))
				{
					return null;
				}
				return (TypeDef)this.Mep[typeDef];
			}

			// Token: 0x0600069D RID: 1693 RVA: 0x0005C864 File Offset: 0x0005AA64
			public override IMethod Map(MethodDef methodDef)
			{
				if (!this.Mep.ContainsKey(methodDef))
				{
					return null;
				}
				return (MethodDef)this.Mep[methodDef];
			}

			// Token: 0x0600069E RID: 1694 RVA: 0x0005C887 File Offset: 0x0005AA87
			public override IField Map(FieldDef fieldDef)
			{
				if (!this.Mep.ContainsKey(fieldDef))
				{
					return null;
				}
				return (FieldDef)this.Mep[fieldDef];
			}

			// Token: 0x04000598 RID: 1432
			public readonly Dictionary<IDnlibDef, IDnlibDef> Mep = new Dictionary<IDnlibDef, IDnlibDef>();

			// Token: 0x04000599 RID: 1433
			public readonly ModuleDef TargetModule;
		}
	}
}
