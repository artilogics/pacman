using System.Collections;
using System.Linq; // Necessari per OrderBy
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
/// <summary>
/// Gestiona l'estat global del joc, com la puntuació, vides i cicles de partida.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton: Assegura que només hi hagi una instància de GameManager.
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Pacman pacman;
    [SerializeField] private Ghost[] ghosts;
    [SerializeField] private Transform pellets;

    [Header("Setup & Audio")]
    // [SerializeField] private AudioSource introAudio; // Ara gestionat per AudioManager
    [SerializeField] private Text readyText;

    [Header("UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text gameOverText;

    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;
    public bool IsStartupSequence { get; private set; } // Flag global per bloquejar comportaments

    private int ghostMultiplier = 1;

    private void Awake()
    {
        // Implementació del patró Singleton
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }

        // Auto-assignació de referències per robustesa/educatiu
        // Sobreescrivim sempre per assegurar que NO ens deixem cap fantasma i els ordenem per la jerarquia
        ghosts = FindObjectsOfType<Ghost>().OrderBy(g => g.transform.GetSiblingIndex()).ToArray();
        
        if (pacman == null) {
            pacman = FindObjectOfType<Pacman>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        NewGame();
    }

    private void Update()
    {
        // Reiniciar el joc si no queden vides i es prem qualsevol tecla
        if (lives <= 0 && Input.anyKeyDown) {
            NewGame();
        }
    }

    private void NewGame()
    {
        // Reiniciar puntuació i vides per començar una nova partida
        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        // Preparar una nova ronda
        gameOverText.enabled = false;
        // Assegurar-nos que readyText comença desactivat per si de cas, tot i que la corrutina el gestionarà
        readyText.enabled = false;

        // Assenyalar tots els punts com a actius de nou
        foreach (Transform pellet in pellets) {
            pellet.gameObject.SetActive(true);
        }

        ResetState(true);
    }

    private void ResetStateRespawn()
    {
        ResetState(false);
    }

    private void ResetState(bool playMusic)
    {
        // Reiniciar les posicions de tots els fantasmes
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].ResetState();
        }

        // Reiniciar la posició del Pacman
        pacman.ResetState();

        // Iniciar la seqüència de 'Ready'
        StartCoroutine(ReadySequence(playMusic));
    }

    private IEnumerator ReadySequence(bool playMusic)
    {
        IsStartupSequence = true;

        // Aturar els fantasmes i el Pacman temporalment
        // Utilitzem simulated = false per assegurar que la física no els mou ni un frame
        pacman.movement.rb.simulated = false;
        pacman.movement.enabled = false;
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].movement.rb.simulated = false;
            ghosts[i].movement.enabled = false;
        }

        // Mostrar text "READY!"
        readyText.enabled = true;

        if (playMusic) {
            // Reproduir música d'intro si està assignada a l'AudioManager
            AudioManager.Instance.PlayIntro();
            
            // Esperem el temps del clip si existeix, o un de per defecte.
            if (AudioManager.Instance.introClip != null) {
                yield return new WaitForSeconds(AudioManager.Instance.introClip.length);
            } else {
                yield return new WaitForSeconds(3f);
            }
        } else {
            // Si és un respawn, només esperem una mica (ex: 2 segons) sense música
            yield return new WaitForSeconds(2f);
        }

        // Amagar text i reactivar el moviment
        readyText.enabled = false;

        pacman.movement.rb.simulated = true;
        pacman.movement.enabled = true;
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].movement.rb.simulated = true;
            ghosts[i].movement.enabled = true;
        }

        IsStartupSequence = false;
    }

    private void GameOver()
    {
        // Mostrar text de Game Over
        gameOverText.enabled = true;

        AudioManager.Instance.PlayGameOver();

        // Desactivar fantasmes i Pacman
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].gameObject.SetActive(false);
        }

        pacman.gameObject.SetActive(false);
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(2, '0');
    }

    public void PacmanEaten()
    {
        // Executar animació de mort
        pacman.DeathSequence();
        
        AudioManager.Instance.PlayPacmanDeath();

        // Restar vida
        SetLives(lives - 1);

        // Si queden vides, reiniciar posicions després de 3 segons. Si no, Game Over.
        if (lives > 0) {
            Invoke(nameof(ResetStateRespawn), 3f);
        } else {
            StartCoroutine(ProcessGameOver());
        }
    }

    private IEnumerator ProcessGameOver()
    {
        // Esperem un temps prudent (ex: 2 segons de la mort del pacman)
        yield return new WaitForSeconds(2f);
        
        GameOver();
    }

    public void GhostEaten(Ghost ghost)
    {
        AudioManager.Instance.PlayGhostEaten();

        // Calcular punts amb el multiplicador actual
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);

        // Incrementar multiplicador per si es mengen més fantasmes a la vegada
        ghostMultiplier++;
    }

    public void PelletEaten(Pellet pellet)
    {
        AudioManager.Instance.PlayMunch();

        // Desactivar el punt menjat
        pellet.gameObject.SetActive(false);

        // Sumar puntuació
        SetScore(score + pellet.points);

        // Comprovar si queden punts, si no, es guanya la ronda
        if (!HasRemainingPellets())
        {
            // Aturem el moviment del Pacman però el deixem visible
            pacman.gameObject.SetActive(true); // Per si de cas
            pacman.movement.enabled = false;
            pacman.movement.rb.simulated = false;

            // Amaguem els fantasmes
            for (int i = 0; i < ghosts.Length; i++) {
                ghosts[i].gameObject.SetActive(false);
            }

            // Repuim música de victòria
            AudioManager.Instance.PlayLevelComplete();

            // Esperem 3 segons abans de reiniciar
            Invoke(nameof(NewRound), 3f);
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        AudioManager.Instance.PlayPowerPellet();

        // Activar l'estat espantat a tots els fantasmes
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].frightened.Enable(pellet.duration);
        }

        // També suma punts com un pellet normal
        PelletEaten(pellet);
        
        // Reiniciar el multiplicador de fantasmes quan s'acabi l'efecte
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        // Comprova si queda algun punt actiu en l'escena
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf) {
                return true;
            }
        }

        return false;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }

}
