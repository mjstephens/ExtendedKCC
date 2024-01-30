namespace GalaxyGourd.KCC
{
    /// <summary>
    /// Defines necessary properties for a player to facilitate controller + camera communications
    /// </summary>
    public interface IKCCOperator : IInputReceiver<DataInputValuesControllerKCC>, IInputReceiver<DataInputValuesKCCCamera>
    {
        #region PROPERTIES

        VirtualCameraKCC ActivePlayerCamera { get; }
        DataInputValuesKCCCamera CameraInputData { get; set; }
        KCCCameraViewState ViewState { get; }

        #endregion PROPERTIES
    }
}