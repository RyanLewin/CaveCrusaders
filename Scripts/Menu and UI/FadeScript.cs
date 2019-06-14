using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScript : MonoBehaviour
{
    public static FadeScript instance;
    
    public Animator _fadeAnim;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        _fadeAnim = GetComponent<Animator>();
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        gameObject.SetActive(true);
        _fadeAnim.Play("Fade In Anim");
        yield return new WaitForSecondsRealtime(0.8f);
        gameObject.SetActive(false);
    }

    public IEnumerator FadeOut()
    {
        gameObject.SetActive(true);
        _fadeAnim.Play("Fade Out Anim");
        
        yield return new WaitForSecondsRealtime(0.8f);
        //gameObject.SetActive(false);
    }
}
