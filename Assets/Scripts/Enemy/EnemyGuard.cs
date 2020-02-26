using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGuard : Enemy
{
    public override void OnHit(int damage)
    {
        Debug.Log("You hit an Guard.");
        base.OnHit(damage);
    }
}
