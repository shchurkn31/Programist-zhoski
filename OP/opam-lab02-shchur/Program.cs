using System;

class Program
{
    static void Main()
    {
        Random rnd = new Random(30);

        int[] mainArray = GenerateArray(24, 1, 4, rnd);

        Task1(mainArray);

        int[] filteredArray = Task2(mainArray, 1);
        Console.WriteLine(PrintArray(filteredArray));

        Task3(mainArray);

        int[] arrayToReverse = (int[])mainArray.Clone();
        Console.WriteLine(PrintArray(arrayToReverse));
        Task4(arrayToReverse);
        Console.WriteLine(PrintArray(arrayToReverse));

        int[,] matrix = GenerateMatrix(3, 6, 1, 4, rnd);
        Task5(matrix);

        Task6();
    }

    static int[] GenerateArray(int size, int minVal, int maxVal, Random rnd)
    {
        int[] arr = new int[size];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = rnd.Next(minVal, maxVal + 1);
        }
        return arr;
    }

    static int[,] GenerateMatrix(int rows, int cols, int minVal, int maxVal, Random rnd)
    {
        int[,] matrix = new int[rows, cols];
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = rnd.Next(minVal, maxVal + 1);
            }
        }
        return matrix;
    }

    static string PrintArray(int[] arr)
    {
        if (arr == null || arr.Length == 0) return "[]";
        string result = "[";
        for (int i = 0; i < arr.Length; i++)
        {
            result += arr[i];
            if (i < arr.Length - 1) result += ", ";
        }
        result += "]";
        return result;
    }

    public static void Task1(int[] arr)
    {
        if (arr == null || arr.Length == 0)
        {
            Console.WriteLine("Результат не визначений");
            return;
        }

        Console.WriteLine(PrintArray(arr));

        int sum = arr[0];
        int minVal = arr[0];
        int minIndex = 0;
        int maxVal = arr[0];
        int maxIndex = 0;
        int zeroCount = (arr[0] == 0) ? 1 : 0;

        for (int i = 1; i < arr.Length; i++)
        {
            sum += arr[i];

            if (arr[i] < minVal)
            {
                minVal = arr[i];
                minIndex = i;
            }

            if (arr[i] > maxVal)
            {
                maxVal = arr[i];
                maxIndex = i;
            }

            if (arr[i] == 0)
            {
                zeroCount++;
            }
        }

        double average = (double)sum / arr.Length;

        Console.WriteLine(sum);
        Console.WriteLine(average);
        Console.WriteLine(minVal + " " + minIndex);
        Console.WriteLine(maxVal + " " + maxIndex);
        Console.WriteLine(zeroCount);
    }

    public static int[] Task2(int[] arr, int threshold)
    {
        if (arr == null || arr.Length == 0)
        {
            return new int[0];
        }

        int count = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] > threshold)
            {
                count++;
            }
        }

        int[] filtered = new int[count];
        int index = 0;

        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] > threshold)
            {
                filtered[index] = arr[i];
                index++;
            }
        }

        return filtered;
    }

    public static void Task3(int[] arr)
    {
        if (arr == null || arr.Length == 0)
        {
            Console.WriteLine("Результат не визначений");
            return;
        }

        int bestVal = arr[0];
        int bestLen = 1;
        int bestStart = 0;

        int currVal = arr[0];
        int currLen = 1;
        int currStart = 0;

        for (int i = 1; i < arr.Length; i++)
        {
            if (arr[i] == arr[i - 1])
            {
                currLen++;
            }
            else
            {
                if (currLen > bestLen)
                {
                    bestLen = currLen;
                    bestVal = currVal;
                    bestStart = currStart;
                }
                currVal = arr[i];
                currLen = 1;
                currStart = i;
            }
        }

        if (currLen > bestLen)
        {
            bestLen = currLen;
            bestVal = currVal;
            bestStart = currStart;
        }

        Console.WriteLine(bestVal + " " + bestLen + " " + bestStart);
    }

    public static void Task4(int[] arr)
    {
        if (arr == null || arr.Length == 0) return;

        int left = 0;
        int right = arr.Length - 1;

        while (left < right)
        {
            int temp = arr[left];
            arr[left] = arr[right];
            arr[right] = temp;

            left++;
            right--;
        }
    }

    public static void Task5(int[,] matrix)
    {
        if (matrix == null || matrix.GetLength(0) == 0 || matrix.GetLength(1) == 0)
        {
            Console.WriteLine("Матриця порожня");
            return;
        }

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write(matrix[i, j] + "\t");
            }
            Console.WriteLine();
        }

        int[] rowSums = new int[rows];
        for (int i = 0; i < rows; i++)
        {
            int sum = 0;
            for (int j = 0; j < cols; j++)
            {
                sum += matrix[i, j];
            }
            rowSums[i] = sum;
            Console.WriteLine(rowSums[i]);
        }

        for (int j = 0; j < cols; j++)
        {
            int max = matrix[0, j];
            for (int i = 1; i < rows; i++)
            {
                if (matrix[i, j] > max)
                {
                    max = matrix[i, j];
                }
            }
            Console.WriteLine(max);
        }

        int maxRowSumIndex = 0;
        int maxRowSum = rowSums[0];
        for (int i = 1; i < rows; i++)
        {
            if (rowSums[i] > maxRowSum)
            {
                maxRowSum = rowSums[i];
                maxRowSumIndex = i;
            }
        }

        Console.WriteLine(maxRowSumIndex);
    }

    public static void Task6()
    {
        int[] empty = new int[0];
        Task1(empty);
        Task3(empty);
        Task4(empty);

        int[] single = new int[] { 5 };
        Task1(single);
        Task3(single);
        Task4(single);
        Console.WriteLine(PrintArray(single));

        int[] identical = new int[] { 3, 3, 3, 3 };
        Task1(identical);
        Task3(identical);
    }
}