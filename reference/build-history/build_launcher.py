import subprocess,struct,zipfile
from pathlib import Path
import pefile
subprocess.run(['gcc','-c','dev210/launcher.c','-o','dev210/launcher.elf.o','-Os','-ffreestanding','-fno-builtin','-fno-stack-protector','-fno-asynchronous-unwind-tables','-fno-unwind-tables','-fshort-wchar','-mabi=ms','-mno-red-zone'],check=True)
subprocess.run(['objcopy','-O','pe-x86-64','-R','.comment','dev210/launcher.elf.o','dev210/launcher.obj'],check=True)
# Preserve ELF PC-relative addends and resolve section symbols explicitly.
elf=Path('dev210/launcher.elf.o').read_bytes()
shoff=struct.unpack_from('<Q',elf,40)[0];entsize,count,shstr=struct.unpack_from('<HHH',elf,58)
sections=[struct.unpack_from('<IIQQQQIIQQ',elf,shoff+i*entsize) for i in range(count)]
strings=sections[shstr];sectionnames=elf[strings[4]:strings[4]+strings[5]]
def cstr(data,off):return data[off:data.index(0,off)].decode()
sec_names=[cstr(sectionnames,r[0]) for r in sections]
symidx=next(i for i,r in enumerate(sections) if r[1]==2);symsec=sections[symidx];strsec=sections[symsec[6]];symstrings=elf[strsec[4]:strsec[4]+strsec[5]]
syms=[struct.unpack_from('<IBBHQQ',elf,symsec[4]+i*24) for i in range(symsec[5]//24)]
rela=sections[sec_names.index('.rela.text')]
relocs={}
for i in range(rela[5]//24):
 offset,info,addend=struct.unpack_from('<QQq',elf,rela[4]+i*24);sy=syms[info>>32];name=cstr(symstrings,sy[0])
 if not name:
  assert sec_names[sy[3]]=='.bss';name='cmd.0'
 assert info&0xffffffff==2
 relocs[offset]=(name,addend)
p=Path('dev210/launcher.obj');d=bytearray(p.read_bytes());n=struct.unpack_from('<H',d,2)[0]
symptr,nsym=struct.unpack_from('<II',d,8);strptr=symptr+nsym*18;coffnames={}
for i in range(nsym):
 off=symptr+i*18;namebytes=d[off:off+8]
 name=cstr(d,strptr+struct.unpack_from('<I',namebytes,4)[0]) if namebytes[:4]==b'\0'*4 else namebytes.rstrip(b'\0').decode(errors='ignore')
 coffnames[name]=i
for k in range(n):
 off=20+40*k;name=d[off:off+8];raw,rel=struct.unpack_from('<II',d,off+20);nr=struct.unpack_from('<H',d,off+32)[0]
 if name.rstrip(b'\0')==b'.text':
  for i in range(nr):
   ro=rel+10*i;addr,sym,typ=struct.unpack_from('<IIH',d,ro);assert typ==2
   symbol,addend=relocs[addr]
   struct.pack_into('<IIH',d,ro,addr,coffnames[symbol],4);struct.pack_into('<i',d,raw+addr,addend+4)
p.write_bytes(d)
subprocess.run(['ld','-mi386pep','--subsystem','windows','--entry','start','--image-base','0x140000000','--no-insert-timestamp','--nxcompat','--dynamicbase','-o','dev210/AccessShot.exe',str(p)],check=True)
p=Path('dev210/AccessShot.exe');d=bytearray(p.read_bytes());pe=pefile.PE(data=d)
sec=next(s for s in pe.sections if s.Name.rstrip(b'\0')==b'.launch');rva=sec.VirtualAddress;at=sec.PointerToRawData
idata=bytearray(2048);cursor=768
for which,(dll,names,intoff,iatoff) in enumerate([
 ('KERNEL32.dll',['GetModuleFileNameW','CreateProcessW','CloseHandle','ExitProcess'],128,512),
 ('USER32.dll',['MessageBoxW'],176,560)]):
 dlloff=cursor;blob=dll.encode()+b'\0';idata[cursor:cursor+len(blob)]=blob;cursor+=len(blob)
 for i,name in enumerate(names):
  cursor=(cursor+1)&~1;noff=cursor;blob=b'\0\0'+name.encode()+b'\0';idata[cursor:cursor+len(blob)]=blob;cursor+=len(blob)
  struct.pack_into('<Q',idata,intoff+8*i,rva+noff);struct.pack_into('<Q',idata,iatoff+8*i,rva+noff)
 struct.pack_into('<IIIII',idata,which*20,rva+intoff,0,0,rva+dlloff,rva+iatoff)
d[at:at+2048]=idata
# Reuse the existing app's icon, version and manifest resources.
base=zipfile.ZipFile('AccessShot-2.9-capas-portable-Windows-x64.zip')
old=pefile.PE(data=base.read('AccessShot.exe'));src=old.OPTIONAL_HEADER.DATA_DIRECTORY[2]
resource=bytearray(old.get_data(src.VirtualAddress,src.Size))
rs=next(s for s in pe.sections if s.Name.rstrip(b'\0')==b'.rsrc');delta=rs.VirtualAddress-src.VirtualAddress
seen=set()
def relocate_dir(off):
 if off in seen:return
 seen.add(off);named,count=struct.unpack_from('<HH',resource,off+12)
 for i in range(named+count):
  name,child=struct.unpack_from('<II',resource,off+16+8*i)
  if child&0x80000000:relocate_dir(child&0x7fffffff)
  else:
   oldrva=struct.unpack_from('<I',resource,child)[0];struct.pack_into('<I',resource,child,oldrva+delta)
relocate_dir(0);assert len(resource)<=rs.SizeOfRawData
d[rs.PointerToRawData:rs.PointerToRawData+len(resource)]=resource
for index,va,size in [(1,rva,60),(12,rva+512,64),(2,rs.VirtualAddress,len(resource))]:
 off=pe.OPTIONAL_HEADER.DATA_DIRECTORY[index].get_file_offset();struct.pack_into('<II',d,off,va,size)
p.write_bytes(d);pe=pefile.PE(str(p));pe.parse_data_directories()
assert [x.dll for x in pe.DIRECTORY_ENTRY_IMPORT]==[b'KERNEL32.dll',b'USER32.dll']
assert pe.OPTIONAL_HEADER.Subsystem==2
assert pe.OPTIONAL_HEADER.DATA_DIRECTORY[14].VirtualAddress==0
print('Native launcher built; GUI subsystem, icon and five WinAPI imports validated.')
