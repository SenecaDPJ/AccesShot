using System.Drawing;
using System.Windows.Forms;

namespace AccessShot;

internal sealed class ActionGroup : Panel
{
	private readonly Label captionLabel;

	public FlowLayoutPanel Buttons { get; }

	public string SpanishCaption { get; }

	public string Caption
	{
		get => captionLabel.Text;
		set => captionLabel.Text = value;
	}

	public ActionGroup(string title, int buttonCount)
	{
		int width = buttonCount * 126;
		base.Size = new Size(width, 140);
		base.Margin = new Padding(0, 0, 12, 0);
		BackColor = Color.Black;
		SpanishCaption = title;
		captionLabel = new Label
		{
			Text = title,
			Location = new Point(0, 114),
			Size = new Size(width, 22),
			TextAlign = ContentAlignment.MiddleCenter,
			ForeColor = Color.FromArgb(170, 180, 194),
			BackColor = Color.Black,
			Font = new Font("Segoe UI", 9f, FontStyle.Bold)
		};
		Buttons = new FlowLayoutPanel
		{
			Location = new Point(0, 0),
			Size = new Size(width, 112),
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			AutoSize = false,
			Padding = Padding.Empty,
			Margin = Padding.Empty,
			BackColor = Color.Black
		};
		base.Controls.Add(captionLabel);
		base.Controls.Add(Buttons);
	}
}
