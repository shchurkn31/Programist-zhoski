using System;
using System.Collections.Generic;
using System.Globalization;

// Власний стек поверх масиву (завдання 4)
class IntStack
{
    private int[] data;
    private int count;

    public IntStack(int capacity)
    {
        data = new int[capacity];
        count = 0;
    }

    public int Count
    {
        get { return count; }
    }

    public int Capacity
    {
        get { return data.Length; }
    }

    public void Push(int x)
    {
        if (count == data.Length)
        {
            // місця немає - подвоюємо масив
            int[] bigger = new int[data.Length * 2];
            for (int i = 0; i < count; i++)
                bigger[i] = data[i];
            data = bigger;
        }
        data[count] = x;
        count++;
    }

    public int Pop()
    {
        if (count == 0)
            throw new InvalidOperationException("Стек порожній");
        count--;
        return data[count];
    }

    public int Peek()
    {
        if (count == 0)
            throw new InvalidOperationException("Стек порожній");
        return data[count - 1];
    }
}

class Program
{
    const int N = 30;
    const int K = 1;

    // ===== Завдання 1: дужки ( ) [ ] { } на стандартному Stack =====
    static bool IsOpen(char c)
    {
        return c == '(' || c == '[' || c == '{';
    }

    static bool IsClose(char c)
    {
        return c == ')' || c == ']' || c == '}';
    }

    static bool Matches(char open, char close)
    {
        return (open == '(' && close == ')')
            || (open == '[' && close == ']')
            || (open == '{' && close == '}');
    }

    // Повертає true, якщо збалансовано. index - індекс першої помилки або -1.
    static bool CheckBrackets(string s, out int index)
    {
        Stack<char> chars = new Stack<char>();
        Stack<int> positions = new Stack<int>();

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (IsOpen(c))
            {
                chars.Push(c);
                positions.Push(i);
            }
            else if (IsClose(c))
            {
                if (chars.Count == 0)           // закривальна при порожньому стеку
                {
                    index = i;
                    return false;
                }
                char open = chars.Pop();
                positions.Pop();
                if (!Matches(open, c))          // не той тип дужки
                {
                    index = i;
                    return false;
                }
            }
        }

        if (chars.Count > 0)                    // стек непорожній після кінця рядка
        {
            index = positions.Peek();           // остання незакрита відкривальна
            return false;
        }

        index = -1;
        return true;
    }

    // Та сама перевірка, але на власному IntStack (завдання 4)
    static bool CheckBracketsOwn(string s, out int index)
    {
        IntStack chars = new IntStack(K % 4 + 2);
        IntStack positions = new IntStack(K % 4 + 2);

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (IsOpen(c))
            {
                chars.Push(c);                  // зберігаємо код символу
                positions.Push(i);
            }
            else if (IsClose(c))
            {
                if (chars.Count == 0)
                {
                    index = i;
                    return false;
                }
                char open = (char)chars.Pop();
                positions.Pop();
                if (!Matches(open, c))
                {
                    index = i;
                    return false;
                }
            }
        }

        if (chars.Count > 0)
        {
            index = positions.Peek();
            return false;
        }

        index = -1;
        return true;
    }

    // ===== Завдання 2: зворотний польський запис, + - * / ^ =====
    static bool IsOperator(string t)
    {
        return t == "+" || t == "-" || t == "*" || t == "/" || t == "^";
    }

    static bool EvalRpn(string expr, out double result, out string error)
    {
        result = 0;
        error = "";

        string[] tokens = expr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            error = "Порожній вираз";
            return false;
        }

        Stack<double> st = new Stack<double>();

        foreach (string t in tokens)
        {
            if (IsOperator(t))
            {
                if (st.Count < 2)
                {
                    error = "Не вистачає чисел для операції " + t;
                    return false;
                }
                double right = st.Pop();   // другий операнд знімається першим
                double left = st.Pop();

                switch (t)
                {
                    case "+": st.Push(left + right); break;
                    case "-": st.Push(left - right); break;   // left - right, а не навпаки
                    case "*": st.Push(left * right); break;
                    case "/":
                        if (right == 0)
                        {
                            error = "Ділення на нуль";
                            return false;
                        }
                        st.Push(left / right);
                        break;
                    case "^": st.Push(Math.Pow(left, right)); break;
                }
            }
            else
            {
                double num;
                if (!double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out num))
                {
                    error = "Невідомий токен: " + t;
                    return false;
                }
                st.Push(num);
            }
        }

        if (st.Count != 1)
        {
            error = "Забагато чисел у виразі (у стеку лишилось " + st.Count + ")";
            return false;
        }

        result = st.Pop();
        return true;
    }

    // ===== Завдання 3: одна черга, один обслуговувач (c = 0) =====
    static void SimulateQueue(int[] weights)
    {
        Queue<int> queue = new Queue<int>();
        for (int i = 0; i < weights.Length; i++)
            queue.Enqueue(i);                    // усі стоять у черзі з початку

        int maxLen = queue.Count;                // довжина черги в момент 0
        int time = 0;

        Console.WriteLine("Заявка | Вага | Початок | Кінець");
        while (queue.Count > 0)
        {
            int idx = queue.Dequeue();
            int start = time;
            int end = start + weights[idx];
            Console.WriteLine($"{idx + 1,6} | {weights[idx],4} | {start,7} | {end,6}");
            time = end;
        }
        Console.WriteLine("Максимальна довжина черги: " + maxLen);
    }

    // ===== Допоміжне для тестів =====
    static void TestBrackets(string s, bool expected)
    {
        int idx;
        bool r1 = CheckBrackets(s, out idx);
        int idx2;
        bool r2 = CheckBracketsOwn(s, out idx2);

        string ok = (r1 == expected) ? "OK" : "НЕ ТЕ";
        string same = (r1 == r2 && idx == idx2) ? "збігається" : "РІЗНІ!";
        Console.WriteLine($"\"{s}\" -> {r1}, індекс {idx}  (очікувано {expected}) {ok}; власний стек {same}");
    }

    static void TestRpn(string expr)
    {
        double res;
        string err;
        if (EvalRpn(expr, out res, out err))
            Console.WriteLine($"\"{expr}\" -> {res.ToString(CultureInfo.InvariantCulture)}");
        else
            Console.WriteLine($"\"{expr}\" -> помилка: {err}");
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // ---------- Завдання 1 ----------
        Console.WriteLine("=== Завдання 1: дужки (стандартний Stack) ===");
        TestBrackets("(a[b]{c})", true);
        TestBrackets("([)]", false);
        TestBrackets(")(", false);
        TestBrackets("((", false);
        TestBrackets("", true);
        // K = 1 відкривальна і K - 1 = 0 закривальних
        TestBrackets("(", false);
        Console.WriteLine();

        // ---------- Завдання 2 ----------
        Console.WriteLine("=== Завдання 2: зворотний польський запис ===");
        TestRpn("3 4 + 2 *");
        TestRpn("5 3 -");
        TestRpn("8 2 /");
        TestRpn("2 3 4 * +");
        TestRpn("5 0 /");
        TestRpn("2 3 ^");
        Console.WriteLine("--- некоректний вхід");
        TestRpn("3 4 abc +");
        TestRpn("3 4 5 +");
        TestRpn("3 +");
        TestRpn("");
        Console.WriteLine("--- власний вираз, результат K = " + K);
        TestRpn("2 3 + 4 - 2 ^");
        Console.WriteLine();

        // ---------- Завдання 3 ----------
        Console.WriteLine("=== Завдання 3: модель обслуговування ===");
        int count = 8 + K % 5;
        Random rnd = new Random(N);
        int[] weights = new int[count];
        for (int i = 0; i < count; i++)
            weights[i] = rnd.Next(1, 10);        // 1..9
        SimulateQueue(weights);
        Console.WriteLine();

        // ---------- Завдання 4 ----------
        Console.WriteLine("=== Завдання 4: власний стек ===");
        IntStack stack = new IntStack(K % 4 + 2);
        Console.WriteLine($"Початково: Count = {stack.Count}, Capacity = {stack.Capacity}");
        int pushes = K % 4 + 3;
        for (int i = 1; i <= pushes; i++)
        {
            stack.Push(i * 10);
            Console.WriteLine($"Push({i * 10}): Count = {stack.Count}, Capacity = {stack.Capacity}");
        }
        Console.WriteLine("Peek = " + stack.Peek());
        while (stack.Count > 0)
            stack.Pop();
        try
        {
            stack.Pop();
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine("Pop на порожньому стеку -> виняток: " + e.Message);
        }
        Console.WriteLine("(завдання 1 на власному стеку перевірено вище, у кожному тесті)");
    }
}