using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Drawing;
public class Entry { public int Kind {get;set;} public string Title {get;set;}="Objeto"; public int Background {get;set;}=unchecked((int)0xff2d3c52); public int Foreground {get;set;}=unchecked((int)0xffffffff); }
public class FakeForm { public List<Entry> entries=new();public string dataFile="";public bool dark=true; }
class Tests {
 static int checks;
 static void Eq(bool x,string reason){checks++;if(!x)throw new Exception(reason);}
 static void Main() {
  foreach(int kind in new[]{0,1,2}) {
   var e=new Entry{Kind=kind};var form=new FakeForm{entries=new(){e}};
   Eq(StylePolicy.State(e).ThemeColors,"legacy default detected");
   var dark=StylePolicy.Colors(e,true,false,false);var light=StylePolicy.Colors(e,false,false,false);
   Eq(dark.Background!=light.Background,"theme switching");
   Eq(StylePolicy.Colors(e,true,true,false).Background!=dark.Background,"themed hover");
   StylePolicy.PrepareCustom(form,e);
   Eq(StylePolicy.State(e).ThemeColors,"cancelled dialog retains default ownership");
   Eq(StylePolicy.Colors(e,false,false,false).Background==light.Background,"cancel then theme switch");
   e.Foreground=Color.Lime.ToArgb();StylePolicy.MarkCustom(form,e);
   Eq(!StylePolicy.State(e).ThemeColors,"custom text loses default ownership");
   Eq(StylePolicy.Colors(e,true,false,false)==StylePolicy.Colors(e,false,false,false),"custom ignores both themes");
   Eq(StylePolicy.Colors(e,true,true,false).Background==e.BackgroundColor(),"custom hover inherits object");
   StylePolicy.State(e).HoverBackground=Color.Blue.ToArgb();
   Eq(StylePolicy.Colors(e,true,true,false).Background.ToArgb()==Color.Blue.ToArgb(),"custom hover independent");
   var h=StylePolicy.Colors(e,true,true,false);Eq(StylePolicy.Contrast(h.Background,h.Text)>=4.5,"hover text contrast");
   StylePolicy.State(e).HoverBackground=null;
   Eq(StylePolicy.Colors(e,true,true,false).Background==e.BackgroundColor(),"hover reset");
  }
  // Personalization remains sticky even if the chosen values equal legacy defaults.
  var same=new Entry();StylePolicy.State(same).ThemeColors=false;
  Eq(!StylePolicy.State(same).ThemeColors,"explicit custom overrides color inference");
  var previouslyCustom=new Entry{Background=Color.Red.ToArgb()};Eq(!StylePolicy.State(previouslyCustom).ThemeColors,"legacy custom preserved");
  var def=new Entry{Kind=1};StylePolicy.State(def).HoverBackground=Color.Gold.ToArgb();
  Eq(StylePolicy.State(def).ThemeColors,"hover override preserves theme ownership");
  var dir=Path.Combine(Path.GetTempPath(),"accessshot211-tests-"+Guid.NewGuid());Directory.CreateDirectory(dir);
  try {
   var f=new FakeForm{dataFile=Path.Combine(dir,"entries.json"),entries=new(){same,previouslyCustom,def}};
   File.WriteAllText(f.dataFile,JsonSerializer.Serialize(f.entries));StylePolicy.Save(f);
   f.entries=JsonSerializer.Deserialize<List<Entry>>(File.ReadAllText(f.dataFile))!;StylePolicy.Load(f);
   Eq(!StylePolicy.State(f.entries[0]).ThemeColors,"sticky property survives save/load");
   Eq(!StylePolicy.State(f.entries[1]).ThemeColors,"custom color survives save/load");
   Eq(StylePolicy.State(f.entries[2]).ThemeColors,"themed object survives save/load");
   Eq(StylePolicy.State(f.entries[2]).HoverBackground==Color.Gold.ToArgb(),"hover persists");
   f.entries.Reverse();File.WriteAllText(f.dataFile,JsonSerializer.Serialize(f.entries));StylePolicy.Save(f);
   f.entries=JsonSerializer.Deserialize<List<Entry>>(File.ReadAllText(f.dataFile))!;StylePolicy.Load(f);
   Eq(StylePolicy.State(f.entries[0]).HoverBackground==Color.Gold.ToArgb(),"layer ordering preserves style association");
  }finally{Directory.Delete(dir,true);}
  foreach(bool dark in new[]{true,false})foreach(bool hot in new[]{true,false})foreach(int kind in new[]{0,1,2}) {
   var v=StylePolicy.Theme(kind,dark,hot);Eq(StylePolicy.Contrast(v.Background,v.Text)>=4.5,"palette contrast");
  }
  Console.WriteLine("PASS: "+checks+" policy, theme/custom ownership, cancel, hover, JSON persistence, layer order and contrast checks.");
 }
}
static class Ext{public static Color BackgroundColor(this Entry e)=>Color.FromArgb(e.Background);}
