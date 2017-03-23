using System;
using System.Collections.Generic;
using System.Security.Cryptography;
namespace FileRandomiser.Helpers
{
    public static class ListHelper
    {
        public static void Shuffle<T>(this List<T> list)
        {
            try
            {
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to shuffle list. " + e.Message);
            }
        }

        public static void ShuffleBetter<T>(this List<T> list)
        {
            try
            {
                RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
                int n = list.Count;
                while (n > 1)
                {
                    byte[] box = new byte[1];
                    do
                    {
                        provider.GetBytes(box);
                    }
                    while (!(box[0] < n * (Byte.MaxValue / n)));

                    int k = (box[0] % n);
                    n--;

                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to shuffle list. " + e.Message);
            }
        }
    }
}