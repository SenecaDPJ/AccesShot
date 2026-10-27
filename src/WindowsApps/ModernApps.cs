// SPDX-License-Identifier: MIT
// Copyright (c) 2026 David De Prado (Seneca DPJ)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
[assembly:AssemblyVersion("2.12.0.0")]
public sealed record WindowsApp(string Name,string Id) {
 public bool Modern=>Id.Contains('!');
 public string SearchText {
  get {
   string extra="";string s=Id.ToLowerInvariant();
   if(s.Contains("photos"))extra+=" Fotos Photos";
   if(s.Contains("screensketch")||s.Contains("snippingtool"))extra+=" Recortes Herramienta de recorte Snipping Tool";
   if(s.Contains("yourphone")||s.Contains("phonelink"))extra+=" Enlace móvil Phone Link";
   if(s.Contains("whatsapp"))extra+=" WhatsApp";
   return Name+" "+Id+extra;
  }
 }
 public bool Matches(string query)=>CultureInfo.GetCultureInfo("es-ES").CompareInfo.IndexOf(SearchText,query.Trim(),CompareOptions.IgnoreCase|CompareOptions.IgnoreNonSpace)>=0;
}
public static class AppTargets {
 public const string Prefix="shell:AppsFolder\\";
 public static bool IsShellTarget(string? target) {
  if(target==null || !target.StartsWith(Prefix,StringComparison.OrdinalIgnoreCase))return false;
  string id=target.Substring(Prefix.Length);
  return id.Length>0 && id.Length<=1024 && !id.Any(c=>char.IsControl(c)||c=='"'||c=='<'||c=='>'||c=='|'||c=='/'||c=='\\');
 }
 public static bool Exists(string target) {
  if(!IsShellTarget(target))return File.Exists(target);
  string id=target.Substring(Prefix.Length);
  return WindowsAppCatalog.Get().Any(a=>string.Equals(a.Id,id,StringComparison.OrdinalIgnoreCase));
 }
 public static ProcessStartInfo CreateStartInfo(string target) {
  if(target.StartsWith(Prefix,StringComparison.OrdinalIgnoreCase)) {
   if(!IsShellTarget(target))throw new ArgumentException("El acceso de aplicación de Windows no es válido.");
   var info=new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"explorer.exe")){UseShellExecute=true};
   info.ArgumentList.Add(Prefix+target.Substring(Prefix.Length));return info;
  }
  return new ProcessStartInfo(target);
 }
}
public static class WindowsAppCatalog {
 static readonly object gate=new();static List<WindowsApp>? cache;
 public static List<WindowsApp> Get(bool refresh=false) {
  lock(gate) {if(cache!=null&&!refresh)return new(cache);}
  List<WindowsApp> list=Read();lock(gate){cache=list;}return new(list);
 }
 static void Release(object? o){if(o!=null&&Marshal.IsComObject(o))Marshal.FinalReleaseComObject(o);}
 static List<WindowsApp> Read() {
  object? shell=null,folder=null,items=null;var result=new Dictionary<string,WindowsApp>(StringComparer.OrdinalIgnoreCase);
  try {
   Type? type=Type.GetTypeFromProgID("Shell.Application");if(type==null)throw new InvalidOperationException("Windows no ofrece el catálogo de aplicaciones.");
   shell=Activator.CreateInstance(type)!;folder=((dynamic)shell).NameSpace("shell:::{4234d49b-0245-4df3-b780-3893943456e1}");
   if(folder==null)throw new InvalidOperationException("No se pudo abrir el catálogo de aplicaciones de Windows.");
   items=((dynamic)folder).Items();int count=((dynamic)items).Count;
   for(int i=0;i<count;i++) {
    object? item=null;
    try {
     item=((dynamic)items).Item(i);string name=Convert.ToString(((dynamic)item).Name)??"";
     string id=Convert.ToString(((dynamic)item).Path)??"";
     if(name.Length>0&&AppTargets.IsShellTarget(AppTargets.Prefix+id))result[id]=new WindowsApp(name,id);
    }catch(COMException){}finally{Release(item);}
   }
  }finally{Release(items);Release(folder);Release(shell);}
  return result.Values.OrderByDescending(a=>a.Modern).ThenBy(a=>a.Name,StringComparer.CurrentCultureIgnoreCase).ToList();
 }
}
public static class WindowsAppsPicker {
 const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static object Field(object o,string n)=>o.GetType().GetField(n,Flags)!.GetValue(o)!;
 public static void Attach(object editContext) {
  if(Convert.ToInt32(Field(editContext,"kind"))!=0)return;
  Form dialog=(Form)Field(editContext,"dialog");TextBox title=(TextBox)Field(editContext,"title"),target=(TextBox)Field(editContext,"target");
  object form=Field(editContext,"<>4__this");bool dark=(bool)Field(form,"dark");
  var button=new Button {Text="Buscar apps de Windows…",Left=105,Top=93,Width=380,Height=29,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(255,240,232),ForeColor=Color.FromArgb(145,55,9)};
  button.FlatAppearance.BorderColor=Color.FromArgb(215,101,50);
  button.Click+=(_,_)=>{
   using var picker=new Picker(dark);
   if(picker.ShowDialog(dialog)!=DialogResult.OK||picker.SelectedApp==null)return;
   target.Text=AppTargets.Prefix+picker.SelectedApp.Id;
   if(string.IsNullOrWhiteSpace(title.Text))title.Text=picker.SelectedApp.Name;
  };
  dialog.Controls.Add(button);
 }
 sealed class Picker:Form {
  readonly TextBox search=new(){Dock=DockStyle.Fill};
  readonly CheckBox modern=new(){Text="Solo apps modernas",AutoSize=true,Checked=true};
  readonly ListView list=new(){Dock=DockStyle.Fill,View=View.Details,FullRowSelect=true,MultiSelect=false,HideSelection=false};
  readonly Label status=new(){AutoSize=true,Text="Consultando aplicaciones instaladas…"};
  readonly Button accept=new(){Text="Seleccionar",Width=120,Enabled=false};
  readonly Button refresh=new(){Text="Actualizar",Width=100};
  List<WindowsApp> apps=new();
  public WindowsApp? SelectedApp {get;private set;}
  public Picker(bool dark) {
   Text="Buscar apps de Windows · AccessShot 2.12";Width=800;Height=590;MinimumSize=new Size(660,430);StartPosition=FormStartPosition.CenterParent;Font=new Font("Segoe UI",10);
   Color bg=dark?Color.FromArgb(20,23,30):Color.FromArgb(235,240,245),fg=dark?Color.FromArgb(237,242,252):Color.FromArgb(28,45,67),surface=dark?Color.FromArgb(32,38,49):Color.White;
   BackColor=bg;ForeColor=fg;search.BackColor=surface;search.ForeColor=fg;list.BackColor=surface;list.ForeColor=fg;
   var layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(16),ColumnCount=1,RowCount=7};
   layout.RowStyles.Add(new RowStyle(SizeType.Absolute,30));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,40));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,42));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,32));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,34));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,42));
   layout.Controls.Add(new Label{Text="Busca aplicaciones instaladas en este equipo",AutoSize=true,Font=new Font(Font,FontStyle.Bold)},0,0);
   var top=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2};top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,108));top.Controls.Add(search,0,0);top.Controls.Add(refresh,1,0);layout.Controls.Add(top,0,1);
   var quick=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false};
   foreach(string name in new[]{"Todas","Fotos","Recortes","Enlace móvil","WhatsApp"}) {
    var b=new Button{Text=name,AutoSize=true,Height=31};
    b.Click+=(_,_)=>{if(name=="WhatsApp")modern.Checked=false;search.Text=name=="Todas"?"":name;};quick.Controls.Add(b);
   }
   layout.Controls.Add(quick,0,2);layout.Controls.Add(modern,0,3);
   list.Columns.Add("Aplicación",490);list.Columns.Add("Tipo",180);layout.Controls.Add(list,0,4);layout.Controls.Add(status,0,5);
   var bottom=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft};
   var cancel=new Button{Text="Cancelar",Width=110,DialogResult=DialogResult.Cancel};bottom.Controls.Add(cancel);bottom.Controls.Add(accept);layout.Controls.Add(bottom,0,6);Controls.Add(layout);CancelButton=cancel;AcceptButton=accept;
   search.TextChanged+=(_,_)=>Filter();modern.CheckedChanged+=(_,_)=>Filter();refresh.Click+=(_,_)=>LoadApps(true);
   list.SelectedIndexChanged+=(_,_)=>accept.Enabled=list.SelectedItems.Count==1;
   accept.Click+=(_,_)=>Choose();list.DoubleClick+=(_,_)=>Choose();Shown+=(_,_)=>{search.Focus();LoadApps(false);};
  }
  void Choose(){if(list.SelectedItems.Count!=1)return;SelectedApp=(WindowsApp)list.SelectedItems[0].Tag!;DialogResult=DialogResult.OK;Close();}
  void Filter() {
   if(refresh.Enabled==false)return;list.BeginUpdate();list.Items.Clear();
   foreach(var a in apps.Where(a=>(!modern.Checked||a.Modern)&&a.Matches(search.Text))) {
    var row=new ListViewItem(a.Name){Tag=a};row.SubItems.Add(a.Modern?"App moderna":"Aplicación clásica");list.Items.Add(row);
   }
   list.EndUpdate();accept.Enabled=false;status.Text=$"{list.Items.Count} aplicaciones · Solo se muestran las instaladas para este usuario.";
  }
  void Post(Action action) {
   try {if(!IsDisposed&&IsHandleCreated)BeginInvoke((Action)(()=>{if(!IsDisposed)action();}));}
   catch(InvalidOperationException){}
  }
  void LoadApps(bool force) {
   refresh.Enabled=false;accept.Enabled=false;status.Text="Consultando aplicaciones instaladas…";
   var thread=new Thread(()=>{
    try {
     var found=WindowsAppCatalog.Get(force);
     Post(()=>{apps=found;refresh.Enabled=true;Filter();});
    }catch(Exception ex) {
     Post(()=>{refresh.Enabled=true;status.Text="No se pudo consultar Windows: "+ex.Message;});
    }
   });thread.IsBackground=true;thread.SetApartmentState(ApartmentState.STA);thread.Start();
  }
 }
}
