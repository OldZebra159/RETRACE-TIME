using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackhole_Skill : Skill
{
    [SerializeField] private UI_SkillTreeSlot blackHoleUnlockButton;
    public bool blackholeUnlocked {  get; private set; }
    [SerializeField] private int amountOfAttacks;
    [SerializeField] private float cloneCooldown;
    [SerializeField] private float blackholeDuration;
    [Space]
    [SerializeField] private GameObject blackHolePrefab;
    [SerializeField] private float maxSize;
    [SerializeField] private float growSpeed;
    [SerializeField] private float shrinkSpeed;


    Blackhole_Skill_Controller currentBlackhole;

    protected override void CheckUnlock()
    {
        UnlockBlackhole();
    }

    private void UnlockBlackhole()
    {
        if (blackHoleUnlockButton.unlocked)
            blackholeUnlocked = true;
    }

    public override bool CanUseSkill()
    {
        return base.CanUseSkill();
    }

    public override void UseSkill()
    {
        base.UseSkill();

        GameObject newBlackHole = Instantiate(blackHolePrefab,player.transform.position,Quaternion.identity);

        currentBlackhole = newBlackHole.GetComponent<Blackhole_Skill_Controller>();

        currentBlackhole.SetupBlackhole(maxSize, growSpeed, shrinkSpeed, amountOfAttacks, cloneCooldown,blackholeDuration);

        AudioManager.instance.PlaySFX(18, player.transform);
        AudioManager.instance.PlaySFX(19, player.transform);
    }

    protected override void Start()
    {
        base.Start();

        blackHoleUnlockButton.OnUnlocked += UnlockBlackhole;
    }

    protected override void Update()
    {
        base.Update();
    }


    public bool SkillCompleted()
    {
        if (!currentBlackhole)
            return false;


        if (currentBlackhole.playerCanExitState)
        {
            currentBlackhole = null;
            return true;
        }


        return false;
    }

    public float GetBlackholeRadius()
    {
        return maxSize / 2;
    }
}
