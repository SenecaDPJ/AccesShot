using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AccessShot;

internal sealed class EntryTile : Control
{
	private readonly Entry item;

	private readonly bool dark;

	public bool Selected { get; set; }

	public bool Dragging { get; set; }

	public bool Resizing { get; set; }

	public Point ScreenStart { get; set; }

	public Rectangle StartBounds { get; set; }

	public ResizeEdge ResizeEdges { get; set; }

	public Entry Item => item;

	public bool SelectionOnlyClick { get; set; }

	public EntryTile(Entry entry, bool isDark)
	{
		item = entry;
		dark = isDark;
		base.Size = (entry.Orientation % 180 == 0)
			? new Size(Math.Max(90, entry.Width), Math.Max(50, entry.Height))
			: new Size(Math.Max(50, entry.Height), Math.Max(90, entry.Width));
		Cursor = Cursors.Hand;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		AccessShotVisuals.Paint(this, e, item, dark, Selected);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		Invalidate();
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (e.Button == MouseButtons.None)
		{
			Cursor = CursorFor(GetResizeEdges(e.Location));
		}
	}

	public ResizeEdge GetResizeEdges(Point point)
	{
		ResizeEdge resizeEdge = ResizeEdge.None;
		if (point.X <= 8)
		{
			resizeEdge |= ResizeEdge.Left;
		}
		else if (point.X >= base.Width - 8)
		{
			resizeEdge |= ResizeEdge.Right;
		}
		if (point.Y <= 8)
		{
			resizeEdge |= ResizeEdge.Top;
		}
		else if (point.Y >= base.Height - 8)
		{
			resizeEdge |= ResizeEdge.Bottom;
		}
		return resizeEdge;
	}

	private static Cursor CursorFor(ResizeEdge edge)
	{
		if (edge == (ResizeEdge.Left | ResizeEdge.Top) || edge == (ResizeEdge.Right | ResizeEdge.Bottom))
		{
			return Cursors.SizeNWSE;
		}
		if (edge == (ResizeEdge.Right | ResizeEdge.Top) || edge == (ResizeEdge.Left | ResizeEdge.Bottom))
		{
			return Cursors.SizeNESW;
		}
		if ((uint)(edge - 1) <= 1u)
		{
			return Cursors.SizeWE;
		}
		if ((edge == ResizeEdge.Top || edge == ResizeEdge.Bottom) ? true : false)
		{
			return Cursors.SizeNS;
		}
		return Cursors.Hand;
	}

	internal static GraphicsPath RoundRect(Rectangle r, int radius)
	{
		int num = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddArc(r.X, r.Y, num, num, 180f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Y, num, num, 270f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Bottom - num, num, num, 0f, 90f);
		graphicsPath.AddArc(r.X, r.Bottom - num, num, num, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}
}
