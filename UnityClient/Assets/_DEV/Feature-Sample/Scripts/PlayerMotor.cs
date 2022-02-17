using UnityEngine;
using UnityEngine.AI;

namespace _DEV.Feature_Sample.Scripts {
    public class PlayerMotor : MonoBehaviour {
        [SerializeField] private float rotationDeltaTimeModifier;

        private NavMeshAgent _agent;
        private Transform _target;

        private void Awake() {
            _agent = GetComponent<NavMeshAgent>();
        }

        void Update() {
            if (_target) {
                FaceTarget();
                _agent.SetDestination(_target.position);
            }
        }

        public void MoveToPoint(Vector3 point) {
            _agent.SetDestination(point);
        }

        public void FollowTarget(Interactable newTarget) {
            _agent.stoppingDistance = newTarget.radius;
            _agent.updateRotation = false;
            _target = newTarget.interactionTransform;
        }

        public void StopFollowTarget() {
            _agent.stoppingDistance = 0f;
            _agent.updateRotation = true;
            _target = null;
        }

        private void FaceTarget() {
            Vector3 direction = (_target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation =
                Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationDeltaTimeModifier);
        }
    }
}