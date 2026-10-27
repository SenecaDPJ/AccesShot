// Native Windows x64 launcher. No CRT and no .NET requirement for the launcher.
typedef unsigned short W;
typedef unsigned long D;
typedef void *H;
// Windows DWORD is always 32 bits, including on x64.
typedef unsigned int U32;
typedef struct {U32 cb; W *reserved,*desktop,*title; U32 x,y,xsize,ysize,xchars,ychars,fill,flags; unsigned short show,reserved2; unsigned char *reservedptr; H in,out,err;} SI;
typedef struct {H process,thread; U32 pid,tid;} PI;
__attribute__((section(".launch"),aligned(8))) unsigned char imports[2048]={0};
__attribute__((section(".rsrc"),aligned(8))) unsigned char resources[131072]={0};
typedef U32 (*GetName)(H,W*,U32);
typedef int (*CreateProc)(const W*,W*,void*,void*,int,U32,void*,const W*,SI*,PI*);
typedef int (*Close)(H);
typedef void (*Exit)(U32);
typedef int (*Message)(H,const W*,const W*,U32);
static void copy(W *dst,const W *src) {while((*dst++=*src++)!=0){} }
void start(void) {
 static W path[32768],cwd[32768],cmd[32770]; SI si; PI pi;
 U32 n=((GetName)(*(void**)(imports+512)))(0,path,32768);
 if(!n || n>=32768) goto fail;
 U32 pos=n; while(pos && path[pos-1]!='\\') --pos;
 if(!pos || pos+30>=32768) goto fail;
 copy(path+pos,(const W*)L"Aplicacion\\AccessShot.exe");
 for(U32 i=0;i<pos;i++)cwd[i]=path[i];
 copy(cwd+pos,(const W*)L"Aplicacion");
 cmd[0]='"';U32 j=0;while(path[j]){cmd[j+1]=path[j];j++;}cmd[j+1]='"';cmd[j+2]=0;
 for(U32 i=0;i<sizeof(si);i++)((volatile unsigned char*)&si)[i]=0;
 for(U32 i=0;i<sizeof(pi);i++)((volatile unsigned char*)&pi)[i]=0;
 si.cb=sizeof(si);
 if(!((CreateProc)(*(void**)(imports+520)))(path,cmd,0,0,0,0,0,cwd,&si,&pi))goto fail;
 ((Close)(*(void**)(imports+528)))(pi.thread);
 ((Close)(*(void**)(imports+528)))(pi.process);
 ((Exit)(*(void**)(imports+536)))(0);
fail:
 ((Message)(*(void**)(imports+560)))(0,(const W*)L"No se pudo iniciar AccessShot. Extrae todo el ZIP y conserva la carpeta Aplicacion junto a AccessShot.exe.",(const W*)L"AccessShot 2.12",0x10);
 ((Exit)(*(void**)(imports+536)))(1);
}
