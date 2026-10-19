import sys
import numpy as np

# ---------- Завдання 1: гама ----------
def to_light(code):
    c = np.asarray(code, dtype=np.float64) / 255
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)

def to_code(light):
    l = np.asarray(light, dtype=np.float64)
    s = np.where(l <= 0.0031308, l * 12.92, 1.055 * np.maximum(l, 0) ** (1 / 2.4) - 0.055)
    return s * 255

# ---------- Завдання 2: шахівниця ----------
def make_board(size=512, cell=8):
    yy, xx = np.indices((size, size))
    return (((yy // cell + xx // cell) % 2) * 255).astype(np.uint8)

def box_average(img, k):
    h, w = img.shape
    return img.reshape(h // k, k, w // k, k).mean(axis=(1, 3))

# ---------- Завдання 3: YCbCr ----------
def rgb_to_ycbcr(img):
    x = img.astype(np.float64)
    r, g, b = x[..., 0], x[..., 1], x[..., 2]
    y  =       0.299    * r + 0.587    * g + 0.114    * b
    cb = 128 - 0.168736 * r - 0.331264 * g + 0.5      * b
    cr = 128 + 0.5      * r - 0.418688 * g - 0.081312 * b
    return y, cb, cr

def ycbcr_to_rgb(y, cb, cr):
    r = y                          + 1.402    * (cr - 128)
    g = y - 0.344136 * (cb - 128)  - 0.714136 * (cr - 128)
    b = y + 1.772    * (cb - 128)
    return np.rint(np.stack([r, g, b], axis=-1)).clip(0, 255).astype(np.uint8)

# ---------- Завдання 4: HSV ----------
def rgb_to_hsv(img):
    x = img.astype(np.float64) / 255
    r, g, b = x[..., 0], x[..., 1], x[..., 2]
    mx = x.max(-1); mn = x.min(-1); d = mx - mn
    h = np.zeros_like(mx)
    m = (d != 0) & (mx == r); h[m] = (60 * ((g - b)[m] / d[m])) % 360
    m = (d != 0) & (mx == g) & (mx != r); h[m] = 60 * ((b - r)[m] / d[m]) + 120
    m = (d != 0) & (mx == b) & (mx != r) & (mx != g); h[m] = 60 * ((r - g)[m] / d[m]) + 240
    s = np.where(mx == 0, 0, d / np.where(mx == 0, 1, mx))
    return h, s, mx

def hsv_to_rgb(h, s, v):
    h = np.asarray(h) % 360
    i = np.floor(h / 60).astype(int) % 6
    f = h / 60 - np.floor(h / 60)
    p = v * (1 - s)
    q = v * (1 - f * s)
    t = v * (1 - (1 - f) * s)
    # шаблони каналів для секторів 0..5
    r = np.choose(i, [v, q, p, p, t, v])
    g = np.choose(i, [t, v, v, q, p, p])
    b = np.choose(i, [p, p, t, v, v, q])
    return np.rint(np.stack([r, g, b], axis=-1) * 255).clip(0, 255).astype(np.uint8)

# ---------- Допоміжне: PPM/PGM/BMP ----------
def read_ppm(path):
    with open(path, 'rb') as f:
        data = f.read()
    tokens, pos = [], 0
    while len(tokens) < 4:
        while data[pos:pos+1].isspace(): pos += 1
        if data[pos:pos+1] == b'#':
            while data[pos:pos+1] != b'\n': pos += 1
            continue
        start = pos
        while not data[pos:pos+1].isspace(): pos += 1
        tokens.append(data[start:pos])
    pos += 1
    w, h = int(tokens[1]), int(tokens[2])
    return np.frombuffer(data, np.uint8, w * h * 3, pos).reshape(h, w, 3)

def save_bmp(path, img):
    """24-бітний BMP; img: (h,w) або (h,w,3) uint8"""
    img = np.asarray(img).clip(0, 255).round().astype(np.uint8)
    if img.ndim == 2:
        img = np.stack([img] * 3, -1)
    h, w, _ = img.shape
    row = (w * 3 + 3) // 4 * 4
    pix = np.zeros((h, row), np.uint8)
    pix[:, :w * 3] = img[::-1, :, ::-1].reshape(h, w * 3)
    hdr = (b'BM' + (54 + row * h).to_bytes(4, 'little') + b'\0\0\0\0' + (54).to_bytes(4, 'little')
           + (40).to_bytes(4, 'little') + w.to_bytes(4, 'little') + h.to_bytes(4, 'little')
           + (1).to_bytes(2, 'little') + (24).to_bytes(2, 'little') + b'\0' * 24)
    with open(path, 'wb') as f:
        f.write(hdr + pix.tobytes())

def demo_image(w=256, h=256):
    """Запасна картинка, якщо фото не задано"""
    yy, xx = np.indices((h, w))
    img = np.stack([xx * 255 // (w - 1), yy * 255 // (h - 1), 255 - xx * 255 // (w - 1)], -1)
    img[h // 3: 2 * h // 3, w // 3: 2 * w // 3] = (30, 200, 40)
    return img.astype(np.uint8)

if __name__ == '__main__':
    # --- Завдання 1
    mid = (to_light(0) + to_light(255)) / 2
    print("1) 188:", round(float(to_code(mid))))
    for c in (128, 64, 192):
        print(f"   to_light({c}) = {float(to_light(c)) * 100:.1f}%")
    print("   масив:", to_code(to_light(np.array([0, 128, 255]))))

    # --- Завдання 2
    board = make_board(512, 8)
    naive = box_average(board.astype(np.float64), 16)
    correct = to_code(box_average(to_light(board), 16))
    print("2) наївно:   ", round(naive.mean(), 1))
    print("   правильно:", round(correct.mean(), 1))
    L_orig = to_light(board).mean() * 100
    L_naive = to_light(naive).mean() * 100
    L_corr = to_light(correct).mean() * 100
    save_bmp("board_naive.bmp", naive)
    save_bmp("board_correct.bmp", correct)

    # --- Завдання 5: таблиця
    table = (
        "| Зображення               | Середній код | Середнє світло |\n"
        "|--------------------------|--------------|----------------|\n"
        f"| Оригінальна шахівниця    | {board.mean():.1f}        | {L_orig:.1f}%          |\n"
        f"| Зменшена наївно в sRGB   | {naive.mean():.1f}        | {L_naive:.1f}%          |\n"
        f"| Зменшена в лінійному просторі | {correct.mean():.1f}   | {L_corr:.1f}%          |\n")
    print(table)
    open("table.md", "w", encoding="utf-8").write(table)

    # --- Завдання 3
    img = read_ppm(sys.argv[1]) if len(sys.argv) > 1 else demo_image()
    y, cb, cr = rgb_to_ycbcr(img)
    back = ycbcr_to_rgb(y, cb, cr)
    print("3) макс. різниця туди-назад:", np.abs(back.astype(int) - img.astype(int)).max())
    save_bmp("gray_Y.bmp", np.stack([y] * 3, -1))
    save_bmp("gray_naive.bmp", img.astype(np.float64).mean(-1))

    # --- Завдання 4
    h, s, v = rgb_to_hsv(img)
    print("4) HSV туди-назад, різниця:", np.abs(hsv_to_rgb(h, s, v).astype(int) - img.astype(int)).max())
    save_bmp("hsv_s03.bmp", hsv_to_rgb(h, s * 0.3, v))
    save_bmp("hsv_s2.bmp", hsv_to_rgb(h, np.minimum(s * 2, 1), v))
    save_bmp("hsv_h60.bmp", hsv_to_rgb((h + 60) % 360, s, v))