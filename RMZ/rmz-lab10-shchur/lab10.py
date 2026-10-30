BPP = {'RGBA32': 32, 'ASTC 4×4': 8, 'ASTC 6×6': 128 / 36, 'ASTC 8×8': 2}

FORMAT_BY_KIND = {
    'color': 'ASTC 6×6',
    'normal': 'ASTC 4×4',
    'ui': 'ASTC 4×4',
    'far': 'ASTC 6×6',
    'util': 'ASTC 8×8',
}

BUDGET = 24.0


def mem(side, fmt, mips=True):
    b = side * side * BPP[fmt] / 8
    if mips:
        b *= 4 / 3
    return b / 2 ** 20


SCENE = [
    ('Ground_Albedo', 2048, 'color', 2048),
    ('Ground_Normal', 2048, 'normal', 1024),
]


def run(scene, budget=BUDGET):
    total_before = 0.0
    total_after = 0.0
    print('%-20s %6s %6s %-10s %8s %8s %8s' %
          ('текстура', 'було', 'стало', 'формат', 'до, МБ', 'після', 'економія'))
    for name, side, kind, new_side in scene:
        fmt = FORMAT_BY_KIND[kind]
        before = mem(side, 'RGBA32')
        after = mem(new_side, fmt)
        total_before += before
        total_after += after
        print('%-20s %6d %6d %-10s %8.2f %8.2f %8.2f' %
              (name, side, new_side, fmt, before, after, before - after))
    print('до: %.2f МБ' % total_before)
    print('після: %.2f МБ' % total_after)
    print('бюджет: %.2f МБ, %s' %
          (budget, 'вкладаємось' if total_after <= budget else 'не вкладаємось'))
    return total_before, total_after


if __name__ == '__main__':
    print(mem(2048, 'RGBA32'))
    run(SCENE)