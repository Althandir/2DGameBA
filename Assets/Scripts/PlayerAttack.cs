using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerAttack : MonoBehaviour, IPlayerControlls
{
    bool m_attackPressed;
    [SerializeField] float m_attackTimer;
    [SerializeField] int m_damage = 25;

    Animator m_animator;
    [SerializeField] List<BoxCollider2D> m_objectsToHit;
    [SerializeField] float attackDelay;
   
    void Awake()
    {
        m_animator = transform.parent.GetComponent<Animator>();
        attackDelay = 0.5f;
    }

    private void Start()
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<BoxCollider2D>())
        {
            m_objectsToHit.Add(collision.GetComponent<BoxCollider2D>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (m_objectsToHit.Contains(collision.GetComponent<BoxCollider2D>()))
        {
            m_objectsToHit.Remove(collision.GetComponent<BoxCollider2D>());
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
        m_animator.SetTrigger("Attacking");

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
        this.enabled = false;
    }
}
