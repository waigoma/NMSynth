namespace NMSynth;

public class Test
{
    private string _tmp = "test";
    private bool _isLoop = true;
    
    public void Run()
    {
        var _ = Loop();
        while (_isLoop)
        {
            Console.WriteLine(_tmp);
            _tmp = Console.ReadLine();

            if (_tmp == "exit")
                _isLoop = false;
        }
    }
    
    private async Task Loop()
    {
        while (_isLoop)
        {
            Console.WriteLine(_tmp);
            await Task.Delay(1000);
        }
    }
}