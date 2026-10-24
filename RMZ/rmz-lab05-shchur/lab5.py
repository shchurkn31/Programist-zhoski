import os
import numpy as np
from PIL import Image, ImageDraw, ImageFont
from skimage import data
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt


# ---------- Завдання 1: RGB <-> YCbCr ----------
def rgb2ycbcr(img):
    a = img.astype(np.float64)
    R, G, B = a[..., 0], a[..., 1], a[..., 2]
    Y  = 0.299 * R + 0.587 * G + 0.114 * B
    Cb = 128 - 0.168736 * R - 0.331264 * G + 0.5 * B
    Cr = 128 + 0.5 * R - 0.418688 * G - 0.081312 * B
    return Y, Cb, Cr

def ycbcr2rgb(Y, Cb, Cr):
    R = Y + 1.402 * (Cr - 128)
    G = Y - 0.344136 * (Cb - 128) - 0.714136 * (Cr - 128)
    B = Y + 1.772 * (Cb - 128)
    rgb = np.stack([R, G, B], axis=-1)
    return np.clip(rgb, 0, 255).round().astype(np.uint8)


# ---------- Завдання 2-3: проріджування ----------
def down422(C):
    h, w = C.shape
    return C.reshape(h, w // 2, 2).mean(axis=2)

def up422(C, w):
    return np.repeat(C, 2, axis=1)[:, :w]

def down420(C):
    h, w = C.shape
    return C.reshape(h // 2, 2, w // 2, 2).mean(axis=(1, 3))

def up420(C, h, w):
    return np.repeat(np.repeat(C, 2, axis=0), 2, axis=1)[:h, :w]

def scheme(img, mode):
    Y, Cb, Cr = rgb2ycbcr(img)
    h, w = Y.shape
    if mode == '4:4:4':
        pass
    elif mode == '4:2:2':
        Cb = up422(down422(Cb), w)
        Cr = up422(down422(Cr), w)
    elif mode == '4:2:0':
        Cb = up420(down420(Cb), h, w)
        Cr = up420(down420(Cr), h, w)
    return ycbcr2rgb(Y, Cb, Cr)


# ---------- Тестові зображення ----------
def find_font(size=48):
    """Жирний DejaVu Sans: системний або той, що йде разом із matplotlib"""
    candidates = [
        '/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf',
        os.path.join(matplotlib.get_data_path(), 'fonts', 'ttf', 'DejaVuSans-Bold.ttf'),
        'DejaVuSans-Bold.ttf',
    ]
    for p in candidates:
        try:
            return ImageFont.truetype(p, size)
        except OSError:
            continue
    raise RuntimeError("Не знайдено DejaVuSans-Bold.ttf (шрифт із кирилицею)")

def redtext(W=320, H=96):
    im = Image.new('RGB', (W, H), (0, 0, 0))
    d = ImageDraw.Draw(im)
    d.text((12, 22), 'ЧЕРВОНИЙ', font=find_font(48), fill=(255, 0, 0))
    return np.array(im)

def lines(W=320, H=160):
    im = np.zeros((H, W, 3), np.uint8)
    cols = [(255, 0, 0), (0, 255, 0), (0, 0, 255),
            (255, 255, 0), (0, 255, 255), (255, 0, 255)]
    for i, c in enumerate(cols):                  # вертикальні
        x = 24 + i * 48
        im[12:70, x:x+2]     = c                  # завтовшки 2 пікселі
        im[12:70, x+22:x+23] = c                  # завтовшки 1 піксель
    for i, c in enumerate(cols[:4]):              # горизонтальні
        y = 90 + i * 16
        im[y:y+2,   24:296] = c
        im[y+7:y+8, 24:296] = c
    return im


# ---------- Завдання 4: карта різниці ----------
def diffmap(orig, out):
    d = np.abs(out.astype(int) - orig.astype(int))
    return d.max(axis=2)

def save_diffmap(orig, out, fname):
    dm = diffmap(orig, out)
    plt.figure()
    plt.imshow(np.clip(dm * 2, 0, 255), cmap='inferno', vmin=0, vmax=255)  # множник 2 — для видимості
    plt.axis('off')
    plt.savefig(fname, dpi=150, bbox_inches='tight')
    plt.close()


# ---------- Завдання 5: вимірювання ----------
def measure(orig, mode):
    out = scheme(orig, mode)
    d = np.abs(out.astype(int) - orig.astype(int))
    mx = d.max(axis=2)
    return d.max(), d.mean(), (mx > 0).mean() * 100, (mx > 30).mean() * 100


if __name__ == '__main__':
    # Завдання 1
    photo = data.astronaut()
    Y, Cb, Cr = rgb2ycbcr(photo)
    back = ycbcr2rgb(Y, Cb, Cr)
    print('Завдання 1. макс похибка туди-назад:', np.abs(back.astype(int) - photo.astype(int)).max())

    # Завдання 2-3
    txt = redtext()
    for mode, fname in (('4:2:2', 'text422.png'), ('4:2:0', 'text420.png')):
        out = scheme(txt, mode)
        Image.fromarray(np.concatenate([txt, out], axis=0)).save(fname)
        print(f'Завдання 2-3. {mode}: макс похибка', np.abs(out.astype(int) - txt.astype(int)).max())

    # Завдання 4
    save_diffmap(txt, scheme(txt, '4:2:0'), 'diff_text.png')
    save_diffmap(photo, scheme(photo, '4:2:0'), 'diff_photo.png')

    # Завдання 5
    tests = {'червоний текст': txt, 'тонкі лінії': lines(), 'фотографія': photo}
    print('\nЗавдання 5. Таблиця вимірювань')
    print(f"{'зображення':<16}{'схема':<8}{'макс':>5}{'середня':>9}{'змінених':>10}{'помітних':>10}")
    rows = []
    for name, im in tests.items():
        for m in ('4:4:4', '4:2:2', '4:2:0'):
            mx, mean, ch, vis = measure(im, m)
            print(f"{name:<16}{m:<8}{mx:>5d}{mean:>9.2f}{ch:>9.1f}%{vis:>9.2f}%")
            rows.append((name, m, mx, mean, ch, vis))

    c = lambda x, d: f"{x:.{d}f}".replace('.', ',')
    t = ("Таблиця 1. Результати вимірювань\n\n"
         "| Зображення | Схема | Макс | Середня | Змінених | Помітних |\n"
         "|------------|-------|------|---------|----------|----------|\n")
    for name, m, mx, mean, ch, vis in rows:
        t += f"| {name} | {m} | {mx} | {c(mean,2)} | {c(ch,1)} % | {c(vis,2)} % |\n"
    open('table.txt', 'w', encoding='utf-8').write(t)

    lin = lines()
    trio = [scheme(lin, m) for m in ('4:4:4', '4:2:2', '4:2:0')]
    Image.fromarray(np.concatenate(trio, axis=1)).save('lines420.png')
    print('\nЗбережено: text422.png, text420.png, diff_text.png, diff_photo.png, lines420.png, table.txt')