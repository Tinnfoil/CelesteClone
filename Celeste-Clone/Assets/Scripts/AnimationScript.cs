using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour
{

    private Animator anim;
    private Movement move;
    private Collision coll;
    [HideInInspector]
    public SpriteRenderer sr;
    public AudioClip[] stepSounds;
    public AudioSource audioSource;

    void Start()
    {
        anim = GetComponent<Animator>();
        coll = GetComponentInParent<Collision>();
        move = GetComponentInParent<Movement>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        //random step sounds to play
        stepSounds = new AudioClip[]{(AudioClip)Resources.Load("Sounds/step1"),
                                                        (AudioClip)Resources.Load("Sounds/step2"),
                                                        (AudioClip)Resources.Load("Sounds/step3"),
                                                        (AudioClip)Resources.Load("Sounds/step4"),
                                                        (AudioClip)Resources.Load("Sounds/step5")};
    }


    void Update()
    {
        anim.SetBool("onGround", coll.onGround);
        anim.SetBool("onWall", coll.onWall);
        anim.SetBool("onRightWall", coll.onRightWall);
        anim.SetBool("wallGrab", move.wallGrab);
        anim.SetBool("wallSlide", move.wallSlide);
        anim.SetBool("canMove", move.canMove);
        anim.SetBool("isDashing", move.isDashing);
        anim.SetBool("canHold", move.canHold);


    }

    public void SetHorizontalMovement(float x, float y, float yVel)
    {
        anim.SetFloat("HorizontalAxis", x);
        anim.SetFloat("VerticalAxis", y);
        anim.SetFloat("VerticalVelocity", yVel);
    }

    public void SetTrigger(string trigger)
    {
        anim.SetTrigger(trigger);
    }

    public void Flip(int side)
    {

        if (move.wallGrab || move.wallSlide)
        {
            if (side == -1 && sr.flipX)
                return;

            if (side == 1 && !sr.flipX)
            {
                return;
            }
        }

        bool state = (side == 1) ? false : true;
        sr.flipX = state;
    }

    public void PlayFootstep()
    {
        Movement movement = GetComponentInParent<Movement>();
        if(movement.movementType != Movement.MovementType.Classic)
        {
            movement.walkParticle.Play();
            audioSource.clip = stepSounds[Random.Range(0, stepSounds.Length)];
            audioSource.volume = 0.15f;
            audioSource.Play();
        }
    }
}