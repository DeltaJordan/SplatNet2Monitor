using System;
using System.Security.Cryptography;

namespace SplatNet2.Net.Api.Maths
{
    public class RandomGenerator
    {
        private readonly RNGCryptoServiceProvider csp;

        public RandomGenerator()
        {
            this.csp = new RNGCryptoServiceProvider();
        }

        public int Next(int minValue, int maxExclusiveValue)
        {
            if (minValue >= maxExclusiveValue)
                throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue");

            long diff = (long)maxExclusiveValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;

            uint ui;
            do
            {
                ui = this.GetRandomUInt();
            } while (ui >= upperBound);
            return (int)(minValue + (ui % diff));
        }

        public uint GetRandomUInt()
        {
            byte[] randomBytes = this.GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randomBytes, 0);
        }

        public byte[] GenerateRandomBytes(int bytesNumber)
        {
            byte[] buffer = new byte[bytesNumber];
            this.csp.GetBytes(buffer);
            return buffer;
        }
    }
}