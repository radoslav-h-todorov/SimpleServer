using System;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var server = new HttpFileServer(@"C:\temp", 9998, false))
            {
                Console.WriteLine("Press ENTER to terminate ...");
                Console.ReadLine();
            }
        }
    }
}
