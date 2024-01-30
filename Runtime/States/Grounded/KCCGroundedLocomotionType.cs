namespace GalaxyGourd.KCC
{
    /// <summary>
    /// Describes the current state of a grounded KCC based on forward move speed + posture
    /// </summary>
    public enum KCCGroundedLocomotionType
    {
        Idle,
        Walk,
        Run,
        Sprint,
        SlideUnstable,
        CrouchWalk,
        ProneCrawl
    }
}