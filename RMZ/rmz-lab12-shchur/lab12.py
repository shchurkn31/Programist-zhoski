import matplotlib.pyplot as plt
import numpy as np
from scipy.io import wavfile

SR_DEFAULT = 44100


def save_wav(filename, data, sr=SR_DEFAULT):
  scaled = np.int16(np.clip(data, -1.0, 1.0) * 32767)
  wavfile.write(filename, sr, scaled)


def plot_spectrogram(signal, filename, sr=SR_DEFAULT):
  plt.figure(figsize=(8, 4))
  plt.specgram(signal, Fs=sr, NFFT=256, noverlap=128)
  plt.xlabel("Час (с)")
  plt.ylabel("Частота (Гц)")
  plt.title("Спектрограма sweep")
  plt.colorbar(label="Інтенсивність (дБ)")
  plt.tight_layout()
  plt.savefig(filename)
  plt.close()


def plot_spectrum(signals_with_labels, filename, sr=SR_DEFAULT):
  plt.figure(figsize=(10, 5))
  for sig, label in signals_with_labels:
    fft_vals = np.abs(np.fft.rfft(sig))
    freqs = np.fft.rfftfreq(len(sig), 1 / sr)
    fft_db = 20 * np.log10(fft_vals / (np.max(fft_vals) + 1e-12))
    plt.plot(freqs, fft_db, label=label, alpha=0.7)
  plt.xlabel("Частота (Гц)")
  plt.ylabel("Амплітуда (дБ)")
  plt.ylim(-80, 5)
  plt.grid(True, linestyle="--", alpha=0.6)
  plt.legend()
  plt.tight_layout()
  plt.savefig(filename)
  plt.close()


def peak_freq(x, sr):
  fft_vals = np.abs(np.fft.rfft(x))
  freqs = np.fft.rfftfreq(len(x), 1 / sr)
  return freqs[np.argmax(fft_vals)]


def snr_db(orig, quant):
  noise = orig - quant
  p_orig = np.mean(orig**2)
  p_noise = np.mean(noise**2)
  return 10 * np.log10(p_orig / p_noise)


def tone(freq, dur=2.0, amp=0.5, sr=SR_DEFAULT):
  t = np.arange(int(dur * sr)) / sr
  return amp * np.sin(2 * np.pi * freq * t)


def main_task1():
  print("--- Завдання 1 ---")
  for f in (220, 440, 880):
    save_wav(f"tone_{f}.wav", tone(f, 2.0))
    print(f"Збережено tone_{f}.wav")


def sweep(f1, f2, dur, amp=0.5, sr=SR_DEFAULT):
  t = np.arange(int(dur * sr)) / sr
  phase = 2 * np.pi * (f1 * t + (f2 - f1) / (2 * dur) * (t**2))
  return amp * np.sin(phase)


def main_task2():
  print("\n--- Завдання 2 ---")
  sr = 8000
  s = sweep(0, sr, 6.0, sr=sr)
  save_wav("sweep.wav", s, sr=sr)
  plot_spectrogram(s, "sweep.png", sr=sr)
  print("Збережено sweep.wav та sweep.png")

  for f in (3000, 5000, 7000):
    x = tone(f, 1.0, sr=sr)
    save_wav(f"alias_{f}.wav", x, sr=sr)
    print(f, "→", peak_freq(x, sr))


def quantize(x, bits):
  scale = 2 ** (bits - 1) - 1
  return np.round(x * scale) / scale


def main_task3():
  print("\n--- Завдання 3 ---")
  t = tone(440, 2.0, amp=0.9)
  for b in (16, 8, 4):
    q = quantize(t, b)
    save_wav(f"quant_{b}bit.wav", q)
    print(f"{b} біт: SNR {snr_db(t, q):.1f} дБ")


def dither(x, bits):
  lsb = 1.0 / (2 ** (bits - 1) - 1)
  noise = np.random.uniform(-0.5, 0.5, size=x.shape) * lsb
  return quantize(x + noise, bits)


def main_task4():
  print("\n--- Завдання 4 ---")
  q = tone(440, 3.0, amp=0.25)
  save_wav("dither_off.wav", quantize(q, 4))
  save_wav("dither_on.wav", dither(q, 4))
  plot_spectrum(
      [(quantize(q, 4), "без дизеру"), (dither(q, 4), "з дизером")], "dither.png"
  )
  print("SNR без:", round(snr_db(q, quantize(q, 4)), 1), "дБ")
  print("SNR з: ", round(snr_db(q, dither(q, 4)), 1), "дБ")


if __name__ == "__main__":
  main_task1()
  main_task2()
  main_task3()
  main_task4()