/*******************************************************************************
 * @author Reika Kalseki
 *
 * Copyright 2017
 *
 * All rights reserved.
 * Distribution of the software in any form is only allowed with
 * explicit, prior permission from the owner.
 ******************************************************************************/
using System;
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.IO;    //For data read/write methods
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

/** Adapted from the Open Simplex Noise Generator by Kurt Spencer, simplified to 1D.
 * All '//' comments are his. */
public class Simplex1DGenerator : SimplexNoiseGenerator {

	private static readonly double STRETCH_CONSTANT = ((1D/Math.Sqrt(2D+1D))-1D)/2D;
	private static readonly double SQUISH_CONSTANT = (Math.Sqrt(2D+1D)-1D)/2D;

	private static readonly double NORM_CONSTANT = 47;

	public Simplex1DGenerator(long seed) : base(seed) {

	}

	//1D OpenSimplex Noise.
	/// <returns>A value from -1 to +1</returns>
	protected override double calcValue(double x, double y, double z0, double f, double a) {
		if (!Mathf.Approximately((float)f, 1) && f > 0) {
			x *= f;
		}

		//Place input coordinates onto grid.
		double stretchOffset = x * STRETCH_CONSTANT;
		double xs = x + stretchOffset;
		double zs = stretchOffset;

		//Floor to get grid coordinates of rhombus (stretched square) super-cell origin.
		int xsb = (int)Math.Floor(xs);
		int zsb = (int)Math.Floor(zs);

		//Skew out to get actual coordinates of rhombus origin. We'll need these later.
		double squishOffset = (xsb + zsb) * SQUISH_CONSTANT;
		double xb = xsb + squishOffset;
		double zb = zsb + squishOffset;

		//Compute grid coordinates relative to rhombus origin.
		double xins = xs - xsb;
		double zins = zs - zsb;

		//Sum those together to get a value that determines which region we're in.
		double inSum = xins + zins;

		//Positions relative to origin point.
		double dx0 = x - xb;
		double dz0 = -zb;

		//We'll be defining these inside the next block and using them afterwards.
		double dx_ext, dz_ext;
		int xsv_ext, zsv_ext;

		double value = 0;

		//Contribution (1,0)
		double dx1 = dx0 - 1 - SQUISH_CONSTANT;
		double dz1 = dz0 - 0 - SQUISH_CONSTANT;
		double attn1 = 2 - (dx1 * dx1) - (dz1 * dz1);
		if (attn1 > 0) {
			attn1 *= attn1;
			value += attn1 * attn1 * this.extrapolate(xsb + 1, dx1);
		}

		//Contribution (0,1)
		double dx2 = dx0 - 0 - SQUISH_CONSTANT;
		double dz2 = dz0 - 1 - SQUISH_CONSTANT;
		double attn2 = 2 - (dx2 * dx2) - (dz2 * dz2);
		if (attn2 > 0) {
			attn2 *= attn2;
			value += attn2 * attn2 * this.extrapolate(xsb + 0, dx2);
		}

		if (inSum <= 1) { //We're inside the triangle (2-Simplex) at (0,0)
			double dins = 1 - inSum;
			if (dins > xins || dins > zins) { //(0,0) is one of the closest two triangular vertices
				if (xins > zins) {
					xsv_ext = xsb + 1;
					zsv_ext = zsb - 1;
					dx_ext = dx0 - 1;
					dz_ext = dz0 + 1;
				}
				else {
					xsv_ext = xsb - 1;
					zsv_ext = zsb + 1;
					dx_ext = dx0 + 1;
					dz_ext = dz0 - 1;
				}
			}
			else { //(1,0) and (0,1) are the closest two vertices.
				xsv_ext = xsb + 1;
				zsv_ext = zsb + 1;
				dx_ext = dx0 - 1 - (2 * SQUISH_CONSTANT);
				dz_ext = dz0 - 1 - (2 * SQUISH_CONSTANT);
			}
		}
		else { //We're inside the triangle (2-Simplex) at (1,1)
			double dins = 2 - inSum;
			if (dins < xins || dins < zins) { //(0,0) is one of the closest two triangular vertices
				if (xins > zins) {
					xsv_ext = xsb + 2;
					zsv_ext = zsb + 0;
					dx_ext = dx0 - 2 - (2 * SQUISH_CONSTANT);
					dz_ext = dz0 + 0 - (2 * SQUISH_CONSTANT);
				}
				else {
					xsv_ext = xsb + 0;
					zsv_ext = zsb + 2;
					dx_ext = dx0 + 0 - (2 * SQUISH_CONSTANT);
					dz_ext = dz0 - 2 - (2 * SQUISH_CONSTANT);
				}
			}
			else { //(1,0) and (0,1) are the closest two vertices.
				dx_ext = dx0;
				dz_ext = dz0;
				xsv_ext = xsb;
				zsv_ext = zsb;
			}
			xsb += 1;
			zsb += 1;
			dx0 = dx0 - 1 - (2 * SQUISH_CONSTANT);
			dz0 = dz0 - 1 - (2 * SQUISH_CONSTANT);
		}

		//Contribution (0,0) or (1,1)
		double attn0 = 2 - (dx0 * dx0) - (dz0 * dz0);
		if (attn0 > 0) {
			attn0 *= attn0;
			value += attn0 * attn0 * this.extrapolate(xsb, dx0);
		}

		//Extra Vertex
		double attn_ext = 2 - (dx_ext * dx_ext) - (dz_ext * dz_ext);
		if (attn_ext > 0) {
			attn_ext *= attn_ext;
			value += attn_ext * attn_ext * this.extrapolate(xsv_ext, dx_ext);
		}

		return a * value / NORM_CONSTANT;
	}

	private double extrapolate(int xsb, double dx) {
		int index = perm[(perm[xsb & 0xFF]) & 0xFF] & 0x0E;
		return gradients2D[index] * dx;
	}

	//Gradients for 2D. They approximate the directions to the
	//vertices of an octagon from the center.
	private static int[] gradients2D = new int[] {
			5,  2,    2,  5,
			-5,  2,   -2,  5,
			5, -2,    2, -5,
			-5, -2,   -2, -5,
	};
}
