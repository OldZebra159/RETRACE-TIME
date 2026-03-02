using System.Collections.Generic;
using UnityEngine;

public class RewindTrailFX : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxTrailCount = 20;
    [SerializeField] private float trailSpacing = 0.05f;
    [SerializeField] private float trailFadeSpeed = 3f;
    [SerializeField] private float trailScale = 0.85f;
    [SerializeField] private Color trailColor = new Color(0f, 0.5f, 1f, 0.6f);
    [SerializeField] private bool showFullPath = true;
    [SerializeField] private float pathRefreshRate = 0.02f;

    [Header("References")]
    [SerializeField] private SpriteRenderer sourceSpriteRenderer;

    private List<GameObject> trailObjects = new List<GameObject>();
    private List<Vector3> trailPositions = new List<Vector3>();
    private List<float> trailAlphaValues = new List<float>();
    private float lastTrailTime = 0f;
    private float lastPathRefreshTime = 0f;
    private bool isRewinding = false;
    private Vector3 lastRecordedPosition;
    private float positionThreshold = 0.01f;

    private void Start()
    {
        if (sourceSpriteRenderer == null)
        {
            sourceSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        lastRecordedPosition = transform.position;
    }

    private void Update()
    {
        isRewinding = TimeRewindManager.instance != null && TimeRewindManager.instance.IsRewinding;

        if (isRewinding)
        {
            UpdateTrail();
            if (showFullPath)
            {
                UpdateFullPath();
            }
        }
        else
        {
            ClearTrail();
        }
    }

    private void UpdateTrail()
    {
        if (sourceSpriteRenderer == null) return;

        float distanceMoved = Vector3.Distance(transform.position, lastRecordedPosition);
        if (distanceMoved >= positionThreshold || Time.time - lastTrailTime >= trailSpacing)
        {
            CreateTrail();
            lastTrailTime = Time.time;
            lastRecordedPosition = transform.position;
        }

        UpdateTrailObjects();
    }

    private void UpdateFullPath()
    {
        if (Time.time - lastPathRefreshTime >= pathRefreshRate)
        {
            CreateTrail();
            lastPathRefreshTime = Time.time;
        }
    }

    private void CreateTrail()
    {
        Vector3 position = transform.position;
        trailPositions.Add(position);
        trailAlphaValues.Add(1f);

        if (trailPositions.Count > maxTrailCount)
        {
            trailPositions.RemoveAt(0);
            trailAlphaValues.RemoveAt(0);
        }

        // 确保有足够的轨迹对象
        while (trailObjects.Count < trailPositions.Count)
        {
            CreateTrailObject();
        }
    }

    private void CreateTrailObject()
    {
        if (sourceSpriteRenderer == null) return;

        GameObject trailObj = new GameObject("RewindTrail");
        trailObj.transform.parent = transform.parent;
        trailObj.transform.localScale = transform.localScale * trailScale;

        SpriteRenderer trailRenderer = trailObj.AddComponent<SpriteRenderer>();
        trailRenderer.sprite = sourceSpriteRenderer.sprite;
        trailRenderer.flipX = sourceSpriteRenderer.flipX;
        trailRenderer.sortingLayerName = sourceSpriteRenderer.sortingLayerName;
        trailRenderer.sortingOrder = sourceSpriteRenderer.sortingOrder - 1;
        trailRenderer.color = trailColor;

        trailObjects.Add(trailObj);
    }

    private void UpdateTrailObjects()
    {
        for (int i = 0; i < trailObjects.Count; i++)
        {
            if (i < trailPositions.Count && trailObjects[i] != null)
            {
                // 设置轨迹位置
                trailObjects[i].transform.position = trailPositions[i];
                trailObjects[i].transform.rotation = transform.rotation;
                trailObjects[i].transform.localScale = transform.localScale * trailScale;

                // 同步精灵状态
                SpriteRenderer trailRenderer = trailObjects[i].GetComponent<SpriteRenderer>();
                SpriteRenderer sourceRenderer = sourceSpriteRenderer;
                
                if (trailRenderer != null && sourceRenderer != null)
                {
                    trailRenderer.sprite = sourceRenderer.sprite;
                    trailRenderer.flipX = sourceRenderer.flipX;
                    trailRenderer.flipY = sourceRenderer.flipY;

                    // 计算淡出效果
                    float alpha = 1f - ((float)i / trailPositions.Count);
                    alpha *= 0.6f; // 基础透明度
                    
                    // 随时间进一步淡出
                    trailAlphaValues[i] = Mathf.Max(0f, trailAlphaValues[i] - Time.unscaledDeltaTime * trailFadeSpeed);
                    alpha = Mathf.Min(alpha, trailAlphaValues[i]);

                    Color color = trailColor;
                    color.a = alpha;
                    trailRenderer.color = color;

                    // 移除完全透明的轨迹
                    if (alpha <= 0.01f)
                    {
                        Destroy(trailObjects[i]);
                        trailObjects.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }

    private void ClearTrail()
    {
        foreach (GameObject trailObj in trailObjects)
        {
            if (trailObj != null)
            {
                Destroy(trailObj);
            }
        }
        trailObjects.Clear();
        trailPositions.Clear();
        trailAlphaValues.Clear();
        lastTrailTime = 0f;
        lastPathRefreshTime = 0f;
        lastRecordedPosition = transform.position;
    }

    private void OnDestroy()
    {
        ClearTrail();
    }
}
