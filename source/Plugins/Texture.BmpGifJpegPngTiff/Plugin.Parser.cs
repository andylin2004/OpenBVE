using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenBveApi.Colors;
using OpenBveApi.Textures;
using OpenBveApi.Hosts;

namespace Plugin {
	public partial class Plugin : TextureInterface {
		
		/// <summary>Loads a texture from the specified file.</summary>
		/// <param name="file">The file that holds the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		private bool Parse(string file, out Texture texture) {
			/*
			 * Read the bitmap. This will be a bitmap of just
			 * any format, not necessarily the one that allows
			 * us to extract the bitmap data easily.
			 * */

			using (var image = Image.FromFile(file))
			{
				if (image.RawFormat.Equals(ImageFormat.Gif))
				{
					List<Bitmap> frames = new List<Bitmap>();

					if (ImageAnimator.CanAnimate(image))
					{
						var dimension = new FrameDimension(image.FrameDimensionsList[0]);
						int frameCount = image.GetFrameCount(dimension);

						int index = 0;
						int duration = 0;
						for (int i = 0; i < frameCount; i++)
						{
							image.SelectActiveFrame(dimension, i);
							frames.Add(new Bitmap(image));

							var delay = BitConverter.ToInt32(image.GetPropertyItem(20736).Value, index) * 10;
							duration += (delay < 100 ? 100 : delay);

							index += 4;
						}

						int numFrames = duration / frameCount;
						List<byte[]> frameBytes = new List<byte[]>();
						for (int i = 0; i < frames.Count; i++)
						{
							Color24[] p; //unused here, but don't clone the method- BVE2 had no support for animted gif
							frameBytes.Add(GetRawBitmapData(frames[i], out p));
						}

						texture = new Texture(frames[0].Width, frames[0].Height, 32, frameBytes.ToArray(), ((double)duration / frameCount) / 10000000.0, frameCount);
						return true;
					}
				}
			}

			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(file);
			Color24[] pallete;
			byte[] raw = GetRawBitmapData(bitmap, out pallete);
			if (raw != null)
			{
				texture = new Texture(bitmap.Width, bitmap.Height, 32, raw, pallete);
				return true;
			}
			else
			{
				texture = null;
				return false;
			}
		}

		private byte[] GetRawBitmapData(Bitmap bitmap, out Color24[] p)
		{
			p = null;
			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb && bitmap.PixelFormat != PixelFormat.Format24bppRgb)
			{
				/* Only store the color palette data for
				 * textures using a restricted palette
				 * With a large number of textures loaded at
				 * once, this can save a decent chunk of memory
				 * */
				p = new Color24[bitmap.Palette.Entries.Length];
				for (int i = 0; i < bitmap.Palette.Entries.Length; i++)
				{
					p[i] = bitmap.Palette.Entries[i];
				}
			}
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			/* 
			 * If the bitmap format is not already 32-bit BGRA,
			 * then convert it to 32-bit BGRA.
			 * */
			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb) {
				Bitmap compatibleBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
				Graphics graphics = Graphics.FromImage(compatibleBitmap);
				graphics.DrawImage(bitmap, rect, rect, GraphicsUnit.Pixel);
				graphics.Dispose();
				bitmap.Dispose();
				bitmap = compatibleBitmap;
			}
			/*
			 * Extract the raw bitmap data.
			 * */
			BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
			if (data.Stride == 4 * data.Width) {
				/*
				 * Copy the data from the bitmap
				 * to the array in BGRA format.
				 * */
				byte[] raw = new byte[data.Stride * data.Height];
				System.Runtime.InteropServices.Marshal.Copy(data.Scan0, raw, 0, data.Stride * data.Height);
				bitmap.UnlockBits(data);
				int width = bitmap.Width;
				int height = bitmap.Height;
				
				/*
				 * Change the byte order from BGRA to RGBA.
				 * */
				for (int i = 0; i < raw.Length; i += 4) {
					byte temp = raw[i];
					raw[i] = raw[i + 2];
					raw[i + 2] = temp;
				}

				return raw;
			} else {
				/*
				 * The stride is invalid. This indicates that the
				 * CLI either does not implement the conversion to
				 * 32-bit BGRA correctly, or that the CLI has
				 * applied additional padding that we do not
				 * support.
				 * */
				bitmap.UnlockBits(data);
				bitmap.Dispose();
				CurrentHost.ReportProblem(ProblemType.InvalidOperation, "Invalid stride encountered.");
				return null;
			}
		}
		
	}
}
