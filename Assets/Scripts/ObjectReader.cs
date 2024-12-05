using System;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public abstract class ObjectReader : MonoBehaviour
{
    [SerializeField]
    public List<NoteObj> objects  = new List<NoteObj>();

    [SerializeField]
    public long sequencePos { get; set; } = 0;

    public NoteObj.NoteObjType readerType { get; set; }

    public static string ObjectNotation { get; protected set; } 

    private const uint reset = 0;

    private float noteAlignmentOffset = 0;

    //This will be responsible for reading different types of the same file
    public virtual void ReadTapsFromFile()
    {

    }

    public virtual void ReadHoldsFromFile()
    {

    }

    public virtual void ReadSlidesFromFile()
    {

    }

    public virtual void ReadClicksFromFile()
    {

    }

    public virtual void ReadTrailsFromFile()
    {

    }

    public virtual long GetSequencePosition() => sequencePos;


    public virtual void SequencePositionReset()
    {
        sequencePos = reset;
    }

    public virtual GameObject GetTypeFromPool(ObjectPooler _pooler)
    {
        switch (readerType)
        {
            case NoteObj.NoteObjType.Tap:
                return _pooler.GetMember("Approach Circle");
            case NoteObj.NoteObjType.Hold:
                return _pooler.GetMember("Approach Circle");
            case NoteObj.NoteObjType.Burst:
                return _pooler.GetMember("Approach Circle");
            default:
                return null;
        }
    }

    public virtual void Next()
    {
        sequencePos++;
    }

    public virtual void Previous()
    {
        sequencePos--;
    }

    public virtual void SetToType(NoteObj.NoteObjType type)
    {
        readerType = type;
    }

    protected virtual bool IsNoteIncoming()
    {
        return default;
    }

    protected virtual void SpawnNoteObject()
    {

    }

    protected virtual void EnableTapObject()
    {

    }

    protected virtual void EnableHoldObject()
    {

    }

    protected virtual void EnableBurstObject()
    {

    }
}
