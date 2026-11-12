import os
import wave
import numpy as np
import matplotlib.pyplot as plt

FS = 16000
N = 1024
HOP = N // 2
WIN = 0.5 - 0.5 * np.cos(2.0 * np.pi * np.arange(N) / N)


def bark(f):
    f = np.asarray(f, dtype=float)
    return 13 * np.arctan(0.00076 * f) + 3.5 * np.arctan((f / 7500) ** 2)


FREQS = np.fft.rfftfreq(N, d=1.0 / FS)
BZ = bark(FREQS)


def test_bark():
    b100 = bark(100)
    b1000 = bark(1000)
    cb_1k = 100.0 / (bark(1050) - bark(950))
    print(f"[Тест 1] 100 Гц -> {b100:.2f} барк (очікується ~0.99)")
    print(f"[Тест 1] 1000 Гц -> {b1000:.2f} барк (очікується ~8.51)")
    print(f"[Тест 1] Критична смуга на 1 кГц -> {cb_1k:.1f} Гц (очікується ~157 Гц)")
    assert np.isclose(b100, 0.99, atol=0.02)
    assert np.isclose(b1000, 8.51, atol=0.02)
    assert np.isclose(cb_1k, 157.0, atol=2.0)


def mask_curve(db, bz=BZ):
    thr = np.full_like(db, -300.0)
    for i in np.where(db > db.max() - 50)[0]:
        dz = bz - bz[i]
        sl = np.where(dz < 0, 27 * (-dz), 10 * dz)
        thr = np.maximum(thr, db[i] - 10 - sl)
    return thr


def test_mask_curve():
    db = np.full_like(FREQS, -120.0)
    idx_1000 = np.argmin(np.abs(FREQS - 1000.0))
    idx_800 = np.argmin(np.abs(FREQS - 800.0))
    idx_1200 = np.argmin(np.abs(FREQS - 1200.0))
    idx_6000 = np.argmin(np.abs(FREQS - 6000.0))

    db[idx_1000] = 0.0
    thr = mask_curve(db, BZ)

    print(f"[Тест 2] Поріг під тоном (~1000 Гц): {thr[idx_1000]:.1f} дБ (очікується -10.0 дБ)")
    print(f"[Тест 2] Поріг на 800 Гц: {thr[idx_800]:.1f} дБ | на 1200 Гц: {thr[idx_1200]:.1f} дБ")
    print(f"[Тест 2] Поріг на 6000 Гц: {thr[idx_6000]:.1f} дБ (очікується < -60 дБ)")

    assert np.isclose(thr[idx_1000], -10.0, atol=1e-6)
    assert thr[idx_1200] > thr[idx_800], "Крива маскування має бути несиметричною"
    assert thr[idx_6000] < -60.0, "На 6000 Гц поріг має впасти нижче -60 дБ"


def frames(x):
    for s in range(0, len(x) - N + 1, HOP):
        yield s, x[s:s + N] * WIN


def codec(x, floor_db, use_mask=False, bz=BZ):
    out = np.zeros_like(x, dtype=float)
    kept = 0
    tot = 0
    for s, fr in frames(x):
        sp = np.fft.rfft(fr)
        mag = np.abs(sp)
        db = 20 * np.log10(mag / (mag.max() + 1e-12) + 1e-12)
        thr = np.full_like(db, float(floor_db))
        if use_mask:
            thr = np.maximum(thr, mask_curve(db, bz))
        keep = db > thr
        kept += int(keep.sum())
        tot += len(keep)
        out[s:s + N] += np.fft.irfft(sp * keep)

    ratio = tot / max(kept, 1)
    return out, ratio


def calc_snr(orig, rec):
    valid = slice(HOP, len(orig) - HOP)
    sig = orig[valid]
    err = sig - rec[valid]
    return 10.0 * np.log10(np.sum(sig ** 2) / (np.sum(err ** 2) + 1e-30))


def test_codec(x, bz):
    rec, _ = codec(x, floor_db=-300, use_mask=False, bz=bz)
    snr = calc_snr(x, rec)
    print(f"[Тест 3] SNR без викидання (-300 дБ): {snr:.2f} дБ (очікується > 100 дБ)")
    assert snr > 100.0, f"Очікувався SNR > 100 дБ, отримано {snr:.2f} дБ"


def load_or_generate_audio():
    for fname in sorted(os.listdir(".")):
        if fname.endswith(".wav") and not fname.startswith(("plain_", "mask_")):
            with wave.open(fname, "rb") as wf:
                fs = wf.getframerate()
                n_channels = wf.getnchannels()
                sampwidth = wf.getsampwidth()
                raw = wf.readframes(wf.getnframes())
            dtype = np.int16 if sampwidth == 2 else np.uint8
            data = np.frombuffer(raw, dtype=dtype).astype(float)
            if n_channels > 1:
                data = data.reshape(-1, n_channels).mean(axis=1)
            data /= np.max(np.abs(data)) + 1e-12
            print(f"Завантажено файл: {fname} ({fs} Гц)")
            return data, fs

    t = np.arange(4 * FS) / FS
    x = (
        0.45 * np.sin(2 * np.pi * 440 * t)
        + 0.25 * np.sin(2 * np.pi * 880 * t)
        + 0.15 * np.sin(2 * np.pi * 1320 * t)
        + 0.08 * np.sin(2 * np.pi * 2600 * t) * (0.5 + 0.5 * np.sin(2 * np.pi * 2 * t))
        + 0.01 * np.random.default_rng(42).normal(size=len(t))
    )
    env = 0.6 + 0.4 * np.sin(2 * np.pi * 1.5 * t)
    x = x * env
    x /= np.max(np.abs(x)) + 1e-12
    print("Використано синтезований тестовий аудіосигнал (16 кГц, 4 с).")
    return x, FS


def save_wav(filename, sig, fs):
    sig_norm = np.clip(sig / (np.max(np.abs(sig)) + 1e-12), -1.0, 1.0)
    pcm = (sig_norm * 32767).astype(np.int16)
    with wave.open(filename, "wb") as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(fs)
        wf.writeframes(pcm.tobytes())


def run_experiment():
    test_bark()
    test_mask_curve()

    x, fs = load_or_generate_audio()
    bz = bark(np.fft.rfftfreq(N, d=1.0 / fs))

    x = x[: (len(x) // HOP) * HOP]
    test_codec(x, bz)

    thresholds = [-60, -50, -40, -30, -25]
    save_thresholds = {-60, -40, -25}

    plain_ratios, plain_snrs = [], []
    mask_ratios, mask_snrs = [], []

    print("\n" + "=" * 62)
    print(f"{'Поріг':>8} | {'Лише поріг':>11} {'SNR':>7} | {'З маскуванням':>14} {'SNR':>7}")
    print("-" * 62)

    for thr in thresholds:
        rec_plain, r_plain = codec(x, floor_db=thr, use_mask=False, bz=bz)
        snr_plain = calc_snr(x, rec_plain)

        rec_mask, r_mask = codec(x, floor_db=thr, use_mask=True, bz=bz)
        snr_mask = calc_snr(x, rec_mask)

        plain_ratios.append(r_plain)
        plain_snrs.append(snr_plain)
        mask_ratios.append(r_mask)
        mask_snrs.append(snr_mask)

        print(
            f"{thr:>5} дБ | {r_plain:>10.1f}× {snr_plain:>7.1f} | "
            f"{r_mask:>13.1f}× {snr_mask:>7.1f}"
        )

        if thr in save_thresholds:
            tag = abs(thr)
            save_wav(f"plain_{tag}.wav", rec_plain, fs)
            save_wav(f"mask_{tag}.wav", rec_mask, fs)

    print("=" * 62)
    print("Збережено аудіофайли: plain_60.wav, mask_60.wav, plain_40.wav, mask_40.wav, plain_25.wav, mask_25.wav")

    plt.figure(figsize=(8, 5))
    plt.plot(plain_ratios, plain_snrs, "o-", label="Лише поріг (plain)")
    plt.plot(mask_ratios, mask_snrs, "s-", label="З маскуванням (mask)")

    for thr, rp, sp, rm, sm in zip(thresholds, plain_ratios, plain_snrs, mask_ratios, mask_snrs):
        plt.annotate(f"{thr} дБ", (rp, sp), textcoords="offset points", xytext=(0, 7), ha="center", fontsize=8)
        plt.annotate(f"{thr} дБ", (rm, sm), textcoords="offset points", xytext=(0, -13), ha="center", fontsize=8)

    plt.xlabel("Коефіцієнт стиснення (разів)")
    plt.ylabel("SNR (дБ)")
    plt.title("Залежність SNR від ступеня стиснення")
    plt.grid(True, linestyle="--", alpha=0.6)
    plt.legend()
    plt.tight_layout()
    plt.savefig("lab14_curves.png", dpi=150)
    plt.show()


if __name__ == "__main__":
    run_experiment()