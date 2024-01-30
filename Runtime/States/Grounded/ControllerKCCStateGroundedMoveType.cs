namespace GalaxyGourd.KCC
{
    /// <summary>
    /// The move type describes the current movement "type" as defined by user inputs; note that this is not the same as the
    /// "actual" move type used for gameplay logic, which is a separate value derived from the KCC forward move speed and
    /// posture, which can be found at KCCGroundedLocomotionType
    /// </summary>
    public enum ControllerKCCStateGroundedMoveType
    {
        Walking,    // Explicit walking (right shift on keyboards, not needed for gamepad)
        Default,    // Normal running 
        Sprinting   // Sprint action
    }
}