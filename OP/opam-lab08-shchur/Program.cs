using System.Diagnostics;

// ===== Варіант: N = 30, перша літера імені А -> K = 1 =====
const int N = 30, K = 1;
int a = N % 3, b = N % 4, c = N % 5;
Console.WriteLine($"N = {N}, K = {K}, a = {a}, b = {b}, c = {c}");

Func<int, int, int> cmp = (p, q) => p.CompareTo(q);   // a = 0: int[]
int X = 10 * K, Y = 12 * K, Absent = 20 * K + 1;

var rnd = new Random(N);
var original = new int[100_000];
for (int i = 0; i < original.Length; i++) original[i] = rnd.Next(0, 20 * K + 1);

var arr = (int[])original.Clone();
Array.Sort(arr, Comparer<int>.Create((p, q) => cmp(p, q)));   // сортуємо тим самим cmp

var queries = new (string name, int value)[]
{
    ($"X = {X}", X),
    ("перший елемент", arr[0]),
    ($"відсутнє {Absent}", Absent),
};

// ===== Завдання 1 і 2 =====
Console.WriteLine("\n== Завдання 1 і 2: лінійний vs бінарний ==");
Console.WriteLine("| запит | лінійний: індекс / порівн. | бінарний: індекс / порівн. |");
Console.WriteLine("|---|---|---|");
foreach (var (name, v) in queries)
{
    int li = Search.LinearSearch(arr, v, cmp, out int lc);
    int bi = Search.BinarySearch(arr, v, cmp, out int bc);
    Console.WriteLine($"| {name} | {li} / {lc} | {bi} / {bc} |");
}

// ----- Поломка 1 -----
Console.WriteLine("\n== Поломка 1: while (lo < hi) ==");
var notFound = new List<int>();
for (int v = 0; v <= 20 * K; v++)
    if (Broken.LoLessHi(arr, v, cmp, out _) == -1 && Search.BinarySearch(arr, v, cmp, out _) != -1)
        notFound.Add(v);
Console.WriteLine($"На масиві з 100 000: наявні, але не знайдені: {(notFound.Count == 0 ? "жодного" : string.Join(", ", notFound))}");
var small = new[] { 1, 2, 3, 4, 5, 6, 7 };
for (int t = 1; t <= 7; t++)
{
    int good = Search.BinarySearch(small, t, cmp, out _);
    int bad = Broken.LoLessHi(small, t, cmp, out _);
    if (good != bad) Console.WriteLine($"  масив {{1..7}}, шукаємо {t}: правильно {good}, зламана {bad}");
}

// ----- Поломка 2 -----
Console.WriteLine("\n== Поломка 2: lo = mid ==");
var r2 = Broken.LoEqualsMid(new[] { 1, 2 }, 2, cmp);
Console.WriteLine($"{{1, 2}}, шукаємо 2: idx={r2.idx}, завис={r2.stuck}, lo={r2.lo}, hi={r2.hi}");
var r2b = Broken.LoEqualsMid(arr, Absent, cmp);
Console.WriteLine($"100 000 елементів, шукаємо {Absent}: idx={r2b.idx}, завис={r2b.stuck}, lo={r2b.lo}, hi={r2b.hi}");

// ----- Поломка 3 -----
Console.WriteLine("\n== Поломка 3: mid = (lo + hi) / 2 ==");
int r3 = Broken.SumMid(arr, Absent, cmp, out int c3);
Search.BinarySearch(arr, Absent, cmp, out int c3ok);
Console.WriteLine($"100 000 елементів: зламана -> {r3}, {c3} порівнянь; правильна -> {c3ok} порівнянь");
foreach (int n in new[] { 100_000, 1_073_741_824, 1_073_741_825 })
{
    var o = Broken.SimulateOverflow(n);
    Console.WriteLine($"n = {n}: переповнення = {o.overflowed}");
}

// ----- Поломка 4 -----
Console.WriteLine("\n== Поломка 4: масив не відсортований ==");
var qr = new Random(1);
int missed = 0;
for (int t = 0; t < 100; t++)
{
    int v = original[qr.Next(original.Length)];   // значення, що точно є
    if (Search.BinarySearch(original, v, cmp, out _) == -1) missed++;
}
Console.WriteLine($"із 100 запитів наявних значень повернули -1: {missed}");

// ===== Завдання 3: b = 2 =====
Console.WriteLine("\n== Завдання 3 ==");
int lb = Search.LowerBound(arr, X, cmp, out int cLb);
int ub = Search.UpperBound(arr, X, cmp, out int cUb);
Console.WriteLine($"Перший елемент >= {X}: індекс {lb}, порівнянь {cLb}");
int linCmp = 0, linIdx = arr.Length;
for (int i = 0; i < arr.Length; i++) { linCmp++; if (cmp(arr[i], X) >= 0) { linIdx = i; break; } }
Console.WriteLine($"  лінійно: індекс {linIdx}, порівнянь {linCmp}");
Console.WriteLine($"Кількість елементів, рівних {X}: {ub - lb}, порівнянь {cLb + cUb} (лінійно: {arr.Length})");

int i7 = Search.FirstWhere(arr, e => e % 7 == 0, out int c7);
int i7lin = -1;
for (int i = 0; i < arr.Length; i++) if (arr[i] % 7 == 0) { i7lin = i; break; }
Console.WriteLine($"Кратний 7: FirstWhere -> {i7} (порівнянь {c7}), правильно -> {i7lin}, збігається: {i7 == i7lin}");

// ===== Завдання 4: c = 0, рекурсивний бінарний =====
Console.WriteLine("\n== Завдання 4: рекурсивний бінарний ==");
Console.WriteLine("| запит | класичний: індекс / порівн. | рекурсивний: індекс / порівн. | глибина |");
Console.WriteLine("|---|---|---|---|");
int maxDepth = 0;
foreach (var (name, v) in queries)
{
    int bi = Search.BinarySearch(arr, v, cmp, out int bc);
    int ri = Search.RecursiveBinarySearch(arr, v, cmp, out int rc);
    maxDepth = Math.Max(maxDepth, Search.MaxDepth);
    Console.WriteLine($"| {name} | {bi} / {bc} | {ri} / {rc} | {Search.MaxDepth} |");
}
Console.WriteLine($"Максимальна глибина рекурсії: {maxDepth}");

// ===== Крайові випадки =====
Console.WriteLine("\n== Крайові випадки ==");
Console.WriteLine("| Вхід | Запит | Очікувано | Отримано | зламана #1 | зламана #2 |");
Console.WriteLine("|---|---|---|---|---|---|");
void SearchRow(string input, int[] arrIn, int target, int expected)
{
    int good = Search.BinarySearch(arrIn, target, cmp, out _);
    int b1 = Broken.LoLessHi(arrIn, target, cmp, out _);
    var b2 = Broken.LoEqualsMid(arrIn, target, cmp);
    Console.WriteLine($"| {input} | пошук {target} | {expected} | {good} | {b1} | {(b2.stuck ? "ЗАВИС" : b2.idx.ToString())} |");
}
void BoundRow(string input, int[] arrIn, int x, string expected)
{
    int lo = Search.LowerBound(arrIn, x, cmp, out _);
    int hi = Search.UpperBound(arrIn, x, cmp, out _);
    Console.WriteLine($"| {input} | межі для {x} | {expected} | ({lo}, {hi}) | - | - |");
}
SearchRow("порожній масив", Array.Empty<int>(), 5, -1);
SearchRow("[7], він же шуканий", new[] { 7 }, 7, 0);
SearchRow("[3, 7], шуканий — другий", new[] { 3, 7 }, 7, 1);
BoundRow("шукане менше за всі: [5,6,7]", new[] { 5, 6, 7 }, 1, "(0, 0)");
BoundRow("шукане більше за всі: [5,6,7]", new[] { 5, 6, 7 }, 9, "(3, 3)");
BoundRow("усі рівні X: [4,4,4,4,4]", new[] { 4, 4, 4, 4, 4 }, 4, "(0, 5)");

// ===== Завдання 5: вимірювання =====
Console.WriteLine("\n== Завдання 5, таблиця 1: порівняння ==");
Measure.ComparisonTable(cmp, N, K);

Console.WriteLine("\n== Завдання 5, таблиця 2: час, 100 000 елементів ==");
var absentQ = new int[100];
Array.Fill(absentQ, Absent);                       // найгірший випадок для лінійного
var presentQ = new int[100];
var qr2 = new Random(2);
for (int i = 0; i < presentQ.Length; i++) presentQ[i] = qr2.Next(0, 20 * K + 1);
Measure.BreakEven($"Сценарій А: усі запити — відсутнє значення {Absent}", original, absentQ, cmp);
Measure.BreakEven("Сценарій Б: випадкові наявні значення", original, presentQ, cmp);
Console.WriteLine($"\n(лекційна оцінка 2*log2(n) = {2 * Math.Log2(100_000):F1}; Sink = {Measure.Sink})");

// ======================================================================
//                              КЛАСИ
// ======================================================================

/// <summary>Правильні пошуки. Кожен рахує порівняння в out-параметр.</summary>
static class Search
{
    public static int LinearSearch<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons)
    {
        comparisons = 0;
        for (int i = 0; i < a.Length; i++)
        {
            comparisons++;
            if (cmp(a[i], target) == 0) return i;
        }
        return -1;
    }

    public static int BinarySearch<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons)
    {
        comparisons = 0;
        int lo = 0, hi = a.Length - 1;
        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2;
            comparisons++;
            int c = cmp(a[mid], target);
            if (c == 0) return mid;
            if (c < 0) lo = mid + 1;
            else hi = mid - 1;
        }
        return -1;
    }

    /// <summary>Індекс першого елемента, що задовольняє умову (або a.Length). Умова має бути монотонною.</summary>
    public static int FirstWhere<T>(T[] a, Func<T, bool> pred, out int comparisons)
    {
        comparisons = 0;
        int lo = 0, hi = a.Length;
        while (lo < hi)
        {
            int mid = lo + (hi - lo) / 2;
            comparisons++;
            if (pred(a[mid])) hi = mid;
            else lo = mid + 1;
        }
        return lo;
    }

    public static int LowerBound<T>(T[] a, T x, Func<T, T, int> cmp, out int comparisons)
        => FirstWhere(a, e => cmp(e, x) >= 0, out comparisons);

    public static int UpperBound<T>(T[] a, T x, Func<T, T, int> cmp, out int comparisons)
        => FirstWhere(a, e => cmp(e, x) > 0, out comparisons);

    // ----- c = 0: рекурсивний бінарний -----
    public static int MaxDepth;

    public static int RecursiveBinarySearch<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons)
    {
        comparisons = 0;
        MaxDepth = 0;
        return Rec(a, target, cmp, 0, a.Length - 1, 1, ref comparisons);
    }

    static int Rec<T>(T[] a, T target, Func<T, T, int> cmp, int lo, int hi, int depth, ref int cnt)
    {
        if (depth > MaxDepth) MaxDepth = depth;
        if (lo > hi) return -1;
        int mid = lo + (hi - lo) / 2;
        cnt++;
        int c = cmp(a[mid], target);
        if (c == 0) return mid;
        return c < 0
            ? Rec(a, target, cmp, mid + 1, hi, depth + 1, ref cnt)
            : Rec(a, target, cmp, lo, mid - 1, depth + 1, ref cnt);
    }
}

/// <summary>Зламані копії бінарного пошуку (завдання 2).</summary>
static class Broken
{
    // Поломка 1: lo < hi замість <=
    public static int LoLessHi<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons)
    {
        comparisons = 0;
        int lo = 0, hi = a.Length - 1;
        while (lo < hi)                       // <-- має бути <=
        {
            int mid = lo + (hi - lo) / 2;
            comparisons++;
            int c = cmp(a[mid], target);
            if (c == 0) return mid;
            if (c < 0) lo = mid + 1;
            else hi = mid - 1;
        }
        return -1;
    }

    // Поломка 2: lo = mid замість mid + 1 (цикл обмежений лічильником)
    public static (int idx, int comps, bool stuck, int lo, int hi) LoEqualsMid<T>(
        T[] a, T target, Func<T, T, int> cmp, int cap = 1000)
    {
        int comps = 0, iter = 0;
        int lo = 0, hi = a.Length - 1;
        while (lo <= hi)
        {
            if (++iter > cap) return (-1, comps, true, lo, hi);
            int mid = lo + (hi - lo) / 2;
            comps++;
            int c = cmp(a[mid], target);
            if (c == 0) return (mid, comps, false, lo, hi);
            if (c < 0) lo = mid;              // <-- має бути mid + 1
            else hi = mid - 1;
        }
        return (-1, comps, false, lo, hi);
    }

    // Поломка 3: mid = (lo + hi) / 2
    public static int SumMid<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons)
    {
        comparisons = 0;
        int lo = 0, hi = a.Length - 1;
        while (lo <= hi)
        {
            int mid = unchecked(lo + hi) / 2; // <-- може переповнитись
            comparisons++;
            int c = cmp(a[mid], target);
            if (c == 0) return mid;
            if (c < 0) lo = mid + 1;
            else hi = mid - 1;
        }
        return -1;
    }

    /// <summary>Пошук останнього елемента у "віртуальному" масиві довжини n: чи переповнюється lo + hi.</summary>
    public static (bool overflowed, int lo, int hi, int sumWrapped) SimulateOverflow(int n)
    {
        int target = n - 1;
        int lo = 0, hi = n - 1;
        while (lo <= hi)
        {
            int sum = unchecked(lo + hi);
            if (sum < 0) return (true, lo, hi, sum);
            int mid = sum / 2;
            if (mid == target) return (false, lo, hi, sum);
            if (mid < target) lo = mid + 1;
            else hi = mid - 1;
        }
        return (false, lo, hi, 0);
    }
    // Поломка 4 (невідсортований масив) окремого коду не має: див. верх файла.
}

/// <summary>Вимірювання завдання 5. Запускати: dotnet run -c Release</summary>
static class Measure
{
    public static int Sink; // щоб JIT не викинув виклики

    public static void ComparisonTable(Func<int, int, int> cmp, int seed, int K)
    {
        Console.WriteLine("| n | Лінійний | Бінарний | Відношення | Теорія: n/2 і log2 n |");
        Console.WriteLine("|---|---|---|---|---|");
        foreach (int n in new[] { 1_000, 10_000, 100_000, 1_000_000 })
        {
            var rnd = new Random(seed);
            var arr = new int[n];
            for (int i = 0; i < n; i++) arr[i] = rnd.Next(0, 20 * K + 1);
            Array.Sort(arr, Comparer<int>.Create((p, q) => cmp(p, q)));

            long lin = 0, bin = 0;
            for (int t = 0; t < 100; t++)
            {
                int v = rnd.Next(0, 20 * K + 2);       // 20K+1 — відсутнє
                Search.LinearSearch(arr, v, cmp, out int lc);
                Search.BinarySearch(arr, v, cmp, out int bc);
                lin += lc; bin += bc;
            }
            double la = lin / 100.0, ba = bin / 100.0;
            Console.WriteLine($"| {n} | {la:F0} | {ba:F1} | {la / ba:F0} | {n / 2} і {Math.Ceiling(Math.Log2(n))} |");
        }
    }

    static double Min3(Action act)
    {
        act(); // прогрів
        double best = double.MaxValue;
        for (int r = 0; r < 3; r++)
        {
            long t0 = Stopwatch.GetTimestamp();
            act();
            double ms = (Stopwatch.GetTimestamp() - t0) * 1000.0 / Stopwatch.Frequency;
            best = Math.Min(best, ms);
        }
        return best;
    }

    public static void BreakEven(string title, int[] original, int[] queries, Func<int, int, int> cmp)
    {
        Console.WriteLine($"\n{title}");
        Console.WriteLine("| q | Лінійно, мс | Сортування + бінарно, мс | Хто швидший |");
        Console.WriteLine("|---|---|---|---|");
        foreach (int q in new[] { 1, 5, 10, 20, 30, 50, 100 })
        {
            double lin = Min3(() =>
            {
                int s = 0;
                for (int i = 0; i < q; i++) s += Search.LinearSearch(original, queries[i], cmp, out _);
                Sink = s;
            });
            double sb = Min3(() =>
            {
                var copy = (int[])original.Clone();
                Array.Sort(copy, Comparer<int>.Create((p, r) => cmp(p, r)));
                int s = 0;
                for (int i = 0; i < q; i++) s += Search.BinarySearch(copy, queries[i], cmp, out _);
                Sink = s;
            });
            Console.WriteLine($"| {q} | {lin:F3} | {sb:F3} | {(lin <= sb ? "лінійно" : "сортування + бінарно")} |");
        }
    }
}