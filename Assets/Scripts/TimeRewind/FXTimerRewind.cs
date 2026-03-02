using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FXTimerRewind : MonoBehaviour, ITimeRewindable
{
    [System.Serializable]
    public class FXStateData
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public bool isPlaying;
        public float lifetime;
        public float normalizedTime;
        public int particleCount;

        public FXStateData(Transform transform, ParticleSystem ps)
        {
            position = transform.position;
            rotation = transform.eulerAngles;
            scale = transform.localScale;
            isPlaying = ps.isPlaying;
            lifetime = ps.main.duration;
            normalizedTime = ps.time / lifetime;
            particleCount = ps.particleCount;
        }
    }

    private ParticleSystem particleSystem;
    private List<FXStateData> fxStateHistory = new List<FXStateData>();
    private List<TimeRecordData> recordHistory = new List<TimeRecordData>();
    
    public bool IsRewinding { get; set; }

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
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

        var fxState = new FXStateData(transform, particleSystem);
        fxStateHistory.Add(fxState);

        var record = new TimeRecordData(
            transform, 
            null, 
            1, 
            0,
            "",
            1f,
            null
        );
        recordHistory.Add(record);

        int maxRecords = Mathf.CeilToInt(TimeRewindManager.instance.RecordDuration * 50f);
        if (fxStateHistory.Count > maxRecords)
        {
            fxStateHistory.RemoveAt(0);
        }

        if (recordHistory.Count > maxRecords)
        {
            recordHistory.RemoveAt(0);
        }
    }

    public void RestoreState(TimeRecordData data)
    {
        if (data == null) return;

        transform.position = data.position;
        transform.rotation = Quaternion.Euler(0, data.rotation, 0);

        int index = recordHistory.IndexOf(data);
        if (index >= 0 && index < fxStateHistory.Count)
        {
            RestoreFXState(fxStateHistory[index]);
        }
    }

    private void RestoreFXState(FXStateData state)
    {
        if (particleSystem == null) return;

        transform.position = state.position;
        transform.eulerAngles = state.rotation;
        transform.localScale = state.scale;

        if (state.isPlaying && !particleSystem.isPlaying)
        {
            particleSystem.Play();
        }
        else if (!state.isPlaying && particleSystem.isPlaying)
        {
            particleSystem.Pause();
        }

        // 重置粒子系统状态
        particleSystem.Stop();
        particleSystem.Clear();
        
        if (state.isPlaying)
        {
            particleSystem.Simulate(state.normalizedTime * state.lifetime, true, true);
            particleSystem.Play();
        }
    }

    public List<TimeRecordData> GetRecordHistory()
    {
        return recordHistory;
    }

    public void ClearHistory()
    {
        recordHistory.Clear();
        fxStateHistory.Clear();
    }
}
