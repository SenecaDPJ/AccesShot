using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AccessShot;

public static class TileContourFix
{
	public static void ApplyContour(Control P_0, int P_1, Graphics P_2)
	{
		Region region = P_0.Region;
		if (region != null)
		{
			RectangleF bounds = region.GetBounds(P_2);
			if (bounds.Width == (float)P_0.Width && bounds.Height == (float)P_0.Height)
			{
				return;
			}
		}
		GraphicsPath graphicsPath = EntryTile.RoundRect(new Rectangle(0, 0, P_0.Width, P_0.Height), P_1);
		P_0.Region = new Region(graphicsPath);
		((IDisposable)graphicsPath).Dispose();
	}
}
