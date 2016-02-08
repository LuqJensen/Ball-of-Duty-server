using Entity.Components.Physics.Collision;
using Entity.Entities.CharacterSpecializations;

namespace Entity.Entities
{
    public interface ICharacter : IGameObject, ICollidable, IInhibited
    {
        double Score { get; }
        Specializations Specialization { get; }
        double HighScore { get; }

        /// <summary>
        /// Called when character gets a kill. 
        /// Increment killCount. 
        /// Increment score with _scoreUP and _scoreFactor percent of the victims score. 
        /// Notifies its observers (only player should react).  
        /// </summary>
        /// <param name="victim">The victim</param>
        void AddKill(ICharacter victim);
    }
}