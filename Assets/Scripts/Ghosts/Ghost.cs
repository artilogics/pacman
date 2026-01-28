using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(Movement))]
/// <summary>
/// Classe base per als fantasmes. Gestiona els diferents comportaments (perseguir, dispersar-se, espantar-se) i les col·lisions.
/// </summary>
public class Ghost : MonoBehaviour
{
    public Movement movement { get; private set; }
    public GhostHome home { get; private set; }
    public GhostScatter scatter { get; private set; }
    public GhostChase chase { get; private set; }
    public GhostFrightened frightened { get; private set; }
    public GhostBehavior initialBehavior;
    public Transform target;
    public int points = 200;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        home = GetComponent<GhostHome>();
        scatter = GetComponent<GhostScatter>();
        chase = GetComponent<GhostChase>();
        frightened = GetComponent<GhostFrightened>();
    }

    private void Start()
    {
        ResetState();
    }

    /// <summary>
    /// Restableix l'estat inicial del fantasma i els seus comportaments.
    /// </summary>
    public void ResetState()
    {
        gameObject.SetActive(true);
        movement.ResetState();

        frightened.Disable();
        chase.Disable();
        scatter.Enable();

        if (home != initialBehavior) {
            home.Disable();
        }

        if (initialBehavior != null) {
            initialBehavior.Enable();
        }
    }

    public void SetPosition(Vector3 position)
    {
        // Mantenir la posició Z igual ja que determina la profunditat de dibuix
        position.z = transform.position.z;
        transform.position = position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Gestionar col·lisió amb el Pacman
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            if (frightened.enabled) {
                GameManager.Instance.GhostEaten(this);
            } else {
                GameManager.Instance.PacmanEaten();
            }
        }
    }

}
