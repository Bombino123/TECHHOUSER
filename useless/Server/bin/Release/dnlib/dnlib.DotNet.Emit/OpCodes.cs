namespace dnlib.DotNet.Emit;

public static class OpCodes
{
	public static readonly OpCode[] OneByteOpCodes;

	public static readonly OpCode[] TwoByteOpCodes;

	public static readonly OpCode UNKNOWN1;

	public static readonly OpCode UNKNOWN2;

	public static readonly OpCode Nop;

	public static readonly OpCode Break;

	public static readonly OpCode Ldarg_0;

	public static readonly OpCode Ldarg_1;

	public static readonly OpCode Ldarg_2;

	public static readonly OpCode Ldarg_3;

	public static readonly OpCode Ldloc_0;

	public static readonly OpCode Ldloc_1;

	public static readonly OpCode Ldloc_2;

	public static readonly OpCode Ldloc_3;

	public static readonly OpCode Stloc_0;

	public static readonly OpCode Stloc_1;

	public static readonly OpCode Stloc_2;

	public static readonly OpCode Stloc_3;

	public static readonly OpCode Ldarg_S;

	public static readonly OpCode Ldarga_S;

	public static readonly OpCode Starg_S;

	public static readonly OpCode Ldloc_S;

	public static readonly OpCode Ldloca_S;

	public static readonly OpCode Stloc_S;

	public static readonly OpCode Ldnull;

	public static readonly OpCode Ldc_I4_M1;

	public static readonly OpCode Ldc_I4_0;

	public static readonly OpCode Ldc_I4_1;

	public static readonly OpCode Ldc_I4_2;

	public static readonly OpCode Ldc_I4_3;

	public static readonly OpCode Ldc_I4_4;

	public static readonly OpCode Ldc_I4_5;

	public static readonly OpCode Ldc_I4_6;

	public static readonly OpCode Ldc_I4_7;

	public static readonly OpCode Ldc_I4_8;

	public static readonly OpCode Ldc_I4_S;

	public static readonly OpCode Ldc_I4;

	public static readonly OpCode Ldc_I8;

	public static readonly OpCode Ldc_R4;

	public static readonly OpCode Ldc_R8;

	public static readonly OpCode Dup;

	public static readonly OpCode Pop;

	public static readonly OpCode Jmp;

	public static readonly OpCode Call;

	public static readonly OpCode Calli;

	public static readonly OpCode Ret;

	public static readonly OpCode Br_S;

	public static readonly OpCode Brfalse_S;

	public static readonly OpCode Brtrue_S;

	public static readonly OpCode Beq_S;

	public static readonly OpCode Bge_S;

	public static readonly OpCode Bgt_S;

	public static readonly OpCode Ble_S;

	public static readonly OpCode Blt_S;

	public static readonly OpCode Bne_Un_S;

	public static readonly OpCode Bge_Un_S;

	public static readonly OpCode Bgt_Un_S;

	public static readonly OpCode Ble_Un_S;

	public static readonly OpCode Blt_Un_S;

	public static readonly OpCode Br;

	public static readonly OpCode Brfalse;

	public static readonly OpCode Brtrue;

	public static readonly OpCode Beq;

	public static readonly OpCode Bge;

	public static readonly OpCode Bgt;

	public static readonly OpCode Ble;

	public static readonly OpCode Blt;

	public static readonly OpCode Bne_Un;

	public static readonly OpCode Bge_Un;

	public static readonly OpCode Bgt_Un;

	public static readonly OpCode Ble_Un;

	public static readonly OpCode Blt_Un;

	public static readonly OpCode Switch;

	public static readonly OpCode Ldind_I1;

	public static readonly OpCode Ldind_U1;

	public static readonly OpCode Ldind_I2;

	public static readonly OpCode Ldind_U2;

	public static readonly OpCode Ldind_I4;

	public static readonly OpCode Ldind_U4;

	public static readonly OpCode Ldind_I8;

	public static readonly OpCode Ldind_I;

	public static readonly OpCode Ldind_R4;

	public static readonly OpCode Ldind_R8;

	public static readonly OpCode Ldind_Ref;

	public static readonly OpCode Stind_Ref;

	public static readonly OpCode Stind_I1;

	public static readonly OpCode Stind_I2;

	public static readonly OpCode Stind_I4;

	public static readonly OpCode Stind_I8;

	public static readonly OpCode Stind_R4;

	public static readonly OpCode Stind_R8;

	public static readonly OpCode Add;

	public static readonly OpCode Sub;

	public static readonly OpCode Mul;

	public static readonly OpCode Div;

	public static readonly OpCode Div_Un;

	public static readonly OpCode Rem;

	public static readonly OpCode Rem_Un;

	public static readonly OpCode And;

	public static readonly OpCode Or;

	public static readonly OpCode Xor;

	public static readonly OpCode Shl;

	public static readonly OpCode Shr;

	public static readonly OpCode Shr_Un;

	public static readonly OpCode Neg;

	public static readonly OpCode Not;

	public static readonly OpCode Conv_I1;

	public static readonly OpCode Conv_I2;

	public static readonly OpCode Conv_I4;

	public static readonly OpCode Conv_I8;

	public static readonly OpCode Conv_R4;

	public static readonly OpCode Conv_R8;

	public static readonly OpCode Conv_U4;

	public static readonly OpCode Conv_U8;

	public static readonly OpCode Callvirt;

	public static readonly OpCode Cpobj;

	public static readonly OpCode Ldobj;

	public static readonly OpCode Ldstr;

	public static readonly OpCode Newobj;

	public static readonly OpCode Castclass;

	public static readonly OpCode Isinst;

	public static readonly OpCode Conv_R_Un;

	public static readonly OpCode Unbox;

	public static readonly OpCode Throw;

	public static readonly OpCode Ldfld;

	public static readonly OpCode Ldflda;

	public static readonly OpCode Stfld;

	public static readonly OpCode Ldsfld;

	public static readonly OpCode Ldsflda;

	public static readonly OpCode Stsfld;

	public static readonly OpCode Stobj;

	public static readonly OpCode Conv_Ovf_I1_Un;

	public static readonly OpCode Conv_Ovf_I2_Un;

	public static readonly OpCode Conv_Ovf_I4_Un;

	public static readonly OpCode Conv_Ovf_I8_Un;

	public static readonly OpCode Conv_Ovf_U1_Un;

	public static readonly OpCode Conv_Ovf_U2_Un;

	public static readonly OpCode Conv_Ovf_U4_Un;

	public static readonly OpCode Conv_Ovf_U8_Un;

	public static readonly OpCode Conv_Ovf_I_Un;

	public static readonly OpCode Conv_Ovf_U_Un;

	public static readonly OpCode Box;

	public static readonly OpCode Newarr;

	public static readonly OpCode Ldlen;

	public static readonly OpCode Ldelema;

	public static readonly OpCode Ldelem_I1;

	public static readonly OpCode Ldelem_U1;

	public static readonly OpCode Ldelem_I2;

	public static readonly OpCode Ldelem_U2;

	public static readonly OpCode Ldelem_I4;

	public static readonly OpCode Ldelem_U4;

	public static readonly OpCode Ldelem_I8;

	public static readonly OpCode Ldelem_I;

	public static readonly OpCode Ldelem_R4;

	public static readonly OpCode Ldelem_R8;

	public static readonly OpCode Ldelem_Ref;

	public static readonly OpCode Stelem_I;

	public static readonly OpCode Stelem_I1;

	public static readonly OpCode Stelem_I2;

	public static readonly OpCode Stelem_I4;

	public static readonly OpCode Stelem_I8;

	public static readonly OpCode Stelem_R4;

	public static readonly OpCode Stelem_R8;

	public static readonly OpCode Stelem_Ref;

	public static readonly OpCode Ldelem;

	public static readonly OpCode Stelem;

	public static readonly OpCode Unbox_Any;

	public static readonly OpCode Conv_Ovf_I1;

	public static readonly OpCode Conv_Ovf_U1;

	public static readonly OpCode Conv_Ovf_I2;

	public static readonly OpCode Conv_Ovf_U2;

	public static readonly OpCode Conv_Ovf_I4;

	public static readonly OpCode Conv_Ovf_U4;

	public static readonly OpCode Conv_Ovf_I8;

	public static readonly OpCode Conv_Ovf_U8;

	public static readonly OpCode Refanyval;

	public static readonly OpCode Ckfinite;

	public static readonly OpCode Mkrefany;

	public static readonly OpCode Ldtoken;

	public static readonly OpCode Conv_U2;

	public static readonly OpCode Conv_U1;

	public static readonly OpCode Conv_I;

	public static readonly OpCode Conv_Ovf_I;

	public static readonly OpCode Conv_Ovf_U;

	public static readonly OpCode Add_Ovf;

	public static readonly OpCode Add_Ovf_Un;

	public static readonly OpCode Mul_Ovf;

	public static readonly OpCode Mul_Ovf_Un;

	public static readonly OpCode Sub_Ovf;

	public static readonly OpCode Sub_Ovf_Un;

	public static readonly OpCode Endfinally;

	public static readonly OpCode Leave;

	public static readonly OpCode Leave_S;

	public static readonly OpCode Stind_I;

	public static readonly OpCode Conv_U;

	public static readonly OpCode Prefix7;

	public static readonly OpCode Prefix6;

	public static readonly OpCode Prefix5;

	public static readonly OpCode Prefix4;

	public static readonly OpCode Prefix3;

	public static readonly OpCode Prefix2;

	public static readonly OpCode Prefix1;

	public static readonly OpCode Prefixref;

	public static readonly OpCode Arglist;

	public static readonly OpCode Ceq;

	public static readonly OpCode Cgt;

	public static readonly OpCode Cgt_Un;

	public static readonly OpCode Clt;

	public static readonly OpCode Clt_Un;

	public static readonly OpCode Ldftn;

	public static readonly OpCode Ldvirtftn;

	public static readonly OpCode Ldarg;

	public static readonly OpCode Ldarga;

	public static readonly OpCode Starg;

	public static readonly OpCode Ldloc;

	public static readonly OpCode Ldloca;

	public static readonly OpCode Stloc;

	public static readonly OpCode Localloc;

	public static readonly OpCode Endfilter;

	public static readonly OpCode Unaligned;

	public static readonly OpCode Volatile;

	public static readonly OpCode Tailcall;

	public static readonly OpCode Initobj;

	public static readonly OpCode Constrained;

	public static readonly OpCode Cpblk;

	public static readonly OpCode Initblk;

	public static readonly OpCode No;

	public static readonly OpCode Rethrow;

	public static readonly OpCode Sizeof;

	public static readonly OpCode Refanytype;

	public static readonly OpCode Readonly;

	static OpCodes()
	{
		OneByteOpCodes = new OpCode[256];
		TwoByteOpCodes = new OpCode[256];
		UNKNOWN1 = new OpCode("UNKNOWN1", Code.UNKNOWN1, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Nternal, StackBehaviour.Push0, StackBehaviour.Pop0);
		UNKNOWN2 = new OpCode("UNKNOWN2", Code.UNKNOWN2, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Nternal, StackBehaviour.Push0, StackBehaviour.Pop0);
		Nop = new OpCode("nop", Code.Nop, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Pop0);
		Break = new OpCode("break", Code.Break, OperandType.InlineNone, FlowControl.Break, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Pop0);
		Ldarg_0 = new OpCode("ldarg.0", Code.Ldarg_0, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldarg_1 = new OpCode("ldarg.1", Code.Ldarg_1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldarg_2 = new OpCode("ldarg.2", Code.Ldarg_2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldarg_3 = new OpCode("ldarg.3", Code.Ldarg_3, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldloc_0 = new OpCode("ldloc.0", Code.Ldloc_0, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldloc_1 = new OpCode("ldloc.1", Code.Ldloc_1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldloc_2 = new OpCode("ldloc.2", Code.Ldloc_2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldloc_3 = new OpCode("ldloc.3", Code.Ldloc_3, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push1, StackBehaviour.Pop0);
		Stloc_0 = new OpCode("stloc.0", Code.Stloc_0, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1);
		Stloc_1 = new OpCode("stloc.1", Code.Stloc_1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1);
		Stloc_2 = new OpCode("stloc.2", Code.Stloc_2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1);
		Stloc_3 = new OpCode("stloc.3", Code.Stloc_3, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1);
		Ldarg_S = new OpCode("ldarg.s", Code.Ldarg_S, OperandType.ShortInlineVar, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldarga_S = new OpCode("ldarga.s", Code.Ldarga_S, OperandType.ShortInlineVar, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Starg_S = new OpCode("starg.s", Code.Starg_S, OperandType.ShortInlineVar, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1);
		Ldloc_S = new OpCode("ldloc.s", Code.Ldloc_S, OperandType.ShortInlineVar, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldloca_S = new OpCode("ldloca.s", Code.Ldloca_S, OperandType.ShortInlineVar, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Stloc_S = new OpCode("stloc.s", Code.Stloc_S, OperandType.ShortInlineVar, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1);
		Ldnull = new OpCode("ldnull", Code.Ldnull, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushref, StackBehaviour.Pop0);
		Ldc_I4_M1 = new OpCode("ldc.i4.m1", Code.Ldc_I4_M1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I4_0 = new OpCode("ldc.i4.0", Code.Ldc_I4_0, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I4_1 = new OpCode("ldc.i4.1", Code.Ldc_I4_1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I4_2 = new OpCode("ldc.i4.2", Code.Ldc_I4_2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I4_3 = new OpCode("ldc.i4.3", Code.Ldc_I4_3, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I4_4 = new OpCode("ldc.i4.4", Code.Ldc_I4_4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I4_5 = new OpCode("ldc.i4.5", Code.Ldc_I4_5, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I4_6 = new OpCode("ldc.i4.6", Code.Ldc_I4_6, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I4_7 = new OpCode("ldc.i4.7", Code.Ldc_I4_7, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I4_8 = new OpCode("ldc.i4.8", Code.Ldc_I4_8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I4_S = new OpCode("ldc.i4.s", Code.Ldc_I4_S, OperandType.ShortInlineI, FlowControl.Next, OpCodeType.Macro, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I4 = new OpCode("ldc.i4", Code.Ldc_I4, OperandType.InlineI, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldc_I8 = new OpCode("ldc.i8", Code.Ldc_I8, OperandType.InlineI8, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi8, StackBehaviour.Pop0);
		Ldc_R4 = new OpCode("ldc.r4", Code.Ldc_R4, OperandType.ShortInlineR, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushr4, StackBehaviour.Pop0);
		Ldc_R8 = new OpCode("ldc.r8", Code.Ldc_R8, OperandType.InlineR, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushr8, StackBehaviour.Pop0);
		Dup = new OpCode("dup", Code.Dup, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1_push1, StackBehaviour.Pop1);
		Pop = new OpCode("pop", Code.Pop, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Pop1);
		Jmp = new OpCode("jmp", Code.Jmp, OperandType.InlineMethod, FlowControl.Call, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Pop0);
		Call = new OpCode("call", Code.Call, OperandType.InlineMethod, FlowControl.Call, OpCodeType.Primitive, StackBehaviour.Varpush, StackBehaviour.Varpop);
		Calli = new OpCode("calli", Code.Calli, OperandType.InlineSig, FlowControl.Call, OpCodeType.Primitive, StackBehaviour.Varpush, StackBehaviour.Varpop);
		Ret = new OpCode("ret", Code.Ret, OperandType.InlineNone, FlowControl.Return, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Varpop);
		Br_S = new OpCode("br.s", Code.Br_S, OperandType.ShortInlineBrTarget, FlowControl.Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop0);
		Brfalse_S = new OpCode("brfalse.s", Code.Brfalse_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Popi);
		Brtrue_S = new OpCode("brtrue.s", Code.Brtrue_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Popi);
		Beq_S = new OpCode("beq.s", Code.Beq_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Bge_S = new OpCode("bge.s", Code.Bge_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Bgt_S = new OpCode("bgt.s", Code.Bgt_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Ble_S = new OpCode("ble.s", Code.Ble_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Blt_S = new OpCode("blt.s", Code.Blt_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Bne_Un_S = new OpCode("bne.un.s", Code.Bne_Un_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Bge_Un_S = new OpCode("bge.un.s", Code.Bge_Un_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Bgt_Un_S = new OpCode("bgt.un.s", Code.Bgt_Un_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Ble_Un_S = new OpCode("ble.un.s", Code.Ble_Un_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Blt_Un_S = new OpCode("blt.un.s", Code.Blt_Un_S, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Br = new OpCode("br", Code.Br, OperandType.InlineBrTarget, FlowControl.Branch, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Pop0);
		Brfalse = new OpCode("brfalse", Code.Brfalse, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi);
		Brtrue = new OpCode("brtrue", Code.Brtrue, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi);
		Beq = new OpCode("beq", Code.Beq, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Bge = new OpCode("bge", Code.Bge, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Bgt = new OpCode("bgt", Code.Bgt, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Ble = new OpCode("ble", Code.Ble, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Blt = new OpCode("blt", Code.Blt, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Bne_Un = new OpCode("bne.un", Code.Bne_Un, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Bge_Un = new OpCode("bge.un", Code.Bge_Un, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Bgt_Un = new OpCode("bgt.un", Code.Bgt_Un, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Ble_Un = new OpCode("ble.un", Code.Ble_Un, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Blt_Un = new OpCode("blt.un", Code.Blt_Un, OperandType.InlineBrTarget, FlowControl.Cond_Branch, OpCodeType.Macro, StackBehaviour.Push0, StackBehaviour.Pop1_pop1);
		Switch = new OpCode("switch", Code.Switch, OperandType.InlineSwitch, FlowControl.Cond_Branch, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi);
		Ldind_I1 = new OpCode("ldind.i1", Code.Ldind_I1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Popi);
		Ldind_U1 = new OpCode("ldind.u1", Code.Ldind_U1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Popi);
		Ldind_I2 = new OpCode("ldind.i2", Code.Ldind_I2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Popi);
		Ldind_U2 = new OpCode("ldind.u2", Code.Ldind_U2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Popi);
		Ldind_I4 = new OpCode("ldind.i4", Code.Ldind_I4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Popi);
		Ldind_U4 = new OpCode("ldind.u4", Code.Ldind_U4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Popi);
		Ldind_I8 = new OpCode("ldind.i8", Code.Ldind_I8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi8, StackBehaviour.Popi);
		Ldind_I = new OpCode("ldind.i", Code.Ldind_I, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Popi);
		Ldind_R4 = new OpCode("ldind.r4", Code.Ldind_R4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushr4, StackBehaviour.Popi);
		Ldind_R8 = new OpCode("ldind.r8", Code.Ldind_R8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushr8, StackBehaviour.Popi);
		Ldind_Ref = new OpCode("ldind.ref", Code.Ldind_Ref, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushref, StackBehaviour.Popi);
		Stind_Ref = new OpCode("stind.ref", Code.Stind_Ref, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi_popi);
		Stind_I1 = new OpCode("stind.i1", Code.Stind_I1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi_popi);
		Stind_I2 = new OpCode("stind.i2", Code.Stind_I2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi_popi);
		Stind_I4 = new OpCode("stind.i4", Code.Stind_I4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi_popi);
		Stind_I8 = new OpCode("stind.i8", Code.Stind_I8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi_popi8);
		Stind_R4 = new OpCode("stind.r4", Code.Stind_R4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi_popr4);
		Stind_R8 = new OpCode("stind.r8", Code.Stind_R8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi_popr8);
		Add = new OpCode("add", Code.Add, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Sub = new OpCode("sub", Code.Sub, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Mul = new OpCode("mul", Code.Mul, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Div = new OpCode("div", Code.Div, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Div_Un = new OpCode("div.un", Code.Div_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Rem = new OpCode("rem", Code.Rem, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Rem_Un = new OpCode("rem.un", Code.Rem_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		And = new OpCode("and", Code.And, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Or = new OpCode("or", Code.Or, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Xor = new OpCode("xor", Code.Xor, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Shl = new OpCode("shl", Code.Shl, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Shr = new OpCode("shr", Code.Shr, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Shr_Un = new OpCode("shr.un", Code.Shr_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Neg = new OpCode("neg", Code.Neg, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1);
		Not = new OpCode("not", Code.Not, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1);
		Conv_I1 = new OpCode("conv.i1", Code.Conv_I1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_I2 = new OpCode("conv.i2", Code.Conv_I2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_I4 = new OpCode("conv.i4", Code.Conv_I4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_I8 = new OpCode("conv.i8", Code.Conv_I8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi8, StackBehaviour.Pop1);
		Conv_R4 = new OpCode("conv.r4", Code.Conv_R4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushr4, StackBehaviour.Pop1);
		Conv_R8 = new OpCode("conv.r8", Code.Conv_R8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushr8, StackBehaviour.Pop1);
		Conv_U4 = new OpCode("conv.u4", Code.Conv_U4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_U8 = new OpCode("conv.u8", Code.Conv_U8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi8, StackBehaviour.Pop1);
		Callvirt = new OpCode("callvirt", Code.Callvirt, OperandType.InlineMethod, FlowControl.Call, OpCodeType.Objmodel, StackBehaviour.Varpush, StackBehaviour.Varpop);
		Cpobj = new OpCode("cpobj", Code.Cpobj, OperandType.InlineType, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popi_popi);
		Ldobj = new OpCode("ldobj", Code.Ldobj, OperandType.InlineType, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push1, StackBehaviour.Popi);
		Ldstr = new OpCode("ldstr", Code.Ldstr, OperandType.InlineString, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushref, StackBehaviour.Pop0);
		Newobj = new OpCode("newobj", Code.Newobj, OperandType.InlineMethod, FlowControl.Call, OpCodeType.Objmodel, StackBehaviour.Pushref, StackBehaviour.Varpop);
		Castclass = new OpCode("castclass", Code.Castclass, OperandType.InlineType, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushref, StackBehaviour.Popref);
		Isinst = new OpCode("isinst", Code.Isinst, OperandType.InlineType, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Popref);
		Conv_R_Un = new OpCode("conv.r.un", Code.Conv_R_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushr8, StackBehaviour.Pop1);
		Unbox = new OpCode("unbox", Code.Unbox, OperandType.InlineType, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Popref);
		Throw = new OpCode("throw", Code.Throw, OperandType.InlineNone, FlowControl.Throw, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popref);
		Ldfld = new OpCode("ldfld", Code.Ldfld, OperandType.InlineField, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push1, StackBehaviour.Popref);
		Ldflda = new OpCode("ldflda", Code.Ldflda, OperandType.InlineField, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Popref);
		Stfld = new OpCode("stfld", Code.Stfld, OperandType.InlineField, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popref_pop1);
		Ldsfld = new OpCode("ldsfld", Code.Ldsfld, OperandType.InlineField, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldsflda = new OpCode("ldsflda", Code.Ldsflda, OperandType.InlineField, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Stsfld = new OpCode("stsfld", Code.Stsfld, OperandType.InlineField, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Pop1);
		Stobj = new OpCode("stobj", Code.Stobj, OperandType.InlineType, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi_pop1);
		Conv_Ovf_I1_Un = new OpCode("conv.ovf.i1.un", Code.Conv_Ovf_I1_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_I2_Un = new OpCode("conv.ovf.i2.un", Code.Conv_Ovf_I2_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_I4_Un = new OpCode("conv.ovf.i4.un", Code.Conv_Ovf_I4_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_I8_Un = new OpCode("conv.ovf.i8.un", Code.Conv_Ovf_I8_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi8, StackBehaviour.Pop1);
		Conv_Ovf_U1_Un = new OpCode("conv.ovf.u1.un", Code.Conv_Ovf_U1_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_U2_Un = new OpCode("conv.ovf.u2.un", Code.Conv_Ovf_U2_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_U4_Un = new OpCode("conv.ovf.u4.un", Code.Conv_Ovf_U4_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_U8_Un = new OpCode("conv.ovf.u8.un", Code.Conv_Ovf_U8_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi8, StackBehaviour.Pop1);
		Conv_Ovf_I_Un = new OpCode("conv.ovf.i.un", Code.Conv_Ovf_I_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_U_Un = new OpCode("conv.ovf.u.un", Code.Conv_Ovf_U_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Box = new OpCode("box", Code.Box, OperandType.InlineType, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushref, StackBehaviour.Pop1);
		Newarr = new OpCode("newarr", Code.Newarr, OperandType.InlineType, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushref, StackBehaviour.Popi);
		Ldlen = new OpCode("ldlen", Code.Ldlen, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Popref);
		Ldelema = new OpCode("ldelema", Code.Ldelema, OperandType.InlineType, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Popref_popi);
		Ldelem_I1 = new OpCode("ldelem.i1", Code.Ldelem_I1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Popref_popi);
		Ldelem_U1 = new OpCode("ldelem.u1", Code.Ldelem_U1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Popref_popi);
		Ldelem_I2 = new OpCode("ldelem.i2", Code.Ldelem_I2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Popref_popi);
		Ldelem_U2 = new OpCode("ldelem.u2", Code.Ldelem_U2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Popref_popi);
		Ldelem_I4 = new OpCode("ldelem.i4", Code.Ldelem_I4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Popref_popi);
		Ldelem_U4 = new OpCode("ldelem.u4", Code.Ldelem_U4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Popref_popi);
		Ldelem_I8 = new OpCode("ldelem.i8", Code.Ldelem_I8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi8, StackBehaviour.Popref_popi);
		Ldelem_I = new OpCode("ldelem.i", Code.Ldelem_I, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushi, StackBehaviour.Popref_popi);
		Ldelem_R4 = new OpCode("ldelem.r4", Code.Ldelem_R4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushr4, StackBehaviour.Popref_popi);
		Ldelem_R8 = new OpCode("ldelem.r8", Code.Ldelem_R8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushr8, StackBehaviour.Popref_popi);
		Ldelem_Ref = new OpCode("ldelem.ref", Code.Ldelem_Ref, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Pushref, StackBehaviour.Popref_popi);
		Stelem_I = new OpCode("stelem.i", Code.Stelem_I, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popref_popi_popi);
		Stelem_I1 = new OpCode("stelem.i1", Code.Stelem_I1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popref_popi_popi);
		Stelem_I2 = new OpCode("stelem.i2", Code.Stelem_I2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popref_popi_popi);
		Stelem_I4 = new OpCode("stelem.i4", Code.Stelem_I4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popref_popi_popi);
		Stelem_I8 = new OpCode("stelem.i8", Code.Stelem_I8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popref_popi_popi8);
		Stelem_R4 = new OpCode("stelem.r4", Code.Stelem_R4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popref_popi_popr4);
		Stelem_R8 = new OpCode("stelem.r8", Code.Stelem_R8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popref_popi_popr8);
		Stelem_Ref = new OpCode("stelem.ref", Code.Stelem_Ref, OperandType.InlineNone, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popref_popi_popref);
		Ldelem = new OpCode("ldelem", Code.Ldelem, OperandType.InlineType, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push1, StackBehaviour.Popref_popi);
		Stelem = new OpCode("stelem", Code.Stelem, OperandType.InlineType, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popref_popi_pop1);
		Unbox_Any = new OpCode("unbox.any", Code.Unbox_Any, OperandType.InlineType, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push1, StackBehaviour.Popref);
		Conv_Ovf_I1 = new OpCode("conv.ovf.i1", Code.Conv_Ovf_I1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_U1 = new OpCode("conv.ovf.u1", Code.Conv_Ovf_U1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_I2 = new OpCode("conv.ovf.i2", Code.Conv_Ovf_I2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_U2 = new OpCode("conv.ovf.u2", Code.Conv_Ovf_U2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_I4 = new OpCode("conv.ovf.i4", Code.Conv_Ovf_I4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_U4 = new OpCode("conv.ovf.u4", Code.Conv_Ovf_U4, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_I8 = new OpCode("conv.ovf.i8", Code.Conv_Ovf_I8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi8, StackBehaviour.Pop1);
		Conv_Ovf_U8 = new OpCode("conv.ovf.u8", Code.Conv_Ovf_U8, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi8, StackBehaviour.Pop1);
		Refanyval = new OpCode("refanyval", Code.Refanyval, OperandType.InlineType, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Ckfinite = new OpCode("ckfinite", Code.Ckfinite, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushr8, StackBehaviour.Pop1);
		Mkrefany = new OpCode("mkrefany", Code.Mkrefany, OperandType.InlineType, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Popi);
		Ldtoken = new OpCode("ldtoken", Code.Ldtoken, OperandType.InlineTok, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Conv_U2 = new OpCode("conv.u2", Code.Conv_U2, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_U1 = new OpCode("conv.u1", Code.Conv_U1, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_I = new OpCode("conv.i", Code.Conv_I, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_I = new OpCode("conv.ovf.i", Code.Conv_Ovf_I, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Conv_Ovf_U = new OpCode("conv.ovf.u", Code.Conv_Ovf_U, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Add_Ovf = new OpCode("add.ovf", Code.Add_Ovf, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Add_Ovf_Un = new OpCode("add.ovf.un", Code.Add_Ovf_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Mul_Ovf = new OpCode("mul.ovf", Code.Mul_Ovf, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Mul_Ovf_Un = new OpCode("mul.ovf.un", Code.Mul_Ovf_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Sub_Ovf = new OpCode("sub.ovf", Code.Sub_Ovf, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Sub_Ovf_Un = new OpCode("sub.ovf.un", Code.Sub_Ovf_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop1_pop1);
		Endfinally = new OpCode("endfinally", Code.Endfinally, OperandType.InlineNone, FlowControl.Return, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.PopAll);
		Leave = new OpCode("leave", Code.Leave, OperandType.InlineBrTarget, FlowControl.Branch, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.PopAll);
		Leave_S = new OpCode("leave.s", Code.Leave_S, OperandType.ShortInlineBrTarget, FlowControl.Branch, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.PopAll);
		Stind_I = new OpCode("stind.i", Code.Stind_I, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi_popi);
		Conv_U = new OpCode("conv.u", Code.Conv_U, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Prefix7 = new OpCode("prefix7", Code.Prefix7, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Nternal, StackBehaviour.Push0, StackBehaviour.Pop0);
		Prefix6 = new OpCode("prefix6", Code.Prefix6, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Nternal, StackBehaviour.Push0, StackBehaviour.Pop0);
		Prefix5 = new OpCode("prefix5", Code.Prefix5, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Nternal, StackBehaviour.Push0, StackBehaviour.Pop0);
		Prefix4 = new OpCode("prefix4", Code.Prefix4, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Nternal, StackBehaviour.Push0, StackBehaviour.Pop0);
		Prefix3 = new OpCode("prefix3", Code.Prefix3, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Nternal, StackBehaviour.Push0, StackBehaviour.Pop0);
		Prefix2 = new OpCode("prefix2", Code.Prefix2, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Nternal, StackBehaviour.Push0, StackBehaviour.Pop0);
		Prefix1 = new OpCode("prefix1", Code.Prefix1, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Nternal, StackBehaviour.Push0, StackBehaviour.Pop0);
		Prefixref = new OpCode("prefixref", Code.Prefixref, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Nternal, StackBehaviour.Push0, StackBehaviour.Pop0);
		Arglist = new OpCode("arglist", Code.Arglist, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ceq = new OpCode("ceq", Code.Ceq, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1_pop1);
		Cgt = new OpCode("cgt", Code.Cgt, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1_pop1);
		Cgt_Un = new OpCode("cgt.un", Code.Cgt_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1_pop1);
		Clt = new OpCode("clt", Code.Clt, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1_pop1);
		Clt_Un = new OpCode("clt.un", Code.Clt_Un, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1_pop1);
		Ldftn = new OpCode("ldftn", Code.Ldftn, OperandType.InlineMethod, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Ldvirtftn = new OpCode("ldvirtftn", Code.Ldvirtftn, OperandType.InlineMethod, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Popref);
		Ldarg = new OpCode("ldarg", Code.Ldarg, OperandType.InlineVar, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldarga = new OpCode("ldarga", Code.Ldarga, OperandType.InlineVar, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Starg = new OpCode("starg", Code.Starg, OperandType.InlineVar, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Pop1);
		Ldloc = new OpCode("ldloc", Code.Ldloc, OperandType.InlineVar, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push1, StackBehaviour.Pop0);
		Ldloca = new OpCode("ldloca", Code.Ldloca, OperandType.InlineVar, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Stloc = new OpCode("stloc", Code.Stloc, OperandType.InlineVar, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Pop1);
		Localloc = new OpCode("localloc", Code.Localloc, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Popi);
		Endfilter = new OpCode("endfilter", Code.Endfilter, OperandType.InlineNone, FlowControl.Return, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi);
		Unaligned = new OpCode("unaligned.", Code.Unaligned, OperandType.ShortInlineI, FlowControl.Meta, OpCodeType.Prefix, StackBehaviour.Push0, StackBehaviour.Pop0);
		Volatile = new OpCode("volatile.", Code.Volatile, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Prefix, StackBehaviour.Push0, StackBehaviour.Pop0);
		Tailcall = new OpCode("tail.", Code.Tailcall, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Prefix, StackBehaviour.Push0, StackBehaviour.Pop0);
		Initobj = new OpCode("initobj", Code.Initobj, OperandType.InlineType, FlowControl.Next, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Popi);
		Constrained = new OpCode("constrained.", Code.Constrained, OperandType.InlineType, FlowControl.Meta, OpCodeType.Prefix, StackBehaviour.Push0, StackBehaviour.Pop0);
		Cpblk = new OpCode("cpblk", Code.Cpblk, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi_popi_popi);
		Initblk = new OpCode("initblk", Code.Initblk, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Push0, StackBehaviour.Popi_popi_popi);
		No = new OpCode("no.", Code.No, OperandType.ShortInlineI, FlowControl.Meta, OpCodeType.Prefix, StackBehaviour.Push0, StackBehaviour.Pop0);
		Rethrow = new OpCode("rethrow", Code.Rethrow, OperandType.InlineNone, FlowControl.Throw, OpCodeType.Objmodel, StackBehaviour.Push0, StackBehaviour.Pop0);
		Sizeof = new OpCode("sizeof", Code.Sizeof, OperandType.InlineType, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop0);
		Refanytype = new OpCode("refanytype", Code.Refanytype, OperandType.InlineNone, FlowControl.Next, OpCodeType.Primitive, StackBehaviour.Pushi, StackBehaviour.Pop1);
		Readonly = new OpCode("readonly.", Code.Readonly, OperandType.InlineNone, FlowControl.Meta, OpCodeType.Prefix, StackBehaviour.Push0, StackBehaviour.Pop0);
		for (int i = 0; i < OneByteOpCodes.Length; i++)
		{
			if (OneByteOpCodes[i] == null)
			{
				OneByteOpCodes[i] = UNKNOWN1;
			}
		}
		for (int j = 0; j < TwoByteOpCodes.Length; j++)
		{
			if (TwoByteOpCodes[j] == null)
			{
				TwoByteOpCodes[j] = UNKNOWN2;
			}
		}
	}
}
