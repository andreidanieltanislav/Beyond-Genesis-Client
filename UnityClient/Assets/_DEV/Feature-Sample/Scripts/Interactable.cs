using UnityEngine;

namespace _DEV.Feature_Sample.Scripts {
    public class Interactable : MonoBehaviour {

        public float offset = 0.5f;
        public float radius = 3f;
        public Transform interactionTransform;

        private bool _isFocus = false;
        private Transform _player;
        private bool _hasInteracted = false;

        public virtual void Interact() { }

        void Update() {
            CheckInteraction();
        }

        private void CheckInteraction() {
            if (_isFocus && !_hasInteracted) {
                var distance = Vector3.Distance(_player.position, interactionTransform.position);
                if (distance <= radius + offset) {
                    Interact();
                    _hasInteracted = true;
                }
            }
        }

        public void OnFocused(Transform playerTransform) {
            _isFocus = true;
            _player = playerTransform;
            _hasInteracted = false;
        }

        public void OnDeFocused() {
            _isFocus = false;
            _player = null;
            _hasInteracted = false;
        }

        private void OnDrawGizmosSelected() {
            if (interactionTransform == null) {
                interactionTransform = transform;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactionTransform.position, radius);
        }
    }
}