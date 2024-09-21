using System;
using System.Numerics;
using System.Threading;

namespace RandomPrime
{
    public static class RandomPrimeNumber
    {
        private static BigInteger _truePrime = BigInteger.Zero;
        private static int _numCount;

        private static int rounds = 28;
        public static int Rounds
        {
            get => rounds;
            set => rounds = value;
        }

        /// <summary>
        /// Generiert eine zufällige Primzahl
        /// </summary>
        /// <param name="bits">Länge der Zahl in Bits</param>
        /// <returns>Gibt eine "echte" Primzahl zurück</returns>
        public static BigInteger GetPrime(int bits)
        {
            while (true)
            {
                BigInteger prime = RandomNumber(bits);

                Console.WriteLine(prime);

                // durch 2 teilbare Zahlen ausschließen und mit neuer Zahl fortfahren
                if (prime % 2 == 0)
                {
                    _numCount++;
                    continue;
                }

                if (IsProbablePrime(prime))
                    return prime;
            }
        }

        /// <summary>
        /// Generiert eine zufällige Primzahl
        /// </summary>
        /// <param name="bits">Länge der Zahl in Bits</param>
        /// <param name="threadCount">Anzahl an Threads, mit denen die Kalkulation durchgeführt werden soll</param>
        /// <param name="numCount">Anzahl der verarbeiteten Zahlen</param>
        /// <returns>Gibt eine "echte" Primzahl zurück</returns>
        public static BigInteger GetPrime(int bits, int threadCount, out int numCount, out string accuracy)
        {
            var (Left, Top) = Console.GetCursorPosition();
            Semaphore semaphore = new(1, 1);

            for (int i = 0; i < threadCount; i++)
            {
                new Thread(() =>
                {
                    while (true)
                    {
                        if (!_truePrime.Equals(BigInteger.Zero))
                            break;

                        BigInteger prime = RandomNumber(bits);

                        semaphore.WaitOne();
                        Console.SetCursorPosition(Left, Top);
                        Console.Write(prime);
                        semaphore.Release();

                        // durch 2 teilbare Zahlen ausschließen und mit neuer Zahl fortfahren
                        if (prime % 2 == 0)
                        {
                            _numCount++;
                            continue;
                        }

                        if (IsProbablePrime(prime))
                            _truePrime = prime;
                    }
                }).Start();
            }

            while (_truePrime.IsZero) { }

            Console.SetCursorPosition(Left, Top);
            Console.Write(new string(' ', _truePrime.ToString().Length));
            Console.SetCursorPosition(Left, Top);

            numCount = _numCount;
            accuracy = CalculateAccuracy();
            return _truePrime;
        }

        /// <summary>
        /// Erstellt eine kryptographisch sichere Zufallszahl in gewünschter Länge (Anzahl an Bits)
        /// </summary>
        /// <param name="bits">Anzahl an Bits</param>
        /// <returns>zufällig generierter BigInteger mit der Länge [bits]</returns>
        public static BigInteger RandomNumber(int bits)
        {
            // 1. Erstellen eines kryptographisch sicheren Zufallsgenerators
            System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create();

            // 2. Bestimmen der erforderlichen Bytegröße:
            // (bits + 7) / 8 rundet die Anzahl der benötigten Bytes auf.
            // Ein Byte besteht aus 8 Bits, daher benötigen wir bei z.B. 9 Bits 2 Bytes.
            byte[] buffer = new byte[(bits + 7) / 8];

            // 3. Füllen des Byte-Arrays mit zufälligen Werten.
            rng.GetBytes(buffer);

            // 4. Sicherstellen, dass die höchste Bitzahl, die wir möchten, gesetzt wird:
            // `bits % 8` gibt die verbleibende Anzahl Bits an, die in einem unvollständigen Byte verwendet werden sollen.
            // Wir setzen das erste Bit im letzten Byte, um sicherzustellen, dass die erzeugte Zahl mindestens `bits` Bits verwendet.
            // `buffer[^1]` greift auf das letzte Byte im Array zu (der Index -1 bedeutet das letzte Element im Array).
            buffer[^1] |= (byte)(0x01 << (bits % 8));

            // Vorzeichenbit auf positiv setzen -> [^1] = [bytes.Length - 1]
            buffer[^1] &= 0x7F;

            // 5. Erstellen eines BigInteger-Objekts aus dem zufälligen Byte-Array.
            BigInteger prime = new(buffer);

            // 6. Rückgabe der generierten BigInteger-Zahl.
            return prime;
        }

        /// <summary>
        /// Prüft eine Pseudoprimzahl auf ihre Primität (Miller-Rabin-Test) OpenAi Implementierung
        /// </summary>
        /// <param name="n">Pseudoprimzahl</param>
        /// <returns>Boolean</returns>
        private static bool IsProbablePrime(BigInteger n)
        {
            if (n < 2)
            {
                return false;
            }

            if (n == 2 || n == 3)
            {
                return true;
            }

            if ((n & 1) == 0)
            {
                return false;
            }

            // Schreibe n - 1 als 2^s * d wobei d ungerade ist
            BigInteger d = n - 1;
            int s = 0;


            while ((d & 1) == 0) // überprüft, ob d gerade ist, indem das niedrigstwertige Bit von d geprüft wird (Bitweises UND mit 1)
            {
                s++;
                d >>= 1; // verschiebt d um eine Zweierpotenz nach recht, was einer Division durch 2 gleich kommt
            }

            BigInteger x = BigInteger.ModPow(rounds, d, n); // berechnet x = witness^d mod  n effizient mit dem "Modular Exponentiation"-Algorithmus

            // Wenn x = 1, besteht eine hohe Wahrscheinlichkeit, dass n eine Primzahl ist (nach Fermats kleinem Satz)
            // Wenn x = n − 1, ist n ebenfalls wahrscheinlich eine Primzahl, weil es darauf hindeutet, dass n den Miller-Rabin-Test bestanden hat
            if (x == 1 || x == n - 1)
            {
                return true;
            }

            // Wiederholter Quadrat-Test
            for (int i = 0; i < s - 1; i++)
            {
                x = BigInteger.ModPow(x, 2, n); // berechnet x^2 mod n

                if (x == 1) // n ist sicher keine Primzahl (es liegt ein Nichttriviales Quadratwurzel von 1 mod n vor, was bedeutet, dass n eine zusammengesetzte Zahl ist)
                {
                    return false;
                }

                if (x == n - 1)
                {
                    return true;
                }
            }

            return false;
        }

        private static string CalculateAccuracy()
        {
            BigInteger powerResult = BigInteger.Pow(4, Rounds);
            BigInteger difference = powerResult - BigInteger.One;

            //double accuracy = (double)differrence / (double)powerResult;
            //double accuracyPercentage = accuracy * 100;

            // Um sicherzustellen, dass wir die Genauigkeit in 32 Nachkommastellen berechnen,
            // multiplizieren wir den Unterschied mit 10000000000000000000000000000000 (10^32)
            BigInteger scaledDifference = difference * BigInteger.Pow(10, 32);

            // Berechnung der Genauigkeit
            BigInteger accuracy = scaledDifference / powerResult;

            string accuracyString = accuracy.ToString();
            // [..2] = Substring(0, 2) | [2..] = Substring(Length - 2)
            string accuracyPercentage = accuracyString[..2] + "." + accuracyString[2..];

            return accuracyPercentage;
        }
    }
}
