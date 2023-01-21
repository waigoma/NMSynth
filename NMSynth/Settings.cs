namespace NMSynth;

public class Settings
{
    public int SampleRate { get; private set; }
    public int Channels { get; private set; }
    public int BufferSize { get; private set; }
    public int BufferQueueSize { get; private set; }
    
    public Settings(int sampleRate, int channels, int bufferSize, int bufferQueueSize)
    {
        SampleRate = sampleRate;
        Channels = channels;
        BufferSize = bufferSize;
        BufferQueueSize = bufferQueueSize;
    }
    
    public void SetSampleRate(int sampleRate)
    {
        SampleRate = sampleRate;
    }
    
    public void SetChannels(int channels)
    {
        Channels = channels;
    }
    
    public void SetBufferSize(int bufferSize)
    {
        BufferSize = bufferSize;
    }
    
    public void SetBufferQueueSize(int bufferQueueSize)
    {
        BufferQueueSize = bufferQueueSize;
    }
}