using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeRewindPreview : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float previewDuration = 3f;
    [SerializeField] private float previewInterval = 0.5f;
    [SerializeField] private int maxPreviewCount = 6;
    [SerializeField] private float previewScale = 0.7f;
    [SerializeField] private Color previewColor = new Color(0f, 0.5f, 1f, 0.6f);
    [SerializeField] private Color selectedPreviewColor = new Color(1f, 1f, 0f, 0.8f);

    [Header("References")]
    [SerializeField] private Transform previewContainer;
    [SerializeField] private SpriteRenderer sourceSpriteRenderer;

    private List<GameObject> previewObjects = new List<GameObject>();
    private List<TimeRecordData> previewData = new List<TimeRecordData>();
    private List<float> previewTimestamps = new List<float>();
    private int selectedPreviewIndex = -1;
    private bool isPreviewing = false;
    private float previewStartTime = 0f;

    public bool IsPreviewing => isPreviewing;
    public int SelectedPreviewIndex => selectedPreviewIndex;

    private void Start()
    {
        if (sourceSpriteRenderer == null)
        {
            sourceSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (previewContainer == null)
        {
            GameObject containerObj = new GameObject("PreviewContainer");
            containerObj.transform.parent = transform.parent;
            containerObj.transform.position = transform.position;
            previewContainer = containerObj.transform;
        }
    }

    public void StartPreview(List<TimeRecordData> history)
    {
        if (isPreviewing) return;
        if (history.Count == 0) return;

        isPreviewing = true;
        selectedPreviewIndex = -1;
        previewStartTime = Time.unscaledTime;

        ClearPreviews();
        GeneratePreviews(history);
    }

    public void UpdatePreview()
    {
        if (!isPreviewing) return;

        float elapsed = Time.unscaledTime - previewStartTime;
        float progress = Mathf.Clamp01(elapsed / previewDuration);

        UpdatePreviewPositions(0f); // 保持预览对象静止
        HandleInput();

        if (progress >= 1f)
        {
            StopPreview();
        }
    }

    public void StopPreview()
    {
        isPreviewing = false;
        ClearPreviews();
    }

    public TimeRecordData GetSelectedState()
    {
        if (selectedPreviewIndex >= 0 && selectedPreviewIndex < previewData.Count)
        {
            return previewData[selectedPreviewIndex];
        }
        return null;
    }

    private void GeneratePreviews(List<TimeRecordData> history)
    {
        int step = Mathf.Max(1, history.Count / maxPreviewCount);
        int count = 0;

        for (int i = history.Count - 1; i >= 0; i -= step)
        {
            if (count >= maxPreviewCount) break;
            
            TimeRecordData data = history[i];
            if (data != null)
            {
                CreatePreview(data, count);
                previewData.Add(data);
                previewTimestamps.Add((float)count / maxPreviewCount);
                count++;
            }
        }
    }

    private void CreatePreview(TimeRecordData data, int index)
    {
        if (sourceSpriteRenderer == null) return;

        GameObject previewObj = new GameObject($"TimePreview_{index}");
        previewObj.transform.parent = previewContainer;
        previewObj.transform.position = data.position;
        previewObj.transform.rotation = Quaternion.identity;
        
        // 设置预览对象的scale为1，与player一样大
        Vector3 scale = Vector3.one;
        scale.x *= data.facingDir;
        previewObj.transform.localScale = scale;

        SpriteRenderer previewRenderer = previewObj.AddComponent<SpriteRenderer>();
        // 使用记录时的精灵
        previewRenderer.sprite = GetSpriteForAnimationState(data);
        previewRenderer.sortingLayerName = sourceSpriteRenderer.sortingLayerName;
        previewRenderer.sortingOrder = sourceSpriteRenderer.sortingOrder - 1;
        previewRenderer.color = previewColor;

        // 添加碰撞器用于选择
        BoxCollider2D collider = previewObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1, 2);

        // 创建一个新的GameObject来显示键位，作为独立对象避免受父对象scale影响
        GameObject textObj = new GameObject($"KeyText_{index}");
        textObj.transform.parent = previewContainer; // 直接作为previewContainer的子对象
        
        // 根据朝向调整x偏移量
        float xOffset = data.facingDir > 0 ? -0.4f : 0.4f; // 向右时x=-0.4，向左时x=0.4
        textObj.transform.position = previewObj.transform.position + new Vector3(xOffset, -0.15f, -0.1f); // 调整偏移量，确保正好对准preview中间
        textObj.transform.localScale = Vector3.one;
        textObj.transform.rotation = Quaternion.identity; // 确保文本不会旋转

        // 尝试添加TextMeshPro
        try
        {
            TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = $"{index + 1}";
                textMesh.fontSize = 8; // 缩小字体大小
                textMesh.color = Color.white;
                textMesh.alignment = TextAlignmentOptions.Center;
                textMesh.sortingOrder = 1000; // 设置一个很高的值，确保显示在最前面
                textMesh.GetComponent<Renderer>().sortingLayerName = previewRenderer.sortingLayerName;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Failed to add TextMeshPro: " + e.Message);
            // 降级使用TextMesh
            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = $"{index + 1}";
            textMesh.fontSize = 8; // 缩小字体大小
            textMesh.color = Color.white;
            textMesh.anchor = TextAnchor.MiddleCenter;
            Renderer textRenderer = textMesh.GetComponent<Renderer>();
            if (textRenderer != null)
            {
                textRenderer.sortingOrder = 1000; // 设置一个很高的值，确保显示在最前面
                textRenderer.sortingLayerName = previewRenderer.sortingLayerName;
            }
        }

        previewObjects.Add(previewObj);
    }

    private void UpdatePreviewPositions(float progress)
    {
        for (int i = 0; i < previewObjects.Count; i++)
        {
            if (previewObjects[i] != null && i < previewData.Count)
            {
                TimeRecordData data = previewData[i];
                Vector3 targetPos = data.position;
                
                // 当progress为0时，保持预览对象在原始位置
                Vector3 newPos = targetPos;
                previewObjects[i].transform.position = newPos;

                // 保持正确的朝向
                Vector3 scale = Vector3.one;
                scale.x = data.facingDir;
                previewObjects[i].transform.localScale = scale;

                // 确保精灵保持为记录时的精灵
                SpriteRenderer renderer = previewObjects[i].GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = GetSpriteForAnimationState(data);
                    renderer.color = (i == selectedPreviewIndex) ? selectedPreviewColor : previewColor;
                }

                // 更新键位文本位置和颜色
                // 根据朝向调整x偏移量
                float xOffset = data.facingDir > 0 ? -0.4f : 0.4f; // 向右时x=-0.4，向左时x=0.4
                
                // 查找previewContainer中的TextMeshPro
                TextMeshPro tmpText = previewContainer.Find($"KeyText_{i}")?.GetComponent<TextMeshPro>();
                if (tmpText != null)
                {
                    tmpText.transform.position = newPos + new Vector3(xOffset, -0.15f, -0.1f);
                    tmpText.color = (i == selectedPreviewIndex) ? Color.yellow : Color.white;
                }
                else
                {
                    // 查找previewContainer中的TextMesh
                    TextMesh textMesh = previewContainer.Find($"KeyText_{i}")?.GetComponent<TextMesh>();
                    if (textMesh != null)
                    {
                        textMesh.transform.position = newPos + new Vector3(xOffset, -0.15f, -0.1f);
                        textMesh.color = (i == selectedPreviewIndex) ? Color.yellow : Color.white;
                    }
                }
            }
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                for (int i = 0; i < previewObjects.Count; i++)
                {
                    if (previewObjects[i] == hit.collider.gameObject)
                    {
                        selectedPreviewIndex = i;
                        break;
                    }
                }
            }
        }

        // 键盘选择
        if (Input.GetKeyDown(KeyCode.Q))
        {
            selectedPreviewIndex = Mathf.Max(0, selectedPreviewIndex - 1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            selectedPreviewIndex = Mathf.Min(previewObjects.Count - 1, selectedPreviewIndex + 1);
        }

        // 数字键选择 (Alpha1最新, Alpha6最久)，按数字键直接结束预览并应用选择
        for (int i = 0; i < 6; i++)
        {
            KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" + (i + 1));
            if (Input.GetKeyDown(key))
            {
                // 计算目标索引：Alpha1 -> 0 (最新), Alpha6 -> 5 (最久)
                int targetIndex = i;
                if (targetIndex < previewObjects.Count)
                {
                    selectedPreviewIndex = targetIndex;
                    // 直接结束预览并应用选择
                    TimeRewindManager.instance.ConfirmPreviewSelection();
                }
                break;
            }
        }
    }

    private void ClearPreviews()
    {
        foreach (GameObject obj in previewObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        
        // 清除文本对象
        if (previewContainer != null)
        {
            for (int i = previewContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = previewContainer.GetChild(i);
                if (child.name.StartsWith("KeyText_"))
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        previewObjects.Clear();
        previewData.Clear();
        previewTimestamps.Clear();
        selectedPreviewIndex = -1;
    }

    private void OnDestroy()
    {
        ClearPreviews();
    }

    private Sprite GetSpriteForAnimationState(TimeRecordData data)
    {
        if (data != null && data.currentSprite != null)
        {
            return data.currentSprite;
        }
        if (sourceSpriteRenderer == null) return null;
        return sourceSpriteRenderer.sprite;
    }
}
