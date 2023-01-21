using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace NMSynth;

/// <summary>
/// リアルタイムで音声を再生するためのクラス
/// </summary>
public sealed class WaveOutProcessor
{
    /// <summary>
    /// 音量
    /// </summary>
    public float Volume
    {
        get => _volumeProvider.Volume;
        
        set => _volumeProvider.Volume = value;
    }

    // バッファに書き込むためのプロバイダ
    private readonly BufferedWaveProvider _waveProvider;
    // 音量を調整するためのプロバイダ
    private readonly VolumeWaveProvider16 _volumeProvider;
    // 音声出力ドライバ
    private readonly WasapiOut _wasapi;

    private bool _isOpen;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="settings">設定</param>
    /// <param name="device">出力に使うデバイス</param>
    public WaveOutProcessor(Settings settings, MMDevice device)
    {
        var waveFormat =  new WaveFormat(settings.SampleRate, settings.Channels);
        _waveProvider = new BufferedWaveProvider(waveFormat); 
        
        _volumeProvider = new VolumeWaveProvider16(_waveProvider);
        _wasapi = new WasapiOut(device, AudioClientShareMode.Shared, false, 0);
        
        Volume = 0.5f;
    }

    /// <summary>
    /// バッファにサンプルを書き込む
    /// </summary>
    /// <param name="data">PCM byte array.</param>
    /// <exception cref="IOException">Output device not open.</exception>
    public void Write(byte[] data)
    {
        if (!_isOpen)
            throw new IOException("Output device is not open.");
        
        _waveProvider.AddSamples(data, 0, data.Length);
    }
    
    /// <summary>
    /// 音声の再生モードをオンにする
    /// </summary>
    public void Open()
    {
        _wasapi.Init(_volumeProvider);
        _wasapi.Play();
        _isOpen = true;
    }
    
    /// <summary>
    /// 音声の再生モードをオフにする
    /// </summary>
    public void Close()
    {
        _wasapi.Stop();
        _isOpen = false;
    }
}