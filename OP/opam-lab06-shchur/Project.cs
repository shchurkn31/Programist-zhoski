using System;
using System.Collections.Generic;
using System.Diagnostics;

class Student
{
    public string Name;
    public string Group;
    public int Score;

    public Student(string name, string group, int score)
    {
        Name = name;
        Group = group;
        Score = score;
    }
}

class Program
{
    static Dictionary<string, int> CountWords(string[] words)
    {
        Dictionary<string, int> d = new Dictionary<string, int>();
        foreach (string w in words)
        {
            if (d.ContainsKey(w)) d[w]++;
            else d[w] = 1;
        }
        return d;
    }

    static Dictionary<string, List<Student>> Group(List<Student> list)
    {
        Dictionary<string, List<Student>> d = new Dictionary<string, List<Student>>();
        foreach (Student s in list)
        {
            string key;
            if (s.Score < 60) key = "0-59";
            else if (s.Score < 75) key = "60-74";
            else if (s.Score < 90) key = "75-89";
            else key = "90-100";
            if (!d.ContainsKey(key)) d[key] = new List<Student>();
            d[key].Add(s);
        }
        return d;
    }

    static List<int> RandomList(int count, int max, int seed)
    {
        Random r = new Random(seed);
        List<int> list = new List<int>();
        for (int i = 0; i < count; i++) list.Add(r.Next(1, max + 1));
        return list;
    }

    static bool Has(List<int> list, int x)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == x) return true;
        }
        return false;
    }

    static List<int> KeepOrder(List<int> list)
    {
        HashSet<int> seen = new HashSet<int>();
        List<int> res = new List<int>();
        foreach (int x in list)
        {
            if (seen.Add(x)) res.Add(x);
        }
        return res;
    }

    static List<int> UniqueList(List<int> list)
    {
        List<int> res = new List<int>();
        foreach (int x in list)
        {
            if (!Has(res, x)) res.Add(x);
        }
        return res;
    }

    static HashSet<int> CommonSet(List<int> a, List<int> b)
    {
        HashSet<int> h = new HashSet<int>(a);
        h.IntersectWith(b);
        return h;
    }

    static List<int> CommonList(List<int> a, List<int> b)
    {
        List<int> res = new List<int>();
        for (int i = 0; i < a.Count; i++)
        {
            for (int j = 0; j < b.Count; j++)
            {
                if (a[i] == b[j] && !Has(res, a[i])) res.Add(a[i]);
            }
        }
        return res;
    }

    static int CountWordsList(string[] words)
    {
        List<string> keys = new List<string>();
        List<int> counts = new List<int>();
        foreach (string w in words)
        {
            int idx = -1;
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i] == w) idx = i;
            }
            if (idx == -1)
            {
                keys.Add(w);
                counts.Add(1);
            }
            else counts[idx]++;
        }
        return keys.Count;
    }

    static double Time(Action a)
    {
        a();
        Stopwatch sw = Stopwatch.StartNew();
        a();
        sw.Stop();
        return sw.Elapsed.TotalMilliseconds;
    }

    static void Row(string name, double t1, double t2)
    {
        Console.WriteLine("| " + name + " | " + t1.ToString("F1") + " | " + t2.ToString("F1") + " | " + (t1 / t2).ToString("F1") + " |");
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        int n = 30, k = 1;
        Console.WriteLine("N = " + n + ", K = " + k + ", a = " + n % 3 + ", b = " + n % 4 + ", c = " + n % 5);

        string text = "Алгоритм описує послідовність дій. Алгоритм можна записати словами, а можна записати кодом. Кодом алгоритм стає тоді, коли його виконує машина, а не людина. Одна задача — багато алгоритмів.";
        string[] words = text.ToLower().Split(new char[] { ' ', '.', ',', '—' }, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, int> freq = CountWords(words);
        foreach (KeyValuePair<string, int> p in freq)
        {
            if (p.Value >= k % 3 + 2) Console.WriteLine(p.Key + " " + p.Value);
        }
        Console.WriteLine("різних слів " + freq.Count);

        List<Student> students = new List<Student>();
        students.Add(new Student("Коваленко", "КН-3/1", 88));
        students.Add(new Student("Мельник", "КН-3/2", 64));
        students.Add(new Student("Іщенко", "КН-3/1", 93));
        students.Add(new Student("Бондар", "КН-3/1", 71));
        students.Add(new Student("Савченко", "КН-3/2", 55));
        students.Add(new Student("Ткачук", "КН-3/2", 90));
        students.Add(new Student("Кравець", "КН-3/1", 76));
        students.Add(new Student("Іванів", "КН-3/1", 48));
        students.Add(new Student("Мороз", "КН-3/2", 82));
        students.Add(new Student("Шевчук", "КН-3/1", 67));
        students.Add(new Student("Бойко", "КН-3/2", 95));
        students.Add(new Student("Кушнір", "КН-3/2", 73));
        foreach (KeyValuePair<string, List<Student>> p in Group(students))
        {
            int sum = 0;
            foreach (Student s in p.Value) sum += s.Score;
            Console.WriteLine(p.Key + " " + p.Value.Count + " " + (double)sum / p.Value.Count);
        }

        List<int> nums = RandomList(25, k % 10 + 6, n);
        Console.WriteLine(string.Join(" ", nums));
        Console.WriteLine(string.Join(" ", new HashSet<int>(nums)));
        Console.WriteLine(string.Join(" ", KeepOrder(nums)));

        List<int> first = RandomList(30, 20 + k, n);
        List<int> second = RandomList(30, 20 + k, n + 100);
        HashSet<int> cs = CommonSet(first, second);
        List<int> cl = CommonList(first, second);
        Console.WriteLine(string.Join(" ", cs) + " " + cs.Count);
        Console.WriteLine(string.Join(" ", cl) + " " + cl.Count);
        bool same = cs.Count == cl.Count;
        foreach (int v in cl)
        {
            if (!cs.Contains(v)) same = false;
        }
        Console.WriteLine(same);

        Random r = new Random(n);
        List<int> big = new List<int>();
        List<int> q = new List<int>();
        for (int i = 0; i < 100000; i++) big.Add(r.Next(200000));
        for (int i = 0; i < 10000; i++) q.Add(r.Next(200000));
        HashSet<int> bigSet = new HashSet<int>(big);
        string[] many = new string[50000];
        for (int i = 0; i < many.Length; i++) many[i] = "слово" + r.Next(3000);
        List<int> big2 = new List<int>();
        for (int i = 0; i < 20000; i++) big2.Add(r.Next(20000));
        List<int> x = new List<int>();
        List<int> y = new List<int>();
        for (int i = 0; i < 5000; i++)
        {
            x.Add(r.Next(10000));
            y.Add(r.Next(10000));
        }

        
    }
}