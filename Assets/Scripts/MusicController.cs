using System.Collections;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource musicPart1;
    public AudioSource musicPart2;

    private void Start()
    {
        if (musicPart1 == null || musicPart2 == null)
        {
            Debug.LogError("AudioSource components are missing!");
            return;
        }

        musicPart1.volume = 8.0f;
        musicPart2.volume = 8.0f;
        
        musicPart1.loop = true;
        musicPart1.Play();

    }

    public void StartMusic()
    {
        musicPart1.Play();
    }

    public void StartPart2()
    {
        musicPart2.loop = true;
        musicPart2.Play();

        musicPart1.Stop();
    }

    public void StopMusic()
    {
        if (musicPart1.isPlaying)
        {
            musicPart1.Stop();
        }

        if (musicPart2.isPlaying)
        {
            musicPart2.Stop();
        }
    }
}
