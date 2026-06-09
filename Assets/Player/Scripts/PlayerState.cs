using UnityEngine;

public struct InputBuffer
{
    private float value;
    private float max;
    public InputBuffer(float maxTime)
    {
        this.value = 0;
        this.max = maxTime;
    }
    public void Press()
    {
        value = max;
    }
    public void Update()
    {
        if (value + Time.deltaTime >= max) value = max;
        else if (value < max) value += Time.deltaTime;
    }

    public bool IsBuffered() => value >= max;

    public void Reset()
    {
        value = 0;
    }
}

public class PlayerStateMachine
{
    public PlayerState currentState;

    public void OnEnter(PlayerState startingState)
    {
        currentState = startingState;
        currentState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
}


public abstract class PlayerState
{

    protected PlayerMovement player;

    public PlayerState(PlayerMovement player)
    {
        this.player = player;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }

}


public class NormalState : PlayerState
{
    InputBuffer ledgeDetectBuffer;

    public NormalState(PlayerMovement player) : base(player) {}

    public override void Enter()
    {
        ledgeDetectBuffer = new InputBuffer(0.15f);
    }

    public override void Update()
    {
        ledgeDetectBuffer.Update();

        if (ledgeDetectBuffer.IsBuffered()) player.ledgeDetect();

        player.NormalStateUpdate();
        
    }

}


public class LedgeGrabState : PlayerState
{

    Vector2 ledgeStandPosition;
    public bool inPlace;
    public bool inPlaceStanding;
    bool isClimbing;
    int currDirec;
    public bool grabLedge;
    //float ledgeInputBuffer = 0;
    //float ledgeInputBufferMax = 0.25f;
    InputBuffer ledgeInputBuffer;

    public LedgeGrabState(PlayerMovement player) : base(player) { }

    public override void Enter()
    {
        inPlace = false;
        ledgeStandPosition = player.ledgeStandPosition;
        currDirec = player.currDirec;
        //ledgeInputBuffer = 0;
        ledgeInputBuffer = new InputBuffer(0.5f);
        inPlaceStanding = false;
        isClimbing = false;
        //move to ledge
        if (!inPlace)
        {
            player.StartCoroutine(player.moveToGrabPoint());
        }
    }

    public override void Update()
    {
        ledgeInputBuffer.Update();
        //dont let player input until we are fully in place
        if (!inPlace) return;
        //move to the standing location. buffered
        if (((Input.GetButtonDown("Horizontal") && currDirec == Input.GetAxisRaw("Horizontal"))
        || (ledgeInputBuffer.IsBuffered() && Input.GetAxisRaw("Horizontal") == currDirec)) && player.ledgeSpace)
        {
            isClimbing = true;
        }
        

        //let player jump
        if ((Input.GetAxisRaw("Jump") != 0) && !isClimbing)
        {
            ledgeJump();
            return;
        }

        //let go of ledge without doing anything special
        //reset jump logic to allow both a ghost jump and a double jump. this improves feeling if player accidentally pressed the left or right key before jump, meaning they couldnt have gotten a double jump
        ledgeDrop();

        if (isClimbing) player.moveToStandPoint();
        if (inPlaceStanding) player.playerStateMachine.ChangeState(player.normalState);
    }

    //jump immediately then allow for double jump
    void ledgeJump()
    {
        player.Jump();
        player.jumpCount = 1;
        player.playerStateMachine.ChangeState(player.normalState);
    }
    void ledgeDrop()
    {
        //not the most efficient but works
        if ((Input.GetButton("Vertical") && ledgeInputBuffer.IsBuffered() || Input.GetButtonDown("Vertical"))   //if we press down or we are fully buffered while holding down
        || (((Input.GetButtonDown("Horizontal") && Input.GetAxisRaw("Horizontal") == -currDirec)) //if we press left or right once
        || (ledgeInputBuffer.IsBuffered() && Input.GetAxisRaw("Horizontal") == -currDirec))) //if we're buffered and holding left or right (but not both)
        {
            if (!isClimbing)
            {
                player.jumpCount = 0;
                player.ghostJumpBuffer.Reset();
                player.setVelocity(Vector2.zero);
                player.playerStateMachine.ChangeState(player.normalState);
            }
        }
    }

}

