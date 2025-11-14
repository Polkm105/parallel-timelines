using System.Diagnostics.CodeAnalysis;

namespace parallel_timelines;

public class TimelineManager
{
    private readonly Stack<TimelineFrame> _mainTimeline = new();
    private readonly List<List<TimelineFrame>> _parallelTimelines = new();

    public TimelineManager()
    {
        TryAddFrame(0);
    }
    
    public void AddEvent(ITimelineEvent timelineEvent)
    {
        _mainTimeline.Peek().AddEvent(timelineEvent);
    }
    
    public void TickForward()
    {
        if (!_mainTimeline.TryPeek(out var mainTimelineFrame)) return;
        if (!TryAddFrame(mainTimelineFrame.FrameNumber + 1)) return;
        
        foreach (var frame in _parallelTimelines[mainTimelineFrame.FrameNumber])
        {
            frame.TickForward(frame == mainTimelineFrame);
        }
    }

    public void TickBackward()
    {
        if (!TryRemoveFrame(out var currentFrame)) return;
        if (!_mainTimeline.TryPeek(out var lastMainFrame)) return;

        for (int i = currentFrame.FrameNumber; i <= lastMainFrame.FrameNumber; i++)
        {
            foreach (var frame in _parallelTimelines[i])
            {
                frame.TickForward(false);
            }
        }
        
        foreach (var frame in _parallelTimelines[lastMainFrame.FrameNumber])
        {
            frame.TickBackward(lastMainFrame == frame);
        }
    }

    public void TimeTravel(int frameNumber)
    {
        if (!TryRemoveFrame(out _)) return;
        if (!_mainTimeline.TryPeek(out var mainTimelineFrame)) return;
        if (!TryAddFrame(frameNumber)) return;
        
        // Rewind to frameNumber if it's earlier than the current main frame
        for (int i = mainTimelineFrame.FrameNumber; i >= frameNumber; i--)
        {
            foreach (var frame in _parallelTimelines[i])
            {
                frame.TickBackward(false);
            }
        }

        // FastForward to frameNumber if it's later than the current main frame
        for (int i = mainTimelineFrame.FrameNumber + 1; i < frameNumber; i++)
        {
            foreach (var frame in _parallelTimelines[i])
            {
                frame.TickForward(false);
            }
        }
    }

    private bool TryAddFrame(int frameNumber)
    {
        if (frameNumber > _parallelTimelines.Count || frameNumber < 0) return false;
        
        if (frameNumber == _parallelTimelines.Count)
        {
            _parallelTimelines.Add(new());
        }
        
        var newFrame = new TimelineFrame(frameNumber);
        _parallelTimelines[frameNumber].Add(newFrame);
        _mainTimeline.Push(newFrame);
        return true;
    }

    private bool TryRemoveFrame([NotNullWhen(true)] out TimelineFrame? frame)
    {
        if (!_mainTimeline.TryPop(out frame)) return false;
        _parallelTimelines[frame.FrameNumber].Remove(frame);
        if (_parallelTimelines[frame.FrameNumber].Count == 0)
        {
            _parallelTimelines.RemoveAt(frame.FrameNumber);
        }
        return true;
    }
}
