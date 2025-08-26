using System;
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.IO;    //For data read/write methods
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

public class Octave {

	public readonly double frequency;
	public readonly double amplitude;
	public readonly double phaseShift;

	internal Octave(double f, double a, double p) {
		amplitude = a;
		frequency = f;
		phaseShift = p;
	}

}
