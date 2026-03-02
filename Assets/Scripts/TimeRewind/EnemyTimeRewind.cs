using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyTimeRewind : MonoBehaviour, ITimeRewindable
{
    private Enemy enemy;
    private Entity entity;
    private CharacterStats stats;
    private Animator anim;
    private Rigidbody2D rb;
    private List<TimeRecordData> recordHistory = new List<TimeRecordData>();
    private bool wasDead = false;
    
    public bool IsRewinding { get; set; }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        entity = GetComponent<Entity>();
        stats = GetComponent<CharacterStats>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (TimeRewindManager.instance != null)
        {
            TimeRewindManager.instance.RegisterRewindable(this);
        }
    }

    private void OnDestroy()
    {
        if (TimeRewindManager.instance != null)
        {
            TimeRewindManager.instance.UnregisterRewindable(this);
        }
    }

    public void RecordState()
    {
        if (IsRewinding || stats.isDead) return;

        string currentAnimState = GetCurrentAnimationState();
        var record = new TimeRecordData(
            transform, 
            rb, 
            entity.facingDir, 
            stats.currentHealth,
            currentAnimState,
            anim != null ? anim.speed : 1f,
            stats
        );

        recordHistory.Add(record);

        int maxRecords = Mathf.CeilToInt(TimeRewindManager.instance.RecordDuration * 50f);
        if (recordHistory.Count > maxRecords)
        {
            recordHistory.RemoveAt(0);
        }
    }

    public void RestoreState(TimeRecordData data)
    {
        if (data == null) return;

        if (stats.isDead && !wasDead)
        {
            ReviveEnemy();
        }

        transform.position = data.position;
        transform.localScale = data.scale;
        if (rb != null)
        {
            rb.velocity = data.velocity;
        }
        
        if (entity.facingDir != data.facingDir)
        {
            entity.Flip();
        }

        stats.currentHealth = data.health;
        RestoreAilments(data);
        RestoreFXStates(data);

        wasDead = stats.isDead;
    }

    private void RestoreFXStates(TimeRecordData data)
    {
        // 恢复所有子级FX对象
        var fxComponents = GetComponentsInChildren<FXTimerRewind>();
        foreach (var fx in fxComponents)
        {
            if (fx != null)
            {
                // 通知FX组件恢复状态
            }
        }
    }

    private void ReviveEnemy()
    {
        if (stats != null)
        {
            stats.isDead = false;
            stats.MakeInvincible(false);
        }
        
        if (anim != null)
        {
            anim.Rebind();
            anim.enabled = true;
        }

        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    private void RestoreAilments(TimeRecordData data)
    {
        if (stats != null)
        {
            stats.isIgnited = data.isIgnited;
            stats.isChilled = data.isChilled;
            stats.isShocked = data.isShocked;
        }
    }

    private string GetCurrentAnimationState()
    {
        if (anim == null) return "";
        
        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Idle") ? "Idle" :
               stateInfo.IsName("Move") ? "Move" :
               stateInfo.IsName("Attack") ? "Attack" :
               stateInfo.IsName("Stunned") ? "Stunned" : "";
    }

    public List<TimeRecordData> GetRecordHistory()
    {
        return recordHistory;
    }

    public void ClearHistory()
    {
        recordHistory.Clear();
    }
}
