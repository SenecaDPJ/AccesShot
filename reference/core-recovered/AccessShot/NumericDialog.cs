using System;
using System.Windows.Forms;

namespace AccessShot;

internal sealed class NumericDialog : Form
{
	private readonly NumericUpDown number;

	public int Value => (int)number.Value;

	public NumericDialog(string title, string prompt, int value, int min, int max)
	{
		Text = title;
		base.Width = 390;
		base.Height = 170;
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Controls.Add(new Label
		{
			Text = prompt,
			Left = 16,
			Top = 23,
			AutoSize = true
		});
		number = new NumericUpDown
		{
			Left = 205,
			Top = 18,
			Width = 150,
			Minimum = min,
			Maximum = max,
			Value = Math.Clamp(value, min, max)
		};
		base.Controls.Add(number);
		Button button = new Button
		{
			Text = "Aceptar",
			Left = 180,
			Top = 70,
			Width = 80,
			DialogResult = DialogResult.OK
		};
		Button button2 = new Button
		{
			Text = "Cancelar",
			Left = 275,
			Top = 70,
			Width = 80,
			DialogResult = DialogResult.Cancel
		};
		base.Controls.Add(button);
		base.Controls.Add(button2);
		base.AcceptButton = button;
		base.CancelButton = button2;
	}
}
