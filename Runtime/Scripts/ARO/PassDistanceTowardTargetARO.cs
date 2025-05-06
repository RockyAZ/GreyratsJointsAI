using Greyrats.JointAI.JointAgent;
using UnityEngine;

namespace Greyrats.JointAI.ARO
{
    public class PassDistanceTowardTargetARO : SpeedTowardTargetARO
    {
        private float startDistance;
        private float minDistanceInEpisode;

        public override void Initialize(AgentControllerBase a)
        {
            base.Initialize(a);
            Target.OnChangedPositionAction += OnTargetChangedPosition;
            startDistance = Vector3.Distance(TransformProvider.position, Target.transform.position);
            agent.ON_FORCE_END += OnTargetChangedPosition;
        }

        public void OnTargetChangedPosition()
        {
            startDistance = Vector3.Distance(TransformProvider.position, Target.transform.position);
            minDistanceInEpisode = startDistance;
        }

        public override void EpisodeReward()
        {
            if(DrawDebug)
            {
                Debug.DrawLine(TransformProvider.position, Target.transform.position, Color.blue);
            }
            float currentDistance = Vector3.Distance(TransformProvider.position, Target.transform.position);
            float diff = minDistanceInEpisode - currentDistance;
            float distancePassedInUpdate = diff / startDistance;

            if(distancePassedInUpdate > 0.0001f)
            {
                distancePassedInUpdate *= RewardMultiplier;
            }

            float distancePassedReward = distancePassedInUpdate;

            Vector3 towardTargetVector = GetTargetOrientation();
            float matchOrientationReward = UseDotOrientation && distancePassedReward > 0f ?
            GetMatchingOrientationReward(towardTargetVector, GetCurrentOrientation()) :
            1;

            agent.AddTitledReward(matchOrientationReward * distancePassedReward, "PassDistance");

            minDistanceInEpisode = Mathf.Min(minDistanceInEpisode, currentDistance);
        }

        public override void Dispose()
        {
            base.Dispose();
            Target.OnChangedPositionAction -= OnTargetChangedPosition;
            agent.ON_FORCE_END -= OnTargetChangedPosition;
        }
    }
}