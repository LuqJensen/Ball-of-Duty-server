namespace GameObject.GameObjects.Components.Physics.Collision
{
    public interface ICollidable
    {
        /// <summary>
        /// Allows a consumer of this contract to define the logic to be
        /// executed if it collides with another ICollidable.
        /// </summary>
        /// <param name="other"></param>
        void CollideWith(ICollidable other);

        /// <summary>
        /// Allows a consumer of this contract to define whether or not it
        /// has any logic to be run if it collides with another ICollidable.
        /// </summary>
        /// <param name="other"></param>
        /// <returns> true if "other" matches the criterias set by this consumer. false otherwise. </returns>
        bool CollisionCriteria(ICollidable other);

        /// <summary>
        /// Allows a consumer of this contract to define special logic deciding
        /// whether or not it collides with another ICollidable.
        /// TODO: This kind of violates separation of concerns.
        /// </summary>
        /// <param name="other"></param>
        /// <returns> true if "other" collides with this consumer. false otherwise. </returns>
        bool IsCollidingSpecial(ICollidable other);
    }
}