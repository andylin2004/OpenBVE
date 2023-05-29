using System;

namespace LibRender2.Screens
{
	/// <summary>A screen resolution</summary>
	public class ScreenResolution
	{
		/// <summary>The width</summary>
		public readonly int Width;
		/// <summary>The height</summary>
		public readonly int Height;
		/// <summary>
		/// The scaling of the width
		/// </summary>
		public readonly float ScaledWidth;
		/// <summary>
		/// The scaling of the height
		/// </summary>
		public readonly float ScaledHeight;

		public ScreenResolution(int width, int height)
		{
			Width = width;
			Height = height;
			ScaledWidth = 1;
			ScaledHeight = 1;
		}

		public ScreenResolution(int width, int height, float scaledWidth, float scaledHeight)
		{
			Width = width;
			Height = height;
			ScaledWidth = scaledWidth;
			ScaledHeight = scaledHeight;
		}

		public override string ToString()
		{
			if (ScaledWidth == ScaledHeight)
			{
				return String.Format("{0} x {1} @ {2}x", Width, Height, ScaledHeight);
			} else
			{
				return String.Format("{0}@{1}x x {2}@{3}x", Width, ScaledWidth, Height, ScaledHeight);
			}
			
		}
	}
}
