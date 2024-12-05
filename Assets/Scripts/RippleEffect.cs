using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RippleEffect : MonoBehaviour
{

    /*RippleEffect will be responsible for after you hit a note.
     Really easy. Just have a rate, a percentage value, and slap that
     on the localScale and alpha.*/

    [SerializeField, Tooltip("The rate speed of the ripple effect")]
    private float spreadSpeed = 0.1f;

    [SerializeField, Tooltip("The alpha change rate of the ripple effect")]
    private float deltaOpacity = 0.1f;

    //GameObject Transform
    Transform objTransform;

    //Size of ripple effect
    float size = DEFAULT_VALUE; //One is default scale

    //Opacity of ripple effect
    float opacity = DEFAULT_VALUE; //One is default opacity

    const float DEFAULT_VALUE = 1f;

    //Sprite Render
    Image spriteRenderer;

    void Awake()
    {
        objTransform = transform;
        spriteRenderer = GetComponent<Image>();
    }

    // Start is called before the first frame update
    public void DoRippleEffect(Color color)
    {
        spriteRenderer.color = color;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (true)
        {
            if (!GameManager.Instance.IsGamePaused)
            {
                //We want the object to set itself to inactive one the opacity is 0 (transparent)
                //We'll going between value 1 and 0 on the color alpha channel
                //There is no limit to the size of the ripple.

                //Size is incremented by spreadRate
                size += spreadSpeed;

                //Then is added to the localScale
                objTransform.localScale = new Vector3(size, size, DEFAULT_VALUE);

                //Opacity will be subtracted from OpacityDelat
                opacity -= deltaOpacity;

                //Add opacity to sprite alpha channel
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Log(opacity - 0.25f));

                //Now we check if the opacity is 0
                if (opacity <= 0f)
                    //Disable the object
                    gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(1f / 60f);
        }
    }

    private void OnDisable()
    {
        //Stop any coroutines from running
        StopAllCoroutines();

        //Return to default values
        size = DEFAULT_VALUE;
        opacity = DEFAULT_VALUE;
    }
}
