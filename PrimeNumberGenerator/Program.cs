using RandomPrime;
using System;
using System.Diagnostics;

namespace PrimeNumberGenerator
{
    internal class Program
    {
        private static int numCount = 0;
        private static double accuracy;

        static void Main(string[] args)
        {
            Console.Write("Geben Sie die gewünschte Länge (Anzahl an Bits) der zu kalkulierenden Primzahl an (Standard: 512 Bits): ");

            int bits = 512;
            try
            {
                bits = Convert.ToInt32(Console.ReadLine());
            }
            catch { }

            Console.WriteLine("Wie oft soll eine scheinbare Primzahl auf ihre primität getestet werden (Min: 1; Standard: 10; Max: 54)?: ");

            int testRounds = 10;
            try
            {
                testRounds = Convert.ToInt32(Console.ReadLine());
            }
            catch { }

            RandomPrimeNumber.Rounds = testRounds;

            #region Anwendungsinformationen
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n!!! Hohe CPU-Auslastung bei mehreren Threads !!!");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("\nEmpfehlungen für Threadanzahl:");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Standard\t");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("1 Thread");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Optimum\t\t");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Kernanzahl (inklusive logischer Kerne)\n\t\tBsp.:\t-> 4 Kerne physisch ohne logische Kerne\t= 4 Threads\n\t\t\t-> 4 Kerne physisch + 4 Kerne logisch\t= 8 Threads");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Maximum\t\t");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("8-fache Kernanzahl (inklusive logischer Kerne)\n\t\tBsp.:\t-> 4 Kerne physisch ohne logische Kerne\t= 32 Threads\n\t\t\t-> 4 Kerne physisch + 4 Kerne logisch\t= 64 Threads");
            #endregion

            Console.Write("\nGeben Sie die gewünschte Anzahl an Threads an,\nmit denen gleichzeitig eine Primzahl kalkuliert werden soll: ");

            int threadCount = 1;

            try
            {
                threadCount = Convert.ToInt32(Console.ReadLine());
            }
            catch { }

            Console.WriteLine("\nSuche Primzahl...");

            Stopwatch sw = new();
            sw.Start();

            Console.WriteLine($"\nFolgende Primzahl wurde gefunden:\n\n{RandomPrimeNumber.GetPrime(bits, threadCount, out numCount, out accuracy)}\n");
            Console.WriteLine($"Diese Primzahl wurde mittels dem \"Miller-Rabin-Test\"\nunter der Berücksichtigung einer Prüfung auf \"Zeugen\" für die \"Nicht-Primität\" (Carmichael-Zahlen) überprüft\nund ist unter Vorbehalt dieses Tests nach {RandomPrimeNumber.Rounds} Durchgängen mit {accuracy.ToString("F16")}% iger Wahrscheinlichkeit eine Primzahl.\n");

            sw.Stop();
            Console.WriteLine("{0} Zahlen in {1:00}:{2:00}:{3:00}.{4:0000} überprüft.", numCount, sw.Elapsed.Hours, sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds);
            Console.Read();
        }
    }
}