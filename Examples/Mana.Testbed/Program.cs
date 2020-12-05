using Mana.Example;

namespace Mana.Testbed
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using var testbed = new TestbedGame();
            testbed.Run();
        }
    }
}