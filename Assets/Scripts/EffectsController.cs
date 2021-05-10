using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsController : MonoBehaviour
{
    private static EffectsController instance = null;
    public AudioSource crashAudio;
    public AudioSource engineAudio;

    private void Awake()
    {
        crashAudio.loop = false;

        if (instance == null)
        {
            instance = this;
            PlayEngineAudio();
            return;
        }
        if (instance == this) return;
        Destroy(gameObject);
    }


    public void PlayCrashAudio()
    {
        crashAudio.Play();
    }

    public void PlayEngineAudio()
    {
        engineAudio.Play();
    }
}
