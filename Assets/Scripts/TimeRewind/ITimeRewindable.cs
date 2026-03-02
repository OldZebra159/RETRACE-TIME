using System.Collections.Generic;

public interface ITimeRewindable
{
    void RecordState();
    void RestoreState(TimeRecordData data);
    List<TimeRecordData> GetRecordHistory();
    void ClearHistory();
    bool IsRewinding { get; set; }
}
