using System;
using System.Collections.Generic;
using System.Diagnostics;

class Program
{
    const int N = 30;
    const int K = 1;

    // ===== Завдання 1: новий список за критерієм (a = 0: більші за K % 7) =====
    static List<int> FilterGreater(List<int> list, int limit)
    {
        // спочатку рахуємо, скільки елементів підійде, щоб задати точну місткість
        int count = 0;
        for (int i = 0; i < list.Count; i++)
            if (list[i] > limit) count++;

        List<int> result = new List<int>(count);
        for (int i = 0; i < list.Count; i++)
            if (list[i] > limit) result.Add(list[i]);
        return result;
    }

    // ===== Завдання 2: вилучення кратних 5 (c = 0) =====
    static bool IsRemovable(int x)
    {
        return x % 5 == 0;   // нуль теж кратний 5
    }

    // Хибний варіант: цикл з початку + RemoveAt(i). Змінює список!
    static void RemoveWrongInPlace(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
            if (IsRemovable(list[i]))
                list.RemoveAt(i);   // після видалення наступний елемент з'їжджає на місце i, а ми робимо i++
    }

    // Правильний варіант 1: цикл з кінця. Змінює список!
    static void RemoveBackwardsInPlace(List<int> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
            if (IsRemovable(list[i]))
                list.RemoveAt(i);
    }

    // Правильний варіант 2: новий список, оригінал не чіпаємо
    static List<int> RemoveToNewList(List<int> list)
    {
        int keep = 0;
        for (int i = 0; i < list.Count; i++)
            if (!IsRemovable(list[i])) keep++;

        List<int> result = new List<int>(keep);
        for (int i = 0; i < list.Count; i++)
            if (!IsRemovable(list[i])) result.Add(list[i]);
        return result;
    }

    static bool ListsEqual(List<int> x, List<int> y)
    {
        if (x.Count != y.Count) return false;
        for (int i = 0; i < x.Count; i++)
            if (x[i] != y[i]) return false;
        return true;
    }

    // ===== Завдання 3: злиття двох впорядкованих списків =====
    static List<int> BuildSorted(int length, int seed)
    {
        Random rnd = new Random(seed);
        List<int> list = new List<int>(length);
        if (length == 0) return list;

        int value = rnd.Next(1, 10);
        list.Add(value);
        for (int i = 1; i < length; i++)
        {
            value += rnd.Next(1, 10);
            list.Add(value);
        }
        return list;
    }

    static List<int> Merge(List<int> a, List<int> b)
    {
        List<int> result = new List<int>(a.Count + b.Count);
        int i = 0, j = 0;

        while (i < a.Count && j < b.Count)
        {
            if (a[i] <= b[j])
            {
                result.Add(a[i]);
                i++;
            }
            else
            {
                result.Add(b[j]);
                j++;
            }
        }

        // один зі списків скінчився раніше - дописуємо хвіст іншого
        while (i < a.Count)
        {
            result.Add(a[i]);
            i++;
        }
        while (j < b.Count)
        {
            result.Add(b[j]);
            j++;
        }
        return result;
    }

    static bool IsSorted(List<int> list)
    {
        for (int i = 1; i < list.Count; i++)
            if (list[i] < list[i - 1]) return false;
        return true;
    }

    // ===== Завдання 4: розділення (b = 2: менші за K % 7 і решта) =====
    static void SplitByLess(List<int> list, int limit, out List<int> less, out List<int> rest)
    {
        int lessCount = 0;
        for (int i = 0; i < list.Count; i++)
            if (list[i] < limit) lessCount++;

        less = new List<int>(lessCount);
        rest = new List<int>(list.Count - lessCount);

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] < limit) less.Add(list[i]);
            else rest.Add(list[i]);
        }
    }

    // ===== Допоміжне =====
    static string Show(List<int> list)
    {
        return "[" + string.Join(", ", list) + "]";
    }

    static List<int> Copy(List<int> list)
    {
        return new List<int>(list);
    }

    // Один прогін завдань 1, 2, 4 на списку (для таблиці крайових випадків)
    static void RunEdgeCase(string name, List<int> list)
    {
        Console.WriteLine($"--- {name}: {Show(list)}");

        List<int> t1 = FilterGreater(list, K % 7);
        Console.WriteLine($"  Зад.1: {Show(t1)}, Count = {t1.Count}");

        List<int> w = Copy(list);
        RemoveWrongInPlace(w);
        List<int> b = Copy(list);
        RemoveBackwardsInPlace(b);
        List<int> n = RemoveToNewList(list);
        Console.WriteLine($"  Зад.2: хибний {Show(w)}, з кінця {Show(b)}, новий {Show(n)}, збіг = {ListsEqual(b, n)}");

        List<int> less, rest;
        SplitByLess(list, K % 7, out less, out rest);
        Console.WriteLine($"  Зад.4: {Show(less)} | {Show(rest)}, сума Count = {less.Count + rest.Count == list.Count}");
    }

    // ===== Завдання 5: замір часу =====
    static long MeasureAddNoCapacity(int count)
    {
        Stopwatch sw = Stopwatch.StartNew();
        List<int> list = new List<int>();
        for (int i = 0; i < count; i++) list.Add(i);
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    static long MeasureAddWithCapacity(int count)
    {
        Stopwatch sw = Stopwatch.StartNew();
        List<int> list = new List<int>(count);
        for (int i = 0; i < count; i++) list.Add(i);
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    static long MeasureInsertFront(int count)
    {
        Stopwatch sw = Stopwatch.StartNew();
        List<int> list = new List<int>();
        for (int i = 0; i < count; i++) list.Insert(0, i);
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    static long MeasureContains(int listSize, int times)
    {
        List<int> list = new List<int>(listSize);
        for (int i = 0; i < listSize; i++) list.Add(i);

        int found = 0;   // щоб результат використовувався
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < times; i++)
            if (list.Contains(-1)) found++;   // -1 у списку немає
        sw.Stop();

        if (found > 0) Console.WriteLine("Щось знайшлося?!");
        return sw.ElapsedMilliseconds;
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // ---------- основний список ----------
        Random rnd = new Random(N);
        List<int> main = new List<int>(20);
        for (int i = 0; i < 20; i++)
            main.Add(rnd.Next(-9, 10));   // [-9; 9]

        Console.WriteLine("Основний список: " + Show(main));
        Console.WriteLine();

        // ---------- Завдання 1 ----------
        Console.WriteLine("=== Завдання 1: елементи більші за " + (K % 7) + " ===");
        List<int> filtered = FilterGreater(main, K % 7);
        Console.WriteLine(Show(filtered));
        Console.WriteLine("Count = " + filtered.Count);
        Console.WriteLine();

        // ---------- Завдання 2 ----------
        Console.WriteLine("=== Завдання 2: вилучення кратних 5 ===");

        List<int> wrong = Copy(main);
        RemoveWrongInPlace(wrong);
        Console.WriteLine("Хибний (з початку):  " + Show(wrong));
        Console.Write("  кратні 5, які він пропустив: ");
        bool any = false;
        for (int i = 0; i < wrong.Count; i++)
        {
            if (IsRemovable(wrong[i]))
            {
                Console.Write(wrong[i] + " ");
                any = true;
            }
        }
        Console.WriteLine(any ? "" : "(жодного, на цих даних пощастило)");

        List<int> backwards = Copy(main);
        RemoveBackwardsInPlace(backwards);
        Console.WriteLine("Цикл з кінця:        " + Show(backwards));

        List<int> newList = RemoveToNewList(main);
        Console.WriteLine("Новий список:        " + Show(newList));

        Console.WriteLine("Варіанти 2 і 3 збігаються: " + ListsEqual(backwards, newList));

        // додатково (плюс до захисту): RemoveAll з делегатом
        List<int> viaRemoveAll = Copy(main);
        viaRemoveAll.RemoveAll(x => x % 5 == 0);
        Console.WriteLine("RemoveAll (додатково): " + Show(viaRemoveAll));
        Console.WriteLine();

        // ---------- Завдання 3 ----------
        Console.WriteLine("=== Завдання 3: злиття ===");
        List<int> first = BuildSorted(10 + K % 5, N);
        List<int> second = BuildSorted(6 + K % 4, N + 100);
        Console.WriteLine("Перший:  " + Show(first));
        Console.WriteLine("Другий:  " + Show(second));
        List<int> merged = Merge(first, second);
        Console.WriteLine("Злитий:  " + Show(merged));
        Console.WriteLine("Упорядкований: " + IsSorted(merged));
        Console.WriteLine();

        // ---------- Завдання 4 ----------
        Console.WriteLine("=== Завдання 4: менші за " + (K % 7) + " і решта ===");
        List<int> less, rest;
        SplitByLess(main, K % 7, out less, out rest);
        Console.WriteLine("Менші: " + Show(less));
        Console.WriteLine("Решта: " + Show(rest));
        Console.WriteLine("Сума Count збігається з вихідним: " + (less.Count + rest.Count == main.Count));
        Console.WriteLine();

        // ---------- Крайові випадки ----------
        Console.WriteLine("=== Крайові випадки ===");
        RunEdgeCase("порожній", new List<int>());
        RunEdgeCase("один елемент", new List<int> { 5 });
        RunEdgeCase("жоден не підходить (зад.1: не > 1, зад.2: не кратні 5)", new List<int> { 1, 0, -3, 2 });
        RunEdgeCase("усі підходять (зад.1: усі > 1, зад.2: усі кратні 5)", new List<int> { 5, 5, 5, 5 });

        Console.WriteLine("--- злиття з порожнім списком");
        List<int> empty = new List<int>();
        List<int> m1 = Merge(empty, second);
        List<int> m2 = Merge(first, empty);
        Console.WriteLine($"  порожній + другий: {Show(m1)}, упорядкований = {IsSorted(m1)}");
        Console.WriteLine($"  перший + порожній: {Show(m2)}, упорядкований = {IsSorted(m2)}");
        Console.WriteLine();

        // ---------- Завдання 5 ----------
        Console.WriteLine("=== Завдання 5: замір часу (запускати в Release!) ===");

        // прогрівальний прогін на 1000 елементів, результат не враховуємо
        MeasureAddNoCapacity(1000);
        MeasureAddWithCapacity(1000);
        MeasureInsertFront(1000);
        MeasureContains(1000, 10);

        long t1ms = MeasureAddNoCapacity(100000);
        long t2ms = MeasureAddWithCapacity(100000);
        long t3ms = MeasureInsertFront(100000);
        long t4ms = MeasureContains(100000, 1000);

        Console.WriteLine($"100 000 x Add, без місткості:      {t1ms} мс");
        Console.WriteLine($"100 000 x Add, з місткістю:        {t2ms} мс");
        Console.WriteLine($"100 000 x Insert(0, x):            {t3ms} мс");
        Console.WriteLine($"1 000 x Contains у списку з 100 000: {t4ms} мс");
        if (t1ms > 0)
            Console.WriteLine($"Insert(0) / Add = у {(double)t3ms / t1ms:F1} разів");
    }
}