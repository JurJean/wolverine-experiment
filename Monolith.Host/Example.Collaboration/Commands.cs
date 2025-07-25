﻿namespace Monolith.Host.Example.Collaboration;

public static class Commands
{
    public sealed record OpenRoom
    {
        public required string RoomId { get; init; }
        
        public required string Name { get; init; }
    }

    public sealed record CloseRoom
    {
        public required string RoomId { get; init; }
        
        public required string Reason { get; init; }
    }
}