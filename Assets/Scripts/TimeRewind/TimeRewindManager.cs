using System.Collections.Generic;
using UnityEngine;

public class TimeRewindManager : MonoBehaviour
{
    public static TimeRewindManager instance;

    [Header("Settings")]
    [SerializeField] private float recordDuration = 5f;
    [SerializeField] private float rewindSpeed = 2f;
    [SerializeField] private KeyCode rewindKey = KeyCode.R;
    [SerializeField] private KeyCode confirmKey = KeyCode.R;

    private List<ITimeRewindable> rewindableObjects = new List<ITimeRewindable>();
    private bool isRewinding = false;
    private bool isPreviewing = false;
    private float rewindTimer = 0f;
    private float originalTimeScale;
    private TimeRewindPreview currentPreview;

    public bool IsRewinding => isRewinding;
    public bool IsPreviewing => isPreviewing;
    public float RecordDuration => recordDuration;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        originalTimeScale = Time.timeScale;
    }

    private void Update()
    {
        if (Input.GetKeyDown(rewindKey) && !isRewinding && !isPreviewing)
        {
            StartPreview();
        }
        else if (Input.GetKeyUp(rewindKey) && isRewinding)
        {
            StopRewind();
        }
        else if (Input.GetKeyDown(confirmKey) && isPreviewing)
        {
            ConfirmPreviewSelection();
        }

        if (isPreviewing)
        {
            UpdatePreview();
        }
        else if (isRewinding)
        {
            RewindUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (!isRewinding && !isPreviewing)
        {
            RecordAllStates();
        }
    }

    private void StartPreview()
    {
        isPreviewing = true;
        Time.timeScale = 0.1f; // 慢动作预览效果

        foreach (var rewindable in rewindableObjects)
        {
            var playerRewind = rewindable as PlayerTimeRewind;
            if (playerRewind != null)
            {
                var preview = playerRewind.GetComponent<TimeRewindPreview>();
                if (preview != null)
                {
                    currentPreview = preview;
                    var history = rewindable.GetRecordHistory();
                    preview.StartPreview(history);
                    break;
                }
            }
        }

        AudioManager.instance.PlaySFX(26,null);
    }

    private void UpdatePreview()
    {
        if (currentPreview != null && currentPreview.IsPreviewing)
        {
            currentPreview.UpdatePreview();
        }
        else
        {
            StopPreview();
        }
    }

    private void StopPreview()
    {
        isPreviewing = false;
        Time.timeScale = originalTimeScale;

        if (currentPreview != null)
        {
            currentPreview.StopPreview();
            currentPreview = null;
        }

        AudioManager.instance.StopSFX(26);
    }

    public void ConfirmPreviewSelection()
    {
        if (currentPreview != null)
        {
            var selectedData = currentPreview.GetSelectedState();
            if (selectedData != null)
            {
                // 等待0.3秒后再恢复状态和停止预览，让选中的预览对象有时间显示为黄色
                StartCoroutine(DelayedConfirm(selectedData));
            }
            else
            {
                StopPreview();
            }
        }
        else
        {
            StopPreview();
        }
    }

    private System.Collections.IEnumerator DelayedConfirm(TimeRecordData selectedData)
    {
        // 等待0.3秒，使用与项目中其他地方相同的格式
        yield return new WaitForSeconds(0.01f);
        
        // 恢复到选中状态
        RestoreToSelectedState(selectedData);
        
        // 停止预览
        StopPreview();
    }

    private void RestoreToSelectedState(TimeRecordData data)
    {
        foreach (var rewindable in rewindableObjects)
        {
            rewindable.RestoreState(data);
        }
    }

    public void RegisterRewindable(ITimeRewindable rewindable)
    {
        if (!rewindableObjects.Contains(rewindable))
        {
            rewindableObjects.Add(rewindable);
        }
    }

    public void UnregisterRewindable(ITimeRewindable rewindable)
    {
        if (rewindableObjects.Contains(rewindable))
        {
            rewindableObjects.Remove(rewindable);
        }
    }

    private void RecordAllStates()
    {
        foreach (var rewindable in rewindableObjects)
        {
            rewindable.RecordState();
        }
    }

    public void StartRewind()
    {
        isRewinding = true;
        Time.timeScale = 0f;
        rewindTimer = 0f;

        foreach (var rewindable in rewindableObjects)
        {
            rewindable.IsRewinding = true;
        }

    }

    public void StopRewind()
    {
        isRewinding = false;
        Time.timeScale = originalTimeScale;

        foreach (var rewindable in rewindableObjects)
        {
            rewindable.IsRewinding = false;
        }

    }

    private void RewindUpdate()
    {
        rewindTimer += Time.unscaledDeltaTime * rewindSpeed;

        foreach (var rewindable in rewindableObjects)
        {
            var history = rewindable.GetRecordHistory();
            if (history.Count > 0)
            {
                int targetIndex = Mathf.Clamp(history.Count - 1 - Mathf.FloorToInt(rewindTimer * 50f), 0, history.Count - 1);
                rewindable.RestoreState(history[targetIndex]);
                
                if (targetIndex == 0)
                {
                    StopRewind();
                }
            }
        }
    }

    public void AddFXObject(GameObject fxObject)
    {
        var fxRewind = fxObject.GetComponent<FXTimerRewind>();
        if (fxRewind == null)
        {
            fxRewind = fxObject.AddComponent<FXTimerRewind>();
        }
    }

    public void RemoveFXObject(GameObject fxObject)
    {
        var fxRewind = fxObject.GetComponent<FXTimerRewind>();
        if (fxRewind != null)
        {
            UnregisterRewindable(fxRewind);
        }
    }

    public void ClearAllHistory()
    {
        foreach (var rewindable in rewindableObjects)
        {
            rewindable.ClearHistory();
        }
    }
}
