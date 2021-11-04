using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Collision : MonoBehaviour
{

    [Header("Layers")]
    public LayerMask groundLayer;

    [Space]

    public bool onGround;
    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;
    public bool onTopOfWall;
    public int wallSide;

    [Space]

    [Header("Collision")]

    public float collisionRadius = 0.25f;
    public Vector2 bottomOffset, rightOffset, leftOffset, topRightOffset, topLeftOffset;
    private Color debugCollisionColor = Color.red;
    private bool landed;
    private float speedAverage;

    // Start is called before the first frame update
    void Start()
    {
        landed = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, collisionRadius, groundLayer);
        if (onGround && landed == false)
        {
            Movement movement = GetComponent<Movement>();
            // Shake Camera on land
            if (movement.movementType == Movement.MovementType.Distinct)
            {
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                Camera.main.transform.DOComplete();
                //Debug.Log(speedAverage);
                Camera.main.transform.DOShakePosition(.1f, .07f * (speedAverage < -17 ? -speedAverage / 7 : 0), 10, 90, false, true);
            }
            landed = true;
            speedAverage = 0;
        }
        else if (!onGround && landed == true)
        {
            landed = false;
        }
        else if (!onGround && GetComponent<Rigidbody2D>().velocity.y < 0){
            speedAverage = (speedAverage + GetComponent<Rigidbody2D>().velocity.y) / 2;
        }

        onWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer)
            || Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);

        onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer);
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);

        onTopOfWall = Physics2D.OverlapCircle((Vector2)transform.position + topRightOffset, collisionRadius, groundLayer)
            || Physics2D.OverlapCircle((Vector2)transform.position + topLeftOffset, collisionRadius, groundLayer);

        wallSide = onRightWall ? -1 : 1;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var positions = new Vector2[] { bottomOffset, rightOffset, leftOffset };

        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, collisionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + topRightOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + topLeftOffset, collisionRadius);
    }
}
