[System.Serializable]
public class Octave
{
    public Octave(float freq, float amp)
    {
        Frequency = freq;
        Amplitude = amp;
    }
    
    public float Frequency;
    public float Amplitude;
}
