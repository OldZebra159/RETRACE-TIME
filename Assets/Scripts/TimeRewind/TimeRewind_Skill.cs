using UnityEngine;

public class TimeRewind_Skill : Skill
{
    [Header("Time Rewind")]
    public bool timeRewindUnlocked;
    public float rewindDuration = 3f;
    public float rewindSpeed = 2f;

    private bool isRewinding = false;
    private float rewindTimer = 0f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (isRewinding)
        {
            rewindTimer -= Time.unscaledDeltaTime;
            if (rewindTimer <= 0)
            {
                StopRewind();
            }
        }
    }

    public override bool CanUseSkill()
    {
        if (!timeRewindUnlocked)
        {
            return false;
        }

        if (TimeRewindManager.instance == null)
        {
            return false;
        }

        if (TimeRewindManager.instance.IsRewinding)
        {
            return false;
        }

        return base.CanUseSkill();
    }

    public override void UseSkill()
    {
        base.UseSkill();
        
        if (TimeRewindManager.instance != null)
        {
            StartRewind();
        }
    }

    private void StartRewind()
    {
        isRewinding = true;
        rewindTimer = rewindDuration;
        TimeRewindManager.instance.StartRewind();
    }

    private void StopRewind()
    {
        isRewinding = false;
        if (TimeRewindManager.instance != null)
        {
            TimeRewindManager.instance.StopRewind();
        }
    }

    protected override void CheckUnlock()
    {
        if (timeRewindUnlocked)
        {
            UnlockTimeRewind();
        }
    }

    public void UnlockTimeRewind()
    {
        timeRewindUnlocked = true;
    }
}
