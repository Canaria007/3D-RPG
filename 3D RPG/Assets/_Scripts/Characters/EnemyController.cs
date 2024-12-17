using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD, PATROL, CHASE, DEAD}
public enum EnemyChaseStates { ATTACK, SKILL, NONE}

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private EnemyStates enemyStates;
    private EnemyChaseStates enemyChaseStates;
    private NavMeshAgent agent;

    protected CharacterStats characterStats;

    private Collider cd;
    private Animator anim;

    [Header("Basic Settings")]
    public float sightRadius;
    public bool isGuard;
    private float speed;
    protected GameObject attackTarget;

    private float lastAttackTimer;
    private float lastSkillTimer;

    public float lookAtTime = 2f;
    private float lookAtTimer;

    private Quaternion guardRotation;

    [Header("Patrol State")]
    public float patrolRange;
    private Vector3 wayPoint;
    private Vector3 guardPos;


    // bool 配合动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;

    private void Awake()
    {
        cd = GetComponent<Collider>();
        characterStats = GetComponent<CharacterStats>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        lookAtTimer = lookAtTime;
    }

    private void Start()
    {
        GameManager.Instance.AddObserver(this);
        enemyChaseStates = EnemyChaseStates.NONE;
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
    }

    private void Update()
    {
        isDead = characterStats.characterData.currentHealth <= 0? true : false;

        print("Enemy State: " + enemyStates);
        SwitchStates();
        SwitchAnimation();
        lastAttackTimer -= Time.deltaTime;
        lastSkillTimer -= Time.deltaTime;
    }

    void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Dead", isDead);
    }

    private void SwitchStates()
    {
        if(isDead)
        {
            enemyStates = EnemyStates.DEAD;
        }
        else if(FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
        }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                EnemyGuard();
                break;
            case EnemyStates.PATROL:
                EnemyPatrol();
                break;
            case EnemyStates.CHASE:
                EnemyChase();
                break;
            case EnemyStates.DEAD:
                EnemyDead();
                break;
        }
    }

    private void EnemyDead()
    {
        agent.radius = 0;
        //agent.enabled = false;
        cd.enabled = false;
        GameManager.Instance.RemoveObserver(this);
        Destroy(gameObject, 2f);
    }

    private void EnemyGuard()
    {
        isChase = false;

        if(Vector3.Distance(transform.position, guardPos) > agent.stoppingDistance)
        {
            isWalk = true;
            agent.isStopped = false;
            agent.destination = guardPos;
        }
        else
        {
            isWalk = false;
            agent.isStopped = true;
            transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, .1f);
        }

    }

    private void EnemyPatrol()
    {
        agent.speed = speed * .5f;
        isChase = false;
        if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
        {
            isWalk = false;

            if (lookAtTimer > 0)
            {
                lookAtTimer -= Time.deltaTime;
            }
            else
            {
                GetNewWayPoint();
            }

        }
        else
        {
            isWalk = true;
            agent.destination = wayPoint;
        }

    }

    private void EnemyChase()
    {
        isWalk = false;
        isChase = true;
        agent.speed = speed;
        if (!FoundPlayer())
        {
            isFollow = false;
            if (lookAtTimer > 0)
            {
                agent.destination = transform.position;
                lookAtTimer -= Time.deltaTime;
            }
            else if (isGuard)
            {
                enemyStates = EnemyStates.GUARD;
            }
            else
                enemyStates = EnemyStates.PATROL;
        }
        else
        {
            isFollow = true;
            agent.isStopped = false;
            agent.destination = attackTarget.transform.position;
        }

        if (TargetInAttackRange())
        {
            transform.LookAt(attackTarget.transform);

            Attack();
        }
        else if(TargetInSkillRange())
        {
            transform.LookAt(attackTarget.transform);
            Skill();
        }

    }

    private void OnTrigger()
    {
        enemyChaseStates = EnemyChaseStates.NONE;
    }

    private void Attack()
    {
        isFollow = false;
        agent.isStopped = true;
        enemyChaseStates = EnemyChaseStates.ATTACK;
        if (lastAttackTimer < 0)
        {
            lastAttackTimer = characterStats.attackData.attackCoolDown;
            characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
            anim.SetTrigger("Attack");
        }

    }

    private void Skill()
    {
        isFollow = false;
        agent.isStopped = true;
        enemyChaseStates = EnemyChaseStates.SKILL;
        if (lastSkillTimer < 0)
        {
            lastSkillTimer = characterStats.attackData.skillCoolDown;
            anim.SetTrigger("Skill");
        }

    }

    private void GetNewWayPoint()
    {
        lookAtTimer = lookAtTime;

        float x = Random.Range(-patrolRange, patrolRange);
        float z = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPoint = new Vector3(guardPos.x + x, transform.position.y, guardPos.z + z);
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, sightRadius, 1) ? hit.position : transform.position;

    }

    private bool TargetInAttackRange()
    {
        if(attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        else
            return false;
    }

    private bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        else
            return false;
    }

    private bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);

        foreach (var collider in colliders)
        {
            if(collider.CompareTag("Player"))
            {
                attackTarget = collider.gameObject;
                return true;
            }
        }

        attackTarget = null;
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    // 动画帧调用
    private void Hit()
    {
        if(attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    //void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    //void OnDisable()
    //{
    //    GameManager.Instance.RemoveObserver(this);
    //}

    public void EndNotify()
    {
        anim.SetBool("Win", true);
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
