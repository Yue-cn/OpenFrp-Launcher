namespace OpenFrp.Core
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var app = new Pipe.PipeServer();
            app.Start();
            while (true)
            {
                Console.Title = "Action Service (Server)";
                Console.ReadKey();
            }
        }
    }
}