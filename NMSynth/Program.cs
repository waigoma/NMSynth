// See https://aka.ms/new-console-template for more information

using NAudio.CoreAudioApi;

namespace NMSynth;

internal static class Program
{
    private const int SampleRate = 44100;
    
    public static void Main(string[] args)
    {
        // 出力デバイス開放
        var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        var wop = new WaveOutProcessor(SampleRate, 2, device);
        wop.Open();
        
        // シーケンサ起動
        var sequencer = new Sequencer(SampleRate, "../../../soundfont/TimGM6mb-MuseScore.sf2", wop);
        sequencer.Open();
        
        // 無限ループ、ESC で終了。
        // A で ドを 127 で鳴らす。D is off。 
        while (true)
        {
            Console.WriteLine("Press any key...");
            var key = Console.ReadKey();
            
            switch (key.Key)
            {
                case ConsoleKey.Spacebar:
                    wop.Close();
                    sequencer.Close();
                    return;
                case ConsoleKey.A:
                    sequencer.NoteOn();
                    break;
                case ConsoleKey.D:
                    sequencer.NoteOff();
                    break;
            }
        }
    }
}
