#!/usr/bin/env python3
"""Rename import descriptors of a PE so the listed DLLs resolve to local shims."""
import struct, sys, os

MAP = {
 'advapi32.dll': 'advapi32.dll',   # shim has same name (see build notes)
}

def patch(path, out, rename):
    data = bytearray(open(path,'rb').read())
    pe = struct.unpack_from('<I', data, 0x3c)[0]
    nsec = struct.unpack_from('<H', data, pe+6)[0]
    opt = pe+24
    plus = struct.unpack_from('<H', data, opt)[0] == 0x20b
    dd = opt + (112 if plus else 96)
    imp_rva,_ = struct.unpack_from('<II', data, dd+8)
    so = opt + struct.unpack_from('<H', data, pe+20)[0]
    secs=[]
    for i in range(nsec):
        o=so+i*40; vsz,va,rawsz,raw=struct.unpack_from('<IIII',data,o+8); secs.append((va,vsz,raw,rawsz))
    def r2o(r):
        for va,vsz,raw,rawsz in secs:
            if va<=r<va+max(vsz,rawsz): return raw+(r-va)
        raise ValueError(hex(r))
    off=r2o(imp_rva); changed=[]
    while True:
        oft,ts,fwd,nr,ft=struct.unpack_from('<IIIII',data,off)
        if nr==0: break
        no=r2o(nr); dll=data[no:data.index(b'\0',no)].decode('latin1')
        key=dll.lower()
        if key in rename:
            new=rename[key].encode('latin1')+b'\0'
            old_len=data.index(b'\0',no)-no+1
            if len(new)>old_len:
                print("WARN: %s too long for slot (%d>%d)"%(rename[key],len(new),old_len))
            else:
                data[no:no+len(new)]=new
                changed.append(dll)
        off+=20
    open(out,'wb').write(data)
    return changed

if __name__=='__main__':
    src, dst = sys.argv[1], sys.argv[2]
    renames = {}
    for spec in sys.argv[3:]:
        a,b = spec.split('=')
        renames[a.lower()] = b
    ch = patch(src, dst, renames)
    print("renamed:", ch)
