import numpy as np


def to565(c):
    c = np.asarray(c, float)
    r = int(round(c[0] * 31 / 255))
    g = int(round(c[1] * 63 / 255))
    b = int(round(c[2] * 31 / 255))
    r8 = (r << 3) | (r >> 2)
    g8 = (g << 2) | (g >> 4)
    b8 = (b << 3) | (b >> 2)
    return np.array([r8, g8, b8], float)


def make_palette(c0, c1):
    c0 = np.asarray(c0, float)
    c1 = np.asarray(c1, float)
    return np.stack([c0, c1, (2 * c0 + c1) / 3, (c0 + 2 * c1) / 3])


def pick_endpoints(blk, mode='bbox'):
    if mode == 'bbox':
        return blk.max(axis=0), blk.min(axis=0)
    else:
        L = blk @ np.array([0.299, 0.587, 0.114])
        return blk[L.argmax()], blk[L.argmin()]


def encode_block(blk, mode='bbox'):
    c0, c1 = pick_endpoints(blk, mode)
    pal = make_palette(to565(c0), to565(c1))
    d = ((blk[:, None, :] - pal[None, :, :]) ** 2).sum(-1)
    return pal, d.argmin(axis=1)


def bc1(im, mode='bbox'):
    im = np.asarray(im, float)
    h, w, _ = im.shape
    H, W = -(-h // 4) * 4, -(-w // 4) * 4
    pad = np.pad(im, ((0, H - h), (0, W - w), (0, 0)), mode='edge')
    out = np.zeros_like(pad)
    for y in range(0, H, 4):
        for x in range(0, W, 4):
            blk = pad[y:y + 4, x:x + 4].reshape(16, 3)
            pal, idx = encode_block(blk, mode)
            out[y:y + 4, x:x + 4] = pal[idx].reshape(4, 4, 3)
    return out[:h, :w]


def stats(im, rec):
    diff = np.asarray(im, float) - rec
    mae = np.abs(diff).mean()
    mx = np.abs(diff).max()
    rmse = np.sqrt((diff ** 2).mean())
    return mae, mx, rmse


def angle_error(nm, rec):
    a = np.asarray(nm, float) / 255 * 2 - 1
    b = np.asarray(rec, float) / 255 * 2 - 1
    a /= np.linalg.norm(a, axis=-1, keepdims=True) + 1e-9
    b /= np.linalg.norm(b, axis=-1, keepdims=True) + 1e-9
    cos = np.clip((a * b).sum(-1), -1, 1)
    return np.degrees(np.arccos(cos))


def make_tests(n=96):
    rng = np.random.default_rng(0)
    yy, xx = np.mgrid[0:n, 0:n].astype(float)

    photo = np.stack([
        128 + 60 * np.sin(xx / 13) + 40 * np.cos(yy / 9),
        128 + 50 * np.sin((xx + yy) / 17),
        128 + 55 * np.cos(xx / 11 - yy / 15),
    ], -1) + rng.normal(0, 3, (n, n, 3))
    photo = np.clip(photo, 0, 255)

    gradient = np.stack([
        xx / (n - 1) * 255,
        yy / (n - 1) * 255,
        (xx + yy) / (2 * (n - 1)) * 255,
    ], -1)

    height = 6 * np.sin(xx / 7) * np.cos(yy / 9) + rng.normal(0, 0.15, (n, n))
    gy, gx = np.gradient(height)
    nz = np.ones_like(gx)
    nrm = np.stack([-gx, -gy, nz], -1)
    nrm /= np.linalg.norm(nrm, axis=-1, keepdims=True)
    normal_map = (nrm * 0.5 + 0.5) * 255

    stripes = np.zeros((n, n, 3))
    colors = np.array([[255, 0, 0], [0, 255, 0], [0, 0, 255]], float)
    for x in range(n):
        stripes[:, x] = colors[(x // 37) % 3]

    return {
        'фотографія': photo,
        'градієнт': gradient,
        'нормал-мапа': normal_map,
        'різкі межі': stripes,
    }


TESTS = make_tests()


if __name__ == '__main__':
    white_black = make_palette([255, 255, 255], [0, 0, 0])
    print(white_black[:, 0])

    blk = np.array([[255, 0, 0]] * 8 + [[0, 255, 0]] * 8, float)
    for mode in ('bbox', 'pixel'):
        pal, idx = encode_block(blk, mode)
        print(mode, ((blk - pal[idx]) ** 2).sum())

    for name, im in TESTS.items():
        a = bc1(im, 'bbox')
        b = bc1(im, 'pixel')
        print(name, 'коробка %.2f пікселі %.2f' %
              (stats(im, a)[2], stats(im, b)[2]))

    nm = TESTS['нормал-мапа']
    ang = angle_error(nm, bc1(nm))
    print('середня %.2f°, максимум %.2f°' % (ang.mean(), ang.max()))
    print('більше за 2°: %.1f%%' % ((ang > 2).mean() * 100))