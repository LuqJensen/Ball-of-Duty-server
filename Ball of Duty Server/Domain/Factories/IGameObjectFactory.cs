using Ball_of_Duty_Server.DAO;
using Ball_of_Duty_Server.Domain.GameObjects;
using Ball_of_Duty_Server.DTO;

namespace Ball_of_Duty_Server.Domain.Factories
{
    public interface IGameObjectFactory
    {
        GameObject CreateGameObject(GameObjectDAO data);
        GameObject CreateGameObject(GameObjectDTO data);
    }
}