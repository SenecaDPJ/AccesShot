using System;

namespace AccessShot;

[Flags]
internal enum ResizeEdge
{
	None = 0,
	Left = 1,
	Right = 2,
	Top = 4,
	Bottom = 8
}
