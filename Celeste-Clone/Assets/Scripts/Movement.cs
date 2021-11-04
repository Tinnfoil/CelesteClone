using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random=UnityEngine.Random;

public class Movement : MonoBehaviour
{
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D rb;
    private AnimationScript anim;

    // my variables
    public bool canHold = true;
    private double timer = 0.0;
    public double holdTimerMax;
    public float groundLinearDrag;
    public float airLinearDrag;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;
    public bool pushingWall;

    [Space]

    private bool groundTouch;
    private bool hasDashed;

    public int side = 1;

    [Space]
    [Header("Polish")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;


    public MovementType movementType;
    private float speedModifier = 0;
    public Dictionary<string, Sprite> sprites;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer whiteSpriteRenderer;
    private float whiteTime;
    private Action OnDashStateChanged;
    private float hangTime;
    public float GravityScale = 1;
    public float JumpScale = 1;

    public AudioClip[] jumpSounds;
    public AudioClip[] landSounds;
    public AudioClip dashSound;
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();

        Sprite[] bluesprites = Resources.LoadAll<Sprite>("blue_roboMJ_Spritesheet");
        sprites = new Dictionary<string, Sprite>();
        foreach (Sprite s in bluesprites)
        {
            sprites.Add(s.name, s);
        }
        Sprite[] whitesprites = Resources.LoadAll<Sprite>("white_roboMJ_Spritesheet");
        foreach (Sprite s in whitesprites)
        {
            sprites.Add(s.name, s);
        }
        OnDashStateChanged += TurnWhite;

        audioSource = GetComponent<AudioSource>();
        jumpSounds = new AudioClip[]{(AudioClip)Resources.Load("Sounds/up1"),
                                                        (AudioClip)Resources.Load("Sounds/up2"), 
                                                        (AudioClip)Resources.Load("Sounds/up3"), 
                                                        (AudioClip)Resources.Load("Sounds/up4"),
                                                        (AudioClip)Resources.Load("Sounds/up5"),
                                                        (AudioClip)Resources.Load("Sounds/up6"),
                                                        (AudioClip)Resources.Load("Sounds/up7")};
        landSounds = new AudioClip[]{(AudioClip)Resources.Load("Sounds/down1"),
                                                        (AudioClip)Resources.Load("Sounds/down2"), 
                                                        (AudioClip)Resources.Load("Sounds/down3"), 
                                                        (AudioClip)Resources.Load("Sounds/down4"),
                                                        (AudioClip)Resources.Load("Sounds/down5"),
                                                        (AudioClip)Resources.Load("Sounds/down6")};     
        dashSound =  (AudioClip)Resources.Load("Sounds/dash");
        //Time.timeScale = .25f;
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        Walk(dir);
        anim.SetHorizontalMovement(x, y, rb.velocity.y);

        if (coll.onWall && Input.GetButton("Fire3") && canMove)
        {
            if (side != coll.wallSide)
                anim.Flip(side * -1);
            wallGrab = true;
            wallSlide = false;
        }

        if (Input.GetButtonUp("Fire3") || !coll.onWall || !canMove)
        {
            wallGrab = false;
            wallSlide = false;
        }

        if (coll.onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }

        pushingWall = false;
        if ((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }

        if (wallGrab && !isDashing && canHold)
        {
            rb.gravityScale = 0;
            if (x > .2f || x < -.2f)
                rb.velocity = new Vector2(rb.velocity.x, 0);

            float speedModifier = y > 0 ? .5f : 1;
            bool isClimbing = y > 0 ? true : false;

            rb.velocity = new Vector2(rb.velocity.x, y * (speed * speedModifier));

            // Velocity sustain on wall impact
            if (movementType != MovementType.Classic)
            {
                //Debug.Log(pushingWall + " " + coll.onTopOfWall + "" + y);
                if (!pushingWall && !coll.onTopOfWall && x == 0 && y > 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0);
                    anim.SetHorizontalMovement(x, 0, 0);
                }

                //Debug.Log(timer);
                if (isClimbing)
                {
                    timer += Time.deltaTime * 5;
                }
                else
                {
                    timer += Time.deltaTime;
                }

                if (timer > holdTimerMax)
                {
                    canHold = false;
                    isClimbing = false;
                }
            }
        }
        else
        {
            rb.gravityScale = 3 * (movementType == MovementType.Distinct ? GravityScale : 1);
        }

        if (coll.onWall && !coll.onGround && canHold)
        {
            if (x != 0 && !wallGrab)
            {
                WallSlide();
            }
            else
            {
                speedModifier = 0;
            }
        }

        //Wall slide when not holding
        if ((coll.onLeftWall || coll.onRightWall) && !coll.onGround && !canHold)
        {
            wallSlide = true;
            WallSlide();
        }

        if (!coll.onWall || coll.onGround)
            wallSlide = false;

        if (Input.GetButtonDown("Jump"))
        {
            anim.SetTrigger("jump");

            if (coll.onGround || hangTime > 0)
                Jump(Vector2.up, false);
                audioSource.clip = jumpSounds[Random.Range(0, jumpSounds.Length)];
                audioSource.volume = 0.4f;
                audioSource.Play();
                // if(Gamepad.current != null) StartCoroutine(VibrateController(.08f, .075f));
            if (coll.onWall && !coll.onGround)
                WallJump();
                audioSource.clip = jumpSounds[Random.Range(0, jumpSounds.Length)];
                audioSource.volume = 0.4f;
                audioSource.Play();
                // if(Gamepad.current != null) StartCoroutine(VibrateController(.08f, .075f));
        }

        if (Input.GetButtonDown("Fire1") && !hasDashed)
        {
            if (xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (coll.onGround && !groundTouch)
        {

            GroundTouch();
            groundTouch = true;
        }

        if (!coll.onGround && groundTouch)
        {
            groundTouch = false;
            if (movementType != MovementType.Classic) { hangTime = .05f; }
        }

        WallParticle(y);

        if (wallGrab || wallSlide || !canMove)
            return;

        if (x > 0)
        {
            side = 1;
            anim.Flip(side);
        }
        if (x < 0)
        {
            side = -1;
            anim.Flip(side);
        }

        // Linear fall time
        if (!isDashing && movementType == MovementType.Polished)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -10, 20));
        }

        if (hangTime > 0)
        {
            hangTime -= Time.deltaTime;
        }

    }

    private void LateUpdate()
    {
        if (movementType == MovementType.Classic) return;
        if (whiteTime > 0)
        {
            whiteSpriteRenderer.flipX = side == 1 ? false : true;
            whiteSpriteRenderer.color = new Color(1, 1, 1, whiteTime / .1f);
            whiteSpriteRenderer.sprite = sprites["white_" + spriteRenderer.sprite.name];
            whiteTime -= Time.deltaTime;
        }
        else
        {
            whiteSpriteRenderer.color = new Color(1, 1, 1, 0);
        }

        if (hasDashed)
        {
            spriteRenderer.sprite = sprites["blue_" + spriteRenderer.sprite.name];
        }

    }

    void GroundTouch()
    {
        if (hasDashed)
        {
            OnDashStateChanged?.Invoke(); // Invoke the event
        }
        hasDashed = false;
        isDashing = false;

        canHold = true;
        timer = 0;

        side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();
    }

    private void Dash(float x, float y)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        hasDashed = true;

        anim.SetTrigger("dash");

        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());

        OnDashStateChanged?.Invoke(); // Invoke the event
    }

    IEnumerator DashWait()
    {
        FindObjectOfType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        dashParticle.Play();
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        dashParticle.Stop();
        rb.gravityScale = 3 * (movementType == MovementType.Distinct ? GravityScale : 1);
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.25f);
        if (coll.onGround)
            hasDashed = false;
    }

    private void WallJump()
    {
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            anim.Flip(side);
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {
        if (coll.wallSide != side)
            anim.Flip(side * -1);

        if (!canMove)
            return;

        float push = pushingWall ? 0 : rb.velocity.x;

        if (!wallSlide)
        {
            wallSlide = true;
            if (movementType != MovementType.Classic)
            {
                speedModifier = rb.velocity.y;
            }
        }

        rb.velocity = new Vector2(push, -slideSpeed + speedModifier);
        speedModifier *= .99f;
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce * (movementType == MovementType.Distinct ? JumpScale : 1);

        particle.Play();

        
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    void WallParticle(float vertical)
    {
        var main = slideParticle.main;

        if (wallSlide || (wallGrab && vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }

    void TurnWhite()
    {
        whiteTime = .1f;
    }

    public void ChangeToClassic()
    {
        movementType = MovementType.Classic;
    }

    public void ChangeToPolish()
    {
        movementType = MovementType.Polished;
    }

    public void ChangeToDistinct()
    {
        movementType = MovementType.Distinct;
    }

    public enum MovementType
    {
        Classic,
        Polished,
        Distinct
    }
}
