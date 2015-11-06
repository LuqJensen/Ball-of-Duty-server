namespace Ball_of_Duty_Server.Domain.Entities
{
    public class GameObject : Observable
    {
        private static int _gameObjectsCreated;
        public Body Body { get; set; }
        public int Id { get; private set; }

        public GameObject() 
        {
            Id = ++_gameObjectsCreated; // Important to start at 1. 0 will be used as default value.
        }

        public void Destroy()
        {
        }
    }
}