using GalaxyGourd.Input;
using KinematicCharacterController;

namespace GalaxyGourd.KCC
{
    /// <summary>
    /// Defines interface required to implement kinematic character controller
    /// </summary>
    public interface IKinematicCharacterControllable : IInputReceiver<DataInputValuesControllerKCC>, ICharacterController
    {
        
    }
}