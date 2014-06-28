//
// Author(s):
//     Pavlos Touboulidis <pav@pav.gr>
//
// Created on 2014-6-27
//
using System;
using System.Drawing;

namespace Oratool.Graphics
{
	public struct D3
	{
		public double X;
		public double Y;
		public double Z;

		public D3(double x, double y, double z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}
	}

	public static class ColorDelta
	{
		// http://www.easyrgb.com/index.php?X=MATH&H=02#text2
		public static void RGBToXYZ(byte r, byte g, byte b, out D3 xyz)
		{
			xyz.X = r / 255d;
			if (xyz.X > 0.04045)
				xyz.X = Math.Pow((xyz.X + 0.055) / 1.055, 2.4);
			else
				xyz.X /= 12.92;
			xyz.X *= 100;

			xyz.Y = g / 255d;
			if (xyz.Y > 0.04045)
				xyz.Y = Math.Pow((xyz.Y + 0.055) / 1.055, 2.4);
			else
				xyz.Y /= 12.92;
			xyz.Y *= 100;

			xyz.Z = b / 255d;
			if (xyz.Z > 0.04045)
				xyz.Z = Math.Pow((xyz.Z + 0.055) / 1.055, 2.4);
			else
				xyz.Z /= 12.92;
			xyz.Z *= 100;

			// Observer = 2°, Illuminant = D65
			xyz.X = xyz.X * 0.4124 + xyz.Y * 0.3576 + xyz.Z * 0.1805;
			xyz.Y = xyz.X * 0.2126 + xyz.Y * 0.7152 + xyz.Z * 0.0722;
			xyz.Z = xyz.X * 0.0193 + xyz.Y * 0.1192 + xyz.Z * 0.9505;
		}

		// http://www.easyrgb.com/index.php?X=MATH&H=07#text7
		public static void XYZToLab(D3 xyz, out D3 lab)
		{
			// Observer = 2°, Illuminant= D65
			xyz.X /= 95.047;
			if (xyz.X > 0.008856)
				xyz.X = Math.Pow(xyz.X, 1d / 3d);
			else
				xyz.X = (7.787 * xyz.X) + (16d / 116d);

			xyz.Y /= 100.000;
			if (xyz.Y > 0.008856)
				xyz.Y = Math.Pow(xyz.Y, 1d / 3d);
			else
				xyz.Y = (7.787 * xyz.Y) + (16d / 116d);

			xyz.Z /= 108.883;
			if (xyz.Z > 0.008856)
				xyz.Z = Math.Pow(xyz.Z, 1d / 3d);
			else
				xyz.Z = (7.787 * xyz.Z) + (16d / 116d);

			lab.X = (116d * xyz.Y) - 16d;
			lab.Y = 500d * (xyz.X - xyz.Y);
			lab.Z = 200d * (xyz.Y - xyz.Z); 
		}

		public static D3 ToCIELab(this Color color)
		{
			D3 v;
			RGBToXYZ(color.R, color.G, color.B, out v);
			XYZToLab(v, out v);
			return v;
		}

		// http://www.easyrgb.com/index.php?X=DELT&H=03#text3
		public static double DeltaE(D3 lab1, D3 lab2)
		{
			double dl = lab1.X - lab2.X;
			double da = lab1.Y - lab2.Y;
			double db = lab1.Z - lab2.Z;

			return Math.Sqrt(dl * dl + da * da + db * db);
		}
	}
}
