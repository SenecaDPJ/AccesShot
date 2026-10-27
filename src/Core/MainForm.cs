using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Forms;

namespace AccessShot;

internal sealed class MainForm : Form
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass30_0
	{
		public Form dialog;

		public TextBox target;

		public TextBox title;

		public EntryKind kind;

		public Entry existing;

		public MainForm _003C_003E4__this;

		internal void _003CEditEntry_003Eb__1(object? sender, EventArgs args)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "Aplicaciones y accesos|*.exe;*.lnk;*.bat;*.cmd|Todos los archivos|*.*"
			};
			try
			{
				if (openFileDialog.ShowDialog(dialog) == DialogResult.OK)
				{
					target.Text = openFileDialog.FileName;
					if (string.IsNullOrWhiteSpace(title.Text))
					{
						title.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
					}
				}
			}
			finally
			{
				((IDisposable)(object)openFileDialog)?.Dispose();
			}
		}

		internal void _003CEditEntry_003Eb__0(object? sender, EventArgs args)
		{
			if (string.IsNullOrWhiteSpace(title.Text))
			{
				MessageBox.Show("Indica un nombre.");
				return;
			}
			if (kind == EntryKind.Application && (target == null || !AppTargets.Exists(target.Text)))
			{
				MessageBox.Show("Selecciona un archivo existente o una app instalada de Windows.");
				return;
			}
			if (kind == EntryKind.Url && (target == null || !Uri.TryCreate(target.Text.Trim(), UriKind.Absolute, out Uri result) || (result.Scheme != Uri.UriSchemeHttps && result.Scheme != Uri.UriSchemeHttp) || string.IsNullOrWhiteSpace(result.Host)))
			{
				MessageBox.Show("Introduce una dirección HTTP o HTTPS válida.");
				return;
			}
			Entry entry = existing ?? new Entry
			{
				Kind = kind,
				X = 24 + _003C_003E4__this.entries.Count % 6 * 170,
				Y = 24 + _003C_003E4__this.entries.Count / 6 * 90
			};
			entry.Title = title.Text.Trim();
			entry.Target = target?.Text.Trim() ?? "";
			if (existing == null)
			{
				_003C_003E4__this.entries.Add(entry);
			}
			_003C_003E4__this.selected = entry;
			_003C_003E4__this.Save();
			_003C_003E4__this.Render();
			dialog.DialogResult = DialogResult.OK;
			dialog.Close();
		}
	}

	internal readonly List<Entry> entries = new List<Entry>();

	private readonly Panel canvas = new Panel
	{
		Dock = DockStyle.Fill,
		AutoScroll = true
	};

	private readonly string dataFile = Path.Combine(Environment.GetEnvironmentVariable("ACCESS_SHOT_DATA_DIR") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccessShot"), "entries.json");

	private readonly FlowLayoutPanel toolbar = new FlowLayoutPanel
	{
		Dock = DockStyle.Top,
		AutoSize = true,
		WrapContents = true,
		FlowDirection = FlowDirection.LeftToRight,
		Padding = new Padding(8),
		BackColor = Color.Black
	};

	private readonly Panel offscreenNotice = new Panel
	{
		Size = new Size(420, 62),
		BackColor = Color.FromArgb(20, 25, 34),
		BorderStyle = BorderStyle.FixedSingle,
		Anchor = (AnchorStyles.Bottom | AnchorStyles.Right),
		Visible = false
	};

	private readonly Label offscreenText = new Label
	{
		Location = new Point(10, 4),
		Size = new Size(365, 50),
		ForeColor = Color.White,
		BackColor = Color.Transparent,
		Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
		TextAlign = ContentAlignment.MiddleLeft
	};

	private readonly Button closeNotice = new Button
	{
		Text = "×",
		Location = new Point(378, 12),
		Size = new Size(32, 32),
		FlatStyle = FlatStyle.Flat,
		ForeColor = Color.White,
		BackColor = Color.FromArgb(45, 55, 70),
		Font = new Font("Segoe UI", 15f, FontStyle.Bold),
		TabStop = false
	};

	private const string GitHubHomeUrl = "https://github.com/SenecaDPJ";

	private readonly Panel signaturePanel = new Panel
	{
		Size = new Size(260, 48),
		Anchor = (AnchorStyles.Bottom | AnchorStyles.Left),
		BackColor = Color.FromArgb(20, 23, 30)
	};

	private readonly Label signatureText = new Label
	{
		Text = "© David De Prado · 2026",
		Location = new Point(5, 1),
		Size = new Size(250, 18),
		TextAlign = ContentAlignment.MiddleCenter,
		Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
		BackColor = Color.Transparent
	};

	private readonly GitHubPillButton githubHomeButton = new GitHubPillButton
	{
		Text = "github.com/SenecaDPJ",
		Location = new Point(5, 23),
		Size = new Size(170, 20),
		Font = new Font("Segoe UI", 8f, FontStyle.Bold),
		AccessibleName = "Abrir HOME de GitHub de SenecaDPJ"
	};

	private Entry? selected;

	private bool dark = true;

	private bool moveMode;

	private bool deleteMode;

	private ActionTile? moveButton;

	private ActionTile? deleteModeButton;

	private ActionTile? themeButton;

	private int dismissedOffscreenCount = -1;

	public MainForm()
	{
		Text = "AccessShot 2.13";
		base.Width = 1120;
		base.Height = 760;
		MinimumSize = new Size(640, 480);
		base.StartPosition = FormStartPosition.CenterScreen;
		BackColor = Color.Black;
		ActionGroup actionGroup = new ActionGroup("CREAR", 3);
		ActionGroup actionGroup2 = new ActionGroup("MODOS", 3);
		ActionGroup actionGroup3 = new ActionGroup("OPCIONES", 2);
		AddAction(actionGroup, "APLICACIÓN", "▣", "⊞", Color.FromArgb(255, 72, 0), delegate
		{
			EditEntry(EntryKind.Application);
		});
		AddAction(actionGroup, "URL", "◎", "↗", Color.FromArgb(0, 220, 245), delegate
		{
			EditEntry(EntryKind.Url);
		});
		AddAction(actionGroup, "ETIQUETA", "T", "▱", Color.FromArgb(235, 55, 220), delegate
		{
			EditEntry(EntryKind.Label);
		});
		moveButton = AddAction(actionGroup2, "MODO MOVER", "↔", "⤢", Color.LimeGreen, ToggleMove, toggle: true, Color.LimeGreen);
		deleteModeButton = AddAction(actionGroup2, "MODO ELIMINAR", "▣", "⊘", Color.Yellow, ToggleDelete, toggle: true, Color.Red, Color.Red);
		AddAction(actionGroup2, "ELIMINAR", "▤", "×", Color.Yellow, DeleteSelected, toggle: false, Color.Yellow, Color.Red);
		AddAction(actionGroup3, "RECARGAR", "↻", "✦", Color.LimeGreen, Reload);
		themeButton = AddAction(actionGroup3, "TEMA", "☾", "☼", Color.MediumOrchid, ToggleTheme, toggle: true, Color.MediumOrchid);
		toolbar.Controls.Add(actionGroup);
		toolbar.Controls.Add(actionGroup2);
		toolbar.Controls.Add(actionGroup3);
		base.Controls.Add(canvas);
		base.Controls.Add(toolbar);
		offscreenNotice.Controls.Add(offscreenText);
		offscreenNotice.Controls.Add(closeNotice);
		base.Controls.Add(offscreenNotice);
		offscreenNotice.Location = new Point(Math.Max(8, base.ClientSize.Width - offscreenNotice.Width - 18), Math.Max(8, base.ClientSize.Height - offscreenNotice.Height - 18));
		offscreenNotice.BringToFront();
		signaturePanel.Controls.Add(signatureText);
		signaturePanel.Controls.Add(githubHomeButton);
		base.Controls.Add(signaturePanel);
		signaturePanel.Dock = DockStyle.Bottom;
		signaturePanel.Height = 32;
		signatureText.Size = new Size(190, 20);
		signatureText.Location = new Point(signaturePanel.Width - 390, 6);
		signatureText.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		githubHomeButton.Location = new Point(signaturePanel.Width - 190, 6);
		githubHomeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		githubHomeButton.Activated += delegate
		{
			OpenGitHubHome();
		};
		UpdateSignatureTheme();
		closeNotice.FlatAppearance.BorderSize = 0;
		closeNotice.Click += delegate
		{
			dismissedOffscreenCount = CountOffscreenObjects();
			offscreenNotice.Visible = false;
		};
		canvas.ClientSizeChanged += delegate
		{
			UpdateOffscreenNotice();
		};
		base.Shown += delegate
		{
			UpdateOffscreenNotice();
		};
		LoadEntries();
		Render();
		base.FormClosing += delegate
		{
			Save();
		};
	}

	private ActionTile AddAction(ActionGroup group, string title, string glyph, string hoverGlyph, Color accent, Action action, bool toggle = false, Color? activeAccent = null, Color? hoverAccent = null)
	{
		ActionTile actionTile = new ActionTile(title, glyph, hoverGlyph, accent, toggle, activeAccent ?? accent, hoverAccent ?? accent);
		actionTile.Activated += delegate
		{
			action();
		};
		group.Buttons.Controls.Add(actionTile);
		return actionTile;
	}

	private void ToggleMove()
	{
		moveMode = !moveMode;
		if (moveMode)
		{
			deleteMode = false;
		}
		if (moveButton != null)
		{
			moveButton.Active = moveMode;
		}
		if (deleteModeButton != null)
		{
			deleteModeButton.Active = deleteMode;
		}
		UpdateModeHint();
		Render();
	}

	private void ToggleDelete()
	{
		deleteMode = !deleteMode;
		if (deleteMode)
		{
			moveMode = false;
		}
		if (deleteModeButton != null)
		{
			deleteModeButton.Active = deleteMode;
		}
		if (moveButton != null)
		{
			moveButton.Active = moveMode;
		}
		UpdateModeHint();
		Render();
	}

	private void ToggleTheme()
	{
		dark = !dark;
		if (themeButton != null)
		{
			themeButton.NormalGlyph = (dark ? "☾" : "☼");
			themeButton.HoverGlyph = (dark ? "☼" : "☾");
			themeButton.Active = !dark;
			themeButton.HoldCurrentGlyphUntilLeave = true;
		}
		UpdateSignatureTheme();
		Render();
	}

	private void UpdateSignatureTheme()
	{
		Color color = (dark ? Color.FromArgb(20, 23, 30) : Color.FromArgb(235, 240, 245));
		Color color2 = (dark ? Color.Yellow : Color.FromArgb(24, 52, 95));
		signaturePanel.BackColor = color;
		signatureText.ForeColor = color2;
		githubHomeButton.SurfaceColor = color;
		githubHomeButton.AccentColor = color2;
	}

	private void OpenGitHubHome()
	{
		Open(new Entry
		{
			Kind = EntryKind.Url,
			Target = "https://github.com/SenecaDPJ"
		});
	}

	private void UpdateModeHint()
	{
		canvas.Cursor = (moveMode ? Cursors.SizeAll : (deleteMode ? Cursors.No : Cursors.Default));
		Text = (moveMode ? "AccessShot 2.13 — Modo mover" : (deleteMode ? "AccessShot 2.13 — Modo eliminar" : "AccessShot 2.13"));
	}

	private void LoadEntries()
	{
		entries.Clear();
		try
		{
			if (!File.Exists(dataFile))
			{
				entries.AddRange(CreateStarterEntries());
			}
			if (File.Exists(dataFile))
			{
				entries.AddRange(JsonSerializer.Deserialize<List<Entry>>(File.ReadAllText(dataFile)) ?? new List<Entry>());
				foreach (Entry entry in entries)
				{
					if (entry.Width < 90)
					{
						entry.Width = 160;
					}
					if (entry.Height < 50)
					{
						entry.Height = 78;
					}
					if (entry.FontSize < 8)
					{
						entry.FontSize = 20;
					}
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("No se pudieron cargar los accesos: " + ex.Message, "AccessShot");
		}
		AccessShotVisuals.Load(this);
	}

	private static IEnumerable<Entry> CreateStarterEntries()
	{
		// Public, generic examples. Existing layouts are never replaced.
		yield return new Entry { Kind = EntryKind.Label, Title = "MIS ACCESOS", X = 24, Y = 24, Width = 180, Height = 56, FontSize = 18 };
		yield return new Entry { Kind = EntryKind.Url, Title = "AccesShot en GitHub", Target = "https://github.com/SenecaDPJ/AccesShot", X = 24, Y = 106, Width = 190, Height = 56, FontSize = 14 };
		yield return new Entry { Kind = EntryKind.Label, Title = "ETIQUETA DE EJEMPLO", X = 1172, Y = 92, Width = 90, Height = 50, FontSize = 11 };
		yield return new Entry { Kind = EntryKind.Url, Title = "URL DE EJEMPLO", Target = "https://example.com", X = 1200, Y = 23, Width = 90, Height = 50, FontSize = 11 };
	}

	private void Reload()
	{
		if (MessageBox.Show("Se volverán a cargar los accesos guardados. ¿Continuar?", "Recargar AccessShot", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
		{
			selected = null;
			LoadEntries();
			Render();
		}
	}

	internal void Save()
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(dataFile));
			string text = dataFile + ".tmp";
			File.WriteAllText(text, JsonSerializer.Serialize(entries, new JsonSerializerOptions
			{
				WriteIndented = true
			}));
			File.Move(text, dataFile, overwrite: true);
			StylePolicy.Save(this);
		}
		catch (Exception ex)
		{
			MessageBox.Show("No se pudieron guardar los accesos: " + ex.Message, "AccessShot");
		}
	}

	private void EditEntry(EntryKind kind, Entry? existing = null)
	{
		_003C_003Ec__DisplayClass30_0 CS_0024_003C_003E8__locals55 = new _003C_003Ec__DisplayClass30_0();
		CS_0024_003C_003E8__locals55.kind = kind;
		CS_0024_003C_003E8__locals55.existing = existing;
		CS_0024_003C_003E8__locals55._003C_003E4__this = this;
		CS_0024_003C_003E8__locals55.dialog = new Form
		{
			Text = ((CS_0024_003C_003E8__locals55.existing == null) ? "Nuevo acceso" : "Editar acceso"),
			Width = 520,
			Height = ((CS_0024_003C_003E8__locals55.kind == EntryKind.Label) ? 195 : 245),
			StartPosition = FormStartPosition.CenterParent,
			FormBorderStyle = FormBorderStyle.FixedDialog,
			MaximizeBox = false,
			MinimizeBox = false
		};
		try
		{
			CS_0024_003C_003E8__locals55.title = new TextBox
			{
				Left = 105,
				Top = 18,
				Width = 380,
				Text = (CS_0024_003C_003E8__locals55.existing?.Title ?? "")
			};
			CS_0024_003C_003E8__locals55.dialog.Controls.Add(new Label
			{
				Text = "Nombre:",
				Left = 15,
				Top = 21,
				AutoSize = true
			});
			CS_0024_003C_003E8__locals55.dialog.Controls.Add(CS_0024_003C_003E8__locals55.title);
			CS_0024_003C_003E8__locals55.target = null;
			if (CS_0024_003C_003E8__locals55.kind != EntryKind.Label)
			{
				CS_0024_003C_003E8__locals55.target = new TextBox
				{
					Left = 105,
					Top = 60,
					Width = ((CS_0024_003C_003E8__locals55.kind == EntryKind.Application) ? 300 : 380),
					Text = (CS_0024_003C_003E8__locals55.existing?.Target ?? "")
				};
				CS_0024_003C_003E8__locals55.dialog.Controls.Add(new Label
				{
					Text = ((CS_0024_003C_003E8__locals55.kind == EntryKind.Url) ? "URL:" : "Archivo:"),
					Left = 15,
					Top = 63,
					AutoSize = true
				});
				CS_0024_003C_003E8__locals55.dialog.Controls.Add(CS_0024_003C_003E8__locals55.target);
				if (CS_0024_003C_003E8__locals55.kind == EntryKind.Application)
				{
					Button button = new Button
					{
						Text = "…",
						Left = 415,
						Top = 58,
						Width = 70
					};
					button.Click += delegate
					{
						OpenFileDialog openFileDialog = new OpenFileDialog
						{
							Filter = "Aplicaciones y accesos|*.exe;*.lnk;*.bat;*.cmd|Todos los archivos|*.*"
						};
						try
						{
							if (openFileDialog.ShowDialog(CS_0024_003C_003E8__locals55.dialog) == DialogResult.OK)
							{
								CS_0024_003C_003E8__locals55.target.Text = openFileDialog.FileName;
								if (string.IsNullOrWhiteSpace(CS_0024_003C_003E8__locals55.title.Text))
								{
									CS_0024_003C_003E8__locals55.title.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
								}
							}
						}
						finally
						{
							((IDisposable)(object)openFileDialog)?.Dispose();
						}
					};
					CS_0024_003C_003E8__locals55.dialog.Controls.Add(button);
				}
			}
			WindowsAppsPicker.Attach(CS_0024_003C_003E8__locals55);
			Button button2 = new Button
			{
				Text = "Aceptar",
				Left = 300,
				Top = ((CS_0024_003C_003E8__locals55.kind == EntryKind.Label) ? 90 : 135),
				Width = 90
			};
			Button button3 = new Button
			{
				Text = "Cancelar",
				Left = 395,
				Top = button2.Top,
				Width = 90,
				DialogResult = DialogResult.Cancel
			};
			button2.Click += delegate
			{
				Uri result;
				if (string.IsNullOrWhiteSpace(CS_0024_003C_003E8__locals55.title.Text))
				{
					MessageBox.Show("Indica un nombre.");
				}
				else if (CS_0024_003C_003E8__locals55.kind == EntryKind.Application && (CS_0024_003C_003E8__locals55.target == null || !AppTargets.Exists(CS_0024_003C_003E8__locals55.target.Text)))
				{
					MessageBox.Show("Selecciona un archivo existente o una app instalada de Windows.");
				}
				else if (CS_0024_003C_003E8__locals55.kind == EntryKind.Url && (CS_0024_003C_003E8__locals55.target == null || !Uri.TryCreate(CS_0024_003C_003E8__locals55.target.Text.Trim(), UriKind.Absolute, out result) || (result.Scheme != Uri.UriSchemeHttps && result.Scheme != Uri.UriSchemeHttp) || string.IsNullOrWhiteSpace(result.Host)))
				{
					MessageBox.Show("Introduce una dirección HTTP o HTTPS válida.");
				}
				else
				{
					Entry entry = CS_0024_003C_003E8__locals55.existing ?? new Entry
					{
						Kind = CS_0024_003C_003E8__locals55.kind,
						X = 24 + CS_0024_003C_003E8__locals55._003C_003E4__this.entries.Count % 6 * 170,
						Y = 24 + CS_0024_003C_003E8__locals55._003C_003E4__this.entries.Count / 6 * 90
					};
					entry.Title = CS_0024_003C_003E8__locals55.title.Text.Trim();
					entry.Target = CS_0024_003C_003E8__locals55.target?.Text.Trim() ?? "";
					if (CS_0024_003C_003E8__locals55.existing == null)
					{
						CS_0024_003C_003E8__locals55._003C_003E4__this.entries.Add(entry);
					}
					CS_0024_003C_003E8__locals55._003C_003E4__this.selected = entry;
					CS_0024_003C_003E8__locals55._003C_003E4__this.Save();
					CS_0024_003C_003E8__locals55._003C_003E4__this.Render();
					CS_0024_003C_003E8__locals55.dialog.DialogResult = DialogResult.OK;
					CS_0024_003C_003E8__locals55.dialog.Close();
				}
			};
			CS_0024_003C_003E8__locals55.dialog.Controls.Add(button2);
			CS_0024_003C_003E8__locals55.dialog.Controls.Add(button3);
			CS_0024_003C_003E8__locals55.dialog.AcceptButton = button2;
			CS_0024_003C_003E8__locals55.dialog.CancelButton = button3;
			CS_0024_003C_003E8__locals55.dialog.ShowDialog(this);
		}
		finally
		{
			if (CS_0024_003C_003E8__locals55.dialog != null)
			{
				((IDisposable)CS_0024_003C_003E8__locals55.dialog).Dispose();
			}
		}
	}

	private void Render()
	{
		canvas.SuspendLayout();
		canvas.Controls.Clear();
		canvas.BackColor = (dark ? Color.FromArgb(20, 23, 30) : Color.FromArgb(235, 240, 245));
		UpdateSignatureTheme();
		foreach (Entry item in entries)
		{
			EntryTile tile = new EntryTile(item, dark)
			{
				Location = new Point(Math.Max(8, item.X), Math.Max(8, item.Y)),
				Selected = (item == selected)
			};
			tile.MouseDown += delegate(object? _, MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Left)
				{
					foreach (Control control in canvas.Controls)
					{
						if (control is EntryTile entryTile)
						{
							entryTile.Selected = false;
						}
					}
					selected = item;
					tile.Selected = true;
					tile.ScreenStart = System.Windows.Forms.Cursor.Position;
					tile.StartBounds = tile.Bounds;
					tile.ResizeEdges = ((!moveMode) ? tile.GetResizeEdges(e.Location) : ResizeEdge.None);
					tile.Resizing = !moveMode && tile.ResizeEdges != ResizeEdge.None;
					tile.Dragging = false;
					tile.Capture = true;
				}
			};
			tile.MouseMove += delegate(object? _, MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Left && (moveMode || tile.Resizing))
				{
					int num = System.Windows.Forms.Cursor.Position.X - tile.ScreenStart.X;
					int num2 = System.Windows.Forms.Cursor.Position.Y - tile.ScreenStart.Y;
					if (Math.Abs(num) + Math.Abs(num2) >= 3)
					{
						tile.Dragging = true;
						if (moveMode)
						{
							tile.Left = Math.Max(0, tile.StartBounds.Left + num);
							tile.Top = Math.Max(0, tile.StartBounds.Top + num2);
						}
						else
						{
							ResizeTile(tile, num, num2);
						}
					}
				}
			};
			tile.MouseUp += delegate(object? _, MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Left)
				{
					if (tile.Dragging && tile.Resizing)
					{
						item.X = tile.Left;
						item.Y = tile.Top;
						item.Width = tile.Width;
						item.Height = tile.Height;
						Save();
					}
					else if (tile.Dragging)
					{
						item.X = tile.Left;
						item.Y = tile.Top;
						Save();
					}
					else if (deleteMode)
					{
						DeleteEntry(item);
					}
					else if (!moveMode && !tile.Resizing && item.Kind != EntryKind.Label)
					{
						Open(item);
					}
					tile.Capture = false;
					canvas.Invalidate();
					UpdateOffscreenNotice();
				}
			};
			ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
			contextMenuStrip.Items.Add("Editar", null, delegate
			{
				EditEntry(item.Kind, item);
			});
			contextMenuStrip.Items.Add("Color de fondo", null, delegate
			{
				ChangeColor(item, background: true);
			});
			contextMenuStrip.Items.Add("Color de texto", null, delegate
			{
				ChangeColor(item, background: false);
			});
			contextMenuStrip.Items.Add("Tamaño del texto…", null, delegate
			{
				ChangeFontSize(item);
			});
			contextMenuStrip.Items.Add("Tamaño del elemento…", null, delegate
			{
				ChangeElementSize(item);
			});
			contextMenuStrip.Items.Add("Eliminar", null, delegate
			{
				DeleteEntry(item);
			});
			CanvasLayerMenu.Attach(contextMenuStrip, this, item, tile);
			AccessShotVisuals.Attach(contextMenuStrip, this, item, tile);
			tile.ContextMenuStrip = contextMenuStrip;
			canvas.Controls.Add(tile);
		}
		canvas.ResumeLayout();
		UpdateOffscreenNotice();
	}

	private int CountOffscreenObjects()
	{
		int visibleWidth = canvas.ClientSize.Width;
		int visibleHeight = canvas.ClientSize.Height;
		return entries.Count((Entry item) => item.X < 0 || item.Y < 0 || item.X + item.Width > visibleWidth || item.Y + item.Height > visibleHeight);
	}

	private void UpdateOffscreenNotice()
	{
		if (offscreenNotice.IsDisposed || canvas.ClientSize.Width <= 0 || canvas.ClientSize.Height <= 0)
		{
			return;
		}
		List<Entry> list = entries.Where((Entry item) => item.X < 0 || item.Y < 0 || item.X + item.Width > canvas.ClientSize.Width || item.Y + item.Height > canvas.ClientSize.Height).ToList();
		int count = list.Count;
		if (count == 0)
		{
			dismissedOffscreenCount = -1;
			offscreenNotice.Visible = false;
			return;
		}
		List<string> list2 = new List<string>();
		int num = list.Count((Entry item) => item.Kind == EntryKind.Application);
		int num2 = list.Count((Entry item) => item.Kind == EntryKind.Url);
		int num3 = list.Count((Entry item) => item.Kind == EntryKind.Label);
		if (num > 0)
		{
			list2.Add($"Aplicaciones: {num}");
		}
		if (num2 > 0)
		{
			list2.Add($"URL: {num2}");
		}
		if (num3 > 0)
		{
			list2.Add($"Etiquetas: {num3}");
		}
		offscreenText.Text = $"Hay {count} objeto{((count == 1) ? "" : "s")} fuera de la vista inicial\n{string.Join("  ·  ", list2)}";
		if (dismissedOffscreenCount != count)
		{
			offscreenNotice.Visible = true;
		}
		offscreenNotice.BringToFront();
	}

	private void ResizeTile(EntryTile tile, int dx, int dy)
	{
		Rectangle startBounds = tile.StartBounds;
		int val = startBounds.Left;
		int val2 = startBounds.Top;
		int num = startBounds.Width;
		int num2 = startBounds.Height;
		if ((tile.ResizeEdges & ResizeEdge.Right) != ResizeEdge.None)
		{
			num = Math.Clamp(startBounds.Width + dx, 90, 900);
		}
		if ((tile.ResizeEdges & ResizeEdge.Bottom) != ResizeEdge.None)
		{
			num2 = Math.Clamp(startBounds.Height + dy, 50, 600);
		}
		if ((tile.ResizeEdges & ResizeEdge.Left) != ResizeEdge.None)
		{
			num = Math.Clamp(startBounds.Width - dx, 90, 900);
			val = startBounds.Right - num;
		}
		if ((tile.ResizeEdges & ResizeEdge.Top) != ResizeEdge.None)
		{
			num2 = Math.Clamp(startBounds.Height - dy, 50, 600);
			val2 = startBounds.Bottom - num2;
		}
		tile.SetBounds(Math.Max(0, val), Math.Max(0, val2), num, num2);
		tile.Invalidate();
	}

	private void ChangeFontSize(Entry item)
	{
		using NumericDialog numericDialog = NumberDialog("Tamaño del texto", "Tamaño de fuente (puntos):", item.FontSize, 8, 48);
		if (numericDialog.ShowDialog(this) == DialogResult.OK)
		{
			item.FontSize = numericDialog.Value;
			Save();
			Render();
		}
	}

	private void ChangeElementSize(Entry item)
	{
		using Form form = new Form
		{
			Text = "Tamaño del elemento",
			Width = 360,
			Height = 190,
			StartPosition = FormStartPosition.CenterParent,
			FormBorderStyle = FormBorderStyle.FixedDialog,
			MaximizeBox = false,
			MinimizeBox = false
		};
		NumericUpDown numericUpDown = new NumericUpDown
		{
			Left = 150,
			Top = 20,
			Width = 170,
			Minimum = 90m,
			Maximum = 900m,
			Value = Math.Clamp(item.Width, 90, 900)
		};
		NumericUpDown numericUpDown2 = new NumericUpDown
		{
			Left = 150,
			Top = 58,
			Width = 170,
			Minimum = 50m,
			Maximum = 600m,
			Value = Math.Clamp(item.Height, 50, 600)
		};
		form.Controls.Add(new Label
		{
			Text = "Ancho (px):",
			Left = 20,
			Top = 23,
			AutoSize = true
		});
		form.Controls.Add(numericUpDown);
		form.Controls.Add(new Label
		{
			Text = "Alto (px):",
			Left = 20,
			Top = 61,
			AutoSize = true
		});
		form.Controls.Add(numericUpDown2);
		Button button = new Button
		{
			Text = "Aceptar",
			Left = 150,
			Top = 105,
			Width = 80,
			DialogResult = DialogResult.OK
		};
		Button button2 = new Button
		{
			Text = "Cancelar",
			Left = 240,
			Top = 105,
			Width = 80,
			DialogResult = DialogResult.Cancel
		};
		form.Controls.Add(button);
		form.Controls.Add(button2);
		form.AcceptButton = button;
		form.CancelButton = button2;
		if (form.ShowDialog(this) == DialogResult.OK)
		{
			item.Width = (int)numericUpDown.Value;
			item.Height = (int)numericUpDown2.Value;
			Save();
			Render();
		}
	}

	private static NumericDialog NumberDialog(string title, string prompt, int value, int min, int max)
	{
		return new NumericDialog(title, prompt, value, min, max);
	}

	private void DeleteSelected()
	{
		if (selected == null)
		{
			MessageBox.Show("Selecciona primero un acceso o una etiqueta.", "AccessShot");
		}
		else
		{
			DeleteEntry(selected);
		}
	}

	private void DeleteEntry(Entry item)
	{
		if (MessageBox.Show("¿Eliminar «" + item.Title + "»?", "AccessShot", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
		{
			entries.Remove(item);
			if (selected == item)
			{
				selected = null;
			}
			Save();
			Render();
		}
	}

	private void ChangeColor(Entry item, bool background)
	{
		StylePolicy.PrepareCustom(this, item);
		ColorDialog colorDialog = new ColorDialog
		{
			Color = Color.FromArgb(background ? item.Background : item.Foreground),
			FullOpen = true,
			AnyColor = true
		};
		try
		{
			if (colorDialog.ShowDialog(this) == DialogResult.OK)
			{
				if (background)
				{
					item.Background = colorDialog.Color.ToArgb();
				}
				else
				{
					item.Foreground = colorDialog.Color.ToArgb();
				}
				StylePolicy.MarkCustom(this, item);
				Save();
				Render();
			}
		}
		finally
		{
			((IDisposable)colorDialog)?.Dispose();
		}
	}

	private void Open(Entry item)
	{
		if (item.Kind == EntryKind.Url)
		{
			if (!Uri.TryCreate(item.Target, UriKind.Absolute, out Uri result))
			{
				MessageBox.Show("La URL guardada no es válida.");
				return;
			}
			DialogResult dialogResult = MessageBox.Show("AccessShot abrirá esta dirección en el navegador y podría conectarse a Internet:\n\n" + result.AbsoluteUri + "\n\n¿Permitirlo?", "Permiso para abrir Internet", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			if (dialogResult != DialogResult.Yes)
			{
				return;
			}
		}
		else if (!AppTargets.Exists(item.Target))
		{
			MessageBox.Show("La aplicación o el archivo de destino ya no está disponible.", "AccessShot");
			return;
		}
		try
		{
			ProcessStartInfo processStartInfo = AppTargets.CreateStartInfo(item.Target);
			processStartInfo.UseShellExecute = true;
			Process.Start(processStartInfo);
		}
		catch (Exception ex)
		{
			MessageBox.Show("No se pudo abrir el acceso: " + ex.Message, "AccessShot");
		}
	}
}
