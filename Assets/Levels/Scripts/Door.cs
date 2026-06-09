using Unity.VisualScripting;
using UnityEngine;



public class Door : MonoBehaviour
{
    [HideInInspector] LevelHandler levelHandler;

    public Level parentLevel;

    public Door linkedDoor;

    private SpriteRenderer sprite;

    private BoxCollider2D doorCollider;

    public Transform exitPoint;

    private Level targetLevel;

    
    public void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        parentLevel = GetComponentInParent<Level>(true);
        //match collider to sprite size
        if (sprite)
        {
            doorCollider = gameObject.AddComponent<BoxCollider2D>();
            doorCollider.size = sprite.bounds.size * 0.9f;
        }
        
        levelHandler = GetComponentInParent<LevelHandler>();
    }
    //make sure parentLevel is assigned
    public void OnDoorEnter()
    {
        if (!parentLevel) parentLevel = GetComponentInParent<Level>(true);
    }

    //get the target level
    private void TransitionToLinkedDoor()
    {
        //parent level of the linked door
        targetLevel = linkedDoor.GetComponentInParent<Level>(true);
        
        linkedDoor.OnDoorEnter();

        //if we are traveling to the same level dont bother deactivating and resetting
        if (parentLevel != targetLevel)
        {
            levelHandler.DeactivateLevel(parentLevel);
            levelHandler.ActivateLevel(targetLevel);

            //clamp camera to new level for 1 frame, avoids camera jitter
            levelHandler.cam.clampCameraPosition(targetLevel.minMaxCameraX, targetLevel.minMaxCameraY);

        }
        //teleport player and camera
        levelHandler.player.transform.position = linkedDoor.exitPoint.position + new Vector3(0, 0.005f, 0);
        levelHandler.cam.setVelocity(Vector2.zero);
        levelHandler.cam.transform.position = linkedDoor.exitPoint.position;
        
    }

    //if we enter a collision with the player, transition us to the linked scene/door
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>())
        {
            TransitionToLinkedDoor();
        }
    }
}