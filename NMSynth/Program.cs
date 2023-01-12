// See https://aka.ms/new-console-template for more information

namespace NMSynth;

internal class Program
{
    public static void Main(string[] args)
    {
        var sequencer = new Sequencer();
        sequencer.Run();
        Console.WriteLine("Hello World!");
    }
}
