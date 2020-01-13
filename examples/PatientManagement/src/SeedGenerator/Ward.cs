namespace SeedGenerator
{
    using System;

    public class Ward
    {
        private static readonly Random rand = new Random();
        public static int Get() => rand.Next(1, 20);
    }
}