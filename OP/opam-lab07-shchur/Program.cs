using System;
using System.Collections.Generic;

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
    static List<T> Filter<T>(List<T> src, Func<T, bool> keep)
    {
        List<T> result = new List<T>();
        foreach (T item in src)
        {
            if (keep(item))
            {
                result.Add(item);
            }
        }
        return result;
    }

    static List<TOut> Map<TIn, TOut>(List<TIn> src, Func<TIn, TOut> convert)
    {
        List<TOut> result = new List<TOut>();
        foreach (TIn item in src)
        {
            result.Add(convert(item));
        }
        return result;
    }

    static TAcc Aggregate<T, TAcc>(List<T> src, TAcc start, Func<TAcc, T, TAcc> step)
    {
        TAcc state = start;
        foreach (T item in src)
        {
            state = step(state, item);
        }
        return state;
    }

    static Dictionary<TKey, List<T>> GroupBy<T, TKey>(List<T> src, Func<T, TKey> keyOf)
    {
        Dictionary<TKey, List<T>> groups = new Dictionary<TKey, List<T>>();
        foreach (T item in src)
        {
            TKey key = keyOf(item);
            if (!groups.ContainsKey(key))
            {
                groups[key] = new List<T>();
            }
            groups[key].Add(item);
        }
        return groups;
    }

    static void Print<T>(string title, List<T> list)
    {
        Console.Write(title + " (" + list.Count + "): ");
        foreach (T item in list)
        {
            Console.Write(item + " ");
        }
        Console.WriteLine();
    }

    static void PrintGroups<T, TKey>(string title, Dictionary<TKey, List<T>> groups)
    {
        Console.WriteLine(title);
        foreach (var pair in groups)
        {
            Console.WriteLine("  " + pair.Key + ": " + pair.Value.Count);
        }
    }

    static string ScoreRange(int score)
    {
        if (score < 60) return "0-59";
        if (score < 75) return "60-74";
        if (score < 90) return "75-89";
        return "90-100";
    }

    static void Main()
    {
        int n = 30;
        int k = 1;
        int a = n % 3;
        int b = n % 4;
        int c = n % 5;
        Console.WriteLine("N = " + n + ", K = " + k + ", a = " + a + ", b = " + b + ", c = " + c);
        Console.WriteLine();

        Random random = new Random(n);
        List<int> numbers = new List<int>();
        for (int i = 0; i < 20; i++)
        {
            numbers.Add(random.Next(-k, k + 1));
        }

        List<string> words = new List<string>
        {
            "сонце", "хмара", "кіт", "будинок", "жовто-синій", "комп'ютер",
            "ліс", "кава-чай", "програмування", "вікно", "дощ", "північ-південь"
        };

        List<Student> students = new List<Student>
        {
            new Student("Коваль Олена", "КН-3/1", 92),
            new Student("Бондар Іван", "КН-3/1", 58),
            new Student("Мельник Оксана", "КН-3/2", 76),
            new Student("Шевчук Андрій", "КН-3/2", 81),
            new Student("Кравець Марія", "КН-3/1", 60),
            new Student("Бойко Тарас", "КН-3/3", 45),
            new Student("Ткачук Софія", "КН-3/3", 99),
            new Student("Мороз Дмитро", "КН-3/1", 74),
            new Student("Левчук Ірина", "КН-3/2", 88),
            new Student("Коваленко Петро", "КН-3/3", 66)
        };

        Console.WriteLine("--- Завдання 1: Filter ---");
        List<int> bigNumbers = Filter(numbers, x => x > k);
        List<string> longWords = Filter(words, w => w.Length > 5);
        List<Student> goodStudents = Filter(students, s => s.Score >= 75);
        Print("Числа більші за K", bigNumbers);
        Print("Довгі слова", longWords);
        Print("Студенти з балом >= 75", Map(goodStudents, s => s.Name + " " + s.Score));
        Console.WriteLine();

        Console.WriteLine("--- Завдання 2: Map ---");
        List<int> signs = Map(numbers, x => x > 0 ? 1 : (x < 0 ? -1 : 0));
        List<char> firstLetters = Map(words, w => w[0]);
        List<int> scores = Map(students, s => s.Score);
        Print("Знаки чисел", signs);
        Print("Перші літери слів", firstLetters);
        Print("Бали студентів", scores);
        Console.WriteLine();

        Console.WriteLine("--- Завдання 3: Aggregate ---");
        int sum = Aggregate(numbers, 0, (acc, x) => acc + x);
        int product = Aggregate(numbers, 1, (acc, x) => acc * x);
        int max = Aggregate(numbers, int.MinValue, (acc, x) => x > acc ? x : acc);
        int count = Aggregate(numbers, 0, (acc, x) => x > k ? acc + 1 : acc);
        Console.WriteLine("Сума: " + sum);
        Console.WriteLine("Добуток: " + product);
        Console.WriteLine("Максимум: " + max);
        Console.WriteLine("Кількість більших за K: " + count);
        Console.WriteLine();

        Console.WriteLine("--- Завдання 4: GroupBy ---");
        PrintGroups("За групою:", GroupBy(students, s => s.Group));
        PrintGroups("За першою літерою прізвища:", GroupBy(students, s => s.Name[0]));
        PrintGroups("За діапазоном балу:", GroupBy(students, s => ScoreRange(s.Score)));
        PrintGroups("Слова за довжиною:", GroupBy(words, w => w.Length));
        Console.WriteLine();

        Console.WriteLine("--- Завдання 5: захоплення ---");
        int threshold = k;
        Func<int, bool> check = x => x > threshold;
        threshold = 1000;
        Console.WriteLine("Пункт 1: check(500) = " + check(500));

        List<Func<int>> forList = new List<Func<int>>();
        for (int i = 0; i < 3; i++)
        {
            forList.Add(() => i);
        }
        Console.Write("Пункт 2 (for): ");
        foreach (Func<int> f in forList)
        {
            Console.Write(f() + " ");
        }
        Console.WriteLine();

        List<Func<int>> foreachList = new List<Func<int>>();
        int[] array = { 0, 1, 2 };
        foreach (int x in array)
        {
            foreachList.Add(() => x);
        }
        Console.Write("Пункт 3 (foreach): ");
        foreach (Func<int> f in foreachList)
        {
            Console.Write(f() + " ");
        }
        Console.WriteLine();

        List<Func<int>> fixedList = new List<Func<int>>();
        for (int i = 0; i < 3; i++)
        {
            int copy = i;
            fixedList.Add(() => copy);
        }
        Console.Write("Виправлений for: ");
        foreach (Func<int> f in fixedList)
        {
            Console.Write(f() + " ");
        }
        Console.WriteLine();
        Console.WriteLine();

        Console.WriteLine("--- Крайові випадки ---");
        List<int> emptyNumbers = new List<int>();
        List<string> emptyWords = new List<string>();
        List<Student> emptyStudents = new List<Student>();

        Console.WriteLine("Filter, порожній список: " + Filter(emptyNumbers, x => x > k).Count);
        Console.WriteLine("Map, порожній список: " + Map(emptyWords, w => w.Length).Count);
        Console.WriteLine("GroupBy, порожній список: " + GroupBy(emptyStudents, s => s.Group).Count);

        if (emptyNumbers.Count == 0)
        {
            Console.WriteLine("Максимум порожнього списку: немає значення");
        }
        else
        {
            Console.WriteLine("Максимум: " + Aggregate(emptyNumbers, int.MinValue, (acc, x) => x > acc ? x : acc));
        }

        List<int> nobody = Filter(numbers, x => x > 1000);
        Console.WriteLine("Жоден не підходить: " + nobody.Count);

        List<Student> oneGroup = Filter(students, s => s.Group == "КН-3/1");
        Console.WriteLine("Усі в одній групі, груп: " + GroupBy(oneGroup, s => s.Group).Count);

        List<int> single = new List<int> { 7 };
        Console.WriteLine("Один елемент, максимум: " + Aggregate(single, int.MinValue, (acc, x) => x > acc ? x : acc));
    }
}