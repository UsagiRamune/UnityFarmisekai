using Ai;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public AiController  aiController;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Attack()
    {
        CombatSystem.instance.PerformSlashAttack();
    }
    public void ResetPlayerAttack()
    {
        PlayerMovement.Instance.animator.SetBool("Attack",false);
        CombatSystem.instance.isAttacking =  false;
        PlacementSystem.instance.weapon.SetActive(false);
        PlacementSystem.instance.weapon.GetComponent<Renderer>().enabled = false;
        PlayerMovement.Instance.animator.SetInteger("State",0);
        CombatSystem.instance.attackCooltime = CombatSystem.instance.attackCooldown;
    }
    public void ResetEnemyRangeAttack()
    {
        aiController.isAttacking =  false;
        
    }
}
