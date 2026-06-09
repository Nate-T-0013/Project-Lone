using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class AnimationLogic : MonoBehaviour
{
    [SerializeField] private Animator legAnim;
    [SerializeField] private Animator torsoAnim;
    private PlayerMovement parentScript;
    [SerializeField] private Shooting shootScript;
    [SerializeField] private float currDirec;
    [SerializeField] private bool isGrounded;
    [SerializeField] private int angle;
    [SerializeField] private bool grabLedge;
    [SerializeField] private bool isAiming;

    public AudioSource audioSource;
    [SerializeField] AudioClip footstep;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        legAnim = GetComponent<Animator>();
        parentScript = GetComponentInParent<PlayerMovement>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //currDirec = parentScript.currDirec;
        isGrounded = parentScript.isGrounded;
        angle = shootScript.aimAngle;
        isAiming = shootScript.isAiming;
        grabLedge = parentScript.grabLedge;
        animateTorsoSprites();
        animateLegSprites();
    }

    //giant logic function for managing sprite animation and transitions for legs
    public void animateLegSprites()
    {
        if (Input.GetAxisRaw("Horizontal") != 0) //moving
        {
            currDirec = Input.GetAxisRaw("Horizontal");
            legAnim.SetInteger("speed", 1);
            if (currDirec > 0)
            {
                legAnim.SetBool("facingRight", true);
                if (!isGrounded)
                {
                    legAnim.SetBool("inAir", true);
                    if (grabLedge) legAnim.SetBool("Ledge", true);
                    else legAnim.SetBool("Ledge", false);
                }
                else legAnim.SetBool("inAir", false);
            }
            else if (currDirec < 0)
            {
                legAnim.SetBool("facingRight", false);
                if (!isGrounded)
                {
                    legAnim.SetBool("inAir", true);
                    if (grabLedge) legAnim.SetBool("Ledge", true);
                    else legAnim.SetBool("Ledge", false);
                }
                else legAnim.SetBool("inAir", false);
            }
        }
        else //standing still
        {
            legAnim.SetInteger("speed", 0);
            if (currDirec > 0)
            {
                legAnim.SetBool("facingRight", true);
                if (!isGrounded)
                {
                    legAnim.SetBool("inAir", true);
                    if (grabLedge) legAnim.SetBool("Ledge", true);
                    else legAnim.SetBool("Ledge", false);
                }
                else legAnim.SetBool("inAir", false);
            }
            else if (currDirec < 0)
            {
                legAnim.SetBool("facingRight", false);
                if (!isGrounded)
                {
                    legAnim.SetBool("inAir", true);
                    if (grabLedge) legAnim.SetBool("Ledge", true);
                    else legAnim.SetBool("Ledge", false);
                }
                else legAnim.SetBool("inAir", false);
            }
        }
    }

    //same thing for torso and aiming
    public void animateTorsoSprites()
    {
        if (currDirec > 0)
        {
            torsoAnim.SetBool("facingRight", true);
            if (grabLedge) torsoAnim.SetBool("Ledge", true);
            else torsoAnim.SetBool("Ledge", false);
            torsoAnim.SetInteger("angle", angle);
            torsoAnim.SetBool("isAiming", isAiming);
        }
        else if (currDirec < 0)
        {
            torsoAnim.SetBool("facingRight", false);
            if (grabLedge) torsoAnim.SetBool("Ledge", true);
            else torsoAnim.SetBool("Ledge", false);
            torsoAnim.SetInteger("angle", angle);
            torsoAnim.SetBool("isAiming", isAiming);
        }
        
    }

    public void PlayFootstep()
    {
        float maxPitch = 0.85f;
        float minPitch = 0.95f;

        if (footstep != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(footstep);
        }
    }
}
