using UnityEngine;
using DG.Tweening;

public class GhostTrail : MonoBehaviour
{
    private Movement move;
    private AnimationScript anim;
    private SpriteRenderer sr;
    public Transform ghostsParent;
    public Color trailColor;
    public Color fadeColor;
    public Color polishedTrailColor;
    public Color polishedFadeColor;
    public float ghostInterval;
    public float fadeTime;

    private void Start()
    {
        anim = FindObjectOfType<AnimationScript>();
        move = FindObjectOfType<Movement>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void ShowGhost()
    {
        Sequence s = DOTween.Sequence();

        for (int i = 0; i < ghostsParent.childCount; i++)
        {
            Movement.MovementType movementType = FindObjectOfType<Movement>().movementType;
            Transform currentGhost = ghostsParent.GetChild(i);
            s.AppendCallback(()=> currentGhost.position = move.transform.position);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().flipX = anim.sr.flipX);
            s.AppendCallback(()=>currentGhost.GetComponent<SpriteRenderer>().sprite = anim.sr.sprite);
            s.Append(currentGhost.GetComponent<SpriteRenderer>().material.DOColor(movementType == Movement.MovementType.Classic? trailColor: polishedTrailColor, 0));
            s.AppendCallback(() => FadeSprite(currentGhost));
            s.AppendInterval(ghostInterval);
        }
    }

    public void FadeSprite(Transform current)
    {
        Movement.MovementType movementType = FindObjectOfType<Movement>().movementType;
        current.GetComponent<SpriteRenderer>().material.DOKill();
        current.GetComponent<SpriteRenderer>().material.DOColor(movementType == Movement.MovementType.Classic ? fadeColor: polishedFadeColor, fadeTime);
    }

}
