using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
/// <summary>
/// Gestiona el moviment físic d'objectes (Pacman i fantasmes).
/// </summary>
public class Movement : MonoBehaviour
{
    public float speed = 8f;
    public float speedMultiplier = 1f;
    public Vector2 initialDirection;
    public LayerMask obstacleLayer;

    public Rigidbody2D rb { get; private set; }
    public Vector2 direction { get; private set; }
    public Vector2 nextDirection { get; private set; }
    public Vector3 startingPosition { get; private set; }

    private float lastTeleportTime; // Cooldown per evitar teleportació infinita

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        speedMultiplier = 1f;
        direction = initialDirection;
        nextDirection = Vector2.zero;
        transform.position = startingPosition;
        // isKinematic està obsolet. Fem servir bodyType per controlar si reacciona a físiques o no.
        rb.bodyType = RigidbodyType2D.Dynamic;
        enabled = true;
    }

    private void Update()
    {
        // Intenta moure's en la següent direcció si està en cua, per millorar la resposta
        if (nextDirection != Vector2.zero) {
            SetDirection(nextDirection);
        }
    }

    private void FixedUpdate()
    {
        Vector2 position = rb.position;
        Vector2 translation = speed * speedMultiplier * Time.fixedDeltaTime * direction;

        rb.MovePosition(position + translation);
    }

    public void Teleport(Vector3 position)
    {
        // Evitar teleportar-se si ja ho has fet fa menys de 0.5 segons
        if (Time.time - lastTeleportTime < 0.5f) return;

        lastTeleportTime = Time.time;
        rb.MovePosition(position);
        transform.position = position;
    }

    public void SetDirection(Vector2 direction, bool forced = false)
    {
        // Només canvia de direcció si no hi ha obstacles, o si estem forçant el moviment.
        // Si està ocupat, guarda la intenció (nextDirection) per quan sigui possible.
        if (forced || !Occupied(direction))
        {
            this.direction = direction;
            nextDirection = Vector2.zero;
        }
        else
        {
            nextDirection = direction;
        }
    }

    public bool Occupied(Vector2 direction)
    {
        // Llança un raig (BoxCast) per veure si xoquem amb paret (obstacleLayer)
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.75f, 0f, direction, 1.5f, obstacleLayer);
        return hit.collider != null;
    }

}
