using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public LevelHandler levelHandler;

    private SpriteRenderer sprite;

    private BoxCollider2D collider;

    private AudioSource audioSource;
    [SerializeField] private AudioClip saveSound;

    private bool playerIsTouching = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    private void Awake()
    {
        
        sprite = GetComponentInChildren<SpriteRenderer>();
        collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = sprite.bounds.size;
        collider.isTrigger = true;
        levelHandler = GetComponentInParent<LevelHandler>();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerIsTouching)
        {
            levelHandler.SaveData();
            audioSource.PlayOneShot(saveSound);
        }
    }


    //bool is updated when entering and leaving collision with player to avoid trying to check for save inputs only on physics updates which are less frequent
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>()) playerIsTouching = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>()) playerIsTouching = false;
    }
}
