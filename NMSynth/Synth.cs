using MeltySynth;

namespace NMSynth;

public class Synth
{
    private readonly Synthesizer _synthesizer;
    
    public Synth(int sampleRate, string soundFontPath)
    {
        _synthesizer = new Synthesizer(soundFontPath, sampleRate);
    }

    public void NoteOn()
    {
        _synthesizer.NoteOn(0, 60, 127);
    }
    
    public void NoteOff()
    {
        _synthesizer.NoteOff(0, 60);
    }
    
    public Synthesizer GetSynthesizer()
    {
        return _synthesizer;
    }
}