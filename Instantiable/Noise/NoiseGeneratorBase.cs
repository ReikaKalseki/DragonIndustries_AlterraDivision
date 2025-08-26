using System;
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.IO;    //For data read/write methods
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

public abstract class NoiseGeneratorBase {

	protected double inputFactor = 1;

	protected readonly List<Octave> octaves = new List<Octave>();
	protected double maxRange = 1;

	/// As opposed to scaling
	public bool clampEdge = false;

	public readonly long seed;

	private NoiseGeneratorBase xNoise;
	private NoiseGeneratorBase yNoise;
	private NoiseGeneratorBase zNoise;
	private double xNoiseScale;
	private double yNoiseScale;
	private double zNoiseScale;

	protected NoiseGeneratorBase(long s) {
		seed = s;
	}

	public double getValue(Vector3 vec) {
		return this.getValue(vec.x, vec.y, vec.z);
	}

	public double getValue(double x, double y, double z) {
		return this.calculateValues(x * inputFactor, y * inputFactor, z * inputFactor);
	}

	private double calculateValues(double x, double y, double z) {
		if (this.displaceCalculation()) {
			double x0 = x;
			double y0 = y;
			double z0 = z;
			x += this.getXDisplacement(x0, y0, z0);
			y += this.getYDisplacement(x0, y0, z0);
			z += this.getZDisplacement(x0, y0, z0);
		}

		double val = this.calcValue(x, y, z, 1, 1);

		if (octaves.Count > 0) {
			foreach (Octave o in octaves) {
				val += this.calcValue(x + o.phaseShift, y + o.phaseShift, z + o.phaseShift, o.frequency, o.amplitude);
			}
			if (clampEdge)
				val = Mathf.Clamp((float)val, -1, 1);
			else
				val /= maxRange;
		}

		return val;
	}

	protected virtual bool displaceCalculation() {
		return true;
	}

	protected abstract double calcValue(double x, double y, double z, double freq, double amp);

	public NoiseGeneratorBase setFrequency(double f) {
		inputFactor = f;
		return this;
	}

	public double getFrequencyScale() {
		return inputFactor;
	}

	public NoiseGeneratorBase addOctave(double relativeFrequency, double relativeAmplitude) {
		return this.addOctave(relativeFrequency, relativeAmplitude, 0);
	}

	public NoiseGeneratorBase addOctave(double relativeFrequency, double relativeAmplitude, double phaseShift) {
		octaves.Add(new Octave(relativeFrequency, relativeAmplitude, phaseShift));
		maxRange += relativeAmplitude;
		return this;
	}

	public NoiseGeneratorBase setDisplacementSimple(long seedX, double fx, long seedZ, double fz, double s) {
		return this.setDisplacement(new SimplexNoiseGenerator(seedX).setFrequency(fx), s, null, s, new SimplexNoiseGenerator(seedZ).setFrequency(fz), s);
	}

	public NoiseGeneratorBase setDisplacementSimple(long seedX, double fx, long seedY, double fy, long seedZ, double fz, double s) {
		return this.setDisplacement(new SimplexNoiseGenerator(seedX).setFrequency(fx), s, new SimplexNoiseGenerator(seedY).setFrequency(fy), s, new SimplexNoiseGenerator(seedZ).setFrequency(fz), s);
	}

	public NoiseGeneratorBase setDisplacement(NoiseGeneratorBase x, NoiseGeneratorBase y, NoiseGeneratorBase z, double s) {
		return this.setDisplacement(x, s, y, s, z, s);
	}

	public NoiseGeneratorBase setDisplacement(NoiseGeneratorBase x, double xs, NoiseGeneratorBase y, double ys, NoiseGeneratorBase z, double zs) {
		xNoise = x;
		yNoise = y;
		zNoise = z;
		xNoiseScale = xs;
		yNoiseScale = ys;
		zNoiseScale = zs;
		return this;
	}

	public double getXDisplacement(double x, double y, double z) {
		return xNoise != null ? xNoise.getValue(x, y, z) * xNoiseScale : 0;
	}

	public double getYDisplacement(double x, double y, double z) {
		return yNoise != null ? yNoise.getValue(x, y, z) * yNoiseScale : 0;
	}

	public double getZDisplacement(double x, double y, double z) {
		return zNoise != null ? zNoise.getValue(x, y, z) * zNoiseScale : 0;
	}
}
