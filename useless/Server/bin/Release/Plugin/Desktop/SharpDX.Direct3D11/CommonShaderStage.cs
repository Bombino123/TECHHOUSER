using System;

namespace SharpDX.Direct3D11;

public abstract class CommonShaderStage : CppObject
{
	public const int ConstantBufferApiSlotCount = 14;

	public const int ConstantBufferComponents = 4;

	public const int ConstantBufferComponentBitCount = 32;

	public const int ConstantBufferHwSlotCount = 15;

	public const int ConstantBufferPartialUpdateExtentsByteAlignment = 16;

	public const int ConstantBufferRegisterComponents = 4;

	public const int ConstantBufferRegisterCount = 15;

	public const int ConstantBufferRegisterReadsPerInst = 1;

	public const int ConstantBufferRegisterReadPorts = 1;

	public const int FlowcontrolNestingLimit = 64;

	public const int ImmediateConstantBufferRegisterComponents = 4;

	public const int ImmediateConstantBufferRegisterCount = 1;

	public const int ImmediateConstantBufferRegisterReadsPerInst = 1;

	public const int ImmediateConstantBufferRegisterReadPorts = 1;

	public const int ImmediateValueComponentBitCount = 32;

	public const int InputResourceRegisterComponents = 1;

	public const int InputResourceRegisterCount = 128;

	public const int InputResourceRegisterReadsPerInst = 1;

	public const int InputResourceRegisterReadPorts = 1;

	public const int InputResourceSlotCount = 128;

	public const int SamplerRegisterComponents = 1;

	public const int SamplerRegisterCount = 16;

	public const int SamplerRegisterReadsPerInst = 1;

	public const int SamplerRegisterReadPorts = 1;

	public const int SamplerSlotCount = 16;

	public const int SubRoutineNestingLimit = 32;

	public const int TempRegisterComponents = 4;

	public const int TempRegisterComponentBitCount = 32;

	public const int TempRegisterCount = 4096;

	public const int TempRegisterReadsPerInst = 3;

	public const int TempRegisterReadPorts = 3;

	public const int TextureCoordRangeReductionMaximum = 10;

	public const int TextureCoordRangeReductionMinimum = -10;

	public const int TextureElOffsetMaximumNegative = -8;

	public const int TextureElOffsetMaximumPositive = 7;

	protected CommonShaderStage(IntPtr pointer)
		: base(pointer)
	{
	}

	public Buffer[] GetConstantBuffers(int startSlot, int count)
	{
		Buffer[] array = new Buffer[count];
		GetConstantBuffers(startSlot, count, array);
		return array;
	}

	public SamplerState[] GetSamplers(int startSlot, int count)
	{
		SamplerState[] array = new SamplerState[count];
		GetSamplers(startSlot, count, array);
		return array;
	}

	public ShaderResourceView[] GetShaderResources(int startSlot, int count)
	{
		ShaderResourceView[] array = new ShaderResourceView[count];
		GetShaderResources(startSlot, count, array);
		return array;
	}

	public unsafe void SetConstantBuffer(int slot, Buffer constantBuffer)
	{
		IntPtr intPtr = constantBuffer?.NativePointer ?? IntPtr.Zero;
		SetConstantBuffers(slot, 1, new IntPtr(&intPtr));
	}

	public void SetConstantBuffers(int slot, params Buffer[] constantBuffers)
	{
		SetConstantBuffers(slot, (constantBuffers != null) ? constantBuffers.Length : 0, constantBuffers);
	}

	public void SetConstantBuffers(int slot, ComArray<Buffer> constantBuffers)
	{
		SetConstantBuffers(slot, constantBuffers?.Length ?? 0, constantBuffers);
	}

	public unsafe void SetSampler(int slot, SamplerState sampler)
	{
		IntPtr intPtr = sampler?.NativePointer ?? IntPtr.Zero;
		SetSamplers(slot, 1, new IntPtr(&intPtr));
	}

	public void SetSamplers(int slot, params SamplerState[] samplers)
	{
		SetSamplers(slot, (samplers != null) ? samplers.Length : 0, samplers);
	}

	public void SetSamplers(int slot, ComArray<SamplerState> samplers)
	{
		SetSamplers(slot, samplers?.Length ?? 0, samplers);
	}

	public unsafe void SetShaderResource(int slot, ShaderResourceView resourceView)
	{
		IntPtr intPtr = resourceView?.NativePointer ?? IntPtr.Zero;
		SetShaderResources(slot, 1, new IntPtr(&intPtr));
	}

	public void SetShaderResources(int startSlot, params ShaderResourceView[] shaderResourceViews)
	{
		SetShaderResources(startSlot, shaderResourceViews.Length, shaderResourceViews);
	}

	public void SetShaderResources(int startSlot, ComArray<ShaderResourceView> shaderResourceViews)
	{
		SetShaderResources(startSlot, shaderResourceViews.Length, shaderResourceViews);
	}

	internal abstract void GetShaderResources(int startSlot, int numViews, ShaderResourceView[] shaderResourceViewsRef);

	internal abstract void GetSamplers(int startSlot, int numSamplers, SamplerState[] samplersRef);

	internal abstract void GetConstantBuffers(int startSlot, int numBuffers, Buffer[] constantBuffersRef);

	public unsafe void SetShaderResources(int startSlot, int numViews, params ShaderResourceView[] shaderResourceViews)
	{
		IntPtr* ptr = null;
		if (numViews > 0)
		{
			ptr = stackalloc IntPtr[numViews];
			for (int i = 0; i < numViews; i++)
			{
				ptr[i] = ((shaderResourceViews[i] == null) ? IntPtr.Zero : shaderResourceViews[i].NativePointer);
			}
		}
		SetShaderResources(startSlot, numViews, (IntPtr)ptr);
	}

	public void SetShaderResources(int startSlot, int numViews, ComArray<ShaderResourceView> shaderResourceViewsRef)
	{
		SetShaderResources(startSlot, numViews, shaderResourceViewsRef.NativePointer);
	}

	internal abstract void SetShaderResources(int startSlot, int numViews, IntPtr shaderResourceViewsRef);

	public unsafe void SetSamplers(int startSlot, int numSamplers, params SamplerState[] samplers)
	{
		IntPtr* ptr = null;
		if (numSamplers > 0)
		{
			ptr = stackalloc IntPtr[numSamplers];
			for (int i = 0; i < numSamplers; i++)
			{
				ptr[i] = ((samplers[i] == null) ? IntPtr.Zero : samplers[i].NativePointer);
			}
		}
		SetSamplers(startSlot, numSamplers, (IntPtr)ptr);
	}

	public void SetSamplers(int startSlot, int numSamplers, ComArray<SamplerState> samplers)
	{
		SetSamplers(startSlot, numSamplers, samplers.NativePointer);
	}

	internal abstract void SetSamplers(int startSlot, int numSamplers, IntPtr samplersRef);

	public unsafe void SetConstantBuffers(int startSlot, int numBuffers, params Buffer[] constantBuffers)
	{
		IntPtr* ptr = null;
		if (numBuffers > 0)
		{
			ptr = stackalloc IntPtr[numBuffers];
			for (int i = 0; i < numBuffers; i++)
			{
				ptr[i] = ((constantBuffers[i] == null) ? IntPtr.Zero : constantBuffers[i].NativePointer);
			}
		}
		SetConstantBuffers(startSlot, numBuffers, (IntPtr)ptr);
	}

	public void SetConstantBuffers(int startSlot, int numBuffers, ComArray<Buffer> constantBuffers)
	{
		SetConstantBuffers(startSlot, numBuffers, constantBuffers.NativePointer);
	}

	internal abstract void SetConstantBuffers(int startSlot, int numBuffers, IntPtr constantBuffersRef);

	public abstract void SetShader(DeviceChild shader, ClassInstance[] classInstancesOut, int numClassInstances);

	internal abstract void SetUnorderedAccessViews(int startSlot, int numBuffers, IntPtr unorderedAccessBuffer, IntPtr uavCount);
}
public abstract class CommonShaderStage<T> : CommonShaderStage where T : DeviceChild
{
	protected CommonShaderStage(IntPtr pointer)
		: base(pointer)
	{
	}

	public T Get()
	{
		int numClassInstancesRef = 0;
		GetShader(out var pixelShaderRef, null, ref numClassInstancesRef);
		return pixelShaderRef;
	}

	public T Get(ClassInstance[] classInstances)
	{
		int numClassInstancesRef = classInstances.Length;
		GetShader(out var pixelShaderRef, classInstances, ref numClassInstancesRef);
		return pixelShaderRef;
	}

	public void Set(T shader)
	{
		SetShader(shader, (ComArray<ClassInstance>)null, 0);
	}

	public void Set(T shader, ClassInstance[] classInstances)
	{
		SetShader(shader, classInstances, (classInstances != null) ? classInstances.Length : 0);
	}

	public void Set(T shader, ComArray<ClassInstance> classInstances)
	{
		SetShader(shader, classInstances, classInstances?.Length ?? 0);
	}

	public override void SetShader(DeviceChild shader, ClassInstance[] classInstancesOut, int numClassInstances)
	{
		SetShader((T)shader, classInstancesOut, numClassInstances);
	}

	internal override void SetUnorderedAccessViews(int startSlot, int numBuffers, IntPtr unorderedAccessBuffer, IntPtr uavCount)
	{
		throw new NotSupportedException();
	}

	internal abstract void SetShader(T shaderRef, ClassInstance[] classInstancesRef, int numClassInstances);

	internal abstract void SetShader(T shaderRef, ComArray<ClassInstance> classInstancesRef, int numClassInstances);

	internal abstract void GetShader(out T pixelShaderRef, ClassInstance[] classInstancesRef, ref int numClassInstancesRef);
}
