﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.DTO
{
    public class GameDTO
    {
        public GameObjectDTO[] GameObjects;
        public PlayerDTO[] Players;
        public int GameId;
        public int CharacterId;
        public int MapWidth;
        public int MapHeight;
        public string IPAddress;
    }
}