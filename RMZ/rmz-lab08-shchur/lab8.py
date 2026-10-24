"""Лабораторна 8. Масштабування: інтерполяція, аліасинг, міпмапи, ціна гами.
Потрібні: numpy, Pillow, scikit-image."""
import numpy as np
from PIL import Image
from skimage import data


# ---------- Гама з лабораторної 2 ----------
def to_light(code):
    c = np.asarray(code, dtype=np.float64) / 255
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)

def to_code(light):
    l = np.asarray(light, dtype=np.float64)
    s = np.where(l <= 0.0031308, l * 12.92, 1.055 * np.maximum(l, 0) ** (1 / 2.4) - 0.055)
    return s * 255


# ---------- Завдання 2: тестова шахівниця ----------
def checker(N=512, cell=4, ang=18):
    y, x = np.indices((N, N)).astype(float) - N / 2
    a = np.deg2rad(ang)
    xr = x * np.cos(a) - y * np.sin(a)
    yr = x * np.sin(a) + y * np.cos(a)
    return ((np.floor(xr / cell) + np.floor(yr / cell)) % 2) * 255


# ---------- Завдання 3-4: зменшення вдвічі ----------
def half_naive(img):
    h, w = img.shape
    return img.reshape(h // 2, 2, w // 2, 2).mean(axis=(1, 3))

def half_linear(img):
    L = to_light(img)
    h, w = L.shape
    return to_code(L.reshape(h // 2, 2, w // 2, 2).mean(axis=(1, 3)))


if __name__ == '__main__':
    gray = np.array(Image.fromarray(data.astronaut()).convert('L'))

    # ---------- Завдання 1: збільшення ----------
    small = gray[340:356, 372:388]                      # фрагмент 16x16
    im = Image.fromarray(small)
    im.resize((320, 320), Image.NEAREST).save('up_nearest.png')
    im.resize((320, 320), Image.BILINEAR).save('up_bilinear.png')
    im.resize((320, 320), Image.LANCZOS).save('up_lanczos.png')
    a = np.array(Image.open('up_nearest.png'))
    b = np.array(Image.open('up_bilinear.png'))
    c = np.array(Image.open('up_lanczos.png'))
    blocks_ok = np.array_equal(a[:20, :20], np.full((20, 20), a[0, 0]))
    print("Завдання 1. nearest дає квадрати 20x20:", blocks_ok,
          "| три файли різні:", not (np.array_equal(a, b) or np.array_equal(b, c)))

    # ---------- Завдання 2: аліасинг ----------
    ch = checker()
    k = 4
    nearest = ch[::k, ::k]
    area = ch.reshape(512 // k, k, 512 // k, k).mean(axis=(1, 3))
    print("\nЗавдання 2.")
    stats = {}
    for name, img in (('ds_nearest', nearest), ('ds_area', area)):
        Image.fromarray(img.astype(np.uint8)).save(name + '.png')
        stats[name] = (img.mean(), img.std())
        print(name, 'середнє %.1f розкид %.1f' % stats[name])
    Image.fromarray(ch.astype(np.uint8)).save('checker_orig.png')

    # ---------- Завдання 3: міпмапи ----------
    print("\nЗавдання 3.")
    g = gray.astype(float)
    level = 0
    while g.shape[0] >= 2:
        Image.fromarray(g.astype(np.uint8)).save(f'mip_{level}.png')
        print(level, g.shape[0], 'середня яскравість %.1f' % g.mean())
        g = half_naive(g)
        level += 1
    print("рівнів:", level)

    # ---------- Завдання 4: ціна гами ----------
    print("\nЗавдання 4.")
    cb = np.indices((256, 256)).sum(axis=0) % 2 * 255.0
    print('наївно: %.1f' % half_naive(cb).mean())
    print('правильно: %.1f' % half_linear(cb).mean())

    gn = gray.astype(float)
    gl = gray.astype(float)
    rows = [(0, 512, gn.mean(), gl.mean())]
    print('0', 512, '%.1f %.1f різниця %+.1f' % (gn.mean(), gl.mean(), 0.0))
    for lv in range(1, 6):
        gn, gl = half_naive(gn), half_linear(gl)
        rows.append((lv, gn.shape[0], gn.mean(), gl.mean()))
        print(lv, gn.shape[0], '%.1f %.1f різниця %+.1f' % (gn.mean(), gl.mean(), gl.mean() - gn.mean()))
    Image.fromarray(np.clip(gn, 0, 255).astype(np.uint8)).resize((256, 256), Image.NEAREST).save('half5_naive.png')
    Image.fromarray(np.clip(gl, 0, 255).astype(np.uint8)).resize((256, 256), Image.NEAREST).save('half5_linear.png')

    f = lambda x: f"{x:.1f}".replace('.', ',')
    t = ("Таблиця 1. Зменшення шахівниці у 4 рази\n\n"
         "| Спосіб | Середнє | Розкид |\n|--------|---------|--------|\n"
         f"| кожен 4-й піксель | {f(stats['ds_nearest'][0])} | {f(stats['ds_nearest'][1])} |\n"
         f"| усереднення 4x4 | {f(stats['ds_area'][0])} | {f(stats['ds_area'][1])} |\n\n"
         "Таблиця 2. Накопичення різниці від гами\n\n"
         "| Рівень | Розмір | Наївно | Правильно | Різниця |\n|--------|--------|--------|-----------|---------|\n")
    for lv, sz, n_, l_ in rows:
        d = l_ - n_
        t += f"| {lv} | {sz} | {f(n_)} | {f(l_)} | {('+' if d > 0.05 else '') + f(d)} |\n"
    open('table.txt', 'w', encoding='utf-8').write(t)
    print("\n" + t)