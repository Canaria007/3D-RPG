using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{
    [Header("Skill")]
    public float kickForce = 20;

    public void kickOff()
    {
        transform.LookAt(attackTarget.transform);

        Vector3 kickDir = attackTarget.transform.position - transform.position;
        kickDir.Normalize();
       
        attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
        attackTarget.GetComponent<NavMeshAgent>().velocity = kickDir * kickForce;
        attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
        print("Kick OFF   22222 !!!");

    }
}
