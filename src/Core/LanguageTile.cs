using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AccessShot;

internal sealed class LanguageTile : Control
{
	private const int DividerColor = unchecked((int)0xFF087DFF);
	private bool hovered;
	private bool isEnglish;

	public bool IsEnglish
	{
		get => isEnglish;
		set
		{
			if (isEnglish == value) return;
			isEnglish = value;
			UpdateAccessibleText();
			Invalidate();
		}
	}

	public event EventHandler? Activated;

	public LanguageTile()
	{
		Size = new Size(36, 108);
		Margin = new Padding(0, 2, 8, 2);
		Cursor = Cursors.Hand;
		TabStop = true;
		AccessibleRole = AccessibleRole.PushButton;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
		UpdateAccessibleText();
		Click += (_, e) => Activated?.Invoke(this, e);
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
		Invalidate();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.KeyCode is Keys.Enter or Keys.Space)
		{
			Activated?.Invoke(this, EventArgs.Empty);
			e.Handled = true;
			e.SuppressKeyPress = true;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;

		Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
		using (SolidBrush background = new SolidBrush(hovered ? Color.FromArgb(24, 50, 82) : Color.Black))
			graphics.FillRectangle(background, bounds);
		using (Pen border = new Pen(Color.FromArgb(DividerColor), 1.4f))
			graphics.DrawRectangle(border, bounds);

		int dividerY = Height - 26;
		using (Pen divider = new Pen(Color.FromArgb(DividerColor), 1f))
			graphics.DrawLine(divider, 1, dividerY, Width - 2, dividerY);

		GraphicsState textState = graphics.Save();
		graphics.TranslateTransform(Width / 2f, dividerY / 2f);
		graphics.RotateTransform(-90f);
		using (Font labelFont = new Font("Segoe UI", 8f, FontStyle.Bold, GraphicsUnit.Point))
		using (SolidBrush labelBrush = new SolidBrush(Color.FromArgb(DividerColor)))
		using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
			graphics.DrawString(isEnglish ? "LANGUAGE" : "IDIOMA", labelFont, labelBrush, new RectangleF(-dividerY / 2f + 3, -9, dividerY - 6, 18), format);
		graphics.Restore(textState);

		Rectangle flag = new Rectangle((Width - 22) / 2, dividerY + (Height - dividerY - 14) / 2, 22, 14);
		if (isEnglish) DrawUnitedKingdomFlag(graphics, flag);
		else DrawSpanishFlag(graphics, flag);
	}

	private void UpdateAccessibleText()
	{
		AccessibleName = isEnglish ? "Language: English" : "Idioma: Español";
		AccessibleDescription = isEnglish ? "Switch the application language to Spanish." : "Cambiar el idioma de la aplicación a inglés.";
	}

	private static void DrawSpanishFlag(Graphics graphics, Rectangle bounds)
	{
		using SolidBrush red = new SolidBrush(Color.FromArgb(170, 21, 27));
		using SolidBrush yellow = new SolidBrush(Color.FromArgb(241, 191, 0));
		graphics.FillRectangle(red, bounds);
		graphics.FillRectangle(yellow, bounds.X, bounds.Y + bounds.Height / 4, bounds.Width, bounds.Height / 2);
		using Pen outline = new Pen(Color.FromArgb(180, 210, 215, 225), 0.7f);
		graphics.DrawRectangle(outline, bounds);
	}

	private static void DrawUnitedKingdomFlag(Graphics graphics, Rectangle bounds)
	{
		GraphicsState state = graphics.Save();
		graphics.SetClip(bounds);
		graphics.FillRectangle(Brushes.Navy, bounds);
		using (Pen diagonalWhite = new Pen(Color.White, 5f))
		{
			graphics.DrawLine(diagonalWhite, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
			graphics.DrawLine(diagonalWhite, bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
		}
		using (Pen diagonalRed = new Pen(Color.Firebrick, 2f))
		{
			graphics.DrawLine(diagonalRed, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
			graphics.DrawLine(diagonalRed, bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
		}
		graphics.FillRectangle(Brushes.White, bounds.X + bounds.Width * 2 / 5, bounds.Y, bounds.Width / 5, bounds.Height);
		graphics.FillRectangle(Brushes.White, bounds.X, bounds.Y + bounds.Height * 2 / 5, bounds.Width, bounds.Height / 5);
		graphics.FillRectangle(Brushes.Firebrick, bounds.X + bounds.Width * 9 / 20, bounds.Y, bounds.Width / 10, bounds.Height);
		graphics.FillRectangle(Brushes.Firebrick, bounds.X, bounds.Y + bounds.Height * 9 / 20, bounds.Width, bounds.Height / 10);
		graphics.Restore(state);
		using Pen outline = new Pen(Color.FromArgb(180, 210, 215, 225), 0.7f);
		graphics.DrawRectangle(outline, bounds);
	}
}
