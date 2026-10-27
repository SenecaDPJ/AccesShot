using System.Drawing;

namespace AccessShot;

internal sealed class Entry
{
	public EntryKind Kind { get; set; }

	public string Title { get; set; } = "";

	public string Target { get; set; } = "";

	public int X { get; set; }

	public int Y { get; set; }

	public int Width { get; set; } = 160;

	public int Height { get; set; } = 78;

	public int FontSize { get; set; } = 20;

	public int Background { get; set; } = Color.FromArgb(45, 60, 82).ToArgb();

	public int Foreground { get; set; } = Color.White.ToArgb();
}
