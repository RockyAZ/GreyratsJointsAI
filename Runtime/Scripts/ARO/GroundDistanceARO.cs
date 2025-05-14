using Greyrats.JointAI.JointAgent;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Greyrats.JointAI.ARO
{
	public class GroundDistanceARO : ActionRewardObservation
	{
		public string LayerToCheck = "Ground";
		public AnimationCurve functionCurve01;
		public bool useFunctionCurve;
		[Space]
		public bool debug;

		[SerializeField] private GroundDistanceComponent[] components;
		private Dictionary<GroundDistanceComponent, float> savedComponentToDistance;

		public override int ObservationsCount { get => components.Length; }
		public float CurrentDistance { get; private set; }

		public override void Initialize(AgentControllerBase a)
		{
			base.Initialize(a);
			components = agent.AgentMainBone.GetComponentsInChildren<GroundDistanceComponent>();
			savedComponentToDistance = new Dictionary<GroundDistanceComponent, float>(components.Length);
			foreach (var tmp in components)
			{
				savedComponentToDistance.Add(tmp, tmp.GetDistance(LayerToCheck));
				if (debug)
					Debug.Log("savedComponentToDistance::" + savedComponentToDistance[tmp]);
			}
		}

		public override void Dispose()
		{

		}

		public override void EpisodeObservation(VectorSensor sensor)
		{
			foreach (var tmp in components)
			{
				sensor.AddObservation(CalculateCurrentDistanceValue(tmp, false));
			}
		}

		public override void EpisodeReward()
		{
			foreach (var tmp in components)
			{
				agent.AddTitledReward(CalculateCurrentDistanceValue(tmp, true) * RewardMultiplier, "GroundDistance");
			}
		}

		// private float CalculateCurrentDistanceValue(GroundDistanceComponent gdt, bool useCurve)
		// {
		// 	float diff = -(Mathf.Abs(savedComponentToDistance[gdt] - gdt.GetDistance(LayerToCheck)));
		// 	float result = diff / savedComponentToDistance[gdt];
		// 	if (useFunctionCurve && useCurve)
		// 	{
		// 		CurrentDistance = functionCurve01.Evaluate(result + 1); //+1 to convert to 0-1 range
		// 		return CurrentDistance;
		// 	}

		// 	CurrentDistance = result;
		// 	return result;
		// }

		public float CalculateCurrentDistanceValue(GroundDistanceComponent gdt, bool useCurve)
		{
			float initialDistance = savedComponentToDistance[gdt];
			float currentDistance = gdt.GetDistance(LayerToCheck);
			float absoluteDifference = Mathf.Abs(initialDistance - currentDistance);
			return 1.0f / (absoluteDifference + 1.0f);
		}
	}
}