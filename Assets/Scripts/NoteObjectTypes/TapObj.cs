[System.Serializable]
public class TapObj : NoteObj
{
    public TapObj(uint initialKey, long initialSample) : base()
    {
        InitialKey = initialKey;
        InitialSample = initialSample;
        Type = NoteObjType.Tap;
    }

    public override string AsString() => $"{InitialKey},{InitialSample},{(int)Type},{PatternSetValue},{TickValue},{LayerValue}";

    public override bool Empty() => base.Empty();
    public override void Clear() => base.Clear();
}
