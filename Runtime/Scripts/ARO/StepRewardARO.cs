namespace Greyrats.JointAI.ARO
{
	public class StepRewardARO : ActionRewardObservation
	{
		public float reward = 0.01f;

		public override void EpisodeReward()
		{
			agent.AddTitledReward(reward * RewardMultiplier, "update");
		}

		public override void Dispose() { }
	}
}