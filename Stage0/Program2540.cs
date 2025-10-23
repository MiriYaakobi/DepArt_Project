namespace Stage0
{
    partial class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Welcome2540();
            Welcome1617();
            Console.ReadKey();
        }
        static partial void Welcome1617();
        private static void Welcome2540()
        {
            Console.Write("Enter your name: ");
            string name = Console.ReadLine();
            Console.Write("{0}, wlcome to my first console application", name);
        }
    }
}