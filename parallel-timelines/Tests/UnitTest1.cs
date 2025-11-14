using System.Numerics;

namespace parallel_timelines;

public class Tests
{
    private class Player
    {
        public Player() { }
        public Player(Player player)
        {
            Position = player.Position;
            Velocity = player.Velocity;
        }

        public Vector2 Position = Vector2.Zero;
        public Vector2 Velocity = Vector2.Zero;
    }
    
    private class MovementEvent : ITimelineEvent
    {
        private readonly Player _player;
        private readonly Vector2 _movement;

        public MovementEvent(
            Player player,
            Vector2 movement)
        {
            _player = player;
            _movement = movement;
        }

        public void TickForward(bool _)
        {
            _player.Position += _movement;
        }

        public void TickBackward(bool _)
        {
            _player.Position -= _movement;
        }
    }
    
    [SetUp]
    public void Setup() { }

    [Test]
    public void TickForward_Valid()
    {
        var timeline = new TimelineManager();
        var player = new Player();
        var movement = new Vector2(1, 1);
        var count = 10;
        TickForward_MovePlayer(timeline, player, movement, count);
        
        Assert.That(player.Position, Is.EqualTo(movement * count));
    }

    [Test]
    public void TickBackward_Invalid()
    {
        var timeline = new TimelineManager();
        var player = new Player();
        var count = 10;
        
        TickBackward(timeline, count);
        Assert.That(player.Position, Is.EqualTo(Vector2.Zero));
    }

    [Test]
    public void TickForward_TickBackward_Valid()
    {
        var player = new Player();
        var timeline = new TimelineManager();
        var movement = new Vector2(1, 1);
        var count = 5;
        TickForward_MovePlayer(timeline, player, movement, count * 2);
        TickBackward(timeline, count);
        
        Assert.That(player.Position, Is.EqualTo(movement * count));
    }

    [Test]
    public void TickForward_TickBackward_Invalid()
    {
        var timeline = new TimelineManager();
        var player = new Player();
        var movement = new Vector2(1, 1);
        var count = 5;
        
        TickForward_MovePlayer(timeline, player, movement, count);
        TickBackward(timeline, count * 2);
        
        Assert.That(player.Position, Is.EqualTo(Vector2.Zero));
    }

    [Test]
    public void TickForward_TimeTravelBack_Valid()
    {
        var timeline = new TimelineManager();
        var player = new Player();
        var movement = new Vector2(1, 1);
        var count = 5;
        
        TickForward_MovePlayer(timeline, player, movement, count * 2);
        timeline.TimeTravel(count);
        
        Assert.That(player.Position, Is.EqualTo(movement * count));
    }

    [Test]
    public void TickForward_TimeTravelBack_Invalid()
    {
        var timeline = new TimelineManager();
        var player = new Player();
        var movement = new Vector2(1, 1);
        var count = 5;
        
        TickForward_MovePlayer(timeline, player, movement, count);
        timeline.TimeTravel(-1);
        
        Assert.That(player.Position, Is.EqualTo(movement * count));
    }

    [Test]
    public void TickForward_TimeTravelForward_Invalid()
    {
        var timeline = new TimelineManager();
        var player = new Player();
        var movement = new Vector2(1, 1);
        var count = 5;
        
        TickForward_MovePlayer(timeline, player, movement, count);
        timeline.TimeTravel(count + 1);
        
        Assert.That(player.Position, Is.EqualTo(movement * count));
    }
    
    [Test]
    public void TickForward_TimeTravelBack_TickForward_Valid()
    {
        var timeline = new TimelineManager();
        var player = new Player();
        var movement = new Vector2(1, -1);
        var count = 5;
        
        TickForward_MovePlayer(timeline, player, movement, count * 2);
        timeline.TimeTravel(count);
        var player2 = new Player(player);
        TickForward_MovePlayer(timeline, player2, movement * -1, count);
        
        Assert.Multiple(() =>
        {
            Assert.That(player.Position, Is.EqualTo(movement * count * 2));
            Assert.That(player2.Position, Is.EqualTo(Vector2.Zero));
        });
    }

    [Test]
    public void TickForward_TimeTravelBack_TickForward_TimeTravelForward_Valid()
    {
        var timeline = new TimelineManager();
        var player = new Player();
        var movement = new Vector2(1, 1);
        var count = 5;
        
        TickForward_MovePlayer(timeline, player, movement, count * 2);
        timeline.TimeTravel(count);
        var player2 = new Player(player);
        TickForward_MovePlayer(timeline, player2, movement * -1, 2);
        timeline.TimeTravel(count * 2);
        
        Assert.Multiple(() =>
        {
            Assert.That(player.Position, Is.EqualTo(movement * count * 2));
            Assert.That(player2.Position, Is.EqualTo(movement * (count - 2)));
        });
    }

    [Test]
    public void TickForward_TimeTravelBack_TickForward_TickBackward_Valid()
    {
        var timeline = new TimelineManager();
        var player = new Player();
        var movement = new Vector2(1, 1);
        var count = 5;
        
        TickForward_MovePlayer(timeline, player, movement, count * 2);
        timeline.TimeTravel(count);
        var player2 = new Player(player);
        TickForward_MovePlayer(timeline, player2, movement * -1, count);
        TickBackward(timeline, count * 2);
        Assert.Multiple(() =>
        {
            Assert.That(player.Position, Is.EqualTo(movement * count));
            Assert.That(player2.Position, Is.EqualTo(movement * count));
        });
    }


    private void TickForward_MovePlayer(TimelineManager timeline, Player player, Vector2 movement, int count)
    {
        for (int i = 0; i < count; i++)
        {
            timeline.AddEvent(new MovementEvent(player, movement));
            timeline.TickForward();
        }
    }

    private void TickBackward(TimelineManager timeline, int count)
    {
        for (int i = 0; i < count; i++)
        {
            timeline.TickBackward();
        }
    }
}
