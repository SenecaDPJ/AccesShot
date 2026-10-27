// SPDX-License-Identifier: MIT
// Copyright (c) 2026 David De Prado (Seneca DPJ)
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
public static class AccessShotVisuals {
 sealed class TileState {public bool Hover;public int Width=-1,Height=-1,Kind=-1;}
 static readonly ConditionalWeakTable<Control,TileState> tiles=new();
 static TileState State(Control c)=>tiles.GetValue(c,_=>new());
 public static GraphicsPath Shape(RectangleF r,int kind) {
  var p=new GraphicsPath();float x=r.X,y=r.Y,w=r.Width,h=r.Height;
  if(kind==1) {
   float tip=Math.Min(24,h*.36f);
   p.AddPolygon(new[]{new PointF(x,y),new PointF(x+w-tip,y),new PointF(x+w,y+h/2),new PointF(x+w-tip,y+h),new PointF(x,y+h)});
  } else {
   float diameter=Math.Min(kind==2?h:26,Math.Min(w,h));
   if(diameter<1){p.AddRectangle(r);return p;}
   p.AddArc(x,y,diameter,diameter,180,90);p.AddArc(x+w-diameter,y,diameter,diameter,270,90);
   p.AddArc(x+w-diameter,y+h-diameter,diameter,diameter,0,90);p.AddArc(x,y+h-diameter,diameter,diameter,90,90);p.CloseFigure();
  }
  return p;
 }
 public static void Paint(Control tile,PaintEventArgs args,object entry,bool dark,bool selected) {
  if(tile.Width<4 || tile.Height<4)return;
  TileState state=State(tile);int kind=StylePolicy.Kind(entry);
  if(state.Width!=tile.Width || state.Height!=tile.Height || state.Kind!=kind) {
   state.Width=tile.Width;state.Height=tile.Height;state.Kind=kind;
   using var clip=Shape(new RectangleF(0,0,tile.Width,tile.Height),kind);tile.Region=new Region(clip);
  }
  Graphics g=args.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
  TileColors colors=StylePolicy.Colors(entry,dark,state.Hover,selected);
  using var path=Shape(new RectangleF(1,1,tile.Width-3,tile.Height-3),kind);
  using var brush=new SolidBrush(colors.Background);using var pen=new Pen(colors.Border,selected?2.5f:1.2f);
  g.FillPath(brush,path);g.DrawPath(pen,path);
  string title=StylePolicy.Get<string>(entry,"Title");
  float size=Math.Clamp(StylePolicy.Get<int>(entry,"FontSize"),8,48);Font? font=null;
  int tip=kind==1?(int)Math.Min(24,tile.Height*.36f):0;
  var bounds=new RectangleF(10,6,Math.Max(1,tile.Width-20-tip/2),Math.Max(1,tile.Height-12));
  while(true) {
   font?.Dispose();font=new Font("Segoe UI",size,FontStyle.Bold,GraphicsUnit.Point);
   var measure=g.MeasureString(title,font,(int)bounds.Width);
   if((measure.Width<=bounds.Width && measure.Height<=bounds.Height)||size<=8)break;size-=1;
  }
  using(font)using(var textBrush=new SolidBrush(colors.Text))using(var format=new StringFormat {Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center,Trimming=StringTrimming.EllipsisCharacter})g.DrawString(title,font,textBrush,bounds,format);
 }
 static void Refresh(object form) {StylePolicy.Invoke(form,"Save");StylePolicy.Invoke(form,"Render");}
 public static void Attach(ContextMenuStrip menu,object form,object entry,Control tile) {
  State(tile);tile.MouseEnter+=(_,_)=>{State(tile).Hover=true;tile.Invalidate();};
  tile.MouseLeave+=(_,_)=>{State(tile).Hover=false;tile.Invalidate();};
  menu.Items.Add(new ToolStripSeparator());
  menu.Items.Add("Color al pasar el ratón…",null,(_,_)=>{
   var current=StylePolicy.Colors(entry,(bool)StylePolicy.Field(form,"dark"),true,false);
   using var dialog=new ColorDialog {Color=current.Background,FullOpen=true,AnyColor=true};
   if(dialog.ShowDialog((IWin32Window)form)!=DialogResult.OK)return;
   StylePolicy.State(entry).HoverBackground=dialog.Color.ToArgb();Refresh(form);
  });
  var reset=menu.Items.Add("Restablecer color al pasar el ratón",null,(_,_)=>{StylePolicy.State(entry).HoverBackground=null;Refresh(form);});
  reset.Enabled=StylePolicy.State(entry).HoverBackground.HasValue;
  menu.Items.Add("Restaurar colores del tema",null,(_,_)=>{
   var s=StylePolicy.State(entry);s.ThemeColors=true;s.HoverBackground=null;Refresh(form);
  });
 }
 public static void Load(object form) {
  try {StylePolicy.Load(form);}catch(Exception ex) {MessageBox.Show((IWin32Window)form,"No se pudieron leer los estilos guardados: "+ex.Message,"AccessShot 2.11",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
 }
}
