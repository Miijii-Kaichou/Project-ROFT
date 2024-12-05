[System.Serializable]
public class HoldObjectReader : ObjectReader
{

    public HoldObjectReader()
    {
        SetToType(NoteObj.NoteObjType.Hold);
    }

    public override void ReadHoldsFromFile()
    {
        base.ReadHoldsFromFile();
    }
}
