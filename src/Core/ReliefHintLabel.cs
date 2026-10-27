using System.Drawing;
using System.Windows.Forms;

namespace AccessShot;

internal sealed class ReliefHintLabel : Label
{
	protected override void OnPaint(PaintEventArgs e)
	{
		TextRenderer.DrawText(e.Graphics, Text, Font, new Point(2, 4), Color.FromArgb(95, 95, 100), TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
		TextRenderer.DrawText(e.Graphics, Text, Font, new Point(0, 2), Color.FromArgb(115, 115, 125), TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
		TextRenderer.DrawText(e.Graphics, Text, Font, new Point(1, 3), ForeColor, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
	}
}
