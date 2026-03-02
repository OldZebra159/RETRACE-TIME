using UnityEngine;

[System.Serializable]
public class TimeRecordData
{
    public Vector3 position;
    public Vector2 velocity;
    public float rotation;
    public int facingDir;
    public int health;
    public string animationState;
    public float animationSpeed;
    public bool isIgnited;
    public bool isChilled;
    public bool isShocked;
    public float ignitedTimer;
    public float chilledTimer;
    public float shockedTimer;
    public Vector3 scale;
    public bool isFXPlaying;
    public float fxLifetime;
    public float fxNormalizedTime;
    public int fxParticleCount;
    public Sprite currentSprite;

    public TimeRecordData(Transform transform, Rigidbody2D rb, int facingDir, int health, 
                          string animState, float animSpeed, CharacterStats stats)
    {
        position = transform.position;
        velocity = rb != null ? rb.velocity : Vector2.zero;
        rotation = transform.rotation.eulerAngles.y;
        this.facingDir = facingDir;
        this.health = health;
        animationState = animState;
        animationSpeed = animSpeed;
        scale = transform.localScale;
        isFXPlaying = false;
        fxLifetime = 0f;
        fxNormalizedTime = 0f;
        fxParticleCount = 0;
        
        // 尝试获取当前精灵
        SpriteRenderer sr = transform.GetComponentInChildren<SpriteRenderer>();
        currentSprite = sr != null ? sr.sprite : null;
        
        if (stats != null)
        {
            isIgnited = stats.isIgnited;
            isChilled = stats.isChilled;
            isShocked = stats.isShocked;
            ignitedTimer = 0;
            chilledTimer = 0;
            shockedTimer = 0;
        }
    }

    public TimeRecordData(Transform transform, Rigidbody2D rb, int facingDir, int health, 
                          string animState, float animSpeed, CharacterStats stats,
                          bool isFXPlaying, float fxLifetime, float fxNormalizedTime, int fxParticleCount)
    {
        position = transform.position;
        velocity = rb != null ? rb.velocity : Vector2.zero;
        rotation = transform.rotation.eulerAngles.y;
        this.facingDir = facingDir;
        this.health = health;
        animationState = animState;
        animationSpeed = animSpeed;
        scale = transform.localScale;
        this.isFXPlaying = isFXPlaying;
        this.fxLifetime = fxLifetime;
        this.fxNormalizedTime = fxNormalizedTime;
        this.fxParticleCount = fxParticleCount;
        
        // 尝试获取当前精灵
        SpriteRenderer sr = transform.GetComponentInChildren<SpriteRenderer>();
        currentSprite = sr != null ? sr.sprite : null;
        
        if (stats != null)
        {
            isIgnited = stats.isIgnited;
            isChilled = stats.isChilled;
            isShocked = stats.isShocked;
            ignitedTimer = 0;
            chilledTimer = 0;
            shockedTimer = 0;
        }
    }
}
