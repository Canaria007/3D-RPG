using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;

    private Animator anim;

    private CharacterStats characterStats;

    private GameObject attackTarget;

    private float attackTimer;

    bool isDead;

    private float stopDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        stopDistance = agent.stoppingDistance;
    }

    private void Start()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RegisterPlayer(characterStats);
    }

    private void Update()
    {
        isDead = characterStats.characterData.currentHealth <= 0 ? true : false;
        
        if(isDead)
            GameManager.Instance.NotifyObservers();

        SwitchAnimation();

        attackTimer -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Dead", isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if (isDead) return;

        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }

    public void EventAttack(GameObject target)
    {
        if (isDead) return;

        if (target != null)
        {
            attackTarget = target;
            
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;

        transform.LookAt(attackTarget.transform);

        while(Vector3.Distance(transform.position, attackTarget.transform.position) > characterStats.attackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            if(Vector3.Distance(transform.position, attackTarget.transform.position) <= characterStats.attackData.attackRange)
            {
                agent.isStopped = true;
            }

            yield return null;
        }

        agent.isStopped = true;

        if(attackTimer < 0)
        {
            attackTimer = characterStats.attackData.attackCoolDown;

            characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;

            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
        }
    }

    private void Hit()
    {
        if(attackTarget.CompareTag("Attackable"))
        { 
            if(attackTarget.GetComponent<Rock>())
            {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rock>().FlyToGolem();
                //attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                //attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 25, ForceMode.Impulse);
            }
        }

        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            targetStats.TakeDamage(characterStats, targetStats);
        }
    }
}
