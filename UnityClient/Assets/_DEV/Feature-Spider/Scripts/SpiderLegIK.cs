using System;
using UnityEngine;

public class SpiderLegIK : MonoBehaviour
{
    public Transform upperLeg;
    public Transform middleLeg;
    public Transform lowerLeg;

    public float upperLegLength = 1f;
    public float middleLegLength = 1f;
    public float lowerLegLength = 0.5f;

    public float delta = 0.01f;
    public int iterations = 10;
    public float legSpeed = 10f;
    public float legRaiseHeight = 0.5f;
    public float timeToMoveLeg = 1f;

    public SpiderLegIKTarget CurrentIKTarget
    {
        get => _currentIKTarget;
        set
        {
            _previousIKTarget = _currentIKTarget;
            _currentIKTarget = value;
            UpdateLegPosition();
        }
    }

    private SpiderLegIKTarget _currentIKTarget;
    private SpiderLegIKTarget _intermediateIKTarget;
    private SpiderLegIKTarget _previousIKTarget;

    private float _totalLength;
    private Quaternion _lowerLegRotation;

    private Vector3 UpperLegEnd => upperLeg.position + upperLeg.forward * upperLegLength;
    private Vector3 MiddleLegEnd => middleLeg.position + middleLeg.forward * middleLegLength;
    private Vector3 LowerLegEnd => lowerLeg.position + lowerLeg.forward * lowerLegLength;

    private void Awake()
    {
        _totalLength = upperLegLength + middleLegLength + lowerLegLength;
        _previousIKTarget = new SpiderLegIKTarget {Normal = Vector3.zero, Position = Vector3.zero};
        _intermediateIKTarget = _previousIKTarget;
        Debug.Log($"Intermediate {_intermediateIKTarget.Position}");
    }

    private void Start()
    {
        SolveIK();
    }

    private void Update()
    {
        Vector3 targetPosition = _currentIKTarget.Position;
        if (_totalLength <= (targetPosition - upperLeg.position).magnitude)
        {
            StretchLeg();
            return;
        }

        // int ikIterations = iterations;
        // while ((targetPosition - LowerLegEnd).magnitude > delta && ikIterations > 0)
        // {
            //ComputeIntermediatePosition();
            SolveIK();
            // TODO: Pole
        //     ikIterations--;
        // }
    }

    private void UpdateLegPosition()
    {
        Debug.Log("Lower leg position updated");
        lowerLeg.position = _currentIKTarget.Position + _currentIKTarget.Normal * lowerLegLength;
        lowerLeg.LookAt(_currentIKTarget.Position);
        _lowerLegRotation = lowerLeg.rotation; // Used to freeze rotation of lower leg
        SolveIK();
    }

    private void SolveIK()
    {
        int ikIterations = iterations;
        // while ((_currentIKTarget.Position - LowerLegEnd).magnitude > delta && ikIterations > 0)
        while (ikIterations > 0)
        {
            BackwardIK(ForwardIK());
            ikIterations--;
        }
    }

    private Vector3[] ForwardIK()
    {
        // Vector3 intermediateLowerLegPos =
        //     _intermediateIKTarget.Position + _intermediateIKTarget.Normal * lowerLegLength;
        Vector3 intermediateLowerLegPos = _currentIKTarget.Position + _currentIKTarget.Normal * lowerLegLength;
        Vector3 intermediateMiddleLegPos = intermediateLowerLegPos -
                                           (intermediateLowerLegPos - middleLeg.position).normalized * middleLegLength;

        return new[] {intermediateMiddleLegPos, intermediateLowerLegPos};
    }

    private void BackwardIK(Vector3[] forwardIKPositions)
    {
        upperLeg.LookAt(forwardIKPositions[0]);
        middleLeg.position = UpperLegEnd;
        middleLeg.LookAt(forwardIKPositions[1]);
        lowerLeg.position = MiddleLegEnd;
        lowerLeg.rotation = _lowerLegRotation;
        // lowerLeg.LookAt(_legIKTarget.Position);
    }

    // To be used when target is out of reach
    private void StretchLeg()
    {
        Vector3 targetPosition = _currentIKTarget.Position;
        upperLeg.LookAt(targetPosition);
        middleLeg.position = UpperLegEnd;
        middleLeg.LookAt(targetPosition);
        lowerLeg.position = MiddleLegEnd;
        lowerLeg.LookAt(targetPosition);
    }

    private void ComputeIntermediatePosition()
    {
        Vector3 targetPos = _currentIKTarget.Position;
        Vector3 targetNormal = _currentIKTarget.Normal;
        Vector3 previousPos = _previousIKTarget.Position;
        Vector3 previousNormal = _previousIKTarget.Normal;
        Vector3 middlePos = Vector3.Lerp(previousPos, targetPos, 0.5f);
        Vector3 middleNormal = Vector3.Lerp(previousNormal, targetNormal, 0.5f);
        float totalDistance = (targetPos - previousPos).magnitude;
        if (totalDistance < delta)
        {
            return;
        }

        /*
         * Compute currentDistance removing the elevation of the foot.
         * Done by creating a plane between the previous and target points
         * and projecting the current position on the plane.
         */
        Plane plane = new Plane(-middleNormal, middlePos);
        Vector3 projectedIntermediatePos = plane.ClosestPointOnPlane(_intermediateIKTarget.Position);
        // Debug.Log($"Projected prev: {plane.ClosestPointOnPlane(previousPos)}, " +
        //           $"target: {plane.ClosestPointOnPlane(targetPos)}; " +
        //           $"actual prev: {previousPos}, target: {targetPos}");
        float currentDistance = (targetPos - projectedIntermediatePos).magnitude;


        // Debug.Log($"targetPos: {targetPos}, currentPos: {_intermediateIKTarget.Position}, " +
        //           $"distance left: {currentDistance}, total distance: {totalDistance} " +
        //           $"prevPos: {previousPos}");

        float t = Mathf.Clamp01(currentDistance * 2 / totalDistance);
        Vector3 nextNormal, nextPos;
        Vector3 raisedMiddlePos = middlePos + middleNormal * legRaiseHeight;
        if (currentDistance > totalDistance / 2)
        {
            nextPos = Vector3.Slerp(previousPos, raisedMiddlePos, t);
            nextNormal = Vector3.Slerp(previousNormal, middleNormal, t);
        }
        else
        {
            nextPos = Vector3.Slerp(raisedMiddlePos, targetPos, t);
            nextNormal = Vector3.Slerp(middleNormal, targetNormal, t);
        }


        // float t = currentDistance / totalDistance;
        // Vector3 nextNormal = Vector3.Slerp(previousNormal, targetNormal, t);
        // if (currentDistance > totalDistance / 4)
        // {
        //     // Raise the leg
        //     targetPos += Vector3.Slerp(previousNormal, targetNormal, 0.5f) * legRaiseHeight;
        // }
        //
        // Vector3 nextPos = Vector3.Slerp(_intermediateIKTarget.Position, targetPos, Time.deltaTime * legSpeed);

        // _intermediateIKTarget.Position = nextPos;
        // _intermediateIKTarget.Normal = nextNormal;
        _intermediateIKTarget = new SpiderLegIKTarget {Normal = nextNormal, Position = nextPos};
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_currentIKTarget.Position, 0.5f);
    }
}

public struct SpiderLegIKTarget
{
    public Vector3 Position;
    public Vector3 Normal;
}