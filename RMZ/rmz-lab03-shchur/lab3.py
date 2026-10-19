import io
import os
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

# ---------- Завдання 0: тестові зображення ----------
W, H = 512, 384

# 1. градієнт
grad = np.tile(np.linspace(0, 255, 512).astype(np.uint8), (384, 1))

# 2. білий шум
rng = np.random.default_rng(1)
noise = rng.integers(0, 256, (384, 512)).astype(np.uint8)

# 3. скріншот: свій screen.png, якщо є; інакше синтетичний (кілька відтінків сірого)
def synthetic_screenshot():
    im = Image.new("L", (W, H), 240)
    d = ImageDraw.Draw(im)
    d.rectangle([0, 0, W, 30], fill=60)                 # заголовок вікна
    d.rectangle([0, 30, 110, H], fill=200)              # бокова панель
    for k in range(8):
        d.rectangle([10, 45 + k * 40, 100, 70 + k * 40], fill=150 if k % 2 else 225)
    for k in range(14):
        d.text((130, 45 + k * 22), "The quick brown fox jumps over the lazy dog", fill=20)
    d.rectangle([130, 340, 300, 370], fill=100)
    return np.array(im)

# 4. фотографія: своя photo.jpg, якщо є; інакше синтетична
def synthetic_photo():
    r = np.random.default_rng(7)
    base = r.normal(0, 1, (H, W))
    img = Image.fromarray(((base - base.min()) / (base.max() - base.min()) * 255).astype(np.uint8))
    img = img.filter(ImageFilter.GaussianBlur(18))
    a = np.asarray(img, dtype=np.float64)
    a = (a - a.min()) / (a.max() - a.min())
    yy, xx = np.indices((H, W))
    a = 0.65 * a + 0.35 * (1 - np.hypot(xx - W / 2, yy - H / 2) / 320).clip(0, 1)
    a = a * 190 + 30 + r.normal(0, 4, (H, W))             # трохи шуму сенсора
    return a.clip(0, 255).astype(np.uint8)

if os.path.exists("screen.png"):
    scr = np.array(Image.open("screen.png").convert("L").resize((W, H)))
else:
    scr = synthetic_screenshot()
if os.path.exists("photo.jpg"):
    photo = np.array(Image.open("photo.jpg").convert("L").resize((W, H)))
else:
    photo = synthetic_photo()

images = {"градієнт": grad, "білий шум": noise, "скріншот": scr, "фотографія": photo}
ORIG = W * H   # 196 608 байтів

# ---------- Завдання 1: ентропія ----------
def entropy(img):
    values, counts = np.unique(img, return_counts=True)
    p = counts / counts.sum()
    return float(-(p * np.log2(p)).sum())

# ---------- Завдання 2: RLE ----------
def rle_size(img):
    data = img.ravel()
    n = len(data)
    i = 0
    out = 0
    while i < n:
        j = i
        # серія однакових байтів, не довша за 255
        while j + 1 < n and data[j + 1] == data[i] and (j + 1 - i) < 255:
            j += 1
        out += 2                # одна пара: лічильник + значення
        i = j + 1
    return out

# перевірка на прикладі з 15 байтів (рисунок 2): має бути 6
# (приклад: 3 серії -> 6 байтів)
assert rle_size(np.array([5]*5 + [7]*6 + [9]*4, dtype=np.uint8)) == 6

# ---------- Завдання 3: Хаффман ----------
def huffman_codes(freq):
    if len(freq) == 1:
        return {list(freq)[0]: "0"}
    nodes = [[n, {v: ""}] for v, n in freq.items()]
    while len(nodes) > 1:
        nodes.sort(key=lambda node: node[0])
        a = nodes.pop(0)
        b = nodes.pop(0)
        for v in a[1]:  a[1][v] = "0" + a[1][v]
        for v in b[1]:  b[1][v] = "1" + b[1][v]
        nodes.append([a[0] + b[0], {**a[1], **b[1]}])
    return nodes[0][1]

def huffman_stats(img):
    values, counts = np.unique(img, return_counts=True)
    freq = {int(v): int(c) for v, c in zip(values, counts)}
    codes = huffman_codes(freq)
    total = sum(freq.values())
    avg = sum(freq[v] * len(codes[v]) for v in freq) / total
    lengths = [len(c) for c in codes.values()]
    return avg, min(lengths), max(lengths)

# ---------- Завдання 4: PNG ----------
def png_size(img):
    buf = io.BytesIO()
    Image.fromarray(img).save(buf, format="PNG", optimize=True)
    return len(buf.getvalue())

if __name__ == "__main__":
    rows = []
    for name, im in images.items():
        e = entropy(im)
        avg, lmin, lmax = huffman_stats(im)
        rle = rle_size(im) / ORIG * 100
        png = png_size(im) / ORIG * 100
        print(f"{name:<11} H={e:.2f}  Хаффман={avg:.2f} (коди {lmin}..{lmax} біт)  RLE={rle:.1f}%  PNG={png:.2f}%")
        rows.append((name, e, avg, lmin, lmax, rle, png))
        Image.fromarray(im).save(f"img_{name}.png")

    f = lambda x, d=2: f"{x:.{d}f}".replace(".", ",")
    t = ("Таблиця 1. Результати\n\n"
         "| Зображення | Ентропія | Хаффман | RLE, % | PNG, % |\n"
         "|------------|----------|---------|--------|--------|\n")
    for name, e, avg, lmin, lmax, rle, png in rows:
        t += f"| {name} | {f(e)} | {f(avg)} | {f(rle,1)} | {f(png)} |\n"
    t += "\nКоди Хаффмана (мін..макс довжина, біт):\n"
    for name, e, avg, lmin, lmax, rle, png in rows:
        t += f"- {name}: {lmin}..{lmax}\n"
    open("table.txt", "w", encoding="utf-8").write(t)
    print(); print(t)