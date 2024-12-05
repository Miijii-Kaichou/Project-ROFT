using System.Collections;
using UnityEngine;

public class SongProgression : MonoBehaviour
{
    //This will be use to get different precentiles
    float firstNoteInSamples, lastNoteInSamples;

    //Check if the first note's been delivered
    public static bool isPassedFirstNote;

    //See if the last note was it
    public static bool isFinished;

    private void OnEnable()
    {
        StartCoroutine(ProgressionRoutine());
    }
    IEnumerator ProgressionRoutine()
    {
        while (true)
        {
            if (!RoftPlayer.IsNull() &&
            !MapReader.IsNull() &&
            !RoftPlayer.Record &&
            isFinished == false &&
            MapReader.GetName() != "")
                ShowProgression();

            yield return null;
        }
    }
    public static void ResetProgression()
    {
        isPassedFirstNote = false;
        isFinished = false;
        GameManager.Instance.GetSongProgressionFill().fillAmount = 0;
    }
    void ShowProgression()
    {
        try
        {
            //In order to know how much time we have until the first note,
            //we'll have to reference to the first note in our MapReader
            firstNoteInSamples = MapReader.noteObjs[0].GetInitialeSample();

            //Now we calculate the percentage with the music sample and the firstNoteInSamples
            float firstNotePercentile = RoftPlayer.musicSource.timeSamples / firstNoteInSamples;

            //Now we want to get the percentage between
            //Our current sample, and the last key sample in our song!!!
            int lastKey = MapReader.noteObjs.Count - 1;

            lastNoteInSamples = MapReader.noteObjs[lastKey].GetInitialeSample();

            //We get out percentage
            float lastNotePercentile =
                (RoftPlayer.musicSource.timeSamples - firstNoteInSamples) /
                (lastNoteInSamples - firstNoteInSamples);

            if (!isPassedFirstNote)
            {
                if (GameManager.Instance.GetSongProgressionFill() != null)
                    GameManager.Instance.GetSongProgressionFill().fillAmount = firstNotePercentile;

                if (firstNotePercentile > 0.99f)
                    isPassedFirstNote = true;
            }
            else
            {
                if (GameManager.Instance.GetSongProgressionFill() != null)
                    GameManager.Instance.GetSongProgressionFill().fillAmount = lastNotePercentile;
            }
            if (IsEndSong())
            {
                GameManager.inSong = false;
                //We should be at the end of the song at this point in time
                GameManager.DelayAction(3f, () =>
                {
                    RoftTransition.TransitionTo(7);
                });
            }
        }
        catch
        {

        }
    }

    bool IsEndSong()
    {
        //Now, if the fillAmount is full (or once we hit the last note), have RoftPlayer fade out
        isFinished = RoftPlayer.musicSource.timeSamples > lastNoteInSamples;
        return isFinished;
    }
}
