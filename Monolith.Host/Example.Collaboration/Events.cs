﻿namespace Monolith.Host.Example.Collaboration;

public static class Events
{
    public sealed record RoomOpened
    {
        public required string RoomId { get; init; }
        
        public required string Name { get; init; }
    }

    public sealed record RoomClosed
    {
        public required string RoomId { get; init; }
        
        public required string Reason { get; init; }
    }
}