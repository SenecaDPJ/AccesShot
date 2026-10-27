// SPDX-License-Identifier: MIT
// Copyright (c) 2026 David De Prado (Seneca DPJ)
using System;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Diagnostics.CodeAnalysis;
[assembly: System.Reflection.AssemblyVersion("2.11.0.0")]
public sealed class CreationStyle {
 public bool ThemeColors {get;set;}
 public int? HoverBackground {get;set;}
}
public readonly record struct TileColors(Color Background,Color Text,Color Border);
public static class StylePolicy {
 static readonly ConditionalWeakTable<object,CreationStyle> states=new();
 const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 public static object Field(object o,string n)=>o.GetType().GetField(n,Flags)!.GetValue(o)!;
 public static T Get<T>(object o,string n)=>(T)o.GetType().GetProperty(n)!.GetValue(o)!;
 public static void Set(object o,string n,object v)=>o.GetType().GetProperty(n)!.SetValue(o,v);
 public static void Invoke(object o,string n)=>o.GetType().GetMethod(n,Flags)!.Invoke(o,null);
 public static int Kind(object e)=>Convert.ToInt32(e.GetType().GetProperty("Kind")!.GetValue(e));
 public static CreationStyle State(object e)=>states.GetValue(e,k=>new CreationStyle {ThemeColors=Get<int>(k,"Background")==unchecked((int)0xff2d3c52) && Get<int>(k,"Foreground")==unchecked((int)0xffffffff)});
 static Color C(string s)=>Color.FromArgb(unchecked((int)(0xff000000|Convert.ToUInt32(s,16))));
 static readonly string[,,] palettes={
  {{"432A22","FFB28E","B66846","623A2B","FFDDCE","FF8E5C"},{"3D263B","FFA8F2","A860A0","5B3155","FFD5FA","F780E9"},{"173B43","7BEAFF","368D9D","1D5360","D4F9FF","43D9F3"}},
  {{"FFF0E8","913709","D76532","FFE0CF","772B07","D44B11"},{"FFEDFB","8E247F","C445B4","FFD7F5","761B69","B42D9F"},{"E5FAFF","006178","1E91AA","C8F3FC","004D61","007E98"}}
 };
 public static TileColors Theme(int kind,bool dark,bool hot) {
  int t=dark?0:1,k=kind==0?0:kind==2?1:2,s=hot?3:0;
  return new(C(palettes[t,k,s]),C(palettes[t,k,s+1]),C(palettes[t,k,s+2]));
 }
 static Color Mix(Color a,Color b,double f)=>Color.FromArgb((int)(a.R+(b.R-a.R)*f),(int)(a.G+(b.G-a.G)*f),(int)(a.B+(b.B-a.B)*f));
 static double Lum(Color c) {double F(byte b){double v=b/255d;return v<=.04045?v/12.92:Math.Pow((v+.055)/1.055,2.4);}return .2126*F(c.R)+.7152*F(c.G)+.0722*F(c.B);}
 public static double Contrast(Color a,Color b)=>(Math.Max(Lum(a),Lum(b))+.05)/(Math.Min(Lum(a),Lum(b))+.05);
 public static TileColors Colors(object e,bool dark,bool hover,bool selected) {
  CreationStyle s=State(e);TileColors v;
  if(s.ThemeColors)v=Theme(Kind(e),dark,hover||selected);
  else {Color bg=Color.FromArgb(Get<int>(e,"Background")),fg=Color.FromArgb(Get<int>(e,"Foreground"));v=new(bg,fg,selected?Color.White:Kind(e)==2?Color.FromArgb(100,160,255):Color.FromArgb(70,90,120));}
  if(hover && s.HoverBackground is int argb) {
   Color bg=Color.FromArgb(argb),fg=v.Text;
   if(Contrast(bg,fg)<4.5)fg=Contrast(bg,Color.Black)>=Contrast(bg,Color.White)?Color.Black:Color.White;
   v=new(bg,fg,v.Border);
  }
  return v;
 }
 public static void PrepareCustom(object form,object e) {
  if(!State(e).ThemeColors)return;
  var v=Theme(Kind(e),(bool)Field(form,"dark"),false);
  Set(e,"Background",v.Background.ToArgb());Set(e,"Foreground",v.Text.ToArgb());
 }
 public static void MarkCustom(object form,object e)=>State(e).ThemeColors=false;
 public static void Load(object form) {
  IList entries=(IList)Field(form,"entries");string path=(string)Field(form,"dataFile");
  if(!File.Exists(path))return;
  JsonArray? arr=JsonNode.Parse(File.ReadAllText(path)) as JsonArray;
  if(arr==null || arr.Count!=entries.Count)return;
  for(int i=0;i<arr.Count;i++) {
   object e=entries[i]!;CreationStyle s=State(e);
   if(arr[i]?["__AccessShotStyle"] is JsonObject meta && meta["Version"]?.GetValue<int>()==1) {
    s.ThemeColors=meta["ThemeColors"]?.GetValue<bool>()??s.ThemeColors;
    s.HoverBackground=meta["HoverBackground"]?.GetValue<int>();
   }
  }
 }
 public static void Save(object form) {
  IList entries=(IList)Field(form,"entries");string path=(string)Field(form,"dataFile");
  JsonArray arr=(JsonArray)JsonNode.Parse(File.ReadAllText(path))!;
  if(arr.Count!=entries.Count)throw new InvalidDataException("No coincide el número de estilos y objetos.");
  for(int i=0;i<arr.Count;i++) {
   CreationStyle s=State(entries[i]!);
   arr[i]!["__AccessShotStyle"]=new JsonObject { ["Version"]=1,["ThemeColors"]=s.ThemeColors,["HoverBackground"]=s.HoverBackground};
  }
  string temp=path+".style.tmp";
  File.WriteAllText(temp,arr.ToJsonString(new JsonSerializerOptions {WriteIndented=true}));File.Move(temp,path,true);
 }
}
