import numpy as np
from scipy.io import wavfile
from scipy.signal import spectrogram
import matplotlib.pyplot as plt

SR = 44100

def music(dur=3.0, sr=SR):
    t = np.arange(int(dur * sr)) / sr
    x = np.zeros_like(t)
    for f0 in (220.0, 277.18, 329.63):
        for k, a in enumerate([1, .5, .3, .18, .1, .06], start=1):
            x += a * np.sin(2 * np.pi * f0 * k * t + k)
    x *= np.minimum(1, t * 8) * np.exp(-0.35 * t)
    return 0.85 * x / np.abs(x).max()

x = music()
wavfile.write('source.wav', SR, (x * 32767).astype(np.int16))

n = 8192
sp = np.abs(np.fft.rfft(x[:n] * np.hanning(n)))
db = 20 * np.log10(sp / sp.max() + 1e-12)
fr = np.fft.rfftfreq(n, 1 / SR)

plt.figure(figsize=(9, 4))
plt.plot(fr[fr < 2500], db[fr < 2500])
plt.savefig('spectrum.png')
plt.close()

f, t_spec, S = spectrogram(x, SR, nperseg=2048, noverlap=1792)

plt.figure(figsize=(9, 4))
plt.pcolormesh(t_spec, f, 10 * np.log10(S + 1e-12))
plt.savefig('spectrogram.png')
plt.close()

n_win = 1024
for k, label in ((100.0, 'рівно на біні'), (100.5, 'між бінами')):
    f_val = k * SR / n_win
    t_val = np.arange(n_win) / SR
    x_val = np.sin(2 * np.pi * f_val * t_val)
    for name, w in (('прямокутне', np.ones(n_win)),
                    ('Ганна', np.hanning(n_win)),
                    ('Блекмана', np.blackman(n_win))):
        sp_val = np.abs(np.fft.rfft(x_val * w))
        db_val = 20 * np.log10(sp_val / sp_val.max() + 1e-15)
        far = np.concatenate([db_val[:95], db_val[106:]])
        print(label, name, 'найвищий сторонній пік %.1f дБ' % far.max())

dur3 = 1.4
t3 = np.arange(int(dur3 * SR)) / SR
x3 = np.zeros_like(t3)
seg = t3 < 0.6
x3[seg] = 0.5 * np.sin(2 * np.pi * 440 * t3[seg]) + 0.5 * np.sin(2 * np.pi * 480 * t3[seg])

for c in (0.80, 0.92, 1.04, 1.16):
    i = int(c * SR)
    x3[i:i+40] += 1.0 * np.hanning(40)

f_short, tt_short, S_short = spectrogram(x3, SR, nperseg=128, noverlap=int(128 * 0.9))
plt.figure(figsize=(9, 4))
plt.pcolormesh(tt_short, f_short, 10 * np.log10(S_short + 1e-12))
plt.savefig('spectrogram_short.png')
plt.close()

f_long, tt_long, S_long = spectrogram(x3, SR, nperseg=4096, noverlap=int(4096 * 0.9))
plt.figure(figsize=(9, 4))
plt.pcolormesh(tt_long, f_long, 10 * np.log10(S_long + 1e-12))
plt.savefig('spectrogram_long.png')
plt.close()

def eq(x_in, bands, sr=SR):
    sp_in = np.fft.rfft(x_in)
    fr_in = np.fft.rfftfreq(len(x_in), 1 / sr)
    g = np.ones(len(sp_in))
    for lo, hi, db_gain in bands:
        g[(fr_in >= lo) & (fr_in < hi)] = 10 ** (db_gain / 20)
    return np.fft.irfft(sp_in * g, len(x_in))

def band_energy(x_in, lo, hi, sr=SR):
    sp_in = np.abs(np.fft.rfft(x_in))
    fr_in = np.fft.rfftfreq(len(x_in), 1 / sr)
    return 10 * np.log10((sp_in[(fr_in >= lo) & (fr_in < hi)] ** 2).sum() + 1e-20)

y = eq(x, [(0, 250, 12)])

for lo, hi in ((0, 250), (250, 1000), (1000, 4000), (4000, SR // 2)):
    print(lo, hi, '%.1f → %.1f' % (band_energy(x, lo, hi), band_energy(y, lo, hi)))