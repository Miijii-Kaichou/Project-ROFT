using UnityEngine;
using UnityEngine.UI;

public class AppearEffect : CloseInEffect
{
    public KeyCode assignedKeyBind;

    //public SpriteRenderer sprite;
    public SpriteRenderer[] overlaySprites;
    public SpriteRenderer childSprite;

    public GameObject assignedCircle;

    Color originalOverlayAppearance;

    float time;

    void Awake()
    {
        Instance = this;

        //Get sprite renderers
        image = GetComponent<Image>();
        overlaySprites = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer overlaySprite in overlaySprites)
        {
            if (overlaySprite != image)
                childSprite = overlaySprite;
        }
    }

    private void OnEnable()
    {
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        foreach (SpriteRenderer overlaySprite in overlaySprites)
        {
            if (overlaySprite != image)
                childSprite = overlaySprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!RoftPlayer.Record)
            AppearOn();
    }

    void AppearOn()
    {

    }

    protected override float GetPercentage()
    {
        percentage = ((RoftPlayer.musicSource.timeSamples) - noteSpawnOffset) / (initiatedNoteSample - noteSpawnOffset);
        return percentage;
    }

    private void OnDisable()
    {
        Dump();
    }

    /// <summary>
    /// Reset all values to there defaults
    /// </summary>
    void Dump()
    {
        image.color = originalAppearance;

        if (childSprite != null)

            childSprite.color = originalOverlayAppearance;
        percentage = 0;
        assignedKeyBind = KeyCode.None;
        initiatedNoteSample = 0;
        initiatedNoteOffset = 0;
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        if (assignedCircle != null)
            assignedCircle = null;
    }

    void DelayDestroy(float _duration)
    {

        time += Time.deltaTime;
        if (time > _duration)
        {
            time = 0;
            gameObject.SetActive(false);
        }
    }
}