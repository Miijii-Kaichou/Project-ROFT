using System.Collections;
using UnityEngine;

public class AccuracySign : MonoBehaviour
{
    [SerializeField] private Sprite[] signs = new Sprite[5];

    private SpriteRenderer spriteRender;


    private const float showDuration = 10f;

    //AccuracySignUpdate()
    public IEnumerator accuracySignRoutine;

    // Start is called before the first frame update
    void Awake()
    {
        spriteRender = GetComponent<SpriteRenderer>();
    }

    //The image/sprite used to show accuracy
    public void ShowSign(int index)
    {
        spriteRender.sprite = signs[index];
        StartCoroutine(Run(0.075f));
    }

    IEnumerator Run(float _rate)
    {
        float opacity = 1f;
        while (true)
        {
            if (!GameManager.Instance.IsGamePaused)
            {
                opacity -= _rate;
                spriteRender.color = new Color(spriteRender.color.r, spriteRender.color.g, spriteRender.color.b, opacity);

                if (opacity <= 0f)
                {
                    gameObject.SetActive(false);
                    opacity = 1f;
                }

            }
            yield return new WaitForSeconds(1f / 100f);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
