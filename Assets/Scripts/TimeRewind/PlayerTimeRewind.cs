using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerTimeRewind : MonoBehaviour, ITimeRewindable
{
    private Player player;
    private Entity entity;
    private CharacterStats stats;
    private Animator anim;
    private Rigidbody2D rb;
    private List<TimeRecordData> recordHistory = new List<TimeRecordData>();
    
    public bool IsRewinding { get; set; }

    private void Awake()
    {
        player = GetComponent<Player>();
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
        if (IsRewinding) return;

        string currentAnimState = GetCurrentAnimationState();
        var record = new TimeRecordData(
            transform, 
            rb, 
            entity.facingDir, 
            stats.currentHealth,
            currentAnimState,
            anim.speed,
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

        transform.position = data.position;
        transform.localScale = data.scale;
        rb.velocity = data.velocity;
        
        if (entity.facingDir != data.facingDir)
        {
            entity.Flip();
        }

        stats.currentHealth = data.health;

        if (stats.onHealthChanged != null)
        {
            stats.onHealthChanged();
        }

        RestoreAilments(data);
        RestoreFXStates(data);
        RestorePlayerState(data);
    }

    private void RestorePlayerState(TimeRecordData data)
    {
        if (player == null || player.stateMachine == null) return;

        bool isGrounded = entity.IsGroundDetected();
        string recordedAnimState = data.animationState;
        
        if (isGrounded)
        {
            // 地面状态：根据记录的动画状态恢复
            if (recordedAnimState == "Idle")
            {
                player.stateMachine.ChangeState(player.idleState);
            }
            else if (recordedAnimState == "Move")
            {
                player.stateMachine.ChangeState(player.moveState);
            }
            else if (recordedAnimState == "Attack")
            {
                player.stateMachine.ChangeState(player.primaryAttack);
            }
            else if (recordedAnimState == "Dash")
            {
                player.stateMachine.ChangeState(player.dashState);
            }
            else
            {
                // 如果没有记录的状态，默认回到 idle
                player.stateMachine.ChangeState(player.idleState);
            }
        }
        else
        {
            // 空中状态：根据记录的速度和位置恢复
            if (Mathf.Abs(rb.velocity.y) < 0.1f)
            {
                player.stateMachine.ChangeState(player.airState);
            }
            else if (rb.velocity.y > 0)
            {
                player.stateMachine.ChangeState(player.jumpState);
            }
            else
            {
                player.stateMachine.ChangeState(player.airState);
            }
        }
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

    private void RestoreAilments(TimeRecordData data)
    {
        stats.isIgnited = data.isIgnited;
        stats.isChilled = data.isChilled;
        stats.isShocked = data.isShocked;
    }

    private string GetCurrentAnimationState()
    {
        if (anim == null) return "";
        
        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Idle") ? "Idle" :
               stateInfo.IsName("Move") ? "Move" :
               stateInfo.IsName("Jump") ? "Jump" :
               stateInfo.IsName("Attack") ? "Attack" :
               stateInfo.IsName("Dash") ? "Dash" : "";
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
