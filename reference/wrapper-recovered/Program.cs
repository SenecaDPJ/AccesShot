using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

internal static class Program
{
	private struct NativeRect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private struct NativePoint
	{
		public int X;

		public int Y;
	}

	private delegate bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam);

	private const uint SwpNoSize = 1u;

	private const uint SwpNoActivate = 16u;

	private const uint WmSetIcon = 128u;

	private const uint ImageIcon = 1u;

	private const uint LrLoadFromFile = 16u;

	private const uint WmGetText = 13u;

	private const uint WmGetTextLength = 14u;

	private const uint SmtoAbortIfHung = 2u;

	private static readonly IntPtr HwndTop = IntPtr.Zero;

	private static IntPtr bigIcon;

	private static IntPtr smallIcon;

	private static IntPtr iconWindow;

	[STAThread]
	private static void Main()
	{
		string baseDirectory = AppContext.BaseDirectory;
		string text = Path.Combine(baseDirectory, "AccessShotCore.exe");
		if (!File.Exists(text))
		{
			return;
		}
		Process process;
		try
		{
			process = Process.Start(new ProcessStartInfo
			{
				FileName = text,
				WorkingDirectory = baseDirectory,
				UseShellExecute = true
			});
		}
		catch
		{
			return;
		}
		if (process == null)
		{
			return;
		}
		using (process)
		{
			try
			{
				while (true)
				{
					try
					{
						if (process.HasExited)
						{
							break;
						}
					}
					catch
					{
						break;
					}
					IntPtr intPtr = FindMainWindow(process.Id);
					if (intPtr != IntPtr.Zero)
					{
						ApplyWindowIcon(intPtr, baseDirectory);
						KeepOverflowNoticeInCanvas(intPtr, process.Id);
					}
					Thread.Sleep(200);
				}
			}
			finally
			{
				ReleaseIcons();
			}
		}
	}

	private static IntPtr FindMainWindow(int processId)
	{
		IntPtr result = IntPtr.Zero;
		EnumWindowsCallback callback = delegate(IntPtr hWnd, IntPtr _)
		{
			if (!IsWindowVisible(hWnd))
			{
				return true;
			}
			GetWindowThreadProcessId(hWnd, out var processId2);
			if (processId2 != (uint)processId)
			{
				return true;
			}
			string text = ReadText(hWnd);
			if (!text.Contains("AccessShot", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (text.Contains("AccessShot 1.9", StringComparison.OrdinalIgnoreCase))
			{
				SetWindowText(hWnd, text.Replace("AccessShot 1.9", "AccessShot 2.12", StringComparison.OrdinalIgnoreCase));
			}
			result = hWnd;
			return false;
		};
		EnumWindows(callback, IntPtr.Zero);
		return result;
	}

	private static void ApplyWindowIcon(IntPtr main, string root)
	{
		if (iconWindow == main && (bigIcon != IntPtr.Zero || smallIcon != IntPtr.Zero))
		{
			return;
		}
		string text = Path.Combine(root, "assets", "AccessShot.ico");
		int systemMetrics = GetSystemMetrics(11);
		int systemMetrics2 = GetSystemMetrics(12);
		int systemMetrics3 = GetSystemMetrics(49);
		int systemMetrics4 = GetSystemMetrics(50);
		if (File.Exists(text))
		{
			if (bigIcon == IntPtr.Zero)
			{
				bigIcon = LoadImage(IntPtr.Zero, text, 1u, systemMetrics, systemMetrics2, 16u);
			}
			if (smallIcon == IntPtr.Zero)
			{
				smallIcon = LoadImage(IntPtr.Zero, text, 1u, systemMetrics3, systemMetrics4, 16u);
			}
		}
		if (bigIcon != IntPtr.Zero)
		{
			SendMessage(main, 128u, new IntPtr(1), bigIcon);
		}
		if (smallIcon != IntPtr.Zero)
		{
			SendMessage(main, 128u, IntPtr.Zero, smallIcon);
		}
		if (bigIcon != IntPtr.Zero || smallIcon != IntPtr.Zero)
		{
			iconWindow = main;
		}
	}

	private static void ReleaseIcons()
	{
		if (smallIcon != IntPtr.Zero && smallIcon != bigIcon)
		{
			DestroyIcon(smallIcon);
		}
		if (bigIcon != IntPtr.Zero)
		{
			DestroyIcon(bigIcon);
		}
		smallIcon = IntPtr.Zero;
		bigIcon = IntPtr.Zero;
		iconWindow = IntPtr.Zero;
	}

	private static void KeepOverflowNoticeInCanvas(IntPtr main, int processId)
	{
		if (!GetClientRect(main, out var rect))
		{
			return;
		}
		int num = rect.Right - rect.Left;
		int num2 = rect.Bottom - rect.Top;
		if (num <= 0 || num2 <= 0 || IsIconic(main))
		{
			return;
		}
		List<IntPtr> children = new List<IntPtr>();
		EnumWindowsCallback callback = delegate(IntPtr hWnd, IntPtr _)
		{
			children.Add(hWnd);
			return true;
		};
		EnumChildWindows(main, callback, IntPtr.Zero);
		IntPtr intPtr = FindCanvas(main, children, num, num2);
		if (intPtr == IntPtr.Zero || !GetClientRect(intPtr, out var rect2))
		{
			return;
		}
		int num3 = rect2.Right - rect2.Left;
		int num4 = rect2.Bottom - rect2.Top;
		if (num3 > 0 && num4 > 0)
		{
			NativePoint point = new NativePoint
			{
				X = 0,
				Y = 0
			};
			NativePoint point2 = new NativePoint
			{
				X = rect2.Right,
				Y = rect2.Bottom
			};
			if (ClientToScreen(intPtr, ref point) && ClientToScreen(intPtr, ref point2))
			{
				IntPtr panel = FindOverflowNotice(main, processId, children, intPtr, point, point2, num3, num4);
				PlacePanelInCanvas(main, panel, point, point2, num3, num4, alignRight: true, alignBottom: false);
			}
		}
	}

	private static IntPtr FindOverflowNotice(IntPtr main, int processId, List<IntPtr> mainChildren, IntPtr canvas, NativePoint canvasTopLeft, NativePoint canvasBottomRight, int canvasWidth, int canvasHeight)
	{
		List<IntPtr> candidates = new List<IntPtr>(mainChildren);
		EnumWindowsCallback callback = delegate(IntPtr hWnd, IntPtr _)
		{
			if (hWnd == main || !IsWindowVisible(hWnd))
			{
				return true;
			}
			GetWindowThreadProcessId(hWnd, out var processId2);
			if (processId2 != (uint)processId)
			{
				return true;
			}
			candidates.Add(hWnd);
			EnumChildWindows(hWnd, delegate(IntPtr child, IntPtr intPtr4)
			{
				candidates.Add(child);
				return true;
			}, IntPtr.Zero);
			return true;
		};
		EnumWindows(callback, IntPtr.Zero);
		IntPtr intPtr = IntPtr.Zero;
		long num = long.MaxValue;
		foreach (IntPtr item in candidates)
		{
			string text = ReadClassName(item);
			if (!text.Contains("BUTTON", StringComparison.OrdinalIgnoreCase) || !GetWindowRect(item, out var rect))
			{
				continue;
			}
			int num2 = rect.Right - rect.Left;
			int num3 = rect.Bottom - rect.Top;
			if (num2 < 18 || num2 > 64 || num3 < 18 || num3 > 64)
			{
				continue;
			}
			IntPtr parent = GetParent(item);
			if (parent == IntPtr.Zero || parent == main || parent == canvas || !GetWindowRect(parent, out var rect2))
			{
				continue;
			}
			int num4 = rect2.Right - rect2.Left;
			int num5 = rect2.Bottom - rect2.Top;
			if (num4 < 180 || num5 < 35 || num4 > canvasWidth + 24 || num5 > Math.Max(180, canvasHeight / 2 + 24))
			{
				continue;
			}
			int num6 = rect2.Right - rect.Right;
			int num7 = canvasBottomRight.Y - rect2.Bottom;
			bool flag = num6 >= -3 && num6 <= Math.Max(18, num4 / 10);
			bool flag2 = rect2.Right >= canvasBottomRight.X - Math.Max(48, canvasWidth / 12);
			bool flag3 = rect2.Bottom >= canvasBottomRight.Y - Math.Max(45, canvasHeight / 10);
			if (flag && flag2 && flag3)
			{
				long num8 = Math.Abs((long)num6) + Math.Abs((long)num7) * 2;
				if (num8 < num)
				{
					intPtr = parent;
					num = num8;
				}
			}
		}
		if (intPtr != IntPtr.Zero)
		{
			return intPtr;
		}
		foreach (IntPtr item2 in candidates)
		{
			string text2 = ReadText(item2);
			if (!text2.Contains("fuera de la vista inicial", StringComparison.OrdinalIgnoreCase) && !text2.Contains("fuera de pantalla", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			IntPtr intPtr2 = item2;
			IntPtr intPtr3 = IntPtr.Zero;
			long num9 = long.MaxValue;
			for (int num10 = 0; num10 < 12; num10++)
			{
				if (intPtr2 == IntPtr.Zero)
				{
					break;
				}
				if (intPtr2 == main)
				{
					break;
				}
				if (intPtr2 != canvas && GetWindowRect(intPtr2, out var rect3))
				{
					int num11 = rect3.Right - rect3.Left;
					int num12 = rect3.Bottom - rect3.Top;
					long num13 = (long)num11 * (long)num12;
					if (num11 >= 160 && num12 >= 55 && num11 <= canvasWidth + 24 && num12 <= canvasHeight / 2 + 24 && num13 < num9)
					{
						intPtr3 = intPtr2;
						num9 = num13;
						if (HasDismissControl(intPtr2))
						{
							return intPtr2;
						}
					}
				}
				intPtr2 = GetParent(intPtr2);
			}
			if (intPtr3 != IntPtr.Zero)
			{
				return intPtr3;
			}
		}
		return IntPtr.Zero;
	}

	private static string ReadClassName(IntPtr hWnd)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		GetClassName(hWnd, stringBuilder, stringBuilder.Capacity);
		return stringBuilder.ToString();
	}

	private static bool HasDismissControl(IntPtr panel)
	{
		bool found = false;
		EnumChildWindows(panel, delegate(IntPtr child, IntPtr _)
		{
			string text = ReadText(child).Trim();
			bool flag;
			switch (text)
			{
			case "×":
			case "✕":
			case "x":
			case "X":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag || text.Equals("cerrar", StringComparison.OrdinalIgnoreCase) || text.Equals("close", StringComparison.OrdinalIgnoreCase))
			{
				found = true;
				return false;
			}
			return true;
		}, IntPtr.Zero);
		return found;
	}

	private static void PlacePanelInCanvas(IntPtr main, IntPtr panel, NativePoint canvasTopLeft, NativePoint canvasBottomRight, int canvasWidth, int canvasHeight, bool alignRight, bool alignBottom)
	{
		if (panel == IntPtr.Zero || !GetWindowRect(panel, out var rect))
		{
			return;
		}
		int num = Math.Max(1, rect.Right - rect.Left);
		int num2 = Math.Max(1, rect.Bottom - rect.Top);
		int num3 = Math.Min(num, Math.Max(1, canvasWidth - 24));
		int num4 = Math.Min(num2, Math.Max(1, canvasHeight - 24));
		int num5 = (alignRight ? Math.Max(canvasTopLeft.X + 12, canvasBottomRight.X - num3 - 12) : (canvasTopLeft.X + 12));
		int num6 = (alignBottom ? Math.Max(canvasTopLeft.Y + 12, canvasBottomRight.Y - num4 - 12) : (canvasTopLeft.Y + 12));
		int left = rect.Left;
		int top = rect.Top;
		if (Math.Abs(left - num5) < 2 && Math.Abs(top - num6) < 2 && num == num3 && num2 == num4)
		{
			return;
		}
		int x = num5;
		int y = num6;
		if (IsChild(main, panel))
		{
			IntPtr parent = GetParent(panel);
			if (parent == IntPtr.Zero)
			{
				return;
			}
			NativePoint point = new NativePoint
			{
				X = x,
				Y = y
			};
			if (!ScreenToClient(parent, ref point))
			{
				return;
			}
			x = point.X;
			y = point.Y;
		}
		SetWindowPos(panel, HwndTop, x, y, num3, num4, 16u);
	}

	private static IntPtr FindCanvas(IntPtr main, List<IntPtr> children, int formWidth, int formHeight)
	{
		IntPtr result = IntPtr.Zero;
		long num = 0L;
		foreach (IntPtr child in children)
		{
			if (!IsWindowVisible(child) || !GetWindowRect(child, out var rect))
			{
				continue;
			}
			NativePoint point = new NativePoint
			{
				X = rect.Left,
				Y = rect.Top
			};
			NativePoint point2 = new NativePoint
			{
				X = rect.Right,
				Y = rect.Bottom
			};
			if (!ScreenToClient(main, ref point) || !ScreenToClient(main, ref point2))
			{
				continue;
			}
			int num2 = point2.X - point.X;
			int num3 = point2.Y - point.Y;
			if (!((double)num2 < (double)formWidth * 0.65) && !((double)num3 < (double)formHeight * 0.2) && !((double)point.X > (double)formWidth * 0.12) && !((double)point2.X < (double)formWidth * 0.88) && point.Y >= 10 && !((double)point.Y > (double)formHeight * 0.75) && !((double)point2.Y < (double)formHeight * 0.55))
			{
				long num4 = (long)num2 * (long)num3;
				if (num4 > num)
				{
					result = child;
					num = num4;
				}
			}
		}
		return result;
	}

	private static string ReadText(IntPtr hWnd)
	{
		if (hWnd == IntPtr.Zero)
		{
			return string.Empty;
		}
		if (SendMessageTimeout(hWnd, 14u, IntPtr.Zero, IntPtr.Zero, 2u, 1000u, out var result) != IntPtr.Zero)
		{
			int num = Math.Min(2048, Math.Max(0, result.ToInt32()));
			if (num > 0)
			{
				StringBuilder stringBuilder = new StringBuilder(num + 1);
				if (SendMessageTimeout(hWnd, 13u, new IntPtr(stringBuilder.Capacity), stringBuilder, 2u, 1000u, out var _) != IntPtr.Zero)
				{
					return stringBuilder.ToString();
				}
			}
		}
		int num2 = Math.Min(1024, Math.Max(0, GetWindowTextLength(hWnd)));
		StringBuilder stringBuilder2 = new StringBuilder(Math.Max(2, num2 + 1));
		GetWindowText(hWnd, stringBuilder2, stringBuilder2.Capacity);
		return stringBuilder2.ToString();
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool EnumChildWindows(IntPtr parent, EnumWindowsCallback callback, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int GetWindowTextLength(IntPtr hWnd);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SendMessageTimeoutW", SetLastError = true)]
	private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SendMessageTimeoutW", SetLastError = true)]
	private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint message, IntPtr wParam, StringBuilder lParam, uint flags, uint timeout, out IntPtr result);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool SetWindowText(IntPtr hWnd, string text);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr GetParent(IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool IsChild(IntPtr parent, IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool IsIconic(IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect rect);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetClientRect(IntPtr hWnd, out NativeRect rect);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool ClientToScreen(IntPtr hWnd, ref NativePoint point);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool ScreenToClient(IntPtr hWnd, ref NativePoint point);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr LoadImage(IntPtr instance, string name, uint type, int cx, int cy, uint flags);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SendMessage(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int GetSystemMetrics(int index);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool DestroyIcon(IntPtr icon);
}
