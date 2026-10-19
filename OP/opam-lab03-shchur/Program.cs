using System;
using System.IO;

class Program
{
    // ===== Завдання 1: найбільший файл (b = 2) =====
    // Базовий випадок: у папці без підпапок цикл по підпапках не виконується
    // і функція просто повертає найбільший файл цієї папки.
    static (string name, long size, string path) FindLargest(string dir)
    {
        var best = (name: "", size: -1L, path: "");
        try
        {
            foreach (string f in Directory.GetFiles(dir))
            {
                long len = new FileInfo(f).Length;
                if (len > best.size)
                    best = (Path.GetFileName(f), len, Path.GetFullPath(f));
            }

            foreach (string sub in Directory.GetDirectories(dir))
            {
                var r = FindLargest(sub);          // рекурсивний виклик
                if (r.size > best.size)
                    best = r;
            }
        }
        catch (UnauthorizedAccessException)
        {
            // немає доступу до папки - просто пропускаємо
        }
        catch (IOException)
        {
            // на випадок проблем з файлом/папкою
        }
        return best;
    }

    // ===== Завдання 2: максимум через дві половини (a = 0) =====
    // Базовий випадок: lo == hi, залишився один елемент.
    // depth і maxDepth потрібні, щоб порахувати глибину рекурсії.
    static int Solve(int[] a, int lo, int hi, int depth, ref int maxDepth)
    {
        if (depth > maxDepth) maxDepth = depth;

        if (lo == hi)
            return a[lo];

        int mid = (lo + hi) / 2;
        int left = Solve(a, lo, mid, depth + 1, ref maxDepth);
        int right = Solve(a, mid + 1, hi, depth + 1, ref maxDepth);

        return left > right ? left : right;
    }

    // ===== Завдання 3: усі перестановки 4 літер (c = 0) =====
    // Базовий випадок: усі позиції заповнені (pos == letters.Length).
    // Повертає кількість знайдених перестановок.
    static int Permute(char[] letters, bool[] used, char[] current, int pos)
    {
        if (pos == letters.Length)
        {
            Console.WriteLine(new string(current));
            return 1;
        }

        int count = 0;
        for (int i = 0; i < letters.Length; i++)
        {
            if (used[i]) continue;

            used[i] = true;                 // зробити вибір
            current[pos] = letters[i];
            count += Permute(letters, used, current, pos + 1);  // вглиб
            used[i] = false;                // скасувати вибір (повернення)
        }
        return count;
    }

    // ===== Завдання 4: трасування стеку викликів =====
    static int MaxTrace(int[] a, int lo, int hi, int depth)
    {
        string indent = new string(' ', depth * 2);

        if (lo == hi)
        {
            Console.WriteLine($"{indent}enter Max({lo}..{hi}) -> {a[lo]}");
            return a[lo];
        }

        Console.WriteLine($"{indent}enter Max({lo}..{hi})");

        int mid = (lo + hi) / 2;
        int left = MaxTrace(a, lo, mid, depth + 1);
        int right = MaxTrace(a, mid + 1, hi, depth + 1);
        int res = left > right ? left : right;

        Console.WriteLine($"{indent}exit  Max({lo}..{hi}) -> {res}");
        return res;
    }

    // ===== Завдання 5: той самий максимум циклом =====
    static int MaxLoop(int[] a)
    {
        int m = a[0];
        for (int i = 1; i < a.Length; i++)
            if (a[i] > m) m = a[i];
        return m;
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // ---------- Завдання 1 ----------
        Console.WriteLine("=== Завдання 1: найбільший файл ===");
        Console.Write("Введіть шлях до папки: ");
        string dir = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
        {
            Console.WriteLine("Такої папки не існує.");
        }
        else
        {
            var big = FindLargest(dir);
            if (big.size < 0)
                Console.WriteLine("Файлів не знайдено.");
            else
            {
                Console.WriteLine($"Ім'я:   {big.name}");
                Console.WriteLine($"Розмір: {big.size} байт");
                Console.WriteLine($"Шлях:   {big.path}");
            }
        }

        // ---------- Завдання 2 ----------
        Console.WriteLine();
        Console.WriteLine("=== Завдання 2: максимум через дві половини ===");
        Random rnd = new Random(30);
        int[] arr = new int[16];
        for (int i = 0; i < arr.Length; i++)
            arr[i] = rnd.Next(-20, 21);          // [-20; 20]

        Console.WriteLine("Масив: " + string.Join(", ", arr));

        int maxDepth = 0;
        int maxRec = Solve(arr, 0, arr.Length - 1, 0, ref maxDepth);
        Console.WriteLine($"Максимум (рекурсія): {maxRec}");
        Console.WriteLine($"Максимальна глибина рекурсії: {maxDepth}");
        Console.WriteLine($"log2(16) = {Math.Log2(16)}");

        // ---------- Завдання 3 ----------
        Console.WriteLine();
        Console.WriteLine("=== Завдання 3: перестановки 4 літер ===");
        char[] letters = { 'А', 'Б', 'В', 'Г' };   // M = K % 3 + 3 = 4
        int total = Permute(letters, new bool[4], new char[4], 0);
        Console.WriteLine($"Усього перестановок: {total}");

        // ---------- Завдання 4 ----------
        Console.WriteLine();
        Console.WriteLine("=== Завдання 4: трасування ===");
        int[] small = new int[4];
        Random rnd2 = new Random(30);
        for (int i = 0; i < small.Length; i++)
            small[i] = rnd2.Next(-20, 21);
        Console.WriteLine("Масив: " + string.Join(", ", small));
        MaxTrace(small, 0, small.Length - 1, 0);

        // ---------- Завдання 5 ----------
        Console.WriteLine();
        Console.WriteLine("=== Завдання 5: ітеративна версія ===");
        int maxIter = MaxLoop(arr);
        Console.WriteLine($"Максимум (цикл): {maxIter}");
        Console.WriteLine(maxIter == maxRec
            ? "Результати збігаються."
            : "Результати РІЗНІ!");
    }
}