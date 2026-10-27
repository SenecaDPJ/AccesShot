using System;
using System.Windows.Forms;
using AccessShot;

internal static class CanvasLayerMenu
{
	public static void Attach(ContextMenuStrip P_0, MainForm P_1, Entry P_2, Control P_3)
	{
		//Error decoding local variables: Signature type sequence must have at least one element.
		AddChoice(P_0, P_1, P_2, P_3, "MOVER ADELANTE", 0);
		AddChoice(P_0, P_1, P_2, P_3, "MOVER ATRAS", 1);
		AddChoice(P_0, P_1, P_2, P_3, "PONER EL 1º", 2);
		AddChoice(P_0, P_1, P_2, P_3, "PONER EL ÚLTIMO", 3);
	}

	public static void AddChoice(ContextMenuStrip P_0, MainForm P_1, Entry P_2, Control P_3, string P_4, int P_5)
	{
		ToolStripItem toolStripItem = P_0.Items.Add(P_4, null, LayerClicked);
		toolStripItem.Tag = new object[4] { P_1, P_2, P_3, P_5 };
	}

	public static void LayerClicked(object P_0, EventArgs P_1)
	{
		object[] array = (object[])((ToolStripItem)P_0).Tag;
		MainForm mainForm = (MainForm)array[0];
		Entry item = (Entry)array[1];
		Control control = (Control)array[2];
		int num = (int)array[3];
		int num2 = mainForm.entries.IndexOf(item);
		if (num2 >= 0)
		{
			int num3 = mainForm.entries.Count - 1;
			int num4 = Math.Clamp(num switch
			{
				0 => num2 - 1,
				2 => 0,
				3 => num3,
				_ => num2 + 1,
			}, 0, num3);
			if (num2 != num4)
			{
				mainForm.entries.RemoveAt(num2);
				mainForm.entries.Insert(num4, item);
				control.Parent.Controls.SetChildIndex(control, num4);
				control.Parent.Invalidate();
				mainForm.Save();
			}
		}
	}
}
