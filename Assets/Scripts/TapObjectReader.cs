[System.Serializable]

public class TapObjectReader : ObjectReader
{
    public TapObjectReader()
    {
        SetToType(NoteObj.NoteObjType.Tap);
    }

    public override void ReadTapsFromFile()
    {
        base.ReadTapsFromFile();
    }
}
