using UnityEngine;
using System.Collections;

public class AudioPlayer : MonoBehaviour
{
    [Header("Sources")]
    public AudioSource musicSource;   
    public AudioSource sfxSource;     

    [Header("BGM Clips")]
    public AudioClip introClip;
    public AudioClip normalClip;

    void Start()
    {
       
        if (introClip != null && musicSource != null)
        {
            musicSource.loop = false;
            musicSource.clip = introClip;
            musicSource.Play();
            StartCoroutine(SwitchToNormal());
        }
        else
        {
            
            PlayNormalLoop();
        }
    }

    IEnumerator SwitchToNormal()
    {
        
        float wait = (introClip != null) ? Mathf.Min(introClip.length, 3f) : 0f;
        yield return new WaitForSeconds(wait);
        PlayNormalLoop();
    }

    void PlayNormalLoop()
    {
        if (normalClip == null || musicSource == null) return;
        musicSource.clip = normalClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    
}
