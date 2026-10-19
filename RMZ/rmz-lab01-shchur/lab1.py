import sys
import struct
import numpy as np

def write_ppm(path, img):
    height, width, _ = img.shape
    header = f"P6\n{width} {height}\n255\n".encode('ascii')
    with open(path, "wb") as f:
        f.write(header)
        f.write(img.tobytes())

def read_ppm(path):
    with open(path, "rb") as f:
        data = f.read()

    idx = 0
    tokens = []
    while len(tokens) < 4:
        while idx < len(data) and data[idx:idx+1].isspace():
            idx += 1
        if idx < len(data) and data[idx:idx+1] == b'#':
            while idx < len(data) and data[idx:idx+1] != b'\n':
                idx += 1
            continue
        start = idx
        while idx < len(data) and not data[idx:idx+1].isspace():
            idx += 1
        if start < idx:
            tokens.append(data[start:idx].decode('ascii'))

    width, height = int(tokens[1]), int(tokens[2])
    idx += 1

    img_data = data[idx:idx + width * height * 3]
    return np.frombuffer(img_data, dtype=np.uint8).reshape((height, width, 3))

def write_bmp24(path, img):
    height, width, _ = img.shape

    stride   = ((width * 24 + 31) // 32) * 4
    padding  = stride - width * 3
    offbits  = 54
    filesize = offbits + stride * height

    out = bytearray()
    out += struct.pack("<2sIHHI", b"BM", filesize, 0, 0, offbits)
    out += struct.pack("<IiiHHIIiiII", 40, width, height, 1, 24,
                       0, stride * height, 2835, 2835, 0, 0)

    for y in range(height - 1, -1, -1):
        for x in range(width):
            r, g, b = img[y, x]
            out += bytes((b, g, r))
        out += b"\x00" * padding

    with open(path, "wb") as f:
        f.write(out)

def read_bmp24(path):
    with open(path, "rb") as f:
        data = f.read()

    offbits = struct.unpack_from("<I", data, 10)[0]
    width   = struct.unpack_from("<i", data, 18)[0]
    height  = struct.unpack_from("<i", data, 22)[0]
    bits    = struct.unpack_from("<H", data, 28)[0]
    stride  = ((width * bits + 31) // 32) * 4

    img = np.zeros((height, width, 3), dtype=np.uint8)
    for y in range(height):
        start = offbits + y * stride
        for x in range(width):
            b = data[start + x * 3 + 0]
            g = data[start + x * 3 + 1]
            r = data[start + x * 3 + 2]
            img[height - 1 - y, x] = (r, g, b)
    return img

def write_bmp8(path, img):
    height, width, _ = img.shape

    stride   = ((width * 8 + 31) // 32) * 4
    padding  = stride - width
    offbits  = 1078
    filesize = offbits + stride * height

    out = bytearray()
    out += struct.pack("<2sIHHI", b"BM", filesize, 0, 0, offbits)
    out += struct.pack("<IiiHHIIiiII", 40, width, height, 1, 8,
                       0, stride * height, 2835, 2835, 256, 0)

    palette = bytearray()
    for i in range(256):
        if i < 216:
            r = (i // 36) * 51
            g = ((i // 6) % 6) * 51
            b = (i % 6) * 51
        else:
            r = g = b = 0
        palette += bytes((b, g, r, 0))
    out += palette

    for y in range(height - 1, -1, -1):
        for x in range(width):
            r, g, b = img[y, x]
            index = 36 * (r // 51) + 6 * (g // 51) + (b // 51)
            out.append(index)
        out += b"\x00" * padding

    with open(path, "wb") as f:
        f.write(out)

if __name__ == "__main__":
    if len(sys.argv) == 3:
        src, dst = sys.argv[1], sys.argv[2]
        if src.endswith(".ppm") and dst.endswith(".bmp"):
            write_bmp24(dst, read_ppm(src))
            print(f"Успішно конвертовано {src} -> {dst}")
        elif src.endswith(".bmp") and dst.endswith(".ppm"):
            write_ppm(dst, read_bmp24(src))
            print(f"Успішно конвертовано {src} -> {dst}")
        else:
            print("Не знаю, як перетворити ці формати")
    else:
        img = np.zeros((4, 6, 3), dtype=np.uint8)
        img[0:2, 0:3] = (255, 0, 0)
        img[0:2, 3:6] = (0, 255, 0)
        img[2:4, 0:3] = (0, 0, 255)
        img[2:4, 3:6] = (255, 255, 255)

        write_ppm("test.ppm", img)
        write_bmp24("test.bmp", img)
        write_bmp8("test_palette.bmp", img)

        a = read_ppm("test.ppm")
        b = read_bmp24("test.bmp")
        
        print("Розмір масиву:", a.shape)
        print("Перевірка точного збігу PPM та BMP24:", np.array_equal(a, b))