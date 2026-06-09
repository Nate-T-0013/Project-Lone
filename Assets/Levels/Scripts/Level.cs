using UnityEngine;


public class Level : MonoBehaviour
{


    [HideInInspector] public LevelHandler levelHandler;

    public Vector2 minMaxCameraX;
    public Vector2 minMaxCameraY;


    [Header("Doors")]
    public Door[] doors;

    public void Start()
    {
        levelHandler = GetComponentInParent<LevelHandler>();
    }


    public void LateUpdate()
    {
        levelHandler.cam.clampCameraPosition(minMaxCameraX, minMaxCameraY);
    }

    
    private void OnDrawGizmos()
    {
        //shows center of prefab
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }

}
