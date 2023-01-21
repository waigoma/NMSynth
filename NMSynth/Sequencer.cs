using MeltySynth;

namespace NMSynth;

public class Sequencer
{
    private readonly WaveOutProcessor _waveOutProcessor;
    private readonly Synthesizer _synthesizer;
    private readonly int _sampleRate;
    
    private bool _isOpen;
    private Task? _realtimeTask;
    
    public Sequencer(int sampleRate, Synthesizer synthesizer, WaveOutProcessor waveOutProcessor)
    {
        _waveOutProcessor = waveOutProcessor;
        _synthesizer = synthesizer;
        _sampleRate = sampleRate;
    }

    public void Open()
    {
        _isOpen = true;
        _realtimeTask = RealtimeSound();
    }

    public void Close()
    {
        _isOpen = false;
        _realtimeTask?.Wait();
        _realtimeTask?.Dispose();
    }

    private async Task RealtimeSound()
    {
        // The length of a block is 0.001 sec.
        // var blockSize = _sampleRate / 1000;

        // The entire output is 10 ms.
        // var blockCount = 10;

        var bufferSize = 1024;

        // The output buffer.
        // var left = new float[blockSize * blockCount];
        // var right = new float[blockSize * blockCount];
        var left = new float[bufferSize];
        var right = new float[bufferSize];
        var stereo = new float[left.Length + right.Length];
        var pcm = new byte[stereo.Length * 2];
        
        // Sequencer loop.
        while (_isOpen)
        {
            // Render the next block.
            _synthesizer.Render(left, right);
            
            // Convert to Stereo float array.
            ConvertLRToStereo(stereo, left, right, bufferSize);
            
            // Convert to byte array.
            ConvertFloatToPcmBytes(pcm, stereo, stereo.Length);
            
            // Write to WaveOut.
            if (IsPcmExists(pcm))
                _waveOutProcessor.Write(pcm);
            
            await Task.Delay(1);
        }
    }
    
    /// <summary>
    /// レイテンシを計算する。
    /// </summary>
    /// <param name="sampleRate">Sampling rate.</param>
    /// <param name="bufferSize">Buffer size.</param>
    /// <returns>Calculate latency ms.</returns>
    private static float CalculateLatency(int sampleRate, int bufferSize)
    {
        return (float) bufferSize / sampleRate;
    }
    
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// 左右のモノラルサンプルをステレオサンプルに変換する
    /// </summary>
    /// <param name="stereo">Stereo samples buffer.</param>
    /// <param name="left">Left mono samples.</param>
    /// <param name="right">Right mono samples.</param>
    /// <param name="bufferSize">Buffer size.</param>
    /// <returns>Stereo samples with left and right mixed.</returns>
    private static void ConvertLRToStereo(IList<float> stereo, IReadOnlyList<float> left, IReadOnlyList<float> right, int bufferSize)
    {
        for (var i = 0; i < bufferSize; i++)
        {
            stereo[i * 2] = left[i];
            stereo[i * 2 + 1] = right[i];
        }
    }
    
    /// <summary>
    /// float のサンプルを byte の PCM サンプルに変換する
    /// </summary>
    /// <param name="pcm">PCM samples buffer.</param>
    /// <param name="samples">Mono or stereo samples.</param>
    /// <param name="samplesCount">Sample data length.</param>
    /// <returns>PCM samples.</returns>
    private static void ConvertFloatToPcmBytes(IList<byte> pcm, IReadOnlyList<float> samples, int samplesCount)
    {
        var sampleIndex = 0;
        var pcmIndex = 0;

        while (sampleIndex < samplesCount)
        {
            var outSample = (short)(samples[sampleIndex] * short.MaxValue);
            pcm[pcmIndex] = (byte)(outSample & 0xff);
            pcm[pcmIndex + 1] = (byte)((outSample >> 8) & 0xff);

            sampleIndex++;
            pcmIndex += 2;
        }
    }
    
    private static bool IsPcmExists(IEnumerable<byte> pcm)
    {
        return pcm.Any(x => x != 0);
    }
}