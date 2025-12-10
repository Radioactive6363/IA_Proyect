using UnityEngine;

public class Animations : MonoBehaviour
{
    private Animator animator;
    int hit;


    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = 1;

    }



    public void Idle()
    {
        animator.Play("Idle");
    }
    public void Walk()
    {
        animator.Play("Walk");
    }

    public void Attack()
    {
        animator.Play("Attack");
        hit++;
        if( hit > 5)
        {
            Die();
        }

    }
    public void Die()
    {
        animator.Play("Die");
    }
}
