using MeltySynth;

namespace NMSynth;

public class Sequencer
{
    private readonly WaveOutProcessor _waveOutProcessor;
    private readonly Synthesizer _synthesizer;
    private readonly int _sampleRate;
    
    private bool _isOpen;
    private Task? _realtimeTask;
    
    public Sequencer(int sampleRate, string soundFontPath, WaveOutProcessor waveOutProcessor)
    {
        _waveOutProcessor = waveOutProcessor;
        _synthesizer = new Synthesizer(soundFontPath, sampleRate);
        _sampleRate = sampleRate;
    }

    public void NoteOn()
    {
        _synthesizer.NoteOn(0, 60, 127);
    }
    
    public void NoteOff()
    {
        _synthesizer.NoteOff(0, 60);
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
            _waveOutProcessor.Write(pcm);
            
            await Task.Delay((int) (CalculateLatency(_sampleRate, bufferSize) * 1000));
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

    public void Run()
    {
        // The length of a block is 0.1 sec.
        var blockSize = _sampleRate / 10;

        // The entire output is 3 sec.
        var blockCount = 30;

        // Define the melody.
        // A single row indicates the start timing, end timing, and pitch.
        var data = new []
        {
            new [] {  5, 10, 60 },
            new [] { 10, 15, 64 },
            new [] { 15, 25, 67 }
        };

        // The output buffer.
        var left = new float[blockSize * blockCount];
        var right = new float[blockSize * blockCount];

        for (var t = 0; t < blockCount; t++)
        {
            // Process the melody.
            foreach (var row in data)
            {
                if (t == row[0]) _synthesizer.NoteOn(0, row[2], 100);
                if (t == row[1]) _synthesizer.NoteOff(0, row[2]);
            }

            // Render the block.
            var blockLeft = left.AsSpan(blockSize * t, blockSize);
            var blockRight = right.AsSpan(blockSize * t, blockSize);
            _synthesizer.Render(blockLeft, blockRight);
        }
    }
}