[System.Serializable]
public class HoldObj : NoteObj
{
    //Miscellaneous data for Hold Type
    protected long FinalSample = -1;



    public HoldObj(uint initialKey, long initialSample, long finalSample) : base()
    {
        InitialSample = initialKey;
        InitialSample = initialSample;
        FinalSample = finalSample;
        Type = NoteObjType.Hold;
    }

    public override string AsString() => $"{InitialKey},{InitialSample},{(int)Type},{FinalSample},{PatternSetValue},{TickValue},{LayerValue}";

    public override bool Empty() =>
        (InitialKey == 0 && InitialSample == 0 && Type == default && FinalSample == 0);

    public override void Clear()
    {
        base.Clear();
        FinalSample = 0;
    }

    public void SetFinalSample(long value) => FinalSample = value; 
    public long GetFinalSample() => FinalSample;
}
