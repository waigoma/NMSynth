using MeltySynth;

namespace NMSynth;

public class Sequencer
{
    private const int SampleRate = 44100;
    private readonly Synthesizer _synthesizer = new ("soundfont/Equinox_Grand_Pianos.sf2", SampleRate);
    
    public void Run()
    {
        // The length of a block is 0.1 sec.
        var blockSize = SampleRate / 10;

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