using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SortingLab9
{
    public class Student
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public double Score { get; set; }

        public Student(string name, string group, double score)
        {
            Name = name;
            Group = group;
            Score = score;
        }

        public override string ToString() => $"{Name} ({Group}) - {Score}";
    }

    public static class Sorter
    {
        public static bool IsSorted<T>(T[] a, Func<T, T, int> cmp)
        {
            if (a.Length <= 1) return true;
            for (int i = 0; i < a.Length - 1; i++)
            {
                if (cmp(a[i], a[i + 1]) > 0)
                    return false;
            }
            return true;
        }

        public static bool IsPermutationOf<T>(T[] a, T[] b)
        {
            if (a.Length != b.Length) return false;
            var dictA = new Dictionary<T, int>();
            var dictB = new Dictionary<T, int>();

            foreach (var item in a)
            {
                if (!dictA.ContainsKey(item)) dictA[item] = 0;
                dictA[item]++;
            }

            foreach (var item in b)
            {
                if (!dictB.ContainsKey(item)) dictB[item] = 0;
                dictB[item]++;
            }

            if (dictA.Count != dictB.Count) return false;
            foreach (var kvp in dictA)
            {
                if (!dictB.TryGetValue(kvp.Key, out int count) || count != kvp.Value)
                    return false;
            }
            return true;
        }

        public static void SelectionSort<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            int n = a.Length;

            for (int i = 0; i < n - 1; i++)
            {
                int minIdx = i;
                for (int j = i + 1; j < n; j++)
                {
                    comparisons++;
                    if (cmp(a[j], a[minIdx]) < 0)
                    {
                        minIdx = j;
                    }
                }

                if (minIdx != i)
                {
                    (a[i], a[minIdx]) = (a[minIdx], a[i]);
                    moves += 2;
                }
            }
        }

        public static void QuickSortMid<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
        {
            int compCounter = 0;
            int moveCounter = 0;

            void QuickSortRecursive(int low, int high)
            {
                if (low >= high) return;

                T pivot = a[low + (high - low) / 2];
                int i = low;
                int j = high;

                while (i <= j)
                {
                    while (true)
                    {
                        compCounter++;
                        if (cmp(a[i], pivot) < 0) i++;
                        else break;
                    }

                    while (true)
                    {
                        compCounter++;
                        if (cmp(a[j], pivot) > 0) j--;
                        else break;
                    }

                    if (i <= j)
                    {
                        if (i != j)
                        {
                            (a[i], a[j]) = (a[j], a[i]);
                            moveCounter += 2;
                        }
                        i++;
                        j--;
                    }
                }

                QuickSortRecursive(low, j);
                QuickSortRecursive(i, high);
            }

            QuickSortRecursive(0, a.Length - 1);
            comparisons = compCounter;
            moves = moveCounter;
        }

        public static void CountingSort(int[] a, int maxVal, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            int[] count = new int[maxVal + 1];

            for (int i = 0; i < a.Length; i++)
            {
                count[a[i]]++;
            }

            int idx = 0;
            for (int val = 0; val <= maxVal; val++)
            {
                while (count[val] > 0)
                {
                    a[idx++] = val;
                    moves++;
                    count[val]--;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int N = 30;
            int K = 1;
            int a = N % 3;
            int b = N % 4;
            int c = N % 5;

            Console.WriteLine($"N = {N}, K = {K}, a = {a}, b = {b}, c = {c}");
            Func<int, int, int> cmpInt = (x, y) => x.CompareTo(y);

            RunTask1(N, K, cmpInt);
            RunTask2(N, K, cmpInt);
            RunTask3(N, K, cmpInt);
            RunTask4(N, K, cmpInt);
            RunTask5(N, K, cmpInt);
            RunEdgeCases(cmpInt);
        }

        static void RunTask1(int N, int K, Func<int, int, int> cmp)
        {
            Console.WriteLine("\n--- Завдання 1: Сортування вибором ---");
            Random rng = new Random(N);
            int traceLen = K % 5 + 6;
            int[] arr = new int[traceLen];
            for (int i = 0; i < traceLen; i++) arr[i] = rng.Next(0, 20 * K + 1);

            Console.WriteLine("Початковий масив: " + string.Join(", ", arr));
            int[] workArr = (int[])arr.Clone();
            Sorter.SelectionSort(workArr, cmp, out int comps, out int moves);
            Console.WriteLine("Відсортований масив: " + string.Join(", ", workArr));
            Console.WriteLine($"Порівнянь: {comps}, Переставлянь: {moves}");
        }

        static void RunTask2(int N, int K, Func<int, int, int> cmp)
        {
            Console.WriteLine("\n--- Завдання 2: Швидке сортування (опорний — середній) ---");
            Random rng = new Random(N);
            int[] arr = new int[100];
            for (int i = 0; i < 100; i++) arr[i] = rng.Next(0, 20 * K + 1);

            int[] workArr = (int[])arr.Clone();
            Sorter.QuickSortMid(workArr, cmp, out int comps, out int moves);
            Console.WriteLine($"IsSorted: {Sorter.IsSorted(workArr, cmp)}, IsPermutation: {Sorter.IsPermutationOf(arr, workArr)}");
            Console.WriteLine($"Результат (n=100): Порівнянь: {comps}, Переставлянь: {moves}");
        }

        static void RunTask3(int N, int K, Func<int, int, int> cmp)
        {
            Console.WriteLine("\n--- Завдання 3: Сортування підрахунком ---");
            int n = 100000;
            Random rng = new Random(N);
            int[] baseArr = new int[n];
            for (int i = 0; i < n; i++) baseArr[i] = rng.Next(0, 20 * K + 1);

            long minTimeQS = long.MaxValue;
            for (int r = 0; r < 3; r++)
            {
                int[] a1 = (int[])baseArr.Clone();
                Stopwatch sw = Stopwatch.StartNew();
                Sorter.QuickSortMid(a1, cmp, out _, out _);
                sw.Stop();
                if (sw.ElapsedMilliseconds < minTimeQS) minTimeQS = sw.ElapsedMilliseconds;
            }

            long minTimeCS = long.MaxValue;
            for (int r = 0; r < 3; r++)
            {
                int[] a2 = (int[])baseArr.Clone();
                Stopwatch sw = Stopwatch.StartNew();
                Sorter.CountingSort(a2, 20 * K, out _, out _);
                sw.Stop();
                if (sw.ElapsedMilliseconds < minTimeCS) minTimeCS = sw.ElapsedMilliseconds;
            }

            Console.WriteLine($"n = {n}: QuickSortMid = {minTimeQS} ms, CountingSort = {minTimeCS} ms");
        }

        static void RunTask4(int N, int K, Func<int, int, int> cmp)
        {
            Console.WriteLine("\n--- Завдання 4: Таблиця порівнянь ---");
            int[] sizes = new int[] { 100, 1000, 10000 };

            foreach (int n in sizes)
            {
                Random rng = new Random(N);
                int[] randArr = new int[n];
                for (int i = 0; i < n; i++) randArr[i] = rng.Next(0, 20 * K + 1);

                int[] sortedArr = (int[])randArr.Clone();
                Array.Sort(sortedArr);

                int[] revArr = (int[])sortedArr.Clone();
                Array.Reverse(revArr);

                int[] nearArr = (int[])sortedArr.Clone();
                Random rngSwap = new Random(N + 100);
                for (int i = 0; i < K; i++)
                {
                    int idx1 = rngSwap.Next(0, n);
                    int idx2 = rngSwap.Next(0, n);
                    (nearArr[idx1], nearArr[idx2]) = (nearArr[idx2], nearArr[idx1]);
                }

                Dictionary<string, int[]> inputs = new Dictionary<string, int[]>()
                {
                    { "Випадковий", randArr },
                    { "Відсортований", sortedArr },
                    { "Навпаки", revArr },
                    { "Майже", nearArr }
                };

                Console.WriteLine($"\n[n = {n}]");
                foreach (var kvp in inputs)
                {
                    int[] a1 = (int[])kvp.Value.Clone();
                    Sorter.SelectionSort(a1, cmp, out int c1, out int m1);

                    int[] a2 = (int[])kvp.Value.Clone();
                    Sorter.QuickSortMid(a2, cmp, out int c2, out int m2);

                    Console.WriteLine($"{kvp.Key} | Вибором: {c1}/{m1} | Швидке: {c2}/{m2}");
                }
            }
        }

        static void RunTask5(int N, int K, Func<int, int, int> cmp)
        {
            Console.WriteLine("\n--- Завдання 5: Заміри часу та Стабільність ---");
            int[] sizes = new int[] { 1000, 10000, 100000, 1000000 };

            foreach (int n in sizes)
            {
                Random rng = new Random(N);
                int[] baseArr = new int[n];
                for (int i = 0; i < n; i++) baseArr[i] = rng.Next(0, 20 * K + 1);

                long tSimple = -1;
                if (n <= 10000)
                {
                    long minT = long.MaxValue;
                    for (int r = 0; r < 3; r++)
                    {
                        int[] a = (int[])baseArr.Clone();
                        Stopwatch sw = Stopwatch.StartNew();
                        Sorter.SelectionSort(a, cmp, out _, out _);
                        sw.Stop();
                        if (sw.ElapsedMilliseconds < minT) minT = sw.ElapsedMilliseconds;
                    }
                    tSimple = minT;
                }

                long tFast = -1;
                {
                    long minT = long.MaxValue;
                    for (int r = 0; r < 3; r++)
                    {
                        int[] a = (int[])baseArr.Clone();
                        Stopwatch sw = Stopwatch.StartNew();
                        Sorter.QuickSortMid(a, cmp, out _, out _);
                        sw.Stop();
                        if (sw.ElapsedMilliseconds < minT) minT = sw.ElapsedMilliseconds;
                    }
                    tFast = minT;
                }

                long tCount = -1;
                {
                    long minT = long.MaxValue;
                    for (int r = 0; r < 3; r++)
                    {
                        int[] a = (int[])baseArr.Clone();
                        Stopwatch sw = Stopwatch.StartNew();
                        Sorter.CountingSort(a, 20 * K, out _, out _);
                        sw.Stop();
                        if (sw.ElapsedMilliseconds < minT) minT = sw.ElapsedMilliseconds;
                    }
                    tCount = minT;
                }

                long tStd = -1;
                {
                    long minT = long.MaxValue;
                    for (int r = 0; r < 3; r++)
                    {
                        int[] a = (int[])baseArr.Clone();
                        Stopwatch sw = Stopwatch.StartNew();
                        Array.Sort(a);
                        sw.Stop();
                        if (sw.ElapsedMilliseconds < minT) minT = sw.ElapsedMilliseconds;
                    }
                    tStd = minT;
                }

                Console.WriteLine($"n = {n} | Вибором: {(tSimple >= 0 ? tSimple + " ms" : "> 1s")} | Швидке: {tFast} ms | Підрахунком: {tCount} ms | Array.Sort: {tStd} ms");
            }
        }

        static void RunEdgeCases(Func<int, int, int> cmp)
        {
            Console.WriteLine("\n--- Крайові випадки ---");
            List<(string Name, int[] Array)> cases = new List<(string, int[])>()
            {
                ("порожній масив", new int[0]),
                ("один елемент", new int[] { 42 }),
                ("два елементи в порядку", new int[] { 10, 20 }),
                ("два елементи навпаки", new int[] { 20, 10 }),
                ("усі елементи однакові", new int[] { 5, 5, 5, 5, 5 }),
                ("лише два різних значення", new int[] { 1, 2, 1, 2, 1, 2 })
            };

            foreach (var test in cases)
            {
                int[] a1 = (int[])test.Array.Clone();
                Sorter.SelectionSort(a1, cmp, out _, out _);
                bool ok1 = Sorter.IsSorted(a1, cmp) && Sorter.IsPermutationOf(test.Array, a1);

                int[] a2 = (int[])test.Array.Clone();
                Sorter.QuickSortMid(a2, cmp, out _, out _);
                bool ok2 = Sorter.IsSorted(a2, cmp) && Sorter.IsPermutationOf(test.Array, a2);

                Console.WriteLine($"[{test.Name}] -> Вибором: {(ok1 ? "OK" : "FAIL")}, Швидке: {(ok2 ? "OK" : "FAIL")}");
            }
        }
    }
}