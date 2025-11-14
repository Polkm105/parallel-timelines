namespace parallel_timelines;

public interface ITimelineEvent
{
    void TickForward(bool isMainTimeline);
    void TickBackward(bool isMainTimeline);
}
