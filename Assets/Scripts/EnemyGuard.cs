using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGuard : Enemy
{
    [SerializeField] int m_defenseModifier = 4;

    public override void OnHit(int damage)
    {
        Debug.Log("You hit an Guard.");
        m_health -= damage / m_defenseModifier;
    }
}
