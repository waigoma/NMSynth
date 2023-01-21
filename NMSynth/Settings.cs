namespace NMSynth;

public class Settings
{
    /// <summary>
    /// サンプリングレート
    /// </summary>
    public int SampleRate { get; private set; }
    /// <summary>
    /// チャンネル数
    /// </summary>
    public int Channels { get; private set; }
    /// <summary>
    /// バッファの大きさ
    /// </summary>
    public int BufferSize { get; private set; }
    /// <summary>
    /// バッファキューの大きさ
    /// </summary>
    public int BufferQueueSize { get; private set; }
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="sampleRate">サンプリングレート</param>
    /// <param name="channels">チャンネル数</param>
    /// <param name="bufferSize">バッファの大きさ</param>
    /// <param name="bufferQueueSize">バッファキューの大きさ</param>
    public Settings(int sampleRate, int channels, int bufferSize, int bufferQueueSize)
    {
        SampleRate = sampleRate;
        Channels = channels;
        BufferSize = bufferSize;
        BufferQueueSize = bufferQueueSize;
    }
    
    /// <summary>
    /// サンプリングレートを設定する
    /// </summary>
    /// <param name="sampleRate">変更後のサンプリングレート</param>
    public void SetSampleRate(int sampleRate)
    {
        SampleRate = sampleRate;
    }
    
    /// <summary>
    /// チャンネル数を設定する
    /// </summary>
    /// <param name="channels">変更後のチャンネル数</param>
    public void SetChannels(int channels)
    {
        Channels = channels;
    }
    
    /// <summary>
    /// バッファの大きさを設定する
    /// </summary>
    /// <param name="bufferSize">変更後のバッファの大きさ</param>
    public void SetBufferSize(int bufferSize)
    {
        BufferSize = bufferSize;
    }
    
    /// <summary>
    /// バッファキューサイズを設定する
    /// </summary>
    /// <param name="bufferQueueSize">変更後のバッファキューの大きさ</param>
    public void SetBufferQueueSize(int bufferQueueSize)
    {
        BufferQueueSize = bufferQueueSize;
    }
}