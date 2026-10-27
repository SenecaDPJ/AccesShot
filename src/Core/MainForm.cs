using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
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

		public ComboBox? orientation;

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

	private sealed class UndoRecord
	{
		public string Payload { get; set; } = "";
		public string ResultHash { get; set; } = "";
	}

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

	private readonly HashSet<Entry> selectedEntries = new HashSet<Entry>();

	private readonly Dictionary<Entry, Point> dragStarts = new Dictionary<Entry, Point>();

	private bool selectionBoxActive;

	private bool selectionAdditive;

	private bool ctrlDragPending;

	private bool ctrlDragActive;

	private Entry? selectionInitiator;

	private readonly HashSet<Entry> selectionBase = new HashSet<Entry>();

	private Point selectionBoxStart;

	private Point selectionBoxCurrent;

	private readonly string undoFile = Path.Combine(Environment.GetEnvironmentVariable("ACCESS_SHOT_DATA_DIR") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccessShot"), "undo.json");

	private readonly string preferencesFile = Path.Combine(Environment.GetEnvironmentVariable("ACCESS_SHOT_DATA_DIR") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccessShot"), "preferences.json");

	private string? lastSavedConfiguration;

	private string? lastDiskSnapshot;

	private string backupDirectory = "";

	private readonly Panel bottomStrip = new Panel { Dock = DockStyle.Bottom, Height = 30 };

	private readonly ReliefHintLabel optionsHint = new ReliefHintLabel
	{
		Text = "Opciones = Ctrl + O",
		Left = 10,
		Top = 4,
		Width = 180,
		Height = 22,
		Cursor = Cursors.Hand,
		BackColor = Color.Transparent,
		TabStop = true
	};

	private readonly Button backupButton = new Button
	{
		Text = "⧉",
		Width = 36,
		Height = 26,
		Top = 2,
		Anchor = AnchorStyles.Top | AnchorStyles.Right,
		FlatStyle = FlatStyle.Flat,
		Font = new Font("Segoe UI Symbol", 13f, FontStyle.Bold),
		AccessibleName = "Crear copia de seguridad del JSON"
	};

	private bool dark = true;

	private bool moveMode;

	private bool deleteMode;

	private ActionTile? moveButton;

	private ActionTile? deleteModeButton;

	private ActionTile? themeButton;

	private int dismissedOffscreenCount = -1;

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (ActiveControl is TextBoxBase || ActiveControl is ComboBox) return base.ProcessCmdKey(ref msg, keyData);
		if (keyData == (Keys.Control | Keys.O)) { OpenOptions(); return true; }
		if (keyData == (Keys.Control | Keys.Z) && UndoAvailable()) { UndoLastAction(); return true; }
		if (keyData == Keys.Delete && deleteMode) { DeleteSelected(); return true; }
		if (keyData == Keys.Escape && (selectionBoxActive || ctrlDragPending))
		{
			if (selectionAdditive) SetSelection(selectionBase);
			selectionBoxActive = false; ctrlDragPending = false; ctrlDragActive = false; selectionAdditive = false; selectionInitiator = null; selectionBase.Clear(); canvas.Capture = false;
			foreach (EntryTile tile in canvas.Controls.OfType<EntryTile>()) { tile.Dragging = false; tile.Capture = false; }
			canvas.Invalidate(); return true;
		}
		if (keyData == Keys.Escape && dragStarts.Count > 0)
		{
			foreach (EntryTile tile in canvas.Controls.OfType<EntryTile>()) if (dragStarts.TryGetValue(tile.Item, out Point start)) { tile.Location = start; tile.Dragging = false; tile.Capture = false; }
			dragStarts.Clear(); canvas.Invalidate(); return true;
		}
		if (keyData == Keys.Escape && selectedEntries.Count > 0) { selectedEntries.Clear(); selected = null; RenderSelection(); return true; }
		return base.ProcessCmdKey(ref msg, keyData);
	}

	public MainForm()
	{
		Text = "AccesShot 2.15";
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
		base.Controls.Add(bottomStrip);
		base.Controls.Add(toolbar);
		offscreenNotice.Controls.Add(offscreenText);
		offscreenNotice.Controls.Add(closeNotice);
		base.Controls.Add(offscreenNotice);
		offscreenNotice.Location = new Point(Math.Max(8, base.ClientSize.Width - offscreenNotice.Width - 18), Math.Max(8, base.ClientSize.Height - offscreenNotice.Height - 18));
		offscreenNotice.BringToFront();
		signaturePanel.Controls.Add(signatureText);
		signaturePanel.Controls.Add(githubHomeButton);
		base.Controls.Add(signaturePanel);
		bottomStrip.BackColor = dark ? Color.FromArgb(20, 23, 30) : Color.FromArgb(235, 240, 245);
		bottomStrip.Controls.Add(optionsHint);
		bottomStrip.Controls.Add(backupButton);
		backupButton.FlatAppearance.BorderColor = Color.FromArgb(0, 220, 245);
		backupButton.FlatAppearance.BorderSize = 1;
		backupButton.BackColor = bottomStrip.BackColor;
		backupButton.ForeColor = Color.FromArgb(0, 220, 245);
		backupButton.Click += delegate { CreateBackup(); };
		optionsHint.Click += delegate { OpenOptions(); };
		optionsHint.KeyDown += delegate(object? _, KeyEventArgs e) { if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) OpenOptions(); };
		bottomStrip.Resize += delegate { backupButton.Left = Math.Max(8, bottomStrip.ClientSize.Width - backupButton.Width - 8); };
		backupButton.Left = Math.Max(8, bottomStrip.ClientSize.Width - backupButton.Width - 8);
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
			canvas.Invalidate();
		};
		canvas.MouseDown += CanvasMouseDown;
		canvas.MouseMove += CanvasMouseMove;
		canvas.MouseUp += CanvasMouseUp;
		canvas.Paint += CanvasPaint;
		LoadPreferences();
		base.Shown += delegate
		{
			UpdateOffscreenNotice();
		};
		LoadEntries();
		Render();
		backupButton.Left = Math.Max(8, bottomStrip.ClientSize.Width - backupButton.Width - 8);
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
		Text = (moveMode ? "AccessShot 2.15 — Modo mover" : (deleteMode ? "AccessShot 2.15 — Modo eliminar" : "AccessShot 2.15"));
	}

	private void LoadPreferences()
	{
		try { if (File.Exists(preferencesFile)) backupDirectory = JsonSerializer.Deserialize<string>(File.ReadAllText(preferencesFile)) ?? ""; } catch { backupDirectory = ""; }
	}

	private void SavePreferences()
	{
		WriteAtomically(preferencesFile, JsonSerializer.Serialize(backupDirectory));
	}

	private void OpenOptions()
	{
		using Form dialog = new Form { Text = "Opciones de AccesShot", Width = 500, Height = 330, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, ShowInTaskbar = false };
		Label title = new Label { Text = "AccesShot 2.15", Left = 18, Top = 14, Width = 440, Height = 28, Font = new Font("Segoe UI", 13f, FontStyle.Bold) };
		Label help = new Label { Text = "En MOVER o ELIMINAR, arrastra en el lienzo para seleccionar un grupo.\r\nCtrl + clic anade o quita un objeto. Ctrl + O abre estas opciones.\r\nLa recuperacion cubre la ultima edicion.", Left = 20, Top = 52, Width = 445, Height = 64 };
		Label folder = new Label { Text = "Carpeta de copias: " + (string.IsNullOrWhiteSpace(backupDirectory) ? "(elige una al crear la primera copia)" : backupDirectory), Left = 20, Top = 120, Width = 445, Height = 38, AutoEllipsis = true };
		Button undo = new Button { Text = "Deshacer última acción", Left = 20, Top = 170, Width = 210, Height = 34, Enabled = UndoAvailable() };
		Button backup = new Button { Text = "Crear copia ahora", Left = 245, Top = 170, Width = 210, Height = 34 };
		Button choose = new Button { Text = "Cambiar carpeta de copias…", Left = 20, Top = 214, Width = 210, Height = 34 };
		Button restore = new Button { Text = "Restaurar una copia…", Left = 245, Top = 214, Width = 210, Height = 34 };
		Button close = new Button { Text = "Cerrar", Left = 360, Top = 258, Width = 95, Height = 30, DialogResult = DialogResult.Cancel };
		undo.Click += delegate { UndoLastAction(); undo.Enabled = UndoAvailable(); };
		backup.Click += delegate { CreateBackup(); folder.Text = "Carpeta de copias: " + backupDirectory; };
		choose.Click += delegate { if (ChooseBackupFolder()) folder.Text = "Carpeta de copias: " + backupDirectory; };
		restore.Click += delegate { RestoreBackup(); undo.Enabled = UndoAvailable(); };
		dialog.Controls.AddRange(new Control[] { title, help, folder, undo, backup, choose, restore, close }); dialog.CancelButton = close;
		dialog.ShowDialog(this);
	}

	private bool ChooseBackupFolder()
	{
		using FolderBrowserDialog dialog = new FolderBrowserDialog { Description = "Elige dónde guardar las copias JSON de AccesShot", UseDescriptionForTitle = true, ShowNewFolderButton = true };
		if (Directory.Exists(backupDirectory)) dialog.SelectedPath = backupDirectory;
		if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath)) return false;
		backupDirectory = dialog.SelectedPath; SavePreferences(); return true;
	}

	private void CreateBackup()
	{
		try
		{
			if (string.IsNullOrWhiteSpace(backupDirectory) || !Directory.Exists(backupDirectory)) if (!ChooseBackupFolder()) return;
			if (!Save()) return;
			string payload = SerializeConfiguration(); string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			string path = Path.Combine(backupDirectory, $"AccesShot-config-{stamp}.json"); int suffix = 1;
			while (File.Exists(path)) path = Path.Combine(backupDirectory, $"AccesShot-config-{stamp}-{suffix++:00}.json");
			WriteAtomically(path, payload);
			if (File.ReadAllText(path) != payload) throw new IOException("La verificación de la copia no coincide.");
			if (MessageBox.Show(this, "Copia de seguridad guardada correctamente. ¿Abrir la carpeta?", "AccesShot 2.15", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
				Process.Start(new ProcessStartInfo { FileName = backupDirectory, UseShellExecute = true });
		}
		catch (Exception ex) { MessageBox.Show(this, "No se pudo crear la copia: " + ex.Message, "AccesShot 2.15", MessageBoxButtons.OK, MessageBoxIcon.Error); }
	}

	private void RestoreBackup()
	{
		using OpenFileDialog dialog = new OpenFileDialog { Filter = "Copias JSON de AccesShot (*.json)|*.json|Todos los archivos (*.*)|*.*", Title = "Restaurar copia de AccesShot" };
		if (dialog.ShowDialog(this) != DialogResult.OK) return;
		try
		{
			string payload = File.ReadAllText(dialog.FileName);
			List<Entry> restored = JsonSerializer.Deserialize<List<Entry>>(payload) ?? throw new InvalidDataException("El archivo no contiene una lista de objetos.");
			if (restored.Any(entry => entry.Width < 1 || entry.Height < 1 || !Enum.IsDefined(entry.Kind))) throw new InvalidDataException("La copia contiene objetos con datos no válidos.");
			string before = SerializeConfiguration();
			if (MessageBox.Show(this, $"La copia contiene {restored.Count} objetos. Se guardará el estado actual como recuperación. ¿Restaurar?", "Restaurar copia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
			WriteAtomically(dataFile, payload); LoadEntries();
			try { WriteUndoRecord(before, SerializeConfiguration()); }
			catch { WriteAtomically(dataFile, before); LoadEntries(); throw; }
			Render();
		}
		catch (Exception ex) { MessageBox.Show(this, "No se pudo restaurar la copia: " + ex.Message, "AccessShot 2.15", MessageBoxButtons.OK, MessageBoxIcon.Error); }
	}

	private void LoadEntries()
	{
		entries.Clear(); selectedEntries.Clear(); selected = null;
		try
		{
			if (!File.Exists(dataFile)) entries.AddRange(CreateStarterEntries());
			else
			{
				entries.AddRange(JsonSerializer.Deserialize<List<Entry>>(File.ReadAllText(dataFile)) ?? new List<Entry>());
				foreach (Entry entry in entries)
				{
					if (string.IsNullOrWhiteSpace(entry.Id)) entry.Id = Guid.NewGuid().ToString("N");
					if (entry.Width < 90) entry.Width = 160;
					if (entry.Height < 50) entry.Height = 78;
					if (entry.FontSize < 8) entry.FontSize = 20;
					if (entry.Orientation is not (0 or 90 or 180 or 270)) entry.Orientation = 0;
				}
			}
		}
		catch (Exception ex) { MessageBox.Show(this, "No se pudieron cargar los accesos: " + ex.Message, "AccessShot 2.15", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
		AccessShotVisuals.Load(this);
		lastSavedConfiguration = File.Exists(dataFile) ? SerializeConfiguration() : null;
		lastDiskSnapshot = File.Exists(dataFile) ? File.ReadAllText(dataFile) : null;
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

	internal bool Save()
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(dataFile)!);
			string payload = SerializeConfiguration();
			if (File.Exists(dataFile) && lastDiskSnapshot != null && File.ReadAllText(dataFile) != lastDiskSnapshot)
				throw new IOException("El archivo de configuración cambió fuera de AccesShot. Recarga la configuración antes de guardar para no sobrescribir ese cambio.");
			if (File.Exists(dataFile) && lastDiskSnapshot != null && lastDiskSnapshot != payload)
			{
				string migrationBackup = dataFile + ".pre-2.15.bak";
				if (!File.Exists(migrationBackup)) File.Copy(dataFile, migrationBackup);
			}
			if (lastSavedConfiguration != null && lastSavedConfiguration != payload) WriteUndoRecord(lastSavedConfiguration, payload);
			WriteAtomically(dataFile, payload);
			lastSavedConfiguration = payload; lastDiskSnapshot = File.ReadAllText(dataFile);
			return true;
		}
		catch (Exception ex) { MessageBox.Show(this, "No se pudieron guardar los accesos: " + ex.Message, "AccessShot 2.15", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
	}

	private string SerializeConfiguration() => StylePolicy.Serialize(this);

	private static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

	private static void WriteAtomically(string path, string content)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		string temp = path + ".tmp"; File.WriteAllText(temp, content, new UTF8Encoding(false)); File.Move(temp, path, true);
	}

	private void WriteUndoRecord(string previousPayload, string resultingPayload)
	{
		var record = new UndoRecord { Payload = previousPayload, ResultHash = Hash(resultingPayload) };
		WriteAtomically(undoFile, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }));
	}

	private bool UndoAvailable()
	{
		try
		{
			if (!File.Exists(undoFile)) return false;
			UndoRecord? record = JsonSerializer.Deserialize<UndoRecord>(File.ReadAllText(undoFile));
			return record != null && record.ResultHash == Hash(SerializeConfiguration());
		}
		catch { return false; }
	}

	private void UndoLastAction()
	{
		try
		{
			if (!UndoAvailable()) { MessageBox.Show(this, "La recuperación ya no coincide con la configuración actual.", "AccessShot 2.15", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
			UndoRecord record = JsonSerializer.Deserialize<UndoRecord>(File.ReadAllText(undoFile))!;
			WriteAtomically(dataFile, record.Payload); File.Delete(undoFile); LoadEntries(); Render();
		}
		catch (Exception ex) { MessageBox.Show(this, "No se pudo recuperar la última acción: " + ex.Message, "AccessShot 2.15", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
			Height = ((CS_0024_003C_003E8__locals55.kind == EntryKind.Label) ? ((CS_0024_003C_003E8__locals55.existing == null) ? 245 : 195) : 245),
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
			CS_0024_003C_003E8__locals55.orientation = null;
			if (CS_0024_003C_003E8__locals55.kind == EntryKind.Label && CS_0024_003C_003E8__locals55.existing == null)
			{
				CS_0024_003C_003E8__locals55.dialog.Controls.Add(new Label { Text = "Disposición:", Left = 15, Top = 63, AutoSize = true });
				CS_0024_003C_003E8__locals55.orientation = new ComboBox { Left = 105, Top = 58, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
				CS_0024_003C_003E8__locals55.orientation.Items.AddRange(new object[] { "Horizontal", "Vertical" });
				CS_0024_003C_003E8__locals55.orientation.SelectedIndex = 0;
				CS_0024_003C_003E8__locals55.dialog.Controls.Add(CS_0024_003C_003E8__locals55.orientation);
			}
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
				Top = ((CS_0024_003C_003E8__locals55.kind == EntryKind.Label && CS_0024_003C_003E8__locals55.existing == null) ? 140 : ((CS_0024_003C_003E8__locals55.kind == EntryKind.Label) ? 90 : 135)),
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
					if (CS_0024_003C_003E8__locals55.orientation != null) entry.Orientation = CS_0024_003C_003E8__locals55.orientation.SelectedIndex == 1 ? 90 : 0;
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
		canvas.BackColor = dark ? Color.FromArgb(20, 23, 30) : Color.FromArgb(235, 240, 245);
		bottomStrip.BackColor = canvas.BackColor;
		backupButton.BackColor = canvas.BackColor;
		optionsHint.ForeColor = dark ? Color.FromArgb(190, 205, 220) : Color.FromArgb(50, 60, 75);
		UpdateSignatureTheme();
		foreach (Entry item in entries)
		{
			EntryTile tile = new EntryTile(item, dark)
			{
				Location = new Point(Math.Max(0, item.X), Math.Max(0, item.Y)),
				Selected = selectedEntries.Contains(item) || item == selected
			};
			tile.MouseDown += delegate(object? _, MouseEventArgs e)
			{
				if (e.Button != MouseButtons.Left) return;
				bool ctrlSelection = (moveMode || deleteMode) && (ModifierKeys & Keys.Control) == Keys.Control;
				if (ctrlSelection)
				{
					ctrlDragPending = true; ctrlDragActive = false; selectionBoxActive = false; selectionAdditive = true;
					selectionBase.Clear(); selectionBase.UnionWith(selectedEntries); selectionInitiator = item;
					selectionBoxStart = selectionBoxCurrent = ToCanvasPoint(canvas.PointToClient(Cursor.Position)); tile.SelectionOnlyClick = true;
				}
				else if (!selectedEntries.Contains(item)) SetSelection(new[] { item });
				tile.ScreenStart = Cursor.Position; tile.StartBounds = tile.Bounds;
				tile.ResizeEdges = (!moveMode && !deleteMode && selectedEntries.Count <= 1) ? tile.GetResizeEdges(e.Location) : ResizeEdge.None;
				tile.Resizing = tile.ResizeEdges != ResizeEdge.None; tile.Dragging = false; dragStarts.Clear();
				if (moveMode && !ctrlSelection) foreach (Entry chosen in selectedEntries) dragStarts[chosen] = new Point(chosen.X, chosen.Y);
				tile.Capture = true;
			};
			tile.MouseMove += delegate(object? _, MouseEventArgs e)
			{
				if (e.Button != MouseButtons.Left) return;
				if (ctrlDragPending)
				{
					Point screen = Cursor.Position;
					if (!ctrlDragActive && Math.Abs(screen.X - tile.ScreenStart.X) + Math.Abs(screen.Y - tile.ScreenStart.Y) >= 3) { ctrlDragActive = true; selectionBoxActive = true; }
					if (ctrlDragActive) { selectionBoxCurrent = ToCanvasPoint(canvas.PointToClient(screen)); canvas.Invalidate(); }
					return;
				}
				if ((!moveMode && !tile.Resizing) || tile.SelectionOnlyClick) return;
				int dx = Cursor.Position.X - tile.ScreenStart.X; int dy = Cursor.Position.Y - tile.ScreenStart.Y;
				if (Math.Abs(dx) + Math.Abs(dy) < 3) return;
				tile.Dragging = true;
				if (moveMode)
				{
					int minX = dragStarts.Values.Min(p => p.X), minY = dragStarts.Values.Min(p => p.Y);
					dx = Math.Max(-minX, dx); dy = Math.Max(-minY, dy);
					foreach (EntryTile moving in canvas.Controls.OfType<EntryTile>()) if (dragStarts.TryGetValue(moving.Item, out Point start)) moving.Location = new Point(start.X + dx, start.Y + dy);
				}
				else ResizeTile(tile, dx, dy);
			};
			tile.MouseUp += delegate(object? _, MouseEventArgs e)
			{
				if (e.Button != MouseButtons.Left) return;
				if (ctrlDragPending)
				{
					if (ctrlDragActive)
					{
						selectionBoxCurrent = ToCanvasPoint(canvas.PointToClient(Cursor.Position)); Rectangle box = RectFromPoints(selectionBoxStart, selectionBoxCurrent);
						IEnumerable<Entry> group = selectionBase;
						if (selectionInitiator != null) group = group.Append(selectionInitiator);
						group = group.Concat(entries.Where(candidate => box.Contains(new Rectangle(candidate.X, candidate.Y, DisplaySize(candidate).Width, DisplaySize(candidate).Height))));
						SetSelection(group);
					}
					else
					{
						HashSet<Entry> next = new HashSet<Entry>(selectionBase);
						if (!next.Remove(item)) next.Add(item);
						SetSelection(next);
					}
					selectionBoxActive = false; ctrlDragPending = false; ctrlDragActive = false; selectionAdditive = false; selectionInitiator = null; selectionBase.Clear();
					tile.Capture = false; tile.SelectionOnlyClick = false; dragStarts.Clear(); canvas.Invalidate(); return;
				}
				if (tile.Dragging && tile.Resizing)
				{
					item.X = tile.Left; item.Y = tile.Top;
					if (item.Orientation % 180 == 0) { item.Width = tile.Width; item.Height = tile.Height; } else { item.Width = tile.Height; item.Height = tile.Width; }
					if (!Save()) { item.X = tile.StartBounds.Left; item.Y = tile.StartBounds.Top; item.Width = item.Orientation % 180 == 0 ? tile.StartBounds.Width : tile.StartBounds.Height; item.Height = item.Orientation % 180 == 0 ? tile.StartBounds.Height : tile.StartBounds.Width; Render(); }
				}
				else if (tile.Dragging && moveMode)
				{
					foreach (EntryTile moving in canvas.Controls.OfType<EntryTile>()) if (dragStarts.ContainsKey(moving.Item)) { moving.Item.X = moving.Left; moving.Item.Y = moving.Top; }
					if (!Save()) { foreach (var start in dragStarts) { start.Key.X = start.Value.X; start.Key.Y = start.Value.Y; } Render(); }
				}
				else if (!tile.SelectionOnlyClick && !moveMode && !deleteMode && !tile.Resizing && item.Kind != EntryKind.Label) Open(item);
				tile.Capture = false; tile.SelectionOnlyClick = false; dragStarts.Clear(); canvas.Invalidate(); UpdateOffscreenNotice();
			};
			ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
			contextMenuStrip.Items.Add("Editar", null, delegate { EditEntry(item.Kind, item); });
			ToolStripMenuItem orientationMenu = new ToolStripMenuItem("Orientación");
			orientationMenu.DropDownItems.Add("Girar 90° a la izquierda", null, delegate { RotateEntry(item, -90); });
			orientationMenu.DropDownItems.Add("Girar 90° a la derecha", null, delegate { RotateEntry(item, 90); });
			orientationMenu.DropDownItems.Add("Restablecer orientación", null, delegate { SetOrientation(item, 0); });
			contextMenuStrip.Items.Add(orientationMenu);
			contextMenuStrip.Items.Add("Color de fondo", null, delegate { ChangeColor(item, background: true); });
			contextMenuStrip.Items.Add("Color de texto", null, delegate { ChangeColor(item, background: false); });
			contextMenuStrip.Items.Add("Tamaño del texto…", null, delegate { ChangeFontSize(item); });
			contextMenuStrip.Items.Add("Tamaño del elemento…", null, delegate { ChangeElementSize(item); });
			contextMenuStrip.Items.Add("Eliminar", null, delegate { DeleteEntry(item); });
			CanvasLayerMenu.Attach(contextMenuStrip, this, item, tile);
			AccessShotVisuals.Attach(contextMenuStrip, this, item, tile);
			tile.ContextMenuStrip = contextMenuStrip;
			canvas.Controls.Add(tile);
		}
		canvas.ResumeLayout();
		canvas.Invalidate();
		UpdateOffscreenNotice();
	}

	private Size DisplaySize(Entry item) => item.Orientation % 180 == 0 ? new Size(item.Width, item.Height) : new Size(item.Height, item.Width);

	private void SetSelection(IEnumerable<Entry> items)
	{
		selectedEntries.Clear(); foreach (Entry item in items) selectedEntries.Add(item);
		selected = selectedEntries.LastOrDefault(); RenderSelection();
	}

	private void RenderSelection()
	{
		foreach (EntryTile tile in canvas.Controls.OfType<EntryTile>()) { tile.Selected = selectedEntries.Contains(tile.Item); tile.Invalidate(); }
	}

	private Point ToCanvasPoint(Point viewport) => new Point(viewport.X - canvas.AutoScrollPosition.X, viewport.Y - canvas.AutoScrollPosition.Y);

	private static Rectangle RectFromPoints(Point a, Point b) => Rectangle.FromLTRB(Math.Min(a.X,b.X), Math.Min(a.Y,b.Y), Math.Max(a.X,b.X), Math.Max(a.Y,b.Y));

	private void CanvasMouseDown(object? sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left || (!moveMode && !deleteMode)) return;
		selectionAdditive = (ModifierKeys & Keys.Control) == Keys.Control; selectionBase.Clear(); if (selectionAdditive) selectionBase.UnionWith(selectedEntries); selectionInitiator = null;
		selectionBoxActive = true; selectionBoxStart = selectionBoxCurrent = ToCanvasPoint(e.Location); canvas.Capture = true;
	}

	private void CanvasMouseMove(object? sender, MouseEventArgs e)
	{
		if (!selectionBoxActive || e.Button != MouseButtons.Left) return;
		selectionBoxCurrent = ToCanvasPoint(e.Location); canvas.Invalidate();
	}

	private void CanvasMouseUp(object? sender, MouseEventArgs e)
	{
		if (!selectionBoxActive || e.Button != MouseButtons.Left) return;
		selectionBoxCurrent = ToCanvasPoint(e.Location); Rectangle box = RectFromPoints(selectionBoxStart, selectionBoxCurrent);
		IEnumerable<Entry> inside = entries.Where(item => box.Contains(new Rectangle(item.X, item.Y, DisplaySize(item).Width, DisplaySize(item).Height)));
		SetSelection(selectionAdditive ? selectionBase.Union(inside) : inside);
		selectionBoxActive = false; selectionAdditive = false; selectionInitiator = null; selectionBase.Clear(); canvas.Capture = false; canvas.Invalidate();
	}

	private void CanvasPaint(object? sender, PaintEventArgs e)
	{
		if (!selectionBoxActive) return;
		Rectangle logicalBox = RectFromPoints(selectionBoxStart, selectionBoxCurrent); Rectangle box = logicalBox; box.Offset(canvas.AutoScrollPosition);
		using var fill = new SolidBrush(Color.FromArgb(55, 0, 220, 245)); using var border = new Pen(Color.FromArgb(0, 220, 245), 1.5f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
		e.Graphics.FillRectangle(fill, box); e.Graphics.DrawRectangle(border, box);
		IEnumerable<Entry> inside = entries.Where(item => logicalBox.Contains(new Rectangle(item.X, item.Y, DisplaySize(item).Width, DisplaySize(item).Height)));
		HashSet<Entry> preview = selectionAdditive ? new HashSet<Entry>(selectionBase) : new HashSet<Entry>(); if (ctrlDragActive && selectionInitiator != null) preview.Add(selectionInitiator); preview.UnionWith(inside);
		string count = preview.Count == 1 ? "1 objeto" : $"{preview.Count} objetos"; Size badge = TextRenderer.MeasureText(count, Font);
		Rectangle badgeBounds = new Rectangle(box.Left, Math.Max(0, box.Top - badge.Height - 10), badge.Width + 14, badge.Height + 6);
		using var badgeFill = new SolidBrush(Color.FromArgb(225, 20, 25, 34)); using var badgeBorder = new Pen(Color.FromArgb(0, 220, 245), 1f); using var badgeText = new SolidBrush(Color.White);
		e.Graphics.FillRectangle(badgeFill, badgeBounds); e.Graphics.DrawRectangle(badgeBorder, badgeBounds); e.Graphics.DrawString(count, Font, badgeText, badgeBounds.Left + 7, badgeBounds.Top + 3);
	}

	private void RotateEntry(Entry item, int delta) => SetOrientation(item, (item.Orientation + delta + 360) % 360);

	private void SetOrientation(Entry item, int orientation)
	{
		Size before = DisplaySize(item); item.Orientation = ((orientation % 360) + 360) % 360; Size after = DisplaySize(item);
		item.X = Math.Max(0, item.X + (before.Width - after.Width) / 2); item.Y = Math.Max(0, item.Y + (before.Height - after.Height) / 2);
		Save(); Render();
	}

	private int CountOffscreenObjects()
	{
		int visibleWidth = canvas.ClientSize.Width;
		int visibleHeight = canvas.ClientSize.Height;
		return entries.Count(item => { Size size = DisplaySize(item); return item.X < 0 || item.Y < 0 || item.X + size.Width > visibleWidth || item.Y + size.Height > visibleHeight; });
	}

	private void UpdateOffscreenNotice()
	{
		if (offscreenNotice.IsDisposed || canvas.ClientSize.Width <= 0 || canvas.ClientSize.Height <= 0)
		{
			return;
		}
		List<Entry> list = entries.Where(item => { Size size = DisplaySize(item); return item.X < 0 || item.Y < 0 || item.X + size.Width > canvas.ClientSize.Width || item.Y + size.Height > canvas.ClientSize.Height; }).ToList();
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
			num = Math.Clamp(startBounds.Width + dx, tile.Item.Orientation % 180 == 0 ? 90 : 50, tile.Item.Orientation % 180 == 0 ? 900 : 600);
		}
		if ((tile.ResizeEdges & ResizeEdge.Bottom) != ResizeEdge.None)
		{
			num2 = Math.Clamp(startBounds.Height + dy, tile.Item.Orientation % 180 == 0 ? 50 : 90, tile.Item.Orientation % 180 == 0 ? 600 : 900);
		}
		if ((tile.ResizeEdges & ResizeEdge.Left) != ResizeEdge.None)
		{
			num = Math.Clamp(startBounds.Width - dx, tile.Item.Orientation % 180 == 0 ? 90 : 50, tile.Item.Orientation % 180 == 0 ? 900 : 600);
			val = startBounds.Right - num;
		}
		if ((tile.ResizeEdges & ResizeEdge.Top) != ResizeEdge.None)
		{
			num2 = Math.Clamp(startBounds.Height - dy, tile.Item.Orientation % 180 == 0 ? 50 : 90, tile.Item.Orientation % 180 == 0 ? 600 : 900);
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
		List<Entry> targets = selectedEntries.Count > 0 ? selectedEntries.Where(entries.Contains).ToList() : (selected == null ? new List<Entry>() : new List<Entry> { selected });
		if (targets.Count == 0) { MessageBox.Show(this, "Selecciona primero uno o varios objetos.", "AccessShot 2.15", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
		DeleteEntries(targets);
	}

	private void DeleteEntry(Entry item) => DeleteEntries(new[] { item });

	private void DeleteEntries(IEnumerable<Entry> candidates)
	{
		List<Entry> targets = candidates.Where(entries.Contains).Distinct().ToList();
		if (targets.Count == 0 || !ConfirmDeleteEntries(targets)) return;
		List<Entry> previousSelection = selectedEntries.ToList();
		string before = SerializeConfiguration();
		List<(Entry Item, int Index)> removed = targets.Select(item => (item, entries.IndexOf(item))).ToList();
		foreach (Entry item in targets) entries.Remove(item);
		string after = SerializeConfiguration();
		try { WriteUndoRecord(before, after); }
		catch (Exception ex)
		{
			foreach (var pair in removed.OrderBy(pair => pair.Index)) entries.Insert(Math.Min(pair.Index, entries.Count), pair.Item);
			MessageBox.Show(this, "No se eliminó nada porque no se pudo preparar la recuperación: " + ex.Message, "AccessShot 2.15", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		foreach (Entry item in targets) selectedEntries.Remove(item);
		selected = selectedEntries.LastOrDefault();
		if (!Save())
		{
			foreach (var pair in removed.OrderBy(pair => pair.Index)) entries.Insert(Math.Min(pair.Index, entries.Count), pair.Item);
			foreach (Entry item in previousSelection) selectedEntries.Add(item);
			selected = previousSelection.LastOrDefault();
			try { File.Delete(undoFile); } catch { }
			Render(); return;
		}
		Render();
	}

	private bool ConfirmDeleteEntries(List<Entry> targets)
	{
		bool all = targets.Count == entries.Count;
		int apps = targets.Count(item => item.Kind == EntryKind.Application), urls = targets.Count(item => item.Kind == EntryKind.Url), labels = targets.Count(item => item.Kind == EntryKind.Label);
		using Form dialog = new Form { Text = "Confirmar eliminación", Width = 560, Height = all ? 430 : 390, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, ShowInTaskbar = false };
		PictureBox warning = new PictureBox { Image = SystemIcons.Warning.ToBitmap(), SizeMode = PictureBoxSizeMode.StretchImage, Left = 18, Top = 18, Width = 36, Height = 36 };
		Label heading = new Label { Text = all ? $"Vas a vaciar el lienzo: {targets.Count} objetos" : $"Vas a eliminar {targets.Count} objetos del lienzo", Left = 66, Top = 18, Width = 460, Height = 32, Font = new Font("Segoe UI", 12f, FontStyle.Bold) };
		Label details = new Label { Text = $"{apps} aplicaciones · {urls} URL · {labels} etiquetas\r\nSe eliminan los accesos de AccesShot. Los programas y sus archivos permanecen en el PC.", Left = 66, Top = 56, Width = 460, Height = 45 };
		ListBox list = new ListBox { Left = 66, Top = 105, Width = 460, Height = all ? 190 : 175 };
		foreach (Entry item in targets) list.Items.Add($"{KindName(item.Kind)} — {item.Title}");
		CheckBox confirmAll = new CheckBox { Text = "Confirmo que quiero vaciar todo el lienzo", Left = 66, Top = 305, Width = 400, Visible = all };
		Button cancel = new Button { Text = "Cancelar", Left = 270, Top = all ? 350 : 320, Width = 95, Height = 30, DialogResult = DialogResult.Cancel };
		Button delete = new Button { Text = $"Eliminar estos {targets.Count} objetos", Left = 372, Top = cancel.Top, Width = 160, Height = 30, DialogResult = DialogResult.Yes, Enabled = !all };
		confirmAll.CheckedChanged += delegate { delete.Enabled = confirmAll.Checked; };
		dialog.Controls.AddRange(new Control[] { warning, heading, details, list, confirmAll, cancel, delete }); dialog.CancelButton = cancel; dialog.Shown += delegate { cancel.Focus(); };
		return dialog.ShowDialog(this) == DialogResult.Yes;
	}

	private static string KindName(EntryKind kind) => kind switch { EntryKind.Application => "Aplicación", EntryKind.Url => "URL", _ => "Etiqueta" };

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
