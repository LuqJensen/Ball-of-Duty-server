using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.Domain.GameObjects;

namespace Ball_of_Duty_Server.Domain.Factories
{
    public static class GameObjectFactory
    {
        private static Dictionary<EntityType, Func<GameObject>> _factories = new Dictionary<EntityType, Func<GameObject>>();
    }
}