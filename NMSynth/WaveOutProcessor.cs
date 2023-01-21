using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace NMSynth;

/// <summary>
/// リアルタイムで音声を再生するためのクラス
/// </summary>
public sealed class WaveOutProcessor
{
    /// <summary>
    /// 音声波形を実際に書き込む
    /// </summary>
    public BufferedWaveProvider WaveProvider { get; }
    
    /// <summary>
    /// 音量
    /// </summary>
    public float Volume
    {
        get => _floatProvider.Volume;
        
        set => _floatProvider.Volume = value;
    }

    // 音量を調整するためのプロバイダ
    private readonly VolumeWaveProvider16 _floatProvider;
    // 音声出力ドライバ
    private readonly WasapiOut _wasapi;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="sampleRate">サンプリングレート</param>
    /// <param name="channels">チャンネル数</param>
    /// <param name="device">出力に使うデバイス</param>
    public WaveOutProcessor(int sampleRate, int channels, MMDevice device)
    {
        var waveFormat =  new WaveFormat(sampleRate, channels);
        WaveProvider = new BufferedWaveProvider(waveFormat); 
        
        _floatProvider = new VolumeWaveProvider16(WaveProvider);
        _wasapi = new WasapiOut(device, AudioClientShareMode.Shared, false, 200);
        
        Volume = 0.5f;
    }

    public void Write(byte[] data)
    {
        WaveProvider.AddSamples(data, 0, data.Length);
    }
    
    /// <summary>
    /// 音声の再生モードをオンにする
    /// </summary>
    public void Open()
    {
        _wasapi.Init(_floatProvider);
        _wasapi.Play();
    }
    
    /// <summary>
    /// 音声の再生モードをオフにする
    /// </summary>
    public void Close()
    {
        _wasapi.Stop();
    }
    
    public void Run()
    {
        // 44.1kHz, 16-bit, Stereo
        var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(44100, 16, 2));
        
        // allow volume control
        var wavProvider = new VolumeWaveProvider16(bufferedWaveProvider)
        {
            Volume = 0.1f
        };
        
        // create the output device
        var mmDevice = new MMDeviceEnumerator()
            .GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        // listen outside wave
        var t = StartDummySoundSource(bufferedWaveProvider);

        using var wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 200);
        
        //出力に入力を接続して再生開始
        wavPlayer.Init(wavProvider);
        wavPlayer.Play();

        Console.WriteLine("Press ENTER to exit...");
        Console.ReadLine();

        wavPlayer.Stop();
    }
    
    private static async Task StartDummySoundSource(BufferedWaveProvider provider)
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

        var data = File.ReadAllBytes(wavFilePath);

        //若干効率が悪いがヘッダのバイト数を確実に割り出して削る
        await using (var r = new WaveFileReader(wavFilePath))
        {
            var headerLength = (int)(data.Length - r.Length);
            data = data.Skip(headerLength).ToArray();
        }

        var bufsize = 16000;
        for (var i = 0; i + bufsize < data.Length; i += bufsize)
        {
            provider.AddSamples(data, i, bufsize);
            await Task.Delay(100);
        }
    }
}