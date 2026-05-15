using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 100;

    void HandleHit(GameObject hitObject)
    {
        if (hitObject.CompareTag("Player") || hitObject.name == "PlayerCapsule")
        {
            return;
        }

        Debug.Log("Bullet hit: " + hitObject.name);

        // AnimalAI
        AnimalAI animalAI = hitObject.GetComponentInParent<AnimalAI>();
        if (animalAI != null)
        {
            animalAI.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Animal
        Animal animal = hitObject.GetComponentInParent<Animal>();
        if (animal != null)
        {
            animal.TakeDamage(damage);
            Debug.Log($"<color=red>HIT!</color> {animal.name} took {damage} damage.");
            Destroy(gameObject);
            return;
        }

        // Deer
        Deer deer = hitObject.GetComponentInParent<Deer>();
        if (deer != null)
        {
            deer.TakeDamage(damage);
            Debug.Log($"<color=green>HIT!</color> {deer.name} took {damage} damage.");
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject);
    }
}
