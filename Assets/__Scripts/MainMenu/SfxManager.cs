using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance;

    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
    }

    public void PlaySoundEffect(AudioClip sound)
    {
        sfxSource.PlayOneShot(sound);
    }
}

