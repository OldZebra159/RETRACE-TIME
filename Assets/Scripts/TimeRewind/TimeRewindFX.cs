using UnityEngine;

public class TimeRewindFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material rewindMaterial;
    [SerializeField] private ParticleSystem rewindParticles;

    [Header("Settings")]
    [SerializeField] private Color rewindColor = new Color(0f, 0.5f, 1f, 0.5f);
    [SerializeField] private float effectTransitionSpeed = 5f;
    
    private SpriteRenderer[] spriteRenderers;
    private Material[] originalMaterials;
    private bool isEffectActive = false;
    private float currentEffectIntensity = 0f;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalMaterials = new Material[spriteRenderers.Length];
        
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalMaterials[i] = new Material(spriteRenderers[i].material);
        }
    }

    private void Start()
    {
        if (TimeRewindManager.instance != null)
        {
            // 可以通过事件监听来触发特效
        }
    }

    private void Update()
    {
        if (TimeRewindManager.instance != null)
        {
            UpdateEffect(TimeRewindManager.instance.IsRewinding);
        }
    }

    public void UpdateEffect(bool active)
    {
        isEffectActive = active;
        currentEffectIntensity = Mathf.Lerp(currentEffectIntensity, active ? 1f : 0f, Time.unscaledDeltaTime * effectTransitionSpeed);
        
        UpdateMaterialEffect();
        UpdateParticleEffect();
    }

    private void UpdateMaterialEffect()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (rewindMaterial != null && currentEffectIntensity > 0.01f)
            {
                spriteRenderers[i].material = rewindMaterial;
                spriteRenderers[i].material.SetColor("_TintColor", new Color(rewindColor.r, rewindColor.g, rewindColor.b, rewindColor.a * currentEffectIntensity));
            }
            else
            {
                spriteRenderers[i].material = originalMaterials[i];
            }
        }
    }

    private void UpdateParticleEffect()
    {
        if (rewindParticles != null)
        {
            var emission = rewindParticles.emission;
            emission.enabled = isEffectActive;
        }
    }

    public void PlayRewindSound()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(30,null);
        }
    }
}
