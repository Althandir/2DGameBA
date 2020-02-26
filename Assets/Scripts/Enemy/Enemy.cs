using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D),typeof(Collider2D), typeof(Animator))]
public class Enemy : MonoBehaviour , IHitable
{
    [SerializeField] protected int m_health = 100;
    [SerializeField] protected int m_damage = 10;
    [SerializeField] protected int m_speed = 4;
    [SerializeField] int m_defenseModifier = 1;

    Animator m_animator;
    Rigidbody2D m_rb2D;
    EnemyDetectionArea m_detectionArea;
    EnemyAttack m_enemyAttack;

    float m_pausedMovementTimer;
    bool isActive;
    protected Vector3 m_targetPosition;
    
    [Tooltip("Number of different attack animations")]

    IEnumerator m_mainRoutine;

    void Awake()
    {
        m_rb2D = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        m_detectionArea = transform.GetChild(0).GetComponent<EnemyDetectionArea>();
        m_enemyAttack = transform.GetChild(1).GetComponent<EnemyAttack>();

        m_mainRoutine = MainRoutine();
    }

    void Start()
    {
        // Freeze Rotation
        m_rb2D.freezeRotation = true;
        // Add Listener on PlayerInRangeEvent
        m_enemyAttack.PlayerInRangeEvent.AddListener(OnPlayerInRange);
        // Add Listener if PlayerDies
        Player.GetInstance.OnPlayerDeadEvent.AddListener(OnPlayerDead);
        // Start Enemyroutine
        StartCoroutine(m_mainRoutine);
    }

    #region MovementSystem
    /// <summary>
    /// MovementRoutine for Enemys
    /// </summary>
    /// <returns></returns>
    void FixedUpdate()
    {
        if (isActive && m_pausedMovementTimer < 0)
        {
            Vector2 thisPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 targetPos = new Vector2(m_targetPosition.x, transform.position.y);
            float distance = Vector2.Distance(thisPos, targetPos);

            if (distance > 0)
            {
                transform.position = Vector2.MoveTowards(thisPos, targetPos, 0.01f * m_speed);
                RotateToTarget();
                m_animator.SetBool("isMoving", true);
            }
            else
            {
                m_animator.SetBool("isMoving", false);
            }
        }
        else
        {
            m_animator.SetBool("isMoving", false);
            m_pausedMovementTimer -= Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// Function to rotate the sprite to face the target position. Can be overwritten.
    /// </summary>
    protected virtual void RotateToTarget()
    {
        // Debug.Log(m_targetPosition.x - transform.position.x); Gibt pos Zahl wenn rechts, neg wenn links
        if (m_targetPosition.x - transform.position.x > 0)
        {
            if (transform.localScale == Vector3.one)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else if (m_targetPosition.x - transform.position.x < 0)
        {
            if (transform.localScale == new Vector3(-1, 1, 1))
            {
                transform.localScale = Vector3.one; 
            }
        }
    }
    #endregion

    /// <summary>
    /// Idle Behavior for an enemy. Can be overwritten.
    /// </summary>
    protected virtual void IdleBehavior()
    {
        // TODO: Add IdleBehavoir
        m_targetPosition = transform.position;
    }

    /// <summary>
    /// Behavior if the Player has entered the trigger once. Can be overwritten.
    /// </summary>
    protected virtual void AlertedBehavior()
    {
        // Targetposition is the Playerposition
        m_targetPosition = Player.GetInstance.transform.position;
        // Start Attack Routine if not started
        if (!m_enemyAttack.IsActive)
        {
            m_enemyAttack.StartAttackRoutine();
        }
    }


    /// <summary>
    /// Main Coroutine of the Enemy
    /// </summary>
    IEnumerator MainRoutine()
    {
        isActive = true;
        while (m_health > 0 && isActive)
        {
            if (!m_detectionArea.IsAlerted)
            {
                IdleBehavior();
                yield return new WaitForSeconds(1);
            }
            else 
            {
                if (m_animator.GetBool("isAlerted"))
                {
                    AlertedBehavior();
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    m_animator.SetBool("isAlerted", true);
                    yield return new WaitForSeconds(1);
                }
                yield return new WaitForSeconds(1);
            }
        }
    }


    /// <summary>
    /// Method called if enemy has been hit by player
    /// </summary>
    /// <param name="damage"></param>
    public virtual void OnHit(int damage)
    {
        if (this.enabled)
        {
            m_health -= damage / m_defenseModifier;
            if (m_health <= 0)
            {
                Die();
            }
        }
        else
        {
            Debug.Log("Enemy already dead.");
        }
    }

    /// <summary>
    /// Method called if enemy dies
    /// </summary>
    protected void Die()
    {
        StopAllRoutines();

        m_animator.SetBool("isAlerted", false);
        m_animator.SetBool("isMoving", false);
        m_animator.SetTrigger("Dead");
        this.enabled = false;
    }

    void StopAllRoutines()
    {
        isActive = false;
        m_enemyAttack.StopAttackRoutine();
    }

    #region OnPlayerInRange Methods
    void OnPlayerInRange(float delay)
    {
        PauseMovement(delay);
    }

    void PauseMovement(float delay)
    {
        m_pausedMovementTimer = delay;
    }
    #endregion

    #region OnPlayerDead Methods

    void OnPlayerDead()
    {
        StopAllRoutines();

        m_animator.SetBool("isAlerted", true);
        m_animator.SetBool("isMoving", false);
        this.enabled = false;
    }

    #endregion
}
