namespace Example.Collaboration;

public static class Commands
{
    public sealed record OpenRoom
    {
        public required RoomId RoomId { get; init; }
        
        public required string Name { get; init; }
    }

    public sealed record CloseRoom
    {
        public required RoomId RoomId { get; init; }
        
        public required string Reason { get; init; }
    }
}