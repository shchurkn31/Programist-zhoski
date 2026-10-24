"""Лабораторна 6. DCT-кодер: базис, квантування, кодер/декодер, крива якості.
Самодостатній файл. Потрібні: numpy, Pillow, scikit-image, matplotlib."""
import io
import zlib
import numpy as np
from PIL import Image
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from skimage import data
from skimage.metrics import peak_signal_noise_ratio, structural_similarity

# Базова таблиця квантування JPEG для яскравості (якість 50)
Q50 = np.array([
 [16,11,10,16,24,40,51,61],[12,12,14,19,26,58,60,55],[14,13,16,24,40,57,69,56],
 [14,17,22,29,51,87,80,62],[18,22,37,56,68,109,103,77],[24,35,55,64,81,104,113,92],
 [49,64,78,87,103,121,120,101],[72,92,95,98,112,100,103,99]], dtype=np.float64)
def dct_matrix(N=8):
    D = np.zeros((N, N))
    for k in range(N):
        a = np.sqrt(1/N) if k == 0 else np.sqrt(2/N)
        for n in range(N):
            D[k, n] = a*np.cos(np.pi*(2*n+1)*k/(2*N))
    return D
D = dct_matrix()
def dct2(b): return D @ b @ D.T
def idct2(c): return D.T @ c @ D
def qtable(q):
    q = max(1, min(99, q))
    S = 5000/q if q < 50 else 200-2*q
    return np.clip(np.floor((S*Q50+50)/100), 1, 255)
def blockify(img):
    h, w = img.shape
    return img.reshape(h//8, 8, w//8, 8).swapaxes(1, 2).reshape(-1, 8, 8)
def unblockify(bl, h, w):
    return bl.reshape(h//8, w//8, 8, 8).swapaxes(1, 2).reshape(h, w)
def encode(img, q):
    Q = qtable(q)
    blocks = blockify(img.astype(np.float64)-128)
    return np.array([np.round(dct2(b)/Q) for b in blocks]).astype(np.int16), Q
def decode(c, Q, h, w):
    bl = np.array([idct2(x*Q) for x in c])
    return np.clip(unblockify(bl, h, w)+128, 0, 255).round().astype(np.uint8)


# ---------- Допоміжне: розмір, метрики, JPEG ----------
def compressed_size(coeffs):
    """Байти після стиснення коефіцієнтів (zlib, рівень 9)"""
    return len(zlib.compress(coeffs.tobytes(), 9))

def jpeg_size(img, q):
    buf = io.BytesIO()
    Image.fromarray(img).save(buf, format="JPEG", quality=q)
    return len(buf.getvalue())

def load_image():
    """Вбудоване зображення 512x512, яскравість (262 144 байти)"""
    return np.array(Image.fromarray(data.astronaut()).convert("L"))


if __name__ == "__main__":
    img = load_image()
    h, w = img.shape
    RAW = h * w

    # ---------- Тести ----------
    # Завдання 1: базис
    assert np.allclose(D @ D.T, np.eye(8)), "D має бути ортонормованою"
    assert np.allclose(D[0], D[0, 0]), "рядок 0 має бути сталим"
    block = img[:8, :8].astype(np.float64) - 128
    err = np.abs(idct2(dct2(block)) - block).max()
    print(f"Завдання 1. похибка dct2 -> idct2: {err:.1e}")

    # Завдання 2: таблиця квантування
    assert np.array_equal(qtable(50), Q50), "при q=50 має вийти Q50"
    print("Завдання 2. середні значення таблиці:",
          {q: round(float(qtable(q).mean())) for q in (90, 50, 10)}, "(очікується 12, 58, 196)")

    # Завдання 3: кодер і декодер
    coeffs, Q = encode(img, 50)
    back = decode(coeffs, Q, h, w)
    nz = (coeffs != 0).reshape(len(coeffs), -1).sum(axis=1).mean()
    p = peak_signal_noise_ratio(img, back)
    s = structural_similarity(img, back)
    print(f"Завдання 3. q=50: ненульових коефіцієнтів на блок {nz:.1f}, PSNR {p:.1f} дБ, SSIM {s:.3f}")

    # ---------- Завдання 4: крива якості ----------
    levels = (95, 90, 75, 50, 30, 15, 5)
    rows = []
    print("\nЗавдання 4.")
    print(f"{'q':>3}{'байт':>9}{'стиснення':>11}{'нулів':>9}{'PSNR':>7}{'SSIM':>8}{'JPEG, байт':>12}")
    for q in levels:
        c, Qq = encode(img, q)
        dec = decode(c, Qq, h, w)
        Image.fromarray(dec).save(f"q{q:02d}.png")
        size = compressed_size(c)
        zeros = (c == 0).mean() * 100
        p = peak_signal_noise_ratio(img, dec)
        s = structural_similarity(img, dec)
        j = jpeg_size(img, q)
        rows.append((q, size, RAW / size, zeros, p, s, j))
        print(f"{q:>3}{size:>9}{RAW/size:>10.1f}x{zeros:>8.1f}%{p:>7.1f}{s:>8.3f}{j:>12}")
    Image.fromarray(img).save("original.png")

    ru = lambda x, d: f"{x:.{d}f}".replace(".", ",")
    t = ("Таблиця 1. Крива якості (сирий розмір 262 144 байти)\n\n"
         "| q | байт | стиснення | нулів | PSNR, дБ | SSIM | JPEG, байт |\n"
         "|---|------|-----------|-------|----------|------|------------|\n")
    for q, size, ratio, zeros, p, s, j in rows:
        t += f"| {q} | {size} | {ru(ratio,1)}x | {ru(zeros,1)} % | {ru(p,1)} | {ru(s,3)} | {j} |\n"
    open("table.txt", "w", encoding="utf-8").write(t)

    # графік «розмір / якість»
    qs = [r[0] for r in rows]
    fig, ax1 = plt.subplots(figsize=(7, 4))
    ax1.plot(qs, [r[1] for r in rows], "o-", label="наш кодер, байт")
    ax1.plot(qs, [r[6] for r in rows], "s--", label="JPEG (Pillow), байт")
    ax1.set_xlabel("якість q"); ax1.set_ylabel("розмір, байт"); ax1.invert_xaxis()
    ax2 = ax1.twinx()
    ax2.plot(qs, [r[5] for r in rows], "r^-", label="SSIM")
    ax2.set_ylabel("SSIM")
    fig.legend(loc="upper center", ncol=3, fontsize=8)
    fig.tight_layout()
    fig.savefig("curve.png", dpi=150)
    print("\nЗбережено: q95.png ... q05.png, original.png, curve.png, table.txt")
    print("Завдання 5: відкрийте original.png і q*.png у масштабі 100 % і знайдіть свій поріг.")