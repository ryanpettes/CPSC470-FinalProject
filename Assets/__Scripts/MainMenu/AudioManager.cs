using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    public float musicVolume;
    public float fadeDuration;
    
    public AudioClip mainMenuMusic;
    public AudioClip worldMapMusic;
    public AudioClip bossMusic;
    private AudioSource audioSource;

    void Awake()
    {
        // Check if there's already an AudioManager in the scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 

        audioSource = gameObject.AddComponent<AudioSource>();  // Add an AudioSource component
        audioSource.loop = true;  // Set it to loop
        audioSource.clip = mainMenuMusic;
        audioSource.volume = musicVolume;
        
        // Listener for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioClip newClip = null;
        
        // Change the music based on the scene name
        switch (scene.name)
        {
            case "Main Menu":
                newClip = mainMenuMusic;
                break;
            case "2D World Map":
                newClip = worldMapMusic;
                break;
            case "Boss Scene":
                newClip = bossMusic;
                break;
            // Catch-all audio source
            default:
                newClip = mainMenuMusic;
                break;
        }

        StartCoroutine(FadeOutAndIn(newClip));
    }
    
    private IEnumerator FadeOutAndIn(AudioClip newClip)
    {
        // Fade out the current music
        float startVolume = audioSource.volume;

        // Fade out (decrease the volume over time)
        float timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = 0;
        audioSource.Stop();  // Stop current music

        // Change to the new music and fade it in
        audioSource.clip = newClip;
        audioSource.Play();
        timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(0, musicVolume, timeElapsed / fadeDuration); // Fade in
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = musicVolume; // Ensure the volume is fully up at the end
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
