using ExtremeRoles.UnitTest.Mocks;
using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.Interface;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Event;

public class EventManagerTests : SerialTestBase, IClassFixture<SerialFixture>
{
    public EventManagerTests(SerialFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void RegisterAndInvoke_CallsSubscribers()
    {
        var manager = new EventManager();
        var mockSubscriber = new Mock<ISubscriber>();
        mockSubscriber.Setup(s => s.Invoke()).Returns(true);

        manager.Register(mockSubscriber.Object, ModEvent.VisualUpdate);
        manager.Invoke(ModEvent.VisualUpdate);

        mockSubscriber.Verify(s => s.Invoke(), Times.Once);
    }

    [Fact]
    public void Invoke_UnregisteredEvent_DoesNotThrow()
    {
        var manager = new EventManager();
        var exception = Record.Exception(() => manager.Invoke(ModEvent.OptionUpdate));
        Assert.Null(exception);
    }

    [Fact]
    public void Invoke_RemovesSubscriber_WhenInvokeReturnsFalse()
    {
        var manager = new EventManager();
        var mockSubscriber1 = new Mock<ISubscriber>();
        mockSubscriber1.Setup(s => s.Invoke()).Returns(false);

        var mockSubscriber2 = new Mock<ISubscriber>();
        mockSubscriber2.Setup(s => s.Invoke()).Returns(true);

        manager.Register(mockSubscriber1.Object, ModEvent.VisualUpdate);
        manager.Register(mockSubscriber2.Object, ModEvent.VisualUpdate);

        // First invoke: both called, subscriber1 returns false and should be removed
        manager.Invoke(ModEvent.VisualUpdate);
        mockSubscriber1.Verify(s => s.Invoke(), Times.Once);
        mockSubscriber2.Verify(s => s.Invoke(), Times.Once);

        // Second invoke: only subscriber2 called
        manager.Invoke(ModEvent.VisualUpdate);
        mockSubscriber1.Verify(s => s.Invoke(), Times.Once); // Still once
        mockSubscriber2.Verify(s => s.Invoke(), Times.Exactly(2));
    }

    [Fact]
    public void MultipleSubscribers_InvokedInOrder()
    {
        var manager = new EventManager();
        var executionOrder = new System.Collections.Generic.List<int>();

        var sub1 = new Mock<ISubscriber>();
        sub1.Setup(s => s.Invoke()).Callback(() => executionOrder.Add(1)).Returns(true);

        var sub2 = new Mock<ISubscriber>();
        sub2.Setup(s => s.Invoke()).Callback(() => executionOrder.Add(2)).Returns(true);

        manager.Register(sub1.Object, ModEvent.OptionUpdate);
        manager.Register(sub2.Object, ModEvent.OptionUpdate);

        manager.Invoke(ModEvent.OptionUpdate);

        Assert.Equal(new[] { 1, 2 }, executionOrder);
    }
}