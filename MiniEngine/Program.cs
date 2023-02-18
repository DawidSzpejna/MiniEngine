namespace MiniEngine
{
    public static class Program
    {
        public static int Main()
        {
            using (Engine engine = new Engine(1200, 800))
            {
                engine.Run();
            }
            return 0;
        }
    }
}
