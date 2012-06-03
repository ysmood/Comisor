using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace ys
{
	public class ImageProcessor
	{
		public unsafe static WriteableBitmap AutoLevels(
			BitmapSource bmps,
			int threshold = 100,
			double ColorWeightB = (double)1/3,
			double ColorWeightG = (double)1/3,
			double ColorWeightR = (double)1/3)
		{
			int n = 0;	// Enumerator
			byte* p;	// Color Pointer
			int AllPixels = bmps.PixelWidth * bmps.PixelHeight;
			int[] Histogram = new int[256];
			int Threshold = AllPixels / threshold;
			int Integral = 0;
			int Min = 0;
			int Max = 0;
			byte[] ColorOut = new byte[256];
			WriteableBitmap wb;

			#region Check if the source is color indexed or common deepth, and convert it to Bgra32 format.
			if (bmps.Palette != null ||
					(
						bmps.Format.BitsPerPixel != 8 &&				
						bmps.Format.BitsPerPixel != 24 &&
						bmps.Format.BitsPerPixel != 32
					)
				)
			{
				FormatConvertedBitmap fcb = new FormatConvertedBitmap(bmps, PixelFormats.Bgra32, null, 0);
				wb = new WriteableBitmap(fcb);
			}
			else
				wb = new WriteableBitmap(bmps);
			#endregion

			#region Get histogram
			p = (byte*)wb.BackBuffer;
			switch (wb.Format.BitsPerPixel)
			{
				case 8:
					for (n = 0; n < AllPixels; n++)
						Histogram[*p++] += 1;
					break;
				case 24:
					for (n = 0; n < AllPixels; n++)
						Histogram[(int)(ColorWeightB * (*p++) + ColorWeightG * (*p++) + ColorWeightR * (*p++))] += 1;
					break;
				case 32:
					for (n = 0; n < AllPixels; n++)
						Histogram[(int)(ColorWeightB * (*p++) + ColorWeightG * (*p++) + ColorWeightR * (*p++))] += 1;
					p++;
					break;
				default:
					return null;
			}
			#endregion

			#region Get Max and Min
			// Find the min dark color.
			Integral = 0;
			for (n = 0; n <= 255; n++)
			{
				Integral += Histogram[n];
				if (Integral >= Threshold)
				{
					Min = n;
					break;
				}
			}

			// Find the max light color.
			Integral = 0;
			for (n = 255; n >= 0; n--)
			{
				Integral += Histogram[n];
				if (Integral > Threshold)
				{
					Max = n;
					break;
				}
			}
			#endregion

			#region Get color map.
			for (n = 0; n <= 255; n++)
			{
				if (n <= Min)
				{
					ColorOut[n] = 0;
				}
				else if (n >= Max)
				{
					ColorOut[n] = 255;
				}
				else
				{
					ColorOut[n] = (byte)((n - Min) * 255 / (Max - Min));
				}
			}
			#endregion

			#region Save
			p = (byte*)wb.BackBuffer;
			wb.Lock();
			switch (wb.Format.BitsPerPixel)
			{
				case 8:
					for (n = 0; n < AllPixels; n++)
						*p = ColorOut[*p++];
					break;
				case 24:
					for (n = 0; n < AllPixels; n++)
					{
						*p = ColorOut[*p++];
						*p = ColorOut[*p++];
						*p = ColorOut[*p++];
					}
					break;
				case 32:
					for (n = 0; n < AllPixels; n++)
					{
						*p = ColorOut[*p++];
						*p = ColorOut[*p++];
						*p = ColorOut[*p++];
						p++;
					}
					break;
				default:
					return null;
			}
			wb.Unlock();
			#endregion
			
			return wb;
		}

		public static BitmapSource LayWhiteBg(BitmapSource bmp)
		{
			DrawingVisual dv = new DrawingVisual();
			DrawingContext dc = dv.RenderOpen();
			Rect rect = new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
			dc.DrawRectangle(Brushes.White, null, rect);
			dc.PushOpacityMask(Brushes.Black);
			dc.DrawImage(bmp, rect);
			dc.Close();
			RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default);
			rtb.Render(dv);
			return rtb;
		}
	}
}
