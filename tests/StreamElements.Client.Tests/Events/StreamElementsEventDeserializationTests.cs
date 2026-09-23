using System.Text.Json;
using StreamElements.Client.Events;
using StreamElements.Client.Internal;

namespace StreamElements.Client.Tests.Events;

[TestClass]
public sealed class StreamElementsEventDeserializationTests
{
    [TestMethod]
    public void DeserializeTipEvent_ReturnsCorrectAmountAndCurrency()
    {
        const string json = """
            {
              "_id": "tip-1",
              "channel": "ch1",
              "type": "tip",
              "provider": "twitch",
              "data": {
                "username": "tipperfoo",
                "displayName": "TipperFoo",
                "amount": 10.50,
                "currency": "USD",
                "message": "Nice stream!",
                "tipId": "tip-abc"
              },
              "createdAt": "2024-03-28T10:00:00Z"
            }
            """;

        StreamElementsTipEvent? evt = JsonSerializer.Deserialize(
            json,
            StreamElementsJsonContext.Default.StreamElementsTipEvent
        );

        Assert.IsNotNull(evt);
        Assert.AreEqual("tip-1", evt.Id);
        Assert.AreEqual("tip", evt.Type);
        Assert.AreEqual(10.50m, evt.Data?.Amount);
        Assert.AreEqual("USD", evt.Data?.Currency);
        Assert.AreEqual("tipperfoo", evt.Data?.Username);
        Assert.AreEqual("Nice stream!", evt.Data?.Message);
    }

    [TestMethod]
    public void DeserializeSubscriberEvent_ReturnsCorrectTierAndStreak()
    {
        const string json = """
            {
              "_id": "sub-1",
              "channel": "ch1",
              "type": "subscriber",
              "provider": "twitch",
              "data": {
                "username": "subfoo",
                "displayName": "SubFoo",
                "amount": 6,
                "tier": "1000",
                "streak": 3,
                "message": "6 months!"
              },
              "createdAt": "2024-03-28T11:00:00Z"
            }
            """;

        StreamElementsSubscriberEvent? evt = JsonSerializer.Deserialize(
            json,
            StreamElementsJsonContext.Default.StreamElementsSubscriberEvent
        );

        Assert.IsNotNull(evt);
        Assert.AreEqual("sub-1", evt.Id);
        Assert.AreEqual("1000", evt.Data?.Tier);
        Assert.AreEqual(3, evt.Data?.Streak);
    }

    [TestMethod]
    public void DeserializeCheerEvent_ReturnsCorrectBitsAmount()
    {
        const string json = """
            {
              "_id": "cheer-1",
              "channel": "ch1",
              "type": "cheer",
              "provider": "twitch",
              "data": {
                "username": "cheerfoo",
                "amount": 500,
                "message": "Cheer500 poggers"
              },
              "createdAt": "2024-03-28T12:00:00Z"
            }
            """;

        StreamElementsCheerEvent? evt = JsonSerializer.Deserialize(
            json,
            StreamElementsJsonContext.Default.StreamElementsCheerEvent
        );

        Assert.IsNotNull(evt);
        Assert.AreEqual(500m, evt.Data?.Amount);
    }

    [TestMethod]
    public void DeserializeFollowEvent_ReturnsCorrectUsername()
    {
        const string json = """
            {
              "_id": "follow-1",
              "channel": "ch1",
              "type": "follow",
              "provider": "twitch",
              "data": { "username": "followfoo", "displayName": "FollowFoo" },
              "createdAt": "2024-03-28T09:00:00Z"
            }
            """;

        StreamElementsFollowEvent? evt = JsonSerializer.Deserialize(
            json,
            StreamElementsJsonContext.Default.StreamElementsFollowEvent
        );

        Assert.IsNotNull(evt);
        Assert.AreEqual("followfoo", evt.Data?.Username);
    }
}
