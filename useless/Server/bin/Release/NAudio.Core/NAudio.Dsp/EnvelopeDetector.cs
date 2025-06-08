using System;

namespace NAudio.Dsp;

internal class EnvelopeDetector
{
	private double sampleRate;

	private double ms;

	private double coeff;

	public double TimeConstant
	{
		get
		{
			return ms;
		}
		set
		{
			ms = value;
			SetCoef();
		}
	}

	public double SampleRate
	{
		get
		{
			return sampleRate;
		}
		set
		{
			sampleRate = value;
			SetCoef();
		}
	}

	public EnvelopeDetector()
		: this(1.0, 44100.0)
	{
	}

	public EnvelopeDetector(double ms, double sampleRate)
	{
		this.sampleRate = sampleRate;
		this.ms = ms;
		SetCoef();
	}

	public double Run(double inValue, double state)
	{
		return inValue + coeff * (state - inValue);
	}

	private void SetCoef()
	{
		coeff = Math.Exp(-1.0 / (0.001 * ms * sampleRate));
	}
}
