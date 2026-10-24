import socket, struct, sys, time, os
from Crypto.Cipher import DES

HOST = os.environ.get('VNC_HOST', '127.0.0.1')
PORT = int(os.environ.get('VNC_PORT', '5902'))
PASS = os.environ.get('VNC_PASS', '')

def revbits(b):
    r = 0
    for i in range(8):
        r = (r << 1) | ((b >> i) & 1)
    return r

def vnc_key(pw):
    pw = (pw + '\0' * 8)[:8]
    return bytes(revbits(ord(c)) for c in pw)

class VNC:
    def __init__(self, host=HOST, port=PORT, password=PASS):
        self.s = socket.create_connection((host, port), timeout=10)
        self.s.settimeout(15)
        ver = self.recv_exact(12)
        self.version = ver
        if ver.startswith(b'RFB 003.008') or ver.startswith(b'RFB 003.007'):
            self.s.sendall(b'RFB 003.008\n')
            n = self.recv_exact(1)[0]
            if n == 0:
                reason_len = struct.unpack('>I', self.recv_exact(4))[0]
                raise RuntimeError('server refused: ' + self.recv_exact(reason_len).decode('latin1'))
            types = self.recv_exact(n)
            if 2 not in types:
                raise RuntimeError('no VNC auth, types=%r' % (types,))
            self.s.sendall(bytes([2]))
            challenge = self.recv_exact(16)
            resp = DES.new(vnc_key(password), DES.MODE_ECB).encrypt(challenge)
            self.s.sendall(resp)
            res = struct.unpack('>I', self.recv_exact(4))[0]
            if res != 0:
                raise RuntimeError('auth failed %d' % res)
        elif ver.startswith(b'RFB 003.003'):
            sec = struct.unpack('>I', self.recv_exact(4))[0]
            if sec != 2:
                raise RuntimeError('security type %d' % sec)
            challenge = self.recv_exact(16)
            resp = DES.new(vnc_key(password), DES.MODE_ECB).encrypt(challenge)
            self.s.sendall(resp)
            res = struct.unpack('>I', self.recv_exact(4))[0]
            if res != 0:
                raise RuntimeError('auth failed %d' % res)
        else:
            raise RuntimeError('unknown version %r' % ver)
        self.s.sendall(bytes([1]))  # shared
        w, h = struct.unpack('>HH', self.recv_exact(4))
        pf = self.recv_exact(16)
        name_len = struct.unpack('>I', self.recv_exact(4))[0]
        self.name = self.recv_exact(name_len).decode('latin1')
        self.w, self.h = w, h
        # request 32bpp truecolour RGBA order
        self.send_set_pixel_format()
        self.send_encodings([0])
        self.fb = None

    def send_set_pixel_format(self):
        pf = struct.pack('>BBBBHHHBBBxxx', 32, 24, 0, 1, 255, 255, 255, 16, 8, 0)
        self.s.sendall(struct.pack('>Bxxx', 0) + pf)

    def send_encodings(self, encs):
        msg = struct.pack('>BxH', 2, len(encs)) + b''.join(struct.pack('>i', e) for e in encs)
        self.s.sendall(msg)

    def recv_exact(self, n):
        buf = b''
        while len(buf) < n:
            d = self.s.recv(n - len(buf))
            if not d:
                raise EOFError('closed')
            buf += d
        return buf

    def update(self, incremental=0):
        self.s.sendall(struct.pack('>BBHHHH', 3, incremental, 0, 0, self.w, self.h))
        while True:
            t = self.recv_exact(1)[0]
            if t == 0:
                n = struct.unpack('>xH', self.recv_exact(3))[0]
                fbm = self.fb if self.fb else bytearray(self.w * self.h * 4)
                for _ in range(n):
                    x, y, w, h, enc = struct.unpack('>HHHHi', self.recv_exact(12))
                    if enc == 0:
                        data = self.recv_exact(w * h * 4)
                        for row in range(h):
                            src = row * w * 4
                            dst = ((y + row) * self.w + x) * 4
                            fbm[dst:dst + w * 4] = data[src:src + w * 4]
                    elif enc == 1:
                        self.recv_exact(4)
                    else:
                        raise RuntimeError('unexpected encoding %d' % enc)
                self.fb = fbm
                return n
            elif t == 2:
                continue
            elif t == 3:
                l = struct.unpack('>I', self.recv_exact(4))[0]
                self.recv_exact(l)
            else:
                raise RuntimeError('server msg %d' % t)

    def save(self, path):
        from PIL import Image
        if self.fb is None:
            self.update()
        # server sends RGBA order due to pixel format shifts (R<<16,G<<8,B)
        im = Image.frombytes('RGBA', (self.w, self.h), bytes(self.fb), 'raw', 'BGRA')
        im.convert('RGB').save(path)

    def pointer(self, x, y, mask=0):
        self.s.sendall(struct.pack('>BBHH', 5, mask, x, y))

    def click(self, x, y, button=1, delay=0.08):
        self.pointer(x, y, 0)
        time.sleep(delay)
        self.pointer(x, y, button)
        time.sleep(delay)
        self.pointer(x, y, 0)

    def key(self, keysym, down=True):
        self.s.sendall(struct.pack('>BBxxI', 4, 1 if down else 0, keysym))

    def keypress(self, keysym, delay=0.06):
        self.key(keysym, True); time.sleep(delay)
        self.key(keysym, False); time.sleep(delay)

    SHIFT_MAP = {'!': '1', '@': '2', '#': '3', '$': '4', '%': '5', '^': '6',
                 '&': '7', '*': '8', '(': '9', ')': '0', '_': '-', '+': '=',
                 '{': '[', '}': ']', '|': '\\', ':': ';', '"': "'", '<': ',',
                 '>': '.', '?': '/', '~': '`'}

    def type_text(self, text, delay=0.12):
        for ch in text:
            if ch in self.SHIFT_MAP:
                base = ord(self.SHIFT_MAP[ch])
                self.key(0xffe1, True); time.sleep(0.03)
                self.key(base, True); time.sleep(0.03)
                self.key(base, False); time.sleep(0.03)
                self.key(0xffe1, False); time.sleep(delay)
            elif 'A' <= ch <= 'Z':
                self.key(0xffe1, True); time.sleep(0.03)
                self.key(ord(ch.lower()), True); time.sleep(0.03)
                self.key(ord(ch.lower()), False); time.sleep(0.03)
                self.key(0xffe1, False); time.sleep(delay)
            else:
                self.keypress(ord(ch), delay)

if __name__ == '__main__':
    v = VNC()
    print('version', v.version, 'size', v.w, v.h, 'name', repr(v.name))
    v.update()
    v.save(sys.argv[1] if len(sys.argv) > 1 else 'vnc.png')
    print('saved')
