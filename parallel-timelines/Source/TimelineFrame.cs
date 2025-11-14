namespace parallel_timelines;

public class TimelineFrame
{
    public readonly int FrameNumber;
    private readonly List<ITimelineEvent> _events = new();

    public TimelineFrame(int frameNumber)
    {
        FrameNumber = frameNumber;
    }
    
    public void AddEvent(ITimelineEvent e)
    {
        _events.Add(e);
    }

    public void TickForward(bool isMainTimeline)
    {
        foreach (var e in _events)
        {
            e.TickForward(isMainTimeline);
        }
    }

    public void TickBackward(bool isMainTimeline)
    {
        foreach (var e in _events)
        {
            e.TickBackward(isMainTimeline);
        }
    }
}
