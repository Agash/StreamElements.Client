using System.Text.Json;
using StreamElements.Client.Events;
using Xunit;

namespace StreamElements.Client.Tests.Events;

public sealed class StreamElementsEventDeserializationTests
{
    private static readonly JsonSerializerOptions s_opts = new(JsonSerializerDefaults.Web);

    [Fact]
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

        StreamElementsTipEvent? evt = JsonSerializer.Deserialize<StreamElementsTipEvent>(
            json,
            s_opts
        );

        Assert.NotNull(evt);
        Assert.Equal("tip-1", evt.Id);
        Assert.Equal("tip", evt.Type);
        Assert.Equal(10.50m, evt.Data?.Amount);
        Assert.Equal("USD", evt.Data?.Currency);
        Assert.Equal("tipperfoo", evt.Data?.Username);
        Assert.Equal("Nice stream!", evt.Data?.Message);
    }

    [Fact]
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

        StreamElementsSubscriberEvent? evt =
            JsonSerializer.Deserialize<StreamElementsSubscriberEvent>(json, s_opts);

        Assert.NotNull(evt);
        Assert.Equal("sub-1", evt.Id);
        Assert.Equal("1000", evt.Data?.Tier);
        Assert.Equal(3, evt.Data?.Streak);
    }

    [Fact]
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

        StreamElementsCheerEvent? evt = JsonSerializer.Deserialize<StreamElementsCheerEvent>(
            json,
            s_opts
        );

        Assert.NotNull(evt);
        Assert.Equal(500m, evt.Data?.Amount);
    }

    [Fact]
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

        StreamElementsFollowEvent? evt = JsonSerializer.Deserialize<StreamElementsFollowEvent>(
            json,
            s_opts
        );

        Assert.NotNull(evt);
        Assert.Equal("followfoo", evt.Data?.Username);
    }
}
