[System.Serializable]
public class BurstObjectReader : ObjectReader
{
    public BurstObjectReader()
    {
        SetToType(NoteObj.NoteObjType.Burst);
    }

    public override void ReadSlidesFromFile()
    {
        
        base.ReadSlidesFromFile();
    }
}
