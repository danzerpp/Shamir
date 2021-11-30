using System;
using System.Collections.Generic;
using System.Text;

namespace Shamir.Models
{
    public static class MyMath
    {
        public static ModelX Multiply(ModelX firstX, ModelX secondX)
        {
            long a = firstX.A * secondX.A;
            long power = firstX.Power + secondX.Power;
            return new ModelX(a, power);
        }

        public static ModelX Multiply(ModelX x, long val)
        {
            long a = x.A * val;
            return new ModelX(a, x.Power);
        }


        public static long DivMod(long a, long den, long p)
        {
            long inv = ExtendedEuclideanAlgorithm(den, p);
            return a * inv;
        }


        public static long ExtendedEuclideanAlgorithm(long a, long b)
        {
            long x0 = 1, xn = 1;
            long y0 = 0, yn = 0;
            long x1 = 0;
            long y1 = 1;
            long q;
            long r = MyMath.Modulo(a, b);

            while (r > 0)
            {
                q = a / b;
                xn = x0 - q * x1;
                yn = y0 - q * y1;

                x0 = x1;
                y0 = y1;
                x1 = xn;
                y1 = yn;
                a = b;
                b = r;
                r = a % b;
            }

            return xn;

        }
        public static long LongRandom(long min, long max, Random rand)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);
            return (Math.Abs(longRand % (max - min)) + min);
        }

        public static bool IsPrime(long number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var boundary = (long)Math.Floor(Math.Sqrt(number));

            for (long i = 3; i <= boundary; i += 2)
                if (number % i == 0)
                    return false;

            return true;
        }

        public static long Modulo(long x, long m)
        {
            return (x % m + m) % m;
        }


    }
}
