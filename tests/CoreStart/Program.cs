using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "AccessShot-2.13-harness-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        Console.WriteLine($"TestPath={path}");
        Environment.SetEnvironmentVariable("ACCESS_SHOT_DATA_DIR", path);
        try
        {
            var assembly = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "AccessShotCore.dll"));
            var formType = assembly.GetType("AccessShot.MainForm", true)!;
            using var form = (Form)Activator.CreateInstance(formType, true)!;
            Console.WriteLine($"DataFile={formType.GetField("dataFile", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)}");
            var entries = (IList)formType.GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            Console.WriteLine($"Entries={entries.Count}");
            foreach (var entry in entries)
            {
                var type = entry!.GetType();
                Console.WriteLine($"{type.GetProperty("Kind")!.GetValue(entry)} {type.GetProperty("Title")!.GetValue(entry)} {type.GetProperty("X")!.GetValue(entry)} {type.GetProperty("Width")!.GetValue(entry)}x{type.GetProperty("Height")!.GetValue(entry)}");
            }
            if (entries.Count != 4) throw new Exception("Starter count mismatch");
            form.Opacity = 0;
            form.ShowInTaskbar = false;
            form.Show();
            Application.DoEvents();
            var canvas = (Panel)formType.GetField("canvas", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            canvas.Size = new System.Drawing.Size(1103, 516);
            var count = (int)formType.GetMethod("CountOffscreenObjects", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, null)!;
            Console.WriteLine($"Offscreen={count}");
            if (count != 2) throw new Exception("Offscreen count mismatch");
            var notice = (Panel)formType.GetField("offscreenNotice", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            Console.WriteLine($"NoticeVisible={notice.Visible}");
            if (!notice.Visible) throw new Exception("Offscreen notice hidden");
            var noticeText = (Label)formType.GetField("offscreenText", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            if (!noticeText.Text.Contains("Hay 2 objetos")) throw new Exception("Incorrect notice text" );
            Console.WriteLine($"NoticeText={noticeText.Text.Replace(Environment.NewLine, " | ")}");
            formType.GetMethod("Save", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(form, null);
            if (!File.Exists(Path.Combine(path, "entries.json"))) throw new Exception("Layout was not saved");
            form.Close();
            using var second = (Form)Activator.CreateInstance(formType, true)!;
            var existing = (IList)formType.GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(second)!;
            Console.WriteLine($"ReloadedEntries={existing.Count}");
            if (existing.Count != 4) throw new Exception("Existing layout was replaced");
            second.Close();
            File.WriteAllText(Path.Combine(path, "entries.json"), "[{\"Kind\":2,\"Title\":\"MI ETIQUETA\",\"X\":40,\"Y\":60,\"Width\":120,\"Height\":50}]");
            using var third = (Form)Activator.CreateInstance(formType, true)!;
            var preserved = (IList)formType.GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(third)!;
            if (preserved.Count != 1 || (string)preserved[0]!.GetType().GetProperty("Title")!.GetValue(preserved[0])! != "MI ETIQUETA") throw new Exception("User layout was replaced");
            Console.WriteLine("ExistingLayoutPreserved=True");
            third.Close();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ACCESS_SHOT_DATA_DIR", null);
        }
    }
}
