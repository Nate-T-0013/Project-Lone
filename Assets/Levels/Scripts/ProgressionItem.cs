using System.Collections.Generic;
using UnityEngine;

public class ProgressionItem : MonoBehaviour
{
    //progression item type
    
    public enum ProgressionType
    {
        DoubleJump,
        WaterSuit
    }
    public ProgressionType type;
    
    private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    //when we touch item, give associated progreeseion item
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>())
        {

            switch (type)
            {
                case ProgressionType.DoubleJump:
                    if (ProgressionGlobals.hasDoubleJump) return;
                    ProgressionGlobals.hasDoubleJump = true;
                    break;

                case ProgressionType.WaterSuit:
                    if (ProgressionGlobals.hasWaterSuit) return;
                    ProgressionGlobals.hasWaterSuit = true;
                    break;
            }
            collision.gameObject.GetComponentInChildren<AudioSource>().PlayOneShot(pickupSound);
            Debug.Log(type + "unlocked!");
            //make sure we deactivate, but don't delete, that way if we revert saves to before we picked up item, it will still be activateable
            gameObject.SetActive(false);
        }
    }
}
