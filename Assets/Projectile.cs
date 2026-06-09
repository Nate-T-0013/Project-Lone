using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    void Start()
    {
        // Disable collisions for one frame to prevent spawning inside player
        StartCoroutine(EnableCollisionsNextFrame());
    }

    IEnumerator EnableCollisionsNextFrame()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        yield return null;  // Wait one frame
        if (col != null) col.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 6)     //dummy logic to prevent the player shooting themselves. WIP
            return;
        //Debug.Log("Projectile hit (trigger): " + collision.gameObject.name);
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6)    //dummy logic to prevent the player shooting themselves. WIP
            return;
        //Debug.Log("Projectile hit (collision): " + collision.gameObject.name);
        Destroy(gameObject);
    }
}
