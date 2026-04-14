using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator am;
    PlayerMovement pm;
    SpriteRenderer sr;

    void Start()
    {
        am = GetComponent<Animator>();
        pm = GetComponent<PlayerMovement>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        am.SetFloat("X", pm.moveDir.x);
        am.SetFloat("Y", pm.moveDir.y);

        Flip();
    }

    void Flip()
    {
        if (pm.moveDir.x > 0)
        {
            sr.flipX = false; // face right
        }
        else if (pm.moveDir.x < 0)
        {
            sr.flipX = true; // face left
        }
    }
}