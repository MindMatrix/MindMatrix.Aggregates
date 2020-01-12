namespace MindMatrix.Aggregates.Tests
{
    using System;
    using Shouldly;
    using Xunit;

    public class AggregateVersionTests
    {
        [Fact]
        public void ShouldNotThrowOnPositionVersion()
        {
            new AggregateVersion(0);
            new AggregateVersion(1323123);
            new AggregateVersion(int.MaxValue);
        }
        [Fact]
        public void ShouldThrowInvalidVersion()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => new AggregateVersion(-1));
            Should.Throw<ArgumentOutOfRangeException>(() => new AggregateVersion(int.MinValue));
        }
    }
}
