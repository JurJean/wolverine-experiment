using System;

namespace Example;

public static class Commands
{
    public sealed record ScheduleSomething
    {
        public required Guid Id { get; init; }
    }
}