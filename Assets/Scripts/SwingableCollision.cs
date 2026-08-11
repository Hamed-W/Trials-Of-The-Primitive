using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingableCollision : MonoBehaviour
{
    public bool midSwing = false;
    //public bool hasHit = false;
    private HashSet<GameObject> objectsHit = new HashSet<GameObject>();
    public ParticleSystem sparks;
    public ItemData itemData;
    public PlayerStats playerStats;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Entity"))
        {
            if (!itemData.isPickaxe)
            {
                GameObject enemy = other.gameObject;
                if (midSwing && !objectsHit.Contains(enemy))
                {
                    objectsHit.Add(enemy);
                    float damage = itemData.itemUseAmount * playerStats.damageModifier;
                    Debug.Log("Enemy Hit " + damage.ToString());
                    enemy.GetComponent<EntityBehaviour>().TakeDamage(damage);
                }
            }
        }
        if (other.CompareTag("Mineable"))
        {
            if (itemData.isPickaxe)
            {
                GameObject envObject = other.gameObject;
                if (midSwing && !objectsHit.Contains(envObject))//!hasHit)
                {
                    objectsHit.Add(envObject);
                    if (sparks != null) sparks.Emit(5);
                    BreakableObject breakableObject = envObject.GetComponent<BreakableObject>();
                    if (breakableObject != null)
                    {
                        breakableObject.TakeDamage(itemData.itemUseAmount);
                    }
                    //hasHit = true;
                }
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
        objectsHit.Clear();
        //hasHit = false;
    }

}
