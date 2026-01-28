using UnityEngine;

[DefaultExecutionOrder(-100)] // Executar abans que el GameManager
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip introClip;
    public AudioClip munchClip;
    public AudioClip powerPelletClip;
    public AudioClip ghostEatenClip;
    public AudioClip pacmanDeathClip;
    public AudioClip gameOverClip;
    public AudioClip levelCompleteClip;

    private void Awake()
    {
        // Implementació del patró Singleton
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: mantenir entre escenes
        }
    }

    public void PlayIntro()
    {
        if (introClip != null)
        {
            musicSource.Stop();
            musicSource.clip = introClip;
            musicSource.Play();
        }
    }

    public void PlayMunch()
    {
        // Utilitzem PlayOneShot per permetre superposició ràpida si cal
        if (munchClip != null && !sfxSource.isPlaying) // Evitem saturació si ja sona
        {
            sfxSource.PlayOneShot(munchClip);
        }
    }

    public void PlayPowerPellet()
    {
        if (powerPelletClip != null)
        {
            // Això podria ser un efecte que sona un cop, o una música de fons
            // Assumim efecte d'activació per ara
            sfxSource.PlayOneShot(powerPelletClip);
        }
    }

    public void PlayGhostEaten()
    {
        if (ghostEatenClip != null)
        {
            sfxSource.PlayOneShot(ghostEatenClip);
        }
    }

    public void PlayPacmanDeath()
    {
        if (pacmanDeathClip != null)
        {
            musicSource.Stop(); // Aturar música de fons si n'hi hagués
            sfxSource.PlayOneShot(pacmanDeathClip);
        }
    }

    public void PlayGameOver()
    {
        if (gameOverClip != null)
        {
            musicSource.Stop();
            sfxSource.PlayOneShot(gameOverClip);
        }
    }

    public void PlayLevelComplete()
    {
        if (levelCompleteClip != null)
        {
            musicSource.Stop();
            sfxSource.PlayOneShot(levelCompleteClip);
        }
    }
}
