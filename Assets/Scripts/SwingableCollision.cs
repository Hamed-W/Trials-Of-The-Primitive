using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingableCollision : MonoBehaviour
{
    public bool midSwing = false;
    public bool hasHit = false;
    public ParticleSystem sparks;

    void OnTriggerStay(Collider other)
    {
        /*if (other.CompareTag("Enemy"))
        {
            GameObject enemy = other.gameObject;
            if (!canAttack && !enemiesHit.Contains(enemy))
            {
                enemiesHit.Add(enemy);
                enemy.GetComponent<EnemyHealth>().TakeDamage(swordDamage);
            }
        }*/
        if (other.CompareTag("Mineable"))
        {
            GameObject envObject = other.gameObject;
            if (midSwing && !hasHit)
            {
                sparks.Emit(5);
                hasHit = true;
            }
        }
    }

    public void StartSwing()
    {
        midSwing = true;
    }

    public void EndSwing()
    {
        midSwing = false;
    }

    public void ResetHit()
    {
        hasHit = false;
    }

}
