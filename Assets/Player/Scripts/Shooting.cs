using UnityEngine;

public class Shooting : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 15f;
    public float fireRate = 5f; // shots per second
    public int projectileLayer = 9;  // Default to layer 9; change if you use a different layer

    [Header("Aim")]
    public bool rotatePlayer = true;
    public Camera worldCamera;
    public int aimAngle; //for sprite rendering
    public bool isAiming;
    private int currDirec;
    float nextFireTime;

    public AudioSource audioSource;
    [SerializeField] AudioClip beamSound;   //placeholders
    [SerializeField] AudioClip artilerySound;

    void Reset()
    {
        worldCamera = Camera.main;
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = Vector3.right * 0.6f;
            firePoint = fp.transform;
        }
    }

    void Start()
    {
        if (worldCamera == null) worldCamera = Camera.main;
        
        // Configure Physics2D to prevent projectiles from colliding with each other
        Physics2D.IgnoreLayerCollision(projectileLayer, projectileLayer, true);
        
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = Vector3.right * 1.2f;  // Increased distance to avoid overlap
            fp.transform.localRotation = Quaternion.identity;
            // Ensure FirePoint has no collider so projectiles don't spawn inside a collider
            if (fp.GetComponent<Collider2D>() != null)
                DestroyImmediate(fp.GetComponent<Collider2D>());
            firePoint = fp.transform;
        }
    }

    void Update()
    {
        AimTowardsDirection();

        if ((Input.GetButton("Fire1")) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / Mathf.Max(0.0001f, fireRate);
        }
    }

    void AimTowardsDirection()
    {
        if (worldCamera == null) return;
        //changed aiming direction to be tied to arrow keys and right joystick
        Vector2 dir;
        if (Input.GetAxisRaw("Aim X") == 0 && Input.GetAxisRaw("Aim Y") == 0)
        {
            dir = new Vector2(Input.GetAxisRaw("Aim Arrow X"), Input.GetAxisRaw("Aim Arrow Y"));
        }
        else
        {
            dir = new Vector2(Input.GetAxisRaw("Aim X"), Input.GetAxisRaw("Aim Y"));
        }
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if(dir.magnitude == 0)
            isAiming = false;
        else isAiming = true;

        if (dir.x == 0f && dir.y == 0f)
        {
            if(Input.GetAxisRaw("Horizontal") > 0)
                currDirec = 0;
            if (Input.GetAxisRaw("Horizontal") < 0)
                currDirec = 180;
            angle = currDirec;
        }
        
        // Snap to nearest 45 degrees
        angle = Mathf.Round(angle / 45f) * 45f;
        aimAngle = (int)angle;

        if (rotatePlayer)
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;
        
        // Fire in the direction the player is currently facing (snapped to 45 degrees)
        Vector2 dir = (Vector2)transform.right;  // Use player's rotated direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0f, 0f, angle));
        
        // Set projectile layer so it doesn't collide with other projectiles
        proj.layer = projectileLayer;
        
        //Debug.Log("Projectile spawned at: " + firePoint.position + " with velocity direction: " + dir);
        
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * projectileSpeed;
            //Debug.Log("Projectile velocity set to: " + rb.linearVelocity);
        }
        else
        {
            Debug.LogWarning("Projectile prefab has no Rigidbody2D!");
        }

        // Prevent the projectile from colliding with the player that fired it
        Collider2D[] projCols = proj.GetComponents<Collider2D>();
        Collider2D[] myCols = GetComponentsInChildren<Collider2D>();
        int ignoreCount = 0;
        foreach (var pcol in projCols)
        {
            if (pcol == null) continue;
            foreach (var mcol in myCols)
            {
                if (mcol == null) continue;
                Physics2D.IgnoreCollision(pcol, mcol);
                ignoreCount++;
            }
        }
        if (ignoreCount > 0)
            Debug.Log("Ignored " + ignoreCount + " collisions between projectile and player");
        else
            Debug.LogWarning("No collisions were ignored! Check projectile prefab has a Collider2D component.");
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(firePoint.position, firePoint.position + (transform.right * 0.5f));
        Gizmos.DrawWireSphere(firePoint.position, 0.05f);
    }
}
