using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Passage : MonoBehaviour
{
    public Transform connection;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Vector3 position = connection.position;
        position.z = other.transform.position.z;

        Movement movement = other.GetComponent<Movement>();
        if (movement != null) {
            movement.Teleport(position);
        } else {
            // Per objectes sense script de moviment, fer teleport simple
            other.transform.position = position;
        }
    }

}
