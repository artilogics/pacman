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

    [SerializeField] private Ghost[] ghosts;
    [SerializeField] private Pacman pacman;
    [SerializeField] private Transform pellets;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;

    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;

    private int ghostMultiplier = 1;

    private void Awake()
    {
        // Implementació del patró Singleton
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
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

        // Assenyalar tots els punts com a actius de nou
        foreach (Transform pellet in pellets) {
            pellet.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {
        // Reiniciar les posicions de tots els fantasmes
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].ResetState();
        }

        // Reiniciar la posició del Pacman
        pacman.ResetState();
    }

    private void GameOver()
    {
        // Mostrar text de Game Over
        gameOverText.enabled = true;

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

        // Restar vida
        SetLives(lives - 1);

        // Si queden vides, reiniciar posicions després de 3 segons. Si no, Game Over.
        if (lives > 0) {
            Invoke(nameof(ResetState), 3f);
        } else {
            GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        // Calcular punts amb el multiplicador actual
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);

        // Incrementar multiplicador per si es mengen més fantasmes a la vegada
        ghostMultiplier++;
    }

    public void PelletEaten(Pellet pellet)
    {
        // Desactivar el punt menjat
        pellet.gameObject.SetActive(false);

        // Sumar puntuació
        SetScore(score + pellet.points);

        // Comprovar si queden punts, si no, es guanya la ronda
        if (!HasRemainingPellets())
        {
            pacman.gameObject.SetActive(false);
            Invoke(nameof(NewRound), 3f);
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
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
