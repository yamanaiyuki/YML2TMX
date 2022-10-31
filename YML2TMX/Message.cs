namespace Yml2Tmx;

public class Message
{
    private static readonly char[] windmills = { '―', '＼', '｜', '／', };
    private static int oldLength;

    public static int MaxCount { get; set; }

    public static void PrintProgress(int i, string message)
    {
        if (MaxCount < 2)
        {
            return;
        }

        Console.Write(windmills[i % 4]);

        string m = message.Split("\\")[^1];
        string s = string.Concat(Enumerable.Repeat(" ", Math.Max(0, oldLength - m.Length)));

        Console.Write("{0, 4:d0}% {1}", (i + 1) * 100 / MaxCount, m + s);

        oldLength = m.Length;

        (_, int top) = Console.GetCursorPosition();
        Console.SetCursorPosition(0, top);
    }

    public static void PrintProgressEnd()
    {
        if (MaxCount < 2)
        {
            return;
        }

        Console.WriteLine();
    }

    public static void PrintMessage(string message)
    {
        string m = message.Split("\\")[^1];
        Console.WriteLine(m);
    }

    public static void ReadKey(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Console.WriteLine(message);
        }

        Console.ReadKey();
    }
}
