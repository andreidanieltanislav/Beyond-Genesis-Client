using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbController : MonoBehaviour
{
    [SerializeField] private Transform limbTarget;
    [SerializeField] private Transform limbRef;
    [SerializeField] private Transform limbEnd;
    
    [SerializeField] private bool _grounded;

    private Vector3 _desiredPosition;
    private Vector3 _desiredPositionNormal;
    private Vector3 _lastPosition;

    private float _limbOffset;
    private float _stepHeight;
    private float _distanceThreshold;
    private float _walkingSpeed;
    private float _stepOffset;

    private float _percent;
    private static float eps = 0.1f;
    
    public bool Grounded { get => _grounded; }

    public void Init(MovementController movementController, bool initialOffset, int sign)
    {
        // Initialize helping data
        _limbOffset = movementController.limbOffset * sign;
        _stepHeight = movementController.stepHeight;
        _distanceThreshold = movementController.distanceThreshold;
        _walkingSpeed = movementController.walkingSpeed;
        _stepOffset = movementController.stepOffset;

        // Initialize position and auxiliary positions
        Vector3 initialPosition = limbRef.position - limbRef.up * _limbOffset;
        initialPosition += initialOffset ? limbRef.right * _stepOffset : Vector3.zero;
        _desiredPosition = CastOnSurface(initialPosition, 5f, Vector3.up)[0];

        limbTarget.position = _desiredPosition;
        _lastPosition = _desiredPosition;

        _grounded = true;
        _percent = 1;
    }

    void LateUpdate()
    {
        MoveLimb();
    }

    private void MoveLimb()
    {
        limbTarget.position = _lastPosition;
        Vector3 direction = _desiredPosition - _lastPosition;

        if (_percent > 1 - eps)
        {
            _grounded = true;

            // Place foot paralel to ground
            if (_desiredPositionNormal != Vector3.zero)
            {
                int sign = (Vector3.Dot(direction.normalized, transform.forward) < 0 ? -1 : 1);
                limbEnd.rotation = Quaternion.LookRotation(transform.forward * sign, _desiredPositionNormal)
                                        * Quaternion.Euler(180 * Vector3.right);

                // Only for visualization
                limbTarget.rotation = limbEnd.rotation;
            }
            return;
        }

        _percent += Time.deltaTime * _walkingSpeed;

        float heightOffset = Mathf.Sin(_percent * Mathf.PI) * _stepHeight;
        limbTarget.position = _lastPosition + direction * _percent + transform.up * heightOffset;
        _lastPosition = limbTarget.position;

    }

    public void UpdatePosition()
    {
        Vector3 nextPosition = limbRef.position - limbRef.up * _limbOffset;
        Vector3 direction = nextPosition - _lastPosition;
        direction.y = 0;
        nextPosition += direction.normalized * _stepOffset;

        Vector3[] castedLimbRef = CastOnSurface(nextPosition, 5f, Vector3.up);
        _desiredPosition = castedLimbRef[0];
        _desiredPositionNormal = castedLimbRef[1];

        _grounded = false;
        _percent = 0;
    }

    public bool ShouldMove()
    {
        Vector3 nextPosition = limbRef.position - limbRef.up * _limbOffset;
        Vector3[] castedLimbRef = CastOnSurface(nextPosition, 5f, Vector3.up);

        return (_lastPosition - castedLimbRef[0]).magnitude > _distanceThreshold;
    }

    static Vector3[] CastOnSurface(Vector3 point, float halfRange, Vector3 up)
    {
        Vector3[] res = new Vector3[2];
        RaycastHit hit;
        Ray ray = new Ray(point + Vector3.up * halfRange, -up);

        if (Physics.Raycast(ray, out hit, 2f * halfRange))
        {
            res[0] = hit.point;
            res[1] = hit.normal;
        }
        else
        {
            res[0] = point;
        }
        return res;
    }

    public Vector3 GetPosition() => _lastPosition;

    public float GetHeight() => _lastPosition.y;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(limbRef.position, 0.2f);
        Gizmos.DrawWireSphere(_lastPosition, 0.2f);
    }
}
