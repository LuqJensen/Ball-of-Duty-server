using System;

namespace Ball_of_Duty_Server.Domain.Communication
{
    [Flags]
    public enum Opcodes : uint
    {
        BROADCAST_POSITION_UPDATE = 1,
        POSITION_UPDATE = 2,
        REQUEST_BULLET = 4,
        NEW_PLAYER = 8,
        DISCONNECTED_PLAYER = 16,
        BROADCAST_CHARACTER_STAT_UPDATE = 32,
        KILL_NOTIFICATION = 64,
        OBJECT_DESTRUCTION = 128,
        PING = 256,
        UDP_CONNECT = 512,
        SERVER_MESSAGE = 1024,
        TCP_ACTIVITY_OPCODE = REQUEST_BULLET
    }
}