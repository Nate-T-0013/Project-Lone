//Carter V
//Increment 2
//Handles player movement with help from a state machine to handle different movement states

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

/*To Implement:
potential fixes if they come up
*/

//concise structure for handling player corners
struct RaycastCorners
{
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
}

public class PlayerMovement : MonoBehaviour
{


    #region Variables
    //state machine is used to change the players current gameplay state. it disables/enables certain bits of code based on what we are allowed to do, without constantly checking bools and stuff
    public PlayerStateMachine playerStateMachine;
    //normal gameplay state. movement, shooting in all directions, jumping, checking for ledges, that sort of thing
    public NormalState normalState;
    //ledge grab state. disables movement, but allows for input to return us to a movement state/let us do certain interactions with ledges.
    public LedgeGrabState ledgeGrabState;

    [Header("Movement")]
    [Tooltip("Movement speed")]
    public float runSpeed = 6f;
    [Tooltip("Grounded acceleration")]
    public float groundAcceleration = 25;
    [Tooltip("Groounded deceleration")]
    public float groundDeceleration = 25;
    [Tooltip("Air acceleration")]
    public float airAcceleration = 25;
    [Tooltip("Air deceleration")]
    public float airDeceleration = 25;
    public float waterSpeedMultiplier = 0.7f;
    public float waterAccelerationMultiplier = 0.5f;
    public float waterDecelerationMultiplier = 0.7f;
    

    [Header("Jump")]
    [Tooltip("Initial velocity of jump")]
    public float jumpVelocity;
    [Tooltip("Initial velocity of double jump")]
    public float doubleJumpVelocity;
    public float waterJumpMultiplier = 0.5f;
    //IMPLEMENT
    [Tooltip("Max fall velocity")]
    public float terminalVelocity;
    [HideInInspector] public bool useGravity = true;
    [Tooltip("Gravity scalar")]
    public float gravityScale;
    public float waterGravityScale = 0.5f;
    //not currently used
    [Tooltip("Wall gravity scalar")]
    //to implement. allows for a slight moment where the player can still jump upon leaving the grounded state (except for when jumping)
    public float ghostJumpThreshold = 0.25f;
    public InputBuffer ghostJumpBuffer;
    [HideInInspector] public int jumpCount = 0;


    [Header("Ledge")]
    public float horizontalDetectionDistance = 0.4f;
    public float verticalDetectionDistance = 0.5f;
    [Tooltip("The speed at which the player is pulled towards a ledge when entering the state")]
    public float ledgeMagnetism = 10;
    [HideInInspector] public Vector2 ledgeGrabPosition;
    [HideInInspector] public Vector2 ledgeStandPosition;
    [HideInInspector] public bool ledgeSpace = false;

    private BoxCollider2D playerCollider;
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private AudioClip waterLoop;

    //Keep it small. This keeps horizontal corner rays from detecting the ground on accident, and vice versa. This keeps player from gaining infinite speed
    const float rayOffset = 0.0001f;
    private RaycastCorners raycastCorners;
    private float horizontalRaySpacing;
    private float verticalRaySpacing;
    const int horizontalRayCount = 10;
    const int verticalRayCount = 10;
    private bool structureCollision = true;
    private bool isInWater = false;
    public bool isGrounded = false;
    //keeps track of input velocity added per frame
    private Vector2 velocity = new Vector2(0, 0);

    //temporary stuff
    [Header("Debug stuff")]
    [SerializeField] public int currDirec;
    public bool grabLedge => playerStateMachine.currentState is LedgeGrabState;

    //this is the layer the player will collide with. can change to support multiple layers later
    [Tooltip("Collidable layer")]
    public LayerMask structure;
    [Tooltip("Water layer")]
    public LayerMask water;

    #endregion


    #region Awake / Start / Update
    public void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();

        //state machine stuff
        playerStateMachine = new PlayerStateMachine();
        normalState = new NormalState(this);
        ledgeGrabState = new LedgeGrabState(this);
    }

    public void Start()
    {
        ghostJumpBuffer = new InputBuffer(ghostJumpThreshold);
        //calculate bounds
        Bounds bounds = playerCollider.bounds;
        bounds.Expand(rayOffset * -2);
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);

        playerStateMachine.OnEnter(normalState);
        
    }

    public void Update()
    {
        playerStateMachine.currentState.Update();
    }
    #endregion


    #region State Updates
    //-------------------------------all of the update functions for the normal state are grouped here. makes it easier to just call this func in the normal state implementation ------------------------------------------
    public void NormalStateUpdate()
    {
        
        //jump height debug visualizer so we can decide how high level design should be
        Debug.DrawRay(transform.position, Vector2.up * ((jumpVelocity * jumpVelocity) / (2 * (9.81f * gravityScale)) - playerCollider.size.y / 2));
        //this will probably be used for flipping sprite direction
        currDirec = Input.GetAxisRaw("Horizontal") != 0 ? (int)Mathf.Sign(Input.GetAxisRaw("Horizontal")) : currDirec;
        updatePlayerBounds();
        checkWater();
        //update player bounds for collisions
        horizontalMovement();
        //resolve horizontal collisions
        if (structureCollision) horizontalCollision();
        //apply gravity
        gravity();
        //consume the first jump. this allows for double jumping even when falling off a ledge
        handleJump();
        //resolve vertical collisions
        if (structureCollision) verticalCollision();
        //apply velocity (THIS MOVES THE PLAYER)
        transform.position += (new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime);
    }
    #endregion


    #region Movement
    //--------------------------------------- PLAYER MOVEMENT-----------------------------------------------------
    private void horizontalMovement()
    {
        //target velocity = run speed in the desired horizontal direction
        float target = Input.GetAxisRaw("Horizontal") * runSpeed;

        //if grounded, use ground values, otherwise air
        float accel = isGrounded ? groundAcceleration : airAcceleration;
        float decel = isGrounded ? groundDeceleration : airDeceleration;

        //if we're in water, do a simple not-very-accurate drag effect, where we make the player slower, and accelerate slower
        if (isInWater)
        {
            accel *= waterAccelerationMultiplier;
            decel *= waterDecelerationMultiplier;
            target *= waterSpeedMultiplier;
        }

        //apply acceleration and deceleration to the x velocity vector, which is applied each frame, so multiply by frametime to keep it frame independent
        velocity.x = Input.GetAxisRaw("Horizontal") != 0 ? Mathf.Lerp(velocity.x, target, accel * Time.deltaTime) : Mathf.Lerp(velocity.x, 0, decel * Time.deltaTime);
    }

    private void gravity()
    {
        //gravity = gravity scale * generic gravity value
        //cap -y velocity to terminal velocity, max to max allowable float
        float gScale = (isInWater ? waterGravityScale : gravityScale);
        if (useGravity) velocity.y = Mathf.Clamp(velocity.y - (gScale * 9.81f * Time.deltaTime), -terminalVelocity, float.MaxValue);
    }
    #endregion


    //allows for a brief moment of airtime before not allowing us to jump. So you can run off a ledge, and for a split second you will still be allowed to jump, bc it feels more responsive.
    //makes things a bit more complicated because we have to check to make sure we didn't leave the ground by jumping, because otherwise we would be allowed to triple jump from a grounded state, but still worth it.
    #region Jump
    //---------------------------------- JUMP LOGIC ------------------------------------------------------------------------------
    public void handleJump()
    {
        if (isGrounded)
        {
            jumpCount = 0;
            ghostJumpBuffer.Reset();
        }
        else
        {
            ghostJumpBuffer.Update();
        }
        
        if (Input.GetButtonDown("Jump"))
        {
            if ((isGrounded  || !ghostJumpBuffer.IsBuffered()) && jumpCount == 0)
            {
                Jump();
                jumpCount = 1;
                ghostJumpBuffer.Reset();
            }
            else if ((jumpCount == 1 || (jumpCount == 0 && !isGrounded && ghostJumpBuffer.IsBuffered())) && ProgressionGlobals.hasDoubleJump)
            {
                doubleJump();
                jumpCount = 2;
            }
        }
    }

    //public to allow calling from state
    public void Jump()
    {
        float jVelocity = jumpVelocity;
        jVelocity *= isInWater ? waterJumpMultiplier : 1f;
        velocity.y = jVelocity;
    }

    private void doubleJump()
    {
        float jVelocity = doubleJumpVelocity;
        jVelocity *= isInWater ? waterJumpMultiplier : 1f;
        velocity.y = jVelocity;
    }
    #endregion


    #region Ledge

    //----------------------------------LEDGE DETECTION---------------------------------------------------

    public void ledgeDetect()
    {
        ledgeSpace = false;
        //used later so we dont have to keep checking currdirec
        float playerBoundsX;

        if (isGrounded || velocity.y > 0) return;

        RaycastHit2D hit1;
        Vector2 rayOrigin;
        if (currDirec == 1)
        {
            rayOrigin = raycastCorners.topRight;
            playerBoundsX = playerCollider.bounds.max.x;
        }
        else
        {
            rayOrigin = raycastCorners.topLeft;
            playerBoundsX = playerCollider.bounds.min.x;
        }

        //shoot ray left or right depending on direction. no need to check both
        hit1 = Physics2D.Raycast(rayOrigin, Vector2.right, currDirec * horizontalDetectionDistance, structure);
        Debug.DrawRay(rayOrigin, Vector2.right * currDirec * horizontalDetectionDistance, Color.red);
        
        if (!hit1)
        {
            //get origin of second ray
            Vector2 ray2Origin = rayOrigin + new Vector2(currDirec * horizontalDetectionDistance, 0);
            Debug.DrawRay(ray2Origin, Vector2.down * verticalDetectionDistance, Color.blue);

            RaycastHit2D hit2 = Physics2D.Raycast(ray2Origin, Vector2.down, verticalDetectionDistance, structure);
            
            if (hit2)
            {

                //cast from middle to the vertical surface to get x position of corner. hit 2 contains the y value. if we take the x and y we will get the corner position
                RaycastHit2D hit3 = Physics2D.Raycast((new Vector2(playerBoundsX, playerCollider.bounds.center.y)), Vector2.right, currDirec * horizontalDetectionDistance, structure);
                
                Vector2 boxCastPosition = Vector2.zero;
                
                if (hit3)
                {
                    //calculate corner position
                    boxCastPosition = new Vector2(hit3.point.x + currDirec * (playerCollider.size.x/2), hit2.point.y + (playerCollider.size.y/2));
                    
                    //because we are using a grid system, we do not need to exhaustively check every possible bit of space, as long as it would interseect any possible grid square
                    //that lets us use a smaller collider, while avoiding accidentally checking the ground we would stand on
                    Collider2D[] boxCol = Physics2D.OverlapBoxAll(boxCastPosition, new Vector2(playerCollider.size.x * 0.9f, playerCollider.size.y * 0.9f), 0, structure);
                    
                    //if no intersections, we can stand here. else we cant
                    bool canStand = boxCol.Length == 0;

                    if (canStand)
                    {
                        ledgeSpace = true;
                        ledgeStandPosition = boxCastPosition;
                    }
                }

                //need to implement checking
                
                Vector2 grabPositionCheck = new Vector2(hit3.point.x - currDirec * (playerCollider.size.x / 2), hit2.point.y - (playerCollider.size.y / 2));
                //calculate in such a way that avoids grabbing ledges that are the same height as the player
                Collider2D[] grabCheck = Physics2D.OverlapBoxAll(new Vector2(grabPositionCheck.x, hit2.point.y - (playerCollider.size.y * 1.25f / 2)), new Vector2(playerCollider.size.x * 0.9f, playerCollider.size.y * 1.125f), 0, structure);

                //if no intersections, we can stand here. else we cant
                bool canGrab = grabCheck.Length == 0;

                //if there is a valid grabbable ledge, and we get the appropriate input, we switch to the ledgegrab state
                if (canGrab)
                {
                    ledgeGrabPosition = grabPositionCheck;
                    if ((Input.GetAxisRaw("Horizontal") != 0) && velocity.y <= 0)
                    {
                        playerStateMachine.ChangeState(ledgeGrabState);
                    }
                }

            }

        }
    }

    //moves player to the grabbing positioon on the ledge. more like a magnet than a teleport
    public IEnumerator moveToGrabPoint()
    {
        while (Vector2.Distance(transform.position, ledgeGrabPosition) > 0.01)
        {
            transform.position = Vector2.MoveTowards(transform.position, ledgeGrabPosition, ledgeMagnetism * Time.deltaTime);
            yield return null;
        }
        transform.position = ledgeGrabPosition;
        velocity = Vector2.zero;
        ledgeGrabState.inPlace = true;
    }

    //move player to the top of the ledge
    public void moveToStandPoint()
    {
        if (!ledgeGrabState.inPlaceStanding)
        {
            //smoothly move player to grab point. avoids jarring teleport
            transform.position = Vector2.MoveTowards(transform.position, ledgeStandPosition, 15 * Time.deltaTime);
        }
        //snap when we are close enough. Maybe change to when the distance is greater than the movement added per frame?
        if (Vector2.Distance(transform.position, ledgeStandPosition) < 0.01f)
        {
            transform.position = ledgeStandPosition;
            velocity = Vector2.zero;
            ledgeGrabState.inPlaceStanding = true;
        }
    }
    #endregion

    //it seems over-complicated since Unity has a built in collision system for rigidbodies, but this method allows for pretty much pixel perfect collisions, and enabling/disabling collisions without any issue. Can also easily choose what is interactable.
    #region Collision

    private void horizontalCollision()
    {
        float direction = Mathf.Sign(velocity.x);
        //add width so we are still casting from corners
        float rayLength = Mathf.Abs(velocity.x * Time.deltaTime) + rayOffset;

        //send out horizontal rays to check horizontal collision
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 origin;

            if (direction == -1) origin = raycastCorners.bottomLeft;
            else origin = raycastCorners.bottomRight;

            //keep cast from bottom up each frame
            origin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, rayLength, structure);
            if (hit)
            {
                velocity.x = (hit.distance - rayOffset) / Time.deltaTime * direction;
                rayLength = hit.distance;
            }
        }
    }

    private void verticalCollision()
    {
        isGrounded = false;
        float direction = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y * Time.deltaTime) + rayOffset;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 origin;
            if (direction == -1) origin = raycastCorners.bottomLeft;
            else origin = raycastCorners.topLeft;

            origin += Vector2.right * (verticalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up * direction, rayLength, structure);
            if (hit)
            {
                // vertical collision adjustment
                velocity.y = (hit.distance - rayOffset) / Time.deltaTime * direction;
                rayLength = hit.distance;

                if (direction == -1) isGrounded = true;
            }
        }
    }

    //just check if we should update the isInWater bool to apply water physics. more efficient to have as a void, so we arent potentially running multiple times per frame by getting a bool returned directly.
    public void checkWater()
    {
        isInWater = false;
        //if we don't have water suit, apply water physics
        if (!ProgressionGlobals.hasWaterSuit)
        {
            Collider2D hit = Physics2D.OverlapBox(playerCollider.bounds.center, playerCollider.bounds.size * 0.95f, 0, water);
            isInWater = hit != null;
        }
    }

    //play sound on water collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Water")) GetComponentInChildren<AudioSource>().PlayOneShot(splashSound);
    }


    //make sure we know positions of corner for collision raycasts
    private void updatePlayerBounds()
    {
        Bounds bounds = playerCollider.bounds;
        bounds.Expand(rayOffset * -2);

        Vector2 min = bounds.min;
        Vector2 max = bounds.max;

        //calc top left corner
        raycastCorners.topLeft = new Vector2(min.x, max.y);
        //calc top right corner
        raycastCorners.topRight = new Vector2(max.x, max.y);
        //calc bottom left corner
        raycastCorners.bottomLeft = new Vector2(min.x, min.y);
        //calc bottom right corner
        raycastCorners.bottomRight = new Vector2(max.x, min.y);
    }
    #endregion


    #region Getters / Setters
    public void setVelocity(Vector2 velocity)
    {
        this.velocity = velocity; 
    }
    public Vector2 getVelocity()
    {
        return velocity;
    }
    #endregion


}
