using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class PlayerAttack : Attack, IPlayerControlls
{
    bool m_attackPressed;
    float m_attackTimer;

    [Tooltip("Delay to finish AttackAnimation")]
    [SerializeField] float attackDelay = 0.5f;
    [Tooltip("Damage done per hit")]
    [SerializeField] int m_damage = 25;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        Player.GetInstance.OnPlayerDeadEvent.AddListener(PlayerDeathHandler);
    }

    void Update()
    {
        UpdateAttackTimer();
        CheckForInput();
    }

    void FixedUpdate()
    {
        if (m_attackPressed)
        {
            m_attackPressed = false;
            StartCoroutine(HandleAttack());
        }
    }

    /// <summary>
    /// Method to increase the AttackTimer
    /// </summary>
    void UpdateAttackTimer()
    {
        m_attackTimer += Time.deltaTime;
    }

    /// <summary>
    /// Checking if Attack Button has been pressed. Available every 0.5 second.
    /// </summary>
    void CheckForInput()
    {
        if (Input.GetButtonDown("Attack") && m_attackTimer > attackDelay)
        {
            m_attackPressed = true;
            m_attackTimer = 0.0f;
        }
    }

    /// <summary>
    /// Handle Attack Input by setting trigger inside the Animator & calling attacked on each Object inside the HitArea
    /// </summary>
    IEnumerator HandleAttack()
    {
        // Start Animation
        m_animator.SetTrigger("Attacking");
        // Play AttackSound
        PlaySound();
        yield return new WaitForSeconds(attackDelay / 2);

        foreach (Collider2D collider in m_objectsToHit)
        {
            if (IsHitableObject(collider))
            {
                collider.GetComponent<HitableObject>().OnHit(m_damage);
            }
            else if (IsEnemy(collider))
            {
                collider.GetComponent<Enemy>().OnHit(m_damage);
            }
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
    /// Checks if the given collider has a Enemy Script
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    bool IsEnemy(Collider2D collider)
    {
        if (collider.GetComponent<Enemy>())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Method to disable this script in case the player dies
    /// </summary>
    public void PlayerDeathHandler()
    {
        m_animator.SetBool("isMoving", false);

        this.enabled = false;
    }
}
