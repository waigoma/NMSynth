using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace NMSynth;

public class WaveOutProcessor
{
    public void Run()
    {
        // 44.1kHz, 16-bit, Stereo
        var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(44100, 16, 2));
        
        // allow volume control
        var wavProvider = new VolumeWaveProvider16(bufferedWaveProvider);
        wavProvider.Volume = 0.1f;
        
        // create the output device
        var mmDevice = new MMDeviceEnumerator()
            .GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        // listen outside wave
        Task t = StartDummySoundSource(bufferedWaveProvider);
        
        using (IWavePlayer wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 200))
        {
            //出力に入力を接続して再生開始
            wavPlayer.Init(wavProvider);
            wavPlayer.Play();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            wavPlayer.Stop();
        }
    }

    public async Task StartDummySoundSource(BufferedWaveProvider provider)
    {
        //外部入力のダミーとして適当な音声データを用意して使う
        var wavFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "sample.wav"
        );

        if (!File.Exists(wavFilePath))
        {
            Console.WriteLine("Target sound files were not found. Wav file or MP3 file is needed for this program.");
            Console.WriteLine($"expected wav file: {wavFilePath}");
            Console.WriteLine($"expected mp3 file: {wavFilePath}");
            Console.WriteLine("(note: ONE file is enough, two files is not needed)");
            return;
        }

        byte[] data = File.ReadAllBytes(wavFilePath);

        //若干効率が悪いがヘッダのバイト数を確実に割り出して削る
        using (var r = new WaveFileReader(wavFilePath))
        {
            int headerLength = (int)(data.Length - r.Length);
            data = data.Skip(headerLength).ToArray();
        }

        int bufsize = 16000;
        for (int i = 0; i + bufsize < data.Length; i += bufsize)
        {
            provider.AddSamples(data, i, bufsize);
            await Task.Delay(100);
        }
    }
}