using System;

namespace GameObject.DTO
{
    [Serializable]
    public struct GameObjectDTO
    {
        public int Id;
        public BodyDTO Body;
        public PhysicsDTO Physics;
        public int Type;
        public int BulletType;
        public int Specialization;
    }
}