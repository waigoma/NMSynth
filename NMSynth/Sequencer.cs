using System.Diagnostics;
using MeltySynth;

namespace NMSynth;

/// <summary>
/// リアルタイム再生を実現させるためのシーケンサ
/// <br/>
/// バッファサイズとサンプリングレートからバッファの再生時間を求め、
/// その時間ごとにずらしてバッファを書き出す。
/// </summary>
public class Sequencer
{
    /// <summary>
    /// シーケンサが起動しているかどうか
    /// </summary>
    public bool IsOpen { get; private set; }
    
    // 音の生成と再生に必要なもの
    private readonly WaveOutProcessor _waveOutProcessor;
    private readonly Synthesizer _synthesizer;
    private readonly Settings _settings;
    
    // バッファ
    private readonly Queue<byte[]> _bufferQueue = new ();
    private readonly float[] _leftBuffer;
    private readonly float[] _rightBuffer;
    private readonly float[] _stereoBuffer;
    private readonly byte[] _pcmBuffer;
    
    // 他スレッド
    private Task? _sequenceTask;
    private Task? _enqueueTask;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="settings">設定</param>
    /// <param name="synthesizer">音を生成するシンセサイザ</param>
    /// <param name="waveOutProcessor">音を出力する先</param>
    public Sequencer(Settings settings, Synthesizer synthesizer, WaveOutProcessor waveOutProcessor)
    {
        _waveOutProcessor = waveOutProcessor;
        _synthesizer = synthesizer;
        _settings = settings;
        
        // Initialize buffers
        _leftBuffer = new float[settings.BufferSize];
        _rightBuffer = new float[settings.BufferSize];
        _stereoBuffer = new float[_leftBuffer.Length + _rightBuffer.Length];
        _pcmBuffer = new byte[_stereoBuffer.Length * 2];
    }

    /// <summary>
    /// シーケンサを起動する
    /// </summary>
    public void Open()
    {
        IsOpen = true;
        _enqueueTask = Task.Run(EnqueueSampleAsync);
        _sequenceTask = Task.Run(SequenceAsync);
    }

    /// <summary>
    /// シーケンサを停止する
    /// </summary>
    public void Close()
    {
        IsOpen = false;
        _enqueueTask?.Wait();
        _sequenceTask?.Wait();
    }

    /// <summary>
    /// シーケンサ本体
    /// <br/>
    /// stopwatch でレイテンシを計算する。
    /// </summary>
    private async Task SequenceAsync()
    {
        var stopwatch = new Stopwatch();
        
        // latency seconds
        var latencySec = CalculateLatency();
        // latency nano seconds
        var latencyNs = (long)(latencySec * 1_000_000_000);
        
        // start time
        stopwatch.Start();
        var start = stopwatch.ElapsedTicks * 100;
        
        while (IsOpen)
        {
            // if the elapsed time is less than the latency, wait
            if (stopwatch.ElapsedTicks * 100 - start < latencyNs)
                continue;

            // reset start time
            stopwatch.Restart();
            start = stopwatch.ElapsedTicks;
            
            // Write to WaveOut.
            if (_bufferQueue.TryDequeue(out var buffer))
                _waveOutProcessor.Write(buffer);
        }
        
        stopwatch.Stop();
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// バッファキューに設定分のサンプルを追加する
    /// </summary>
    private async Task EnqueueSampleAsync()
    {
        while (IsOpen)
        {
            // if the buffer queue is full, wait
            if (_bufferQueue.Count >= _settings.BufferQueueSize)
                continue;
            
            // Add samples to the buffer queue.
            await EnqueueSampleToQueue();
        }
    }
    
    /// <summary>
    /// キューに追加するデータを作成する
    /// </summary>
    private async Task EnqueueSampleToQueue()
    {
        lock (_leftBuffer)
        lock (_rightBuffer)
        lock (_stereoBuffer)
        lock (_pcmBuffer)
        {
            // Render the next block.
            _synthesizer.Render(_leftBuffer, _rightBuffer);

            // Convert to Stereo float array.
            ConvertLRToStereo(_stereoBuffer, _leftBuffer, _rightBuffer, _settings.BufferSize);

            // Convert to byte array.
            ConvertFloatToPcmBytes(_pcmBuffer, _stereoBuffer, _stereoBuffer.Length);

            // Add to queue.
            var buffer = new byte[_pcmBuffer.Length];
            Array.Copy(_pcmBuffer, buffer, _pcmBuffer.Length);
            _bufferQueue.Enqueue(buffer);
        }
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// レイテンシを計算する。
    /// </summary>
    /// <returns>Calculated latency ms.</returns>
    private float CalculateLatency() => (float) _settings.BufferSize / _settings.SampleRate;
    
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
}