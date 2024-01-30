using GalaxyGourd.Input;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    public struct DataInputValuesControllerKCC
    {
        public Vector3 MoveVector;
        public Vector3 LookDirection;
        
        public BoolAction Jump;
        public BoolAction Sprint;
        public BoolAction Walk;
        public BoolAction Crouch;

        public bool TurningLeft;    // If false, we're turning right
    }
}