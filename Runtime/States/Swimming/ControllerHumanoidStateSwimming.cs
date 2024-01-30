namespace GalaxyGourd.KCC
{
    public class ControllerHumanoidStateSwimming : ControllerKCCState
    {
        #region VARIABLES

        internal override ControllerKCCStateKey Key => ControllerKCCStateKey.Swimming;

        #endregion VARIABLES


        #region CONSTRUCTION

        public ControllerHumanoidStateSwimming(ControllerKCC controller) : base(controller)
        {
            
        }

        #endregion CONSTRUCTION


        #region TRANSITION

        public override void TransitionToState(ControllerKCCStateKey from, string transitionData)
        {
            base.TransitionToState(from, transitionData);
            
            _controller.Motor.SetGroundSolvingActivation(false);
        }
        
        public override void TransitionFromState(ControllerKCCStateKey to)
        {
            base.TransitionFromState(to);
            
        }

        #endregion TRANSITION
    }
}