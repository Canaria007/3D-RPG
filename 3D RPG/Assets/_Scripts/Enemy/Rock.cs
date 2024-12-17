using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Rock: MonoBehaviour
{
    public enum RockStates { HitPlayer, HitEnemy, HitNothing}
    public RockStates rockStates;
    private Rigidbody rb;

    [Header("Basic Settings")]
    public float force;
    public int damage;
    public GameObject target;
    private Vector3 direction;

    public GameObject destroyParticle;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        rockStates = RockStates.HitPlayer;
        FlyToTarget();
    }

    private void FixedUpdate()
    {
        if(rb.velocity.sqrMagnitude < 1f)
        {
            rockStates = RockStates.HitNothing;
        }
    }

    public void FlyToTarget()
    {
        if (target == null)
            target = FindObjectOfType<PlayerController>().gameObject;

        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    public void FlyToGolem()
    {
        Vector3 golPos = FindAnyObjectByType<Golem>().gameObject.transform.position;
        print(golPos);
        direction = (golPos - transform.position).normalized;
        rb.velocity = Vector3.one;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        switch (rockStates)
        {
            case RockStates.HitPlayer:
                if(other.gameObject.CompareTag("Player"))
                {
                    other.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    other.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;

                    other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    other.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, other.gameObject.GetComponent<CharacterStats>());
                    rockStates = RockStates.HitNothing;
                }
                break;
            case RockStates.HitEnemy:
                if(other.gameObject.GetComponent<Golem>())
                {
                    var otherStats = other.gameObject.GetComponent<CharacterStats>();
                    other.gameObject.GetComponent<Animator>().SetTrigger("Hit");
                    otherStats.TakeDamage(damage, otherStats);

                    Instantiate(destroyParticle, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
                break;
        }

    }
}
