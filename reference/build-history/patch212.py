from pathlib import Path
import struct,dnfile,json,shutil
from dncil.cil.body import CilMethodBody
from dncil.cil.body.reader import CilMethodBodyReaderBytes
P=Path('work212/Aplicacion/AccessShotCore.dll');original=P.read_bytes();p=dnfile.dnPE(data=original)
def align(v,k=4):return (v+k-1)//k*k
def u(v):return struct.pack('<I',v)
def blobenc(v):return bytes([v]) if v<128 else struct.pack('>H',v|0x8000)
streams={k:bytearray(s.__data__) for k,s in p.net.metadata.streams.items()};rows={k:bytearray(original[t.file_offset:t.file_offset+t.row_size*len(t.rows)]) for k,t in p.net.mdtables.tables.items()};counts={k:len(t.rows) for k,t in p.net.mdtables.tables.items()}
def string(s):
 i=len(streams[b'#Strings']);streams[b'#Strings']+=s.encode()+b'\0';return i
def blob(s):
 i=len(streams[b'#Blob']);streams[b'#Blob']+=blobenc(len(s))+s;return i
def append(k,b):
 assert len(b)==p.net.mdtables.tables[k].row_size
 rows[k]+=b;counts[k]+=1;return counts[k]
assembly=append(35,struct.pack('<HHHHIHHHH',2,12,0,0,0,0,string('AccessShotWindowsApps'),0,0))
targettype=append(1,struct.pack('<HHH',assembly*4+2,string('AppTargets'),0))
pickertype=append(1,struct.pack('<HHH',assembly*4+2,string('WindowsAppsPicker'),0))
def member(parent,name,sig):return 0x0a000000+append(10,struct.pack('<HHH',parent*8+1,string(name),blob(bytes.fromhex(sig))))
exists=member(targettype,'Exists','0001020e');factory=member(targettype,'CreateStartInfo','000112813d0e');attach=member(pickertype,'Attach','0001011c')
def call(t):return b'\x28'+u(t)
def getbody(idx):return CilMethodBody(CilMethodBodyReaderBytes(p.get_data(p.net.mdtables.MethodDef.rows[idx-1].Rva,12000)))
# Rebuild methods with labelled branches. Widen short branches so inserted calls cannot overflow.
def transform(idx,insertions):
 m=p.net.mdtables.MethodDef.rows[idx-1];b=getbody(idx);data=original[p.get_offset_from_rva(m.Rva):]
 positions={};parts=[];fix=[];off=0
 for ins in b.instructions:
  key=ins.offset-b.header_size;positions[key]=off
  if key in insertions:parts.append(insertions[key]);off+=len(insertions[key])
  name=ins.opcode.name
  if isinstance(ins.operand,int) and name.startswith(('br','leave','beq','bne','bgt','bge','blt','ble')):
   op=ins.opcode.value
   if name.endswith('.s'):op=(op+0xd) if op!=0xde else 0xdd
   raw=bytes([op])+b'\0'*4;fix.append((off+1,ins.operand-b.header_size))
  else:raw=data[ins.offset:ins.offset+ins.size]
  parts.append(raw);off+=len(raw)
 positions[b.code_size]=off
 code=bytearray(b''.join(parts))
 for at,target in fix:struct.pack_into('<i',code,at,positions[target]-at-4)
 header=bytearray(data[:b.header_size]);struct.pack_into('<I',header,4,len(code))
 out=header+code
 if b.exception_handlers:
  out+=b'\0'*(align(len(out))-len(out));eh=bytearray([0x41])+int(4+24*len(b.exception_handlers)).to_bytes(3,'little')
  for e in b.exception_handlers:
   ts,te,hs,he=[positions[v] for v in [e.try_start,e.try_end,e.handler_start,e.handler_end]]
   token=e.catch_type.value if e.catch_type is not None else 0
   eh+=struct.pack('<IIIIII',e.exception_type,ts,te-ts,hs,he-hs,token)
  out+=eh
 return bytes(out)

methods={34:transform(34,{0x276-12:b'\x06'+call(attach)})}
patched=bytearray(original)
# Replace the two file-only validators and preserve the existing branches/error paths.
for method in [45,119]:
 m=p.net.mdtables.MethodDef.rows[method-1];b=getbody(method)
 found=[i for i in b.instructions if i.opcode.name=='call' and i.operand.value==0x0a000060]
 assert len(found)==1
 for i in found:struct.pack_into('<I',patched,p.get_offset_from_rva(m.Rva)+i.offset+1,exists)
# Route shell targets through explorer, leaving file and HTTP targets unchanged.
m=p.net.mdtables.MethodDef.rows[44];b=getbody(45)
constructor=next(i for i in b.instructions if i.opcode.name=='newobj' and i.operand.value==0x0a000055)
o=p.get_offset_from_rva(m.Rva)+constructor.offset;patched[o]=0x28;struct.pack_into('<I',patched,o+1,factory)
def userstring(text):
 index=len(streams[b'#US']);raw=text.encode('utf-16-le')+b'\x01';streams[b'#US']+=blobenc(len(raw))+raw;return 0x70000000+index
messages={45:('El archivo de destino ya no existe.','La aplicación o el archivo de destino ya no está disponible.'),119:('Selecciona un archivo existente.','Selecciona un archivo existente o una app instalada de Windows.')}
for method,(old,new) in messages.items():
 m=p.net.mdtables.MethodDef.rows[method-1]
 i=next(i for i in getbody(method).instructions if i.opcode.name=='ldstr' and p.net.user_strings.get(i.operand.value&0xffffff).value==old)
 struct.pack_into('<I',patched,p.get_offset_from_rva(m.Rva)+i.offset+1,userstring(new))
# Append code and metadata to the existing last section.
last=p.sections[-1];rawpos=align(len(original));rva=last.VirtualAddress+rawpos-last.PointerToRawData;payload=bytearray()
for idx,data in methods.items():
 struct.pack_into('<I',rows[6],(idx-1)*p.net.mdtables.MethodDef.row_size,rva+len(payload));payload+=data+b'\0'*(align(len(data))-len(data))
tables=bytearray(streams[b'#~'][:24])
for k in sorted(rows):tables+=u(counts[k])
for k in sorted(rows):tables+=rows[k]
streams[b'#~']=tables
mdstart=p.get_offset_from_rva(p.net.struct.MetaDataRva);md=bytearray(original[mdstart:mdstart+32]);headerlen=32+sum(8+align(len(k)+1) for k in streams);offset=headerlen;data=bytearray()
for k,s in streams.items():
 md+=u(offset)+u(len(s))+k+b'\0'+b'\0'*(align(len(k)+1)-len(k)-1);data+=s+b'\0'*(align(len(s))-len(s));offset=headerlen+len(data)
md+=data;metadata_rva=rva+len(payload);payload+=md
raw=patched+b'\0'*(rawpos-len(original))+payload;size=len(raw)-last.PointerToRawData;raw+=b'\0'*(align(size,p.OPTIONAL_HEADER.FileAlignment)-size)
so=last.get_file_offset();struct.pack_into('<I',raw,so+8,size);struct.pack_into('<I',raw,so+16,align(size,p.OPTIONAL_HEADER.FileAlignment));struct.pack_into('<I',raw,so+36,0x60000060)
struct.pack_into('<I',raw,p.OPTIONAL_HEADER.get_file_offset()+56,align(last.VirtualAddress+size,p.OPTIONAL_HEADER.SectionAlignment));struct.pack_into('<II',raw,p.net.struct.get_file_offset()+8,metadata_rva,len(md));P.write_bytes(raw)

folder=P.parent;shutil.copyfile('dev212/AccessShotWindowsApps.dll',folder/'AccessShotWindowsApps.dll')
for name in ['AccessShot.deps.json','AccessShotCore.deps.json']:
 path=folder/name;obj=json.loads(path.read_text())
 for target in obj['targets'].values():
  app=next(v for k,v in target.items() if k.startswith('AccessShot/') or k.startswith('AccessShotCore/'))
  app.setdefault('dependencies',{})['AccessShotWindowsApps']='2.12.0'
  target['AccessShotWindowsApps/2.12.0']={'runtime':{'AccessShotWindowsApps.dll':{'assemblyVersion':'2.12.0.0','fileVersion':'2.12.0.0'}}}
 obj['libraries']['AccessShotWindowsApps/2.12.0']={'type':'project','serviceable':False,'sha512':''};path.write_text(json.dumps(obj,indent=2))
for path in [Path('work212/AccessShot.exe'),folder/'AccessShot.dll']:
 data=path.read_bytes();old='AccessShot 2.11'.encode('utf-16-le');assert data.count(old)==1;path.write_bytes(data.replace(old,'AccessShot 2.12'.encode('utf-16-le')))
q=dnfile.dnPE(str(P));hp=dnfile.dnPE('dev212/AccessShotWindowsApps.dll')
for idx in [34,45,119]:
 m=q.net.mdtables.MethodDef.rows[idx-1];b=CilMethodBody(CilMethodBodyReaderBytes(q.get_data(m.Rva,12000)));offsets={i.offset for i in b.instructions}
 for i in b.instructions:
  if hasattr(i.operand,'value'):
   tok=i.operand.value;t=tok>>24;n=tok&0xffffff
   if t!=0x70:assert 0<n<=len(q.net.mdtables.tables[t].rows)
  if isinstance(i.operand,int) and i.opcode.name.startswith(('br','leave','beq','bne','bgt','bge','blt','ble')):assert i.operand in offsets
 for e in b.exception_handlers:
  for v in [e.try_start,e.try_end,e.handler_start,e.handler_end]:assert v+b.header_size in offsets or v==b.code_size
 assert b.instructions[-1].opcode.name=='ret'
# Compare new MemberRefs against compiler-produced definitions, translating type tokens by names.
def sigtypes(pe,sig):
 # New signatures use primitive and TypeRef class arguments; compare names rather than token numbers.
 b=bytes(sig);i=0;parts=[]
 while i<len(b):
  v=b[i];i+=1;parts.append(v)
  if v==0x12:
   n=b[i];i+=1
   if n&0x80:n=((n&0x3f)<<8)|b[i];i+=1
   table=[2,1,27][n&3];row=pe.net.mdtables.tables[table].rows[(n>>2)-1];parts.append(str(row.TypeName))
 return parts
for tref in [exists,factory,attach]:
 ref=q.net.mdtables.MemberRef.rows[(tref&0xffffff)-1];typ=str(ref.Class.row.TypeName)
 t=next(t for t in hp.net.mdtables.TypeDef if str(t.TypeName)==typ)
 method=next(m.row for m in t.MethodList if str(m.row.Name)==str(ref.Name))
 assert sigtypes(q,ref.Signature.value)==sigtypes(hp,method.Signature.value),(typ,ref.Name)
print('PASS: selector attachment, file and modern-app validation/launch calls, signatures, branches/EH and dependency graph.')
