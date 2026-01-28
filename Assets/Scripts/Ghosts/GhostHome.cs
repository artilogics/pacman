using System.Collections;
using UnityEngine;

public class GhostHome : GhostBehavior
{
    public Transform inside;
    public Transform outside;

    private void OnEnable()
    {
        StopAllCoroutines();
    }

    private void OnDisable()
    {
        // Comprovar si està actiu per evitar errors en destruir l'objecte
        if (gameObject.activeInHierarchy) {
            StartCoroutine(ExitTransition());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Invertir direcció en xocar amb parets dins la casa, per fer efecte rebot
        if (enabled && collision.gameObject.layer == LayerMask.NameToLayer("Obstacle")) {
            ghost.movement.SetDirection(-ghost.movement.direction);
        }
    }

    private IEnumerator ExitTransition()
    {
        // Desactivar el moviment físic mentre s'anima manualment
        ghost.movement.SetDirection(Vector2.up, true);
        ghost.movement.rb.bodyType = RigidbodyType2D.Kinematic;
        ghost.movement.enabled = false;

        Vector3 position = transform.position;

        float duration = 0.5f;
        float elapsed = 0f;

        // Animar cap al punt d'inici
        while (elapsed < duration)
        {
            ghost.SetPosition(Vector3.Lerp(position, inside.position, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        // Animar la sortida de casa
        while (elapsed < duration)
        {
            ghost.SetPosition(Vector3.Lerp(inside.position, outside.position, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Escollir direcció a l'atzar (esquerra o dreta) i reactivar físiques
        ghost.movement.SetDirection(new Vector2(Random.value < 0.5f ? -1f : 1f, 0f), true);
        ghost.movement.rb.bodyType = RigidbodyType2D.Dynamic;
        ghost.movement.enabled = true;
    }

}
