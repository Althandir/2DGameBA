using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

interface IPlayerControlls
{
    /// <summary>
    /// Method to disable scripts in case the player dies
    /// </summary>
    void PlayerDeathHandler();
}

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour, IHitable
{
    static Player Instance;
    /// <summary>
    /// Function to get the Instance of the player. Assigned in Awake so call it first in Start.
    /// </summary>
    public static Player GetInstance { get => Instance; }

    [SerializeField] int m_health = 100;
    [SerializeField] UnityEvent m_onPlayerDead = new UnityEvent();

    IEnumerator m_healthChecker;
    bool m_healthCheckerActive;

    /// <summary>
    /// Event to handle the death of the player
    /// </summary>
    public UnityEvent OnPlayerDeadEvent { get => m_onPlayerDead; }

    #region UnityMethods
    private void Awake()
    {
        if (GetInstance)
        {
            Debug.LogError("Double Playerscript detected in " + gameObject.name + "! Script will be deleted!");
            Destroy(this);
        }
        else
        {
            Instance = this;
            Debug.Log("Player set to Gameobject with Name: " + gameObject.name + ".");
        }
    }

    private void Start()
    {
        m_healthChecker = HealthChecker();

        StartCoroutine(m_healthChecker);
    }

    #endregion

    /// <summary>
    /// Coroutine to check if Player is alive
    /// </summary>
    /// <returns></returns>
    IEnumerator HealthChecker()
    {
        if (m_healthCheckerActive)
        {
            Debug.LogWarning("HealthChecker already active");
        }
        else
        {
            m_healthCheckerActive = true;
            while (m_healthCheckerActive)
            {
                yield return new WaitForSeconds(0.25f);
                if (m_health <= 0)
                {
                    m_onPlayerDead.Invoke();
                    m_healthCheckerActive = false;
                }
            }
        }
    }

    /// <summary>
    /// Method called if Player has been hit by something
    /// </summary>
    /// <param name="damage"></param>
    public void OnHit(int damage)
    {
        Debug.Log("Player has been hit");
        m_health -= damage;
    }
}
