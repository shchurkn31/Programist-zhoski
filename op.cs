using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("N = 30, K = 1, a = 0, b = 2, c = 0");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("Оберіть номер завдання (1-4) або 0 для виходу:");
            Console.WriteLine("1 - Завдання 1 (Тип трикутника)");
            Console.WriteLine("2 - Завдання 2 (Кількість цифр числа > K)");
            Console.WriteLine("3 - Завдання 3 (Обробка послідовності)");
            Console.WriteLine("4 - Завдання 4 (Обчислення НСД двома способами)");
            Console.WriteLine("0 - Вихід");
            Console.Write("Ваш вибір: ");

            string choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    Task1();
                    break;
                case "2":
                    Task2();
                    break;
                case "3":
                    Task3();
                    break;
                case "4":
                    Task4();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
                    break;
            }
            Console.WriteLine();
        }
    }

    static void Task1()
    {
        int a = ReadPositiveInt("Введіть сторону a: ");
        int b = ReadPositiveInt("Введіть сторону b: ");
        int c = ReadPositiveInt("Введіть сторону c: ");

        if (a + b <= c || a + c <= b || b + c <= a)
        {
            Console.WriteLine("Трикутник не існує.");
            return;
        }

        long a2 = (long)a * a;
        long b2 = (long)b * b;
        long c2 = (long)c * c;
        bool isRight = (a2 + b2 == c2) || (a2 + c2 == b2) || (b2 + c2 == a2);

        if (a == b && b == c)
        {
            Console.WriteLine("Трикутник рівносторонній.");
        }
        else if (a == b || a == c || b == c)
        {
            if (isRight)
                Console.WriteLine("Трикутник рівнобедрений та прямокутний.");
            else
                Console.WriteLine("Трикутник рівнобедрений.");
        }
        else
        {
            if (isRight)
                Console.WriteLine("Трикутник прямокутний (різносторонній).");
            else
                Console.WriteLine("Трикутник різносторонній.");
        }
    }

    static void Task2()
    {
        int n = ReadPositiveInt("Введіть натуральне число: ");
        int k = 1;
        int count = 0;
        int temp = n;

        while (temp > 0)
        {
            int digit = temp % 10;
            if (digit > k)
            {
                count++;
            }
            temp /= 10;
        }

        Console.WriteLine($"Кількість цифр, більших за K ({k}): {count}");
    }

    static void Task3()
    {
        int k = 1;
        int totalCount = 0;
        int matchCount = 0;

        Console.WriteLine("Вводьте цілі числа по одному в рядок.");
        Console.WriteLine("Для завершення вводу натисніть Enter на порожньому рядку або введіть 'stop':");

        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input) || input.Trim().ToLower() == "stop")
            {
                break;
            }

            int number;
            if (int.TryParse(input, out number))
            {
                totalCount++;
                if (number > k)
                {
                    matchCount++;
                }
            }
            else
            {
                Console.WriteLine("Помилка введення! Введіть ціле число або залишіть порожнім для виходу.");
            }
        }

        if (totalCount == 0)
        {
            Console.WriteLine("Немає даних");
        }
        else
        {
            Console.WriteLine($"Оброблено елементів: {totalCount}");
            Console.WriteLine($"Кількість елементів, більших за K ({k}): {matchCount}");
        }
    }

    static void Task4()
    {
        int n = 30;
        int A = 1000000 + n;
        int B = 999999 - n;

        Console.WriteLine($"Число A = {A}");
        Console.WriteLine($"Число B = {B}");

        int min = A < B ? A : B;
        int gcd1 = 1;
        long iter1 = 0;

        for (int i = min; i >= 1; i--)
        {
            iter1++;
            if (A % i == 0 && B % i == 0)
            {
                gcd1 = i;
                break;
            }
        }

        int a = A;
        int b = B;
        long iter2 = 0;

        while (b != 0)
        {
            iter2++;
            int remainder = a % b;
            a = b;
            b = remainder;
        }
        int gcd2 = a;

        Console.WriteLine($"\nСпосіб 1 (перебор від Min): НСД = {gcd1}, Ітерацій = {iter1}");
        Console.WriteLine($"Спосіб 2 (алгоритм Евкліда): НСД = {gcd2}, Ітерацій = {iter2}");
    }

    static int ReadPositiveInt(string prompt)
    {
        int result;
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();

            if (int.TryParse(input, out result) && result > 0)
            {
                return result;
            }

            Console.WriteLine("Помилка введення! Будь ласка, введіть додатне ціле число.");
        }
    }
}