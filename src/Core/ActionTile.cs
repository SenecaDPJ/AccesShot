using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AccessShot;

internal sealed class ActionTile : Control
{
	private readonly string title;

	private string normalGlyph;

	private string hoverGlyph;

	private bool hovered;

	private string? heldGlyph;

	private readonly Color activeAccent;

	private readonly Color hoverAccent;

	private bool active;

	public Color Accent { get; }

	public bool Active
	{
		get
		{
			return active;
		}
		set
		{
			if (active != value)
			{
				active = value;
				Invalidate();
			}
		}
	}

	public string NormalGlyph
	{
		get
		{
			return normalGlyph;
		}
		set
		{
			normalGlyph = value;
			Invalidate();
		}
	}

	public string HoverGlyph
	{
		get
		{
			return hoverGlyph;
		}
		set
		{
			hoverGlyph = value;
			Invalidate();
		}
	}

	public bool HoldCurrentGlyphUntilLeave
	{
		get
		{
			return heldGlyph != null;
		}
		set
		{
			heldGlyph = (value ? NormalGlyph : null);
			Invalidate();
		}
	}

	public event EventHandler? Activated;

	public ActionTile(string text, string glyph, string hotGlyph, Color accent, bool toggle, Color activeColor, Color hoverColor)
	{
		title = text;
		normalGlyph = glyph;
		hoverGlyph = hotGlyph;
		Accent = accent;
		activeAccent = activeColor;
		hoverAccent = hoverColor;
		base.Size = new Size(112, 108);
		base.Margin = new Padding(7, 2, 7, 2);
		Cursor = Cursors.Hand;
		base.AccessibleName = title;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		base.Click += delegate
		{
			this.Activated?.Invoke(this, EventArgs.Empty);
		};
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
		heldGlyph = null;
		Invalidate();
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button == MouseButtons.Left)
		{
			Focus();
			Invalidate();
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		Invalidate();
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
		Rectangle rect = new Rectangle(1, 1, base.Width - 3, base.Height - 3);
		bool flag = hovered || Active;
		Color color = (Active ? activeAccent : (hovered ? hoverAccent : Accent));
		using GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(rect);
		using (SolidBrush brush = new SolidBrush(flag ? color : Color.Black))
		{
			graphics.FillPath(brush, graphicsPath);
		}
		using (Pen pen = new Pen(color, Active ? 2.5f : 1.2f))
		{
			graphics.DrawPath(pen, graphicsPath);
		}
		Color color2 = (flag ? Color.Black : color);
		string s = ((hovered && heldGlyph != null) ? heldGlyph : (hovered ? hoverGlyph : normalGlyph));
		using (Font font = new Font("Segoe UI Symbol", 36f, FontStyle.Regular, GraphicsUnit.Point))
		{
			using SolidBrush brush2 = new SolidBrush(color2);
			using StringFormat format = new StringFormat
			{
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center
			};
			graphics.DrawString(s, font, brush2, new RectangleF(5f, 2f, base.Width - 10, 64f), format);
		}
		using (Font font2 = new Font("Segoe UI", 9f, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point))
		{
			using SolidBrush brush3 = new SolidBrush(flag ? Color.Black : color);
			using StringFormat format2 = new StringFormat
			{
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center,
				Trimming = StringTrimming.EllipsisCharacter
			};
			graphics.DrawString(title, font2, brush3, new RectangleF(3f, base.Height - 34, base.Width - 6, 27f), format2);
		}
		if (Focused)
		{
			using (Pen pen2 = new Pen(Color.White, 1f))
			{
				pen2.DashStyle = DashStyle.Dot;
				graphics.DrawRectangle(pen2, 4, 4, base.Width - 9, base.Height - 9);
				return;
			}
		}
	}
}
