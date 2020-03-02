using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class Enemy : MonoBehaviour , IHitable
{
    [SerializeField] protected int m_health = 100;
    [SerializeField] protected int m_damage = 10;
    [SerializeField] protected int m_speed = 4;
    [SerializeField] int m_defenseModifier = 1;

    Animator m_animator;
    Rigidbody2D m_rb2D;
    AudioSource m_audioSource;
    AudioSource m_movementAudioSource;
    EnemyDetectionArea m_detectionArea;
    EnemyAttack m_enemyAttack;

    [SerializeField] List<Transform> m_IdlePositions;

    [SerializeField] AudioClip m_idleStayAudio;
    [SerializeField] AudioClip m_alertedStayAudio;
    [SerializeField] AudioClip m_slowMoveAudio;
    [SerializeField] AudioClip m_fastMoveAudio;
    [SerializeField] AudioClip m_onHitAudio;
    [SerializeField] AudioClip m_onDeadAudio;

    float m_pausedMovementTimer;
    float m_pausedAudioMovementTimer;
    float m_stayOnIdlePositionTimer;
    bool isActive;

    protected Vector3 m_targetPosition;
    
    IEnumerator m_mainRoutine;
    IEnumerator m_audioMovementRoutine;

    void Awake()
    {
        m_rb2D = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        m_audioSource = GetComponent<AudioSource>();
        m_detectionArea = transform.GetChild(0).GetComponent<EnemyDetectionArea>();
        m_enemyAttack = transform.GetChild(1).GetComponent<EnemyAttack>();
        m_movementAudioSource = transform.GetChild(2).GetComponent<AudioSource>();

        m_mainRoutine = MainRoutine();
        m_audioMovementRoutine = AudioMovementManager();
    }

    void Start()
    {
        // Freeze Rotation
        m_rb2D.freezeRotation = true;
        // Add Listener on PlayerInRangeEvent
        m_enemyAttack.PlayerInRangeEvent.AddListener(OnPlayerInRange);
        // Add Listener if PlayerDies
        Player.GetInstance.OnPlayerDeadEvent.AddListener(OnPlayerDead);
        // Set MovementAudio to SlowMove
        m_movementAudioSource.clip = m_slowMoveAudio;
        // Set IdleTransforms if empty
        if (m_IdlePositions.Count == 0)
        {
            m_IdlePositions.Add(this.transform);
        }

        // Start Enemyroutine
        StartCoroutine(m_mainRoutine);
        StartCoroutine(m_audioMovementRoutine);
    }

    #region MovementSystem
    /// <summary>
    /// MovementRoutine for Enemys
    /// </summary>
    /// <returns></returns>
    void FixedUpdate()
    {
        if (isActive && m_pausedMovementTimer <= 0)
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
        if (m_targetPosition.x == transform.position.x)
        {
            m_stayOnIdlePositionTimer -= 1;
        }

        if (m_stayOnIdlePositionTimer <= 0)
        {
            m_targetPosition = m_IdlePositions[UnityEngine.Random.Range(0, m_IdlePositions.Count)].position;
            m_stayOnIdlePositionTimer = UnityEngine.Random.Range(3, 5);
        }
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
    /// Coroutine to manage audio of movement
    /// </summary>
    IEnumerator AudioMovementManager()
    {
        while (isActive)
        {
            if (m_pausedAudioMovementTimer > 0)
            {
                m_movementAudioSource.Stop();
                yield return new WaitForSeconds(m_pausedAudioMovementTimer);
                m_pausedAudioMovementTimer = 0;
                m_movementAudioSource.Play();
            }

            if (!m_animator.GetBool("isAlerted") && !m_animator.GetBool("isMoving"))
            {
                ChangeMovementClip(m_idleStayAudio);
            }
            else if (!m_animator.GetBool("isAlerted") && m_animator.GetBool("isMoving"))
            {
                ChangeMovementClip(m_slowMoveAudio);
            }
            else if (m_animator.GetBool("isAlerted") && !m_animator.GetBool("isMoving"))
            {
                ChangeMovementClip(m_alertedStayAudio);
            }
            else if (m_animator.GetBool("isAlerted") && m_animator.GetBool("isMoving"))
            {
                ChangeMovementClip(m_fastMoveAudio);
            }

            yield return new WaitForSeconds(0.25f);

        }
    }
    /// <summary>
    /// Method to change audioclip of movement
    /// </summary>
    /// <param name="newClip"></param>
    void ChangeMovementClip(AudioClip newClip)
    {
        if (m_movementAudioSource.clip != newClip)
        {
            m_movementAudioSource.Stop();
            m_movementAudioSource.clip = newClip;
            m_movementAudioSource.Play();
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
            m_audioSource.PlayOneShot(m_onHitAudio);
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
        
        GetComponent<BoxCollider2D>().enabled = false;
        m_rb2D.gravityScale = 0;
        m_animator.SetBool("isAlerted", false);
        m_animator.SetBool("isMoving", false);
        m_animator.SetTrigger("Dead");
        m_movementAudioSource.Stop();
        m_audioSource.PlayOneShot(m_onDeadAudio);
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
        m_pausedAudioMovementTimer = delay;
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
