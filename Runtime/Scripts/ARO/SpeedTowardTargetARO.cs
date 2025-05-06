using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Greyrats.JointAI.ARO
{
    public class SpeedTowardTargetARO : ActionRewardObservation
    {
        public enum OrientationType
        {
            Forward,
            Up,
            Right
        }

        [Header("Orientation Setup")]
        public Transform TransformProvider;
        public OrientationType orientationType;
        public Vector3 OrientationOffset;
        public Rigidbody PhysicsDataProvider;
        public RespawningTarget Target;
        public float TargetSpeed = 10f;
        public bool UseDotOrientation = true;
        public bool UseUnitySpeedFunction = true;
        [Header("Debug")]
        public bool DrawDebug = false;
        public float DebugTime = 10f;
        public float DebugScale = 3f;

        public override int ObservationsCount => 9;

        void OnValidate()
        {
            if (DrawDebug)
            {
                Debug.DrawRay(TransformProvider.position, GetCurrentOrientation() * 10, Color.red, DebugTime);
            }
        }

        protected Vector3 GetCurrentOrientation()
        {
            Vector3 orientation;
            switch (orientationType)
            {
                case OrientationType.Forward:
                    orientation = TransformProvider.forward;
                    break;
                case OrientationType.Up:
                    orientation = TransformProvider.up;
                    break;
                case OrientationType.Right:
                    orientation = TransformProvider.right;
                    break;
                default:
                    orientation = TransformProvider.forward;
                    break;
            }
            Quaternion rotation = Quaternion.Euler(OrientationOffset);
            return (rotation * orientation).normalized;
        }

        protected Vector3 GetTargetOrientation()
        {
            return (Target.transform.position - TransformProvider.position).normalized;
        }


        public override void EpisodeReward()
        {
            Vector3 towardTargetVector = GetTargetOrientation();
            Vector3 targetSpeed = towardTargetVector * TargetSpeed;
            float matchSpeedReward;
            if (UseUnitySpeedFunction)
                matchSpeedReward = GetMatchingVelocityRewardUnityExample(targetSpeed, PhysicsDataProvider.linearVelocity);
            else
                matchSpeedReward = GetMatchingVelocityRewardSimple(targetSpeed, PhysicsDataProvider.linearVelocity);

            float matchOrientationReward = UseDotOrientation ?
            GetMatchingOrientationReward(towardTargetVector, GetCurrentOrientation()) :
            1;

            agent.AddTitledReward(matchSpeedReward * matchOrientationReward * RewardMultiplier, "MatchingSpeedAndOrientation");
        }

        public override void EpisodeObservation(VectorSensor sensor)
        {
            base.EpisodeObservation(sensor);
            sensor.AddObservation(PhysicsDataProvider.linearVelocity);
            sensor.AddObservation(GetCurrentOrientation());
            sensor.AddObservation(GetTargetOrientation());
        }

        //normalized value of the difference in avg speed vs goal walking speed.
        public float GetMatchingVelocityRewardUnityExample(Vector3 velocityGoal, Vector3 actualVelocity)
        {
            float agentVel = actualVelocity.magnitude;
            float goalVel = velocityGoal.magnitude;

            if (DrawDebug)
            {
                Vector3 yAdd = new Vector3(0, 1, 0);
                Vector3 deltaAdd = new Vector3(0, 0.3f, 0);
                float velocityClamp = Mathf.Clamp(agentVel / goalVel, 0, 1);
                Debug.DrawRay(TransformProvider.position + yAdd, velocityGoal.normalized * DebugScale, Color.green);
                Debug.DrawRay(TransformProvider.position + yAdd + deltaAdd, actualVelocity.normalized * (DebugScale * velocityClamp), Color.Lerp(Color.red, Color.green, velocityClamp));
            }
            //distance between our actual velocity and goal velocity
            var velDeltaMagnitude = Mathf.Clamp(Vector3.Distance(actualVelocity, velocityGoal), 0, TargetSpeed);

            //return the value on a declining sigmoid shaped curve that decays from 1 to 0
            //This reward will approach 1 if it matches perfectly and approach zero as it deviates
            return Mathf.Pow(1 - Mathf.Pow(velDeltaMagnitude / TargetSpeed, 2), 2);
        }

        //normalized value of the difference in avg speed vs goal walking speed.
        public float GetMatchingVelocityRewardSimple(Vector3 velocityGoal, Vector3 actualVelocity)
        {
            float agentVel = actualVelocity.magnitude;
            float goalVel = velocityGoal.magnitude;
            float velocityClamp = Mathf.Clamp(agentVel / goalVel, 0, 1);

            if (DrawDebug)
            {
                Vector3 yAdd = new Vector3(0, 1, 0);
                Vector3 deltaAdd = new Vector3(0, 0.3f, 0);
                Debug.DrawRay(TransformProvider.position + yAdd, velocityGoal.normalized * DebugScale, Color.green);
                Debug.DrawRay(TransformProvider.position + yAdd + deltaAdd, actualVelocity.normalized * (DebugScale * velocityClamp), Color.Lerp(Color.red, Color.green, velocityClamp));
            }
            return velocityClamp;
        }



        public float GetMatchingOrientationReward(Vector3 orientationGoal, Vector3 actualOrientation)
        {
            if (DrawDebug)
            {
                Debug.DrawRay(TransformProvider.position, orientationGoal * 10, Color.blue);
                Debug.DrawRay(TransformProvider.position, actualOrientation * 10, Color.red);
            }

            // float result = (Vector3.Dot(orientationGoal, actualOrientation) + 1) * 0.5f;
            float result = Vector3.Dot(orientationGoal, actualOrientation);
            if (float.IsNaN(result))
            {
                Debug.LogError("NaN in GetMatchingOrientationReward.\n");
                result = 0;
            }
            return result;
        }

        public override void Dispose()
        {
        }
    }
}