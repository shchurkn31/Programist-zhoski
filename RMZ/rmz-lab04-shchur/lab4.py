"""Лабораторна 4. Самодостатній файл: не імпортує інші модулі лабораторних."""
from collections import Counter
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



# ---------- Завдання 1: фільтр одного рядка ----------
def filter_row(cur, prev, f):
    """f: 0 None, 1 Sub, 2 Up"""
    a = np.concatenate(([0], cur[:-1])).astype(np.uint8)   # лівий сусід
    b = prev                                                # верхній сусід
    if f == 0:
        pred = np.zeros_like(cur)
    elif f == 1:
        pred = a
    else:
        pred = b
    return ((cur.astype(int) - pred.astype(int)) % 256).astype(np.uint8)

# ---------- Завдання 2: ціле зображення ----------
def filter_image(img, f):
    out = np.zeros_like(img)
    prev = np.zeros(img.shape[1], dtype=np.uint8)
    for y in range(img.shape[0]):
        out[y] = filter_row(img[y], prev, f)
        prev = img[y]            # вихідний рядок, а не out[y]
    return out

# ---------- Завдання 3: зворотне перетворення ----------
def unfilter_row(enc, prev, f):
    n = len(enc)
    out = np.zeros(n, dtype=np.uint8)
    for x in range(n):
        a = out[x - 1] if x > 0 else 0
        b = prev[x]
        pred = 0 if f == 0 else (a if f == 1 else b)
        out[x] = (int(enc[x]) + int(pred)) % 256
    return out

def unfilter_image(enc, f):
    """f — один номер на все зображення або список номерів по рядках"""
    out = np.zeros_like(enc)
    prev = np.zeros(enc.shape[1], dtype=np.uint8)
    for y in range(enc.shape[0]):
        fy = f if isinstance(f, int) else f[y]
        out[y] = unfilter_row(enc[y], prev, fy)
        prev = out[y]            # тут — уже відновлений рядок
    return out

# ---------- Завдання 4: адаптивний вибір ----------
def sum_abs(row):
    r = row.astype(int)
    signed = np.where(r > 127, r - 256, r)
    return int(np.abs(signed).sum())

def filter_adaptive(img):
    out = np.zeros_like(img)
    chosen = []
    prev = np.zeros(img.shape[1], dtype=np.uint8)
    for y in range(img.shape[0]):
        best_f, best_row, best_s = None, None, None
        for f in (0, 1, 2):
            row = filter_row(img[y], prev, f)
            s = sum_abs(row)
            if best_s is None or s < best_s:
                best_f, best_row, best_s = f, row, s
        out[y] = best_row
        chosen.append(best_f)
        prev = img[y]
    return out, chosen

if __name__ == "__main__":
    # перевірка завдання 1
    row = np.array([40, 42, 44, 46, 48, 50, 52, 54], dtype=np.uint8)
    zeros = np.zeros(8, dtype=np.uint8)
    print(filter_row(row, zeros, 1), filter_row(row, zeros, 0), filter_row(row, row, 2))

    names = list(images)
    res = {}
    print(f"{'':<11}{'До':>6}{'None':>7}{'Sub':>7}{'Up':>7}{'Адапт.':>8}")
    for name in names:
        img = images[name]
        e0 = entropy(img)
        ents = []
        for f in (0, 1, 2):
            enc = filter_image(img, f)
            assert np.array_equal(img, unfilter_image(enc, f)), (name, f)   # завдання 3
            ents.append(entropy(enc))
        ad, chosen = filter_adaptive(img)
        assert np.array_equal(img, unfilter_image(ad, chosen)), name
        ea = entropy(ad)
        cnt = Counter(chosen)
        res[name] = (e0, ents, ea, cnt)
        print(f"{name:<11}{e0:6.2f}{ents[0]:7.2f}{ents[1]:7.2f}{ents[2]:7.2f}{ea:8.2f}  вибір: {dict(cnt)}")
    print("Зворотне перетворення: True для всіх фільтрів і адаптивного режиму")

    # ---------- Таблиці ----------
    fmt = lambda x: f"{x:.2f}".replace(".", ",")
    fnames = {0: "None", 1: "Sub", 2: "Up"}
    t1 = ("Таблиця 1. Ентропія, біт на піксель\n\n"
          "| Зображення | До | None | Sub | Up | Адаптивний |\n"
          "|------------|----|------|-----|----|------------|\n")
    t2 = ("Таблиця 2. Підсумкова таблиця\n\n"
          "| Тип зображення | Оптимальний фільтр | Ентропія до / після | Виграш, % |\n"
          "|----------------|--------------------|---------------------|-----------|\n")
    for name in names:
        e0, ents, ea, cnt = res[name]
        t1 += f"| {name} | {fmt(e0)} | {fmt(ents[0])} | {fmt(ents[1])} | {fmt(ents[2])} | {fmt(ea)} |\n"
        best = min(ents + [ea])
        # оптимальний: найкращий окремий фільтр або адаптивний, якщо він кращий
        bf = int(np.argmin(ents))
        label = fnames[bf]
        if ea < ents[bf] - 1e-9:
            label = "адаптивний (" + ", ".join(f"{fnames[k]}: {v}" for k, v in sorted(cnt.items())) + " рядків)"
        gain = (e0 - best) / e0 * 100
        if gain < 0.5:
            label = "жоден не допомагає"
        t2 += f"| {name} | {label} | {fmt(e0)} / {fmt(best)} | {gain:.1f} |\n".replace(".", ",") if False else \
              f"| {name} | {label} | {fmt(e0)} / {fmt(best)} | {f'{gain:.1f}'.replace('.', ',')} |\n"
    open("table.txt", "w", encoding="utf-8").write(t1 + "\n" + t2)
    print(); print(t1); print(t2)