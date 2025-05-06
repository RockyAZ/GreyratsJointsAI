using Greyrats.JointAI.JointAgent;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Greyrats.JointAI.ARO
{
	public class ContactARO : ActionRewardObservation
	{
		public string[] FatalTags = { "Ground" };
		public ContactComponent[] components;
		[Space]
		public bool endOnFatalContactFORCE;
		public bool dontEndOnFatalContactFORCE;
		public bool observeRewardContacting;
		[Space]
		public float FatalContactReward = -1;
		public float updateContactReward = -0.01f;

		public override int ObservationsCount { get => observeRewardContacting ? components.Length : 0; }

		void OnCollision(ContactType ctype, ContactComponent cc)
		{
			if ((endOnFatalContactFORCE || cc.EndEpisodeOnFatalContact) && !dontEndOnFatalContactFORCE)
				agent.ForceEndEpisode(FatalContactReward);
		}

		public override void EpisodeBegin()
		{
			foreach (var tmp in components)
				tmp.ResetContact();
		}

		public override void Initialize(AgentControllerBase a)
		{
			base.Initialize(a);
			components = agent.AgentMainBone.GetComponentsInChildren<ContactComponent>();
			foreach (var tmp in components)
			{
				tmp.Initialize(FatalTags);
				tmp.COLLISION += OnCollision;
			}
		}

		public override void Dispose()
		{
			foreach (var tmp in components)
			{
				tmp.COLLISION -= OnCollision;
			}
		}

		public override void EpisodeObservation(VectorSensor sensor)
		{
			if (observeRewardContacting)
			{
				foreach (var tmp in components)
				{
					sensor.AddObservation(tmp.Contacting);
				}
			}
		}

		public override void EpisodeReward()
		{
			if (observeRewardContacting)
			{
				foreach (var tmp in components)
				{
					if (tmp.Contacting)
						agent.AddTitledReward(updateContactReward * RewardMultiplier, "contact");
				}
			}
		}
	}
}