"""Лабораторна 7. Згортка: власна conv2, фільтри, роздільне розмиття, порівняння з OpenCV.
Потрібні: numpy, Pillow, scikit-image, opencv-python."""
import time
import numpy as np
from PIL import Image
from skimage import data

try:
    import cv2
except ImportError:
    cv2 = None


# ---------- Завдання 1: згортка ----------
def conv2(img, K):
    kh, kw = K.shape
    ph, pw = kh // 2, kw // 2
    pad = np.pad(img, ((ph, ph), (pw, pw)), mode='edge')
    out = np.zeros_like(img)
    for i in range(kh):
        for j in range(kw):
            out += K[i, j] * pad[i:i + img.shape[0], j:j + img.shape[1]]
    return out


# ---------- Завдання 2: ядра ----------
def gauss1d(sigma, n):
    x = np.arange(n) - n // 2
    k = np.exp(-x**2 / (2 * sigma**2))
    return k / k.sum()


# ---------- Завдання 3: роздільна згортка ----------
def conv_sep(img, k):
    n = len(k) // 2
    pad = np.pad(img, ((0, 0), (n, n)), mode='edge')       # по горизонталі
    tmp = np.zeros_like(img)
    for j, v in enumerate(k):
        tmp += v * pad[:, j:j + img.shape[1]]
    pad = np.pad(tmp, ((n, n), (0, 0)), mode='edge')       # по вертикалі
    out = np.zeros_like(img)
    for i, v in enumerate(k):
        out += v * pad[i:i + img.shape[0], :]
    return out


def best(f, r=5):
    """Найкращий час із r запусків, мс"""
    ts = []
    for _ in range(r):
        t = time.perf_counter(); f(); ts.append(time.perf_counter() - t)
    return min(ts) * 1000


def save(arr, name):
    Image.fromarray(np.clip(arr, 0, 255).round().astype(np.uint8)).save(name)


if __name__ == '__main__':
    gray = np.array(Image.fromarray(data.astronaut()).convert('L')).astype(float)

    # --- Завдання 1: перевірка
    avg = conv2(gray, np.ones((3, 3)) / 9)
    print(f"Завдання 1. середня яскравість: оригінал {gray.mean():.2f}, після розмиття {avg.mean():.2f}")

    # --- Завдання 2: три фільтри
    k = gauss1d(1.6, 7)
    BLUR = np.outer(k, k)
    blur = conv2(gray, BLUR)

    SX = np.array([[-1, 0, 1],
                   [-2, 0, 2],
                   [-1, 0, 1]], float)
    SY = SX.T
    gx, gy = conv2(gray, SX), conv2(gray, SY)
    edges = np.hypot(gx, gy)
    edges = edges / edges.max() * 255

    detail = gray - blur
    sharp = np.clip(gray + 1.2 * detail, 0, 255)
    sharp07 = np.clip(gray + 0.7 * detail, 0, 255)   # м'якший варіант, якщо є ореоли

    save(gray, 'gray.png'); save(blur, 'blur.png'); save(edges, 'edges.png')
    save(sharp, 'sharp.png'); save(sharp07, 'sharp_07.png')
    print("Завдання 2. збережено gray.png, blur.png, edges.png, sharp.png, sharp_07.png")

    # --- Завдання 3: перевірка й заміри
    k5 = gauss1d(2.0, 15)
    diff = np.abs(conv2(gray, np.outer(k5, k5)) - conv_sep(gray, k5)).max()
    print(f"Завдання 3. розбіжність звичайної й роздільної згортки: {diff:.1e}")
    rows = []
    print(f"{'ядро':<8}{'звичайне, мс':>14}{'роздільне, мс':>15}{'виграш':>9}")
    for n in (3, 5, 9, 15, 31):
        kk = gauss1d(max(n / 5, 0.6), n); K = np.outer(kk, kk)
        ta = best(lambda: conv2(gray, K))
        tb = best(lambda: conv_sep(gray, kk))
        rows.append((n, ta, tb, None))
        print(f"{n}x{n:<5}{ta:>14.1f}{tb:>15.1f}{ta / tb:>8.1f}x")

    # --- Завдання 4: OpenCV
    if cv2 is None:
        print("Завдання 4.")
    else:
        n, sigma = 15, 3.0
        kk = gauss1d(sigma, n)
        mine = conv_sep(gray, kk)
        lib = cv2.GaussianBlur(gray, (n, n), sigma, borderType=cv2.BORDER_REPLICATE)
        print(f"Завдання 4. розбіжність з OpenCV: {np.abs(mine - lib).max():.1e}")
        print(f"            ваш час: {best(lambda: conv_sep(gray, kk)):.1f} мс")
        print(f"            OpenCV:  {best(lambda: cv2.GaussianBlur(gray, (n, n), sigma, borderType=cv2.BORDER_REPLICATE)):.1f} мс")

        t = ("Таблиця 1. Час згортки, мс (найкращий із 5 запусків)\n\n"
             "| Ядро | Звичайне | Роздільне | Виграш | OpenCV |\n"
             "|------|----------|-----------|--------|--------|\n")
        for n_, ta, tb, _ in rows:
            kk2 = gauss1d(max(n_ / 5, 0.6), n_)
            tc = best(lambda: cv2.GaussianBlur(gray, (n_, n_), max(n_ / 5, 0.6), borderType=cv2.BORDER_REPLICATE))
            f = lambda x: f"{x:.1f}".replace('.', ',')
            t += f"| {n_}x{n_} | {f(ta)} | {f(tb)} | {f(ta / tb)}x | {f(tc)} |\n"
        open('table.txt', 'w', encoding='utf-8').write(t)
        print("\n" + t)