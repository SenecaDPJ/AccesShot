using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
class Tests {
 static int checks;
 static void Check(bool b,string why){checks++;if(!b)throw new Exception(why);}
 static void Main(){
  var cases=new[]{
   new WindowsApp("Microsoft Photos","Microsoft.Windows.Photos_8wekyb3d8bbwe!App"),
   new WindowsApp("Snipping Tool","Microsoft.ScreenSketch_8wekyb3d8bbwe!App"),
   new WindowsApp("Phone Link","Microsoft.YourPhone_8wekyb3d8bbwe!App"),
   new WindowsApp("WhatsApp","5319275A.WhatsAppDesktop_cv1g1gvanyjgm!App")};
  var queries=new[]{"Fotos","Recortes","Enlace movil","whatsapp"};
  typeof(WindowsAppCatalog).GetField("cache",BindingFlags.Static|BindingFlags.NonPublic)!.SetValue(null,new List<WindowsApp>(cases));
  for(int i=0;i<cases.Length;i++){
   var app=cases[i];Check(app.Modern,"MSIX recognized");Check(app.Matches(queries[i]),"localized search alias");
   Check(app.Matches(queries[i].ToUpperInvariant()),"case-insensitive search");Check(!app.Matches("noExiste123"),"unmatched query");
   string target=AppTargets.Prefix+app.Id;Check(AppTargets.IsShellTarget(target),"valid AUMID");Check(AppTargets.Exists(target),"installed lookup");
   var p=AppTargets.CreateStartInfo(target);Check(p.FileName.EndsWith("explorer.exe"),"Windows launcher");
   Check(p.ArgumentList.Count==1 && p.ArgumentList[0]==target,"one literal shell target argument");Check(p.UseShellExecute,"shell execution");
  }
  Check(!AppTargets.Exists(AppTargets.Prefix+"notInstalled!App"),"uninstalled app rejected");
  foreach(var target in new string?[]{null,"shell:AppsFolder\\","shell:AppsFolder\\x\ny","shell:AppsFolder\\x\"y","shell:AppsFolder\\..\\app.exe","shell:AppsFolder\\x|y","https://blacklab.set"})Check(!AppTargets.IsShellTarget(target),"invalid target rejected");
  string file=Path.GetTempFileName();try{Check(AppTargets.Exists(file),"classic file still accepted");Check(AppTargets.CreateStartInfo(file).FileName==file,"classic launch preserved");}finally{File.Delete(file);}
  Check(AppTargets.CreateStartInfo("https://blacklab.set").FileName=="https://blacklab.set","URL launch preserved");
  Check(new WindowsApp("WhatsApp Desktop","WhatsAppDesktop").Matches("WhatsApp"),"classic app can match search");
  Console.WriteLine("PASS: "+checks+" installed-app lookup, search aliases, targets, launch arguments and regression checks.");
 }
}
