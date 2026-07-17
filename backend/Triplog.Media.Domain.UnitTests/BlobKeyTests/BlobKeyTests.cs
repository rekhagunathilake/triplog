using FluentAssertions;
using Triplog.Media.Domain.Common;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Domain.UnitTests.BlobKeyTests;

public class BlobKeyTests
{
    [Fact]
    public void ForOwner_ProducesOwnerPrefixedKey()
    {
        var ownerId = OwnerId.NewId();
        var mediaItemId = MediaItemId.NewId();

        var key = BlobKey.ForOwner(ownerId, mediaItemId);

        key.Value.Should().Be($"owners/{ownerId.Value}/{mediaItemId.Value}");
    }

    [Fact]
    public void ForOwner_DifferentOwnersProduceDifferentKeys()
    {
        var mediaItemId = MediaItemId.NewId();
        var key1 = BlobKey.ForOwner(OwnerId.NewId(), mediaItemId);
        var key2 = BlobKey.ForOwner(OwnerId.NewId(), mediaItemId);

        key1.Value.Should().NotBe(key2.Value);
    }

    [Fact]
    public void ForOwner_SameArgumentsProduceEqualKeys()
    {
        var ownerId = OwnerId.NewId();
        var mediaItemId = MediaItemId.NewId();

        var key1 = BlobKey.ForOwner(ownerId, mediaItemId);
        var key2 = BlobKey.ForOwner(ownerId, mediaItemId);

        key1.Should().Be(key2); // record equality
        key1.Value.Should().Be(key2.Value);
    }

    [Fact]
    public void Materialize_PreservesValue()
    {
        const string raw = "owners/some-guid/other-guid";

        var key = BlobKey.Materialize(raw);

        key.Value.Should().Be(raw);
    }
}
