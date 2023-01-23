using NAudio.CoreAudioApi;

namespace NMSynth;

internal static class Program
{
    private static readonly Settings Settings = new (44100, 2, 1024, 1);
    
    public static void Main(string[] args)
    {
        // 出力デバイス開放
        var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        var wop = new WaveOutProcessor(Settings, device);
        wop.Open();
        
        // シンセサイザ準備
        var synth = new Synth(Settings.SampleRate, "../../../soundfont/TimGM6mb-MuseScore.sf2");
        
        // シーケンサ起動
        var sequencer = new Sequencer(Settings, synth.GetSynthesizer(), wop);
        sequencer.Open();
        
        // 無限ループ、Space で終了。
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
                    synth.NoteOn();
                    break;
                case ConsoleKey.D:
                    synth.NoteOff();
                    break;
            }
        }
    }
}
