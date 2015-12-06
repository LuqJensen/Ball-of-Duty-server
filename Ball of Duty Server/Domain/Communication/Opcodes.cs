namespace Ball_of_Duty_Server.Domain.Communication
{
    public enum Opcodes : byte
    {
        BROADCAST_POSITION_UPDATE = 1,
        POSITION_UPDATE = 2,
        REQUEST_BULLET = 3,
        NEW_PLAYER = 4,
        DISCONNECTED_PLAYER = 5,
        BROADCAST_SCORE_UPDATE = 6,
        BROADCAST_HEALTH_UPDATE = 7,
        KILL_NOTIFICATION = 8,
        OBJECT_DESTRUCTION = 9,
        PING = 10,
        UDP_CONNECT = 11
    }
}