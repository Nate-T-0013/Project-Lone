using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 5;
    //changed to show in inspector for debugging
    [SerializeField] private int currentHealth;
    //changed to reload on death
    LevelHandler lh;

    void Start()
    {
        lh = GameObject.Find("LevelHandler").GetComponent<LevelHandler>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " took damage. Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died");
        lh.LoadData();
        //changed to not destroy gameobject and instead reload
    }

    public void resetHealth()
    {
        currentHealth = maxHealth;
    }


}
