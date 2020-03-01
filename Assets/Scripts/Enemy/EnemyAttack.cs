using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyAttack : Attack
{
    readonly UnityEventFloat m_playerInRange = new UnityEventFloat();
    [Tooltip("Delay to finish AttackAnimation")]
    [SerializeField] float m_animationDelay = 1;
    [Tooltip("Damage done per hit")]
    [SerializeField] int m_damage = 25;
    [Tooltip("Time delay of AttackRoutine")]
    [SerializeField] float m_attackRoutineDelay = 0.5f;

    IEnumerator m_attackRoutine;
    bool isActive;

    public UnityEventFloat PlayerInRangeEvent { get => m_playerInRange; }
    public bool IsActive { get => isActive; }

    protected override void Awake()
    {
        base.Awake();
        m_attackRoutine = AttackRoutine();
    }

    public void StartAttackRoutine()
    {
        if (!isActive)
        {
            isActive = true;
            StartCoroutine(m_attackRoutine);
        }
    }

    public void StopAttackRoutine()
    {
        if (isActive)
        {
            isActive = false;
            StopCoroutine(m_attackRoutine);
        }
    }

    /// <summary>
    /// Attack Routine for enemies
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackRoutine()
    {
        while (isActive)
        {
            if(IsPlayerInRange())
            {
                // Disable Movement as long animation is played
                m_playerInRange.Invoke(m_animationDelay);

                m_animator.SetInteger("AttackAnimID", m_activeColliderID);

                // Start attack animation
                m_animator.SetTrigger("Attack");
                // Play AttackSound
                PlaySound();
                // wait until half of animation
                yield return new WaitForSeconds(m_animationDelay / 2);
                // deal damage to all objects in list
                foreach (var collider in m_objectsToHit)
                {
                    if (IsHitableObject(collider))
                    {
                        collider.GetComponent<HitableObject>().OnHit(m_damage);
                    }
                    else if (IsPlayer(collider))
                    {
                        collider.GetComponent<Player>().OnHit(m_damage);
                    }
                }

                RandomizeActiveCollider();
            }
            yield return new WaitForSeconds(m_attackRoutineDelay);
        }
    }

    /// <summary>
    /// Checks if the given collider has a HitableObject Script
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    bool IsHitableObject(Collider2D collider)
    {
        if (collider.GetComponent<HitableObject>())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the given collider has a Player Script
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    bool IsPlayer(Collider2D collider)
    {
        if (collider.GetComponent<Player>())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the Player is inside the HitArea
    /// </summary>
    /// <returns></returns>
    bool IsPlayerInRange()
    {
        if (m_objectsToHit.Contains(Player.GetInstance.GetComponent<BoxCollider2D>()))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
