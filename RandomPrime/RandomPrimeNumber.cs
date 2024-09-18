using System;
using System.Numerics;
using System.Threading;

using Extensions;

namespace RandomPrime
{
    public static class RandomPrimeNumber
    {
        private static BigInteger _truePrime = BigInteger.Zero;
        private static int _numCount;

        private static int rounds = 54;
        public static int Rounds
        {
            get => rounds;
            set => rounds = value;
        }

        private static readonly BigInteger powerResult = BigInteger.Pow(4, Rounds);
        private static readonly BigInteger oneBigInt = BigInteger.One * powerResult;
        private static readonly BigInteger differrence = oneBigInt - BigInteger.One;

        private static readonly double _accuracy = (double)differrence / (double)powerResult * 100;

        /// <summary>
        /// Generiert eine zufällige Primzahl
        /// </summary>
        /// <param name="bits">Länge der Zahl in Bits</param>
        /// <returns>Gibt eine "echte" Primzahl zurück</returns>
        public static BigInteger GetPrime(int bits)
        {
            while (true)
            {
                Random random = new();
                byte[] bytes = new byte[(bits +7) / 8]; // 1 Byte = 8 Bit = bits/8
                random.NextBytes(bytes);
                bytes[^1] &= 0x7F; // force sign bit to positive [^1] = [bytes.Length - 1]
                BigInteger prime = new(bytes);
                long maxExpo = (long)Math.Round(BigInteger.Log(prime, 2)); // ermittelt den größten Exponenten zur Basis 2 von prime (Log2(prime))

                Console.WriteLine(prime);

                // durch 2 teilbare Zahlen ausschließen und mit neuer Zahl fortfahren
                if (prime % 2 == 0)
                {
                    _numCount++;
                    continue;
                }

                BigInteger greatestPowerOfTwoDevisor = GetMaxPowerOfTwoDevisor(prime, maxExpo);

                // wenn größter Teiler = 0 
                if (greatestPowerOfTwoDevisor == 0)
                {
                    _numCount++;
                    continue;
                }

                if (IsPrime(prime, greatestPowerOfTwoDevisor))
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
        public static BigInteger GetPrime(int bits, int threadCount, out int numCount, out double accuracy)
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

                        Random random = new();
                        byte[] bytes = new byte[bits / 8]; // 1 Byte = 8 Bit = bits/8
                        random.NextBytes(bytes);
                        bytes[^1] &= 0x7F; // force sign bit to positive [^1] = [bytes.Length - 1]
                        BigInteger prime = new(bytes);
                        long maxExpo = (long)Math.Round(BigInteger.Log(prime, 2)); // ermittelt den größten Exponenten zur Basis 2 von prime (Log2(prime))

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

                        BigInteger greatestPowerOfTwoDevisor = GetMaxPowerOfTwoDevisor(prime, maxExpo);

                        // wenn größter Teiler = 0 
                        if (greatestPowerOfTwoDevisor == 0)
                        {
                            _numCount++;
                            continue;
                        }

                        if (IsPrime(prime, greatestPowerOfTwoDevisor))
                            _truePrime = prime;
                    }
                }).Start();
            }

            while (_truePrime.IsZero) { }

            Console.SetCursorPosition(Left, Top);
            Console.Write(new string(' ', _truePrime.ToString().Length));
            Console.SetCursorPosition(Left, Top);

            numCount = _numCount;
            accuracy = _accuracy;
            return _truePrime;
        }

        /// <summary>
        /// Prüft eine Pseudoprimzahl auf ihre Primität
        /// </summary>
        /// <param name="prime">Pseudoprimzahl</param>
        /// <param name="greatestPowerOfTwoDevisor">größter Teiler der Basis 2 zu prime</param>
        /// <returns>Boolean</returns>
        private static bool IsPrime(BigInteger prime, BigInteger greatestPowerOfTwoDevisor)
        {
            for (int i = 1; i <= Rounds; i++)
            {
                Random random = new();
                BigInteger value = random.NextBigInteger(BigInteger.One, prime);
                var mod = BigInteger.ModPow(value, greatestPowerOfTwoDevisor, prime);

                if (mod == 1)
                    continue;
                else if (mod == prime - 1)
                    continue;
                else
                {
                    BigInteger expo = (prime - 1) / greatestPowerOfTwoDevisor;
                    for (long r = (long)expo - 1; r >= 0; r--)
                    {
                        try
                        {
                            double _mod = (double)BigInteger.ModPow(value, greatestPowerOfTwoDevisor * (long)Math.Pow(2, r), prime);

                            if (_mod == -1)
                                break;

                            if (r == 0)
                            {
                                _numCount++;
                                return false;
                            }
                        }
                        catch { }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Bestimmt den größten Exponenten der Zahl 2 als Teiler von prime - 1
        /// </summary>
        /// <param name="prime">Pseudoprimzahl</param>
        /// <param name="maxExpo">größter Exponent</param>
        /// <returns>gröter Teiler von prime - 1 in Form einer Potenz von 2</returns>
        private static BigInteger GetMaxPowerOfTwoDevisor(BigInteger prime, long maxExpo)
        {
            for (long i = maxExpo; i > 0; i--)
            {
                BigInteger pm1 = prime - 1;
                long exponent = (long)Math.Pow(2, i);
                BigInteger greatestPowerOfTwoDevisor = pm1 / exponent;

                if (greatestPowerOfTwoDevisor * exponent == pm1)
                    return greatestPowerOfTwoDevisor;
            }

            return 0;
        }
    }
}
