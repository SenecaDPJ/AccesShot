using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AccessShot;

internal sealed class GitHubPillButton : Control
{
	private bool hovered;

	private bool pressed;

	private Color accentColor = Color.Yellow;

	private Color surfaceColor = Color.FromArgb(20, 23, 30);

	public Color AccentColor
	{
		get
		{
			return accentColor;
		}
		set
		{
			accentColor = value;
			Invalidate();
		}
	}

	public Color SurfaceColor
	{
		get
		{
			return surfaceColor;
		}
		set
		{
			surfaceColor = value;
			Invalidate();
		}
	}

	public event EventHandler? Activated;

	public GitHubPillButton()
	{
		Cursor = Cursors.Hand;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		base.TabStop = true;
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		hovered = true;
		Invalidate();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		hovered = false;
		pressed = false;
		Invalidate();
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button == MouseButtons.Left)
		{
			pressed = true;
			Focus();
			Invalidate();
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		bool flag = pressed && e.Button == MouseButtons.Left && base.ClientRectangle.Contains(e.Location);
		pressed = false;
		Invalidate();
		if (flag)
		{
			this.Activated?.Invoke(this, EventArgs.Empty);
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		Keys keyCode = e.KeyCode;
		if ((keyCode == Keys.Return || keyCode == Keys.Space) ? true : false)
		{
			this.Activated?.Invoke(this, EventArgs.Empty);
			e.Handled = true;
			e.SuppressKeyPress = true;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		RectangleF layoutRectangle = new RectangleF(0.75f, 0.75f, (float)base.Width - 1.5f, (float)base.Height - 1.5f);
		using GraphicsPath graphicsPath = new GraphicsPath();
		float num = Math.Min(layoutRectangle.Height, layoutRectangle.Width);
		graphicsPath.AddArc(layoutRectangle.X, layoutRectangle.Y, num, num, 90f, 180f);
		graphicsPath.AddArc(layoutRectangle.Right - num, layoutRectangle.Y, num, num, 270f, 180f);
		graphicsPath.CloseFigure();
		bool flag = hovered || pressed;
		using (SolidBrush brush = new SolidBrush(flag ? accentColor : surfaceColor))
		{
			graphics.FillPath(brush, graphicsPath);
		}
		using (Pen pen = new Pen(accentColor, 1f))
		{
			graphics.DrawPath(pen, graphicsPath);
		}
		using Font font = new Font(Font.FontFamily, Font.Size, Font.Style, GraphicsUnit.Point);
		using SolidBrush brush2 = new SolidBrush(flag ? surfaceColor : accentColor);
		using StringFormat format = new StringFormat
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center,
			Trimming = StringTrimming.EllipsisCharacter
		};
		graphics.DrawString(Text, font, brush2, layoutRectangle, format);
		if (Focused)
		{
			using (Pen pen2 = new Pen(accentColor, 1f)
			{
				DashStyle = DashStyle.Dot
			})
			{
				graphics.DrawPath(pen2, graphicsPath);
				return;
			}
		}
	}
}
