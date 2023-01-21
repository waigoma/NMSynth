namespace NMSynth;

/// <summary>
/// LR の float サンプルを変換するメソッド群
/// </summary>
public static class Convert
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// 左右のモノラルサンプルをステレオサンプルに変換する
    /// </summary>
    /// <param name="stereo">Stereo samples buffer.</param>
    /// <param name="left">Left mono samples.</param>
    /// <param name="right">Right mono samples.</param>
    /// <param name="bufferSize">Buffer size.</param>
    /// <returns>Stereo samples with left and right mixed.</returns>
    public static void LRToStereo(IList<float> stereo, IReadOnlyList<float> left, IReadOnlyList<float> right, int bufferSize)
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
    public static void FloatToPcmBytes(IList<byte> pcm, IReadOnlyList<float> samples, int samplesCount)
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