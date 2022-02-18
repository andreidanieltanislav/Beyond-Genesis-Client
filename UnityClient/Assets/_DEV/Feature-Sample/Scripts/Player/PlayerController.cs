using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace _DEV.Feature_Sample.Scripts.Player {
    public class PlayerController : MonoBehaviour {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private int raycastDistance = 100;

        private PlayerMotor _playerMotor;
        private Interactable _focus;
        private Camera _camera;

        private void Awake() {
            _playerMotor = GetComponent<PlayerMotor>();
        }

        private void Start() {
            _camera = Camera.main;
        }

        void Update() {
            CheckClick();
        }

        private void CheckClick() {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            ProcessMovement();
            ProcessInteractions();
        }

        private void ProcessMovement() {
            if (Input.GetMouseButtonDown((int) MouseButton.LeftMouse)) {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, raycastDistance, layerMask)) {
                    _playerMotor.MoveToPoint(hit.point);
                    ClearFocus();
                }
            }
        }

        private void ProcessInteractions() {
            if (Input.GetMouseButtonDown((int) MouseButton.RightMouse)) {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, raycastDistance)) {
                    var interactable = hit.collider.GetComponent<Interactable>();
                    if (interactable != null) {
                        SetFocus(interactable);
                    }
                }
            }
        }

        private void SetFocus(Interactable interactable) {
            if (_focus != interactable) {
                if (_focus != null) {
                    _focus.OnDeFocused();
                }

                _focus = interactable;
                _playerMotor.FollowTarget(interactable);
            }
        
            interactable.OnFocused(transform);
        }

        private void ClearFocus() {
            if (_focus) {
                _focus.OnDeFocused();
            }

            _focus = null;
            _playerMotor.StopFollowTarget();
        }
    }
}