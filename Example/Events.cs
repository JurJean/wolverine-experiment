using System;

namespace Example;

public static class Events
{
    public sealed record SomethingWasScheduled
    {
        public required Guid Id { get; init; }
    }
}