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

    public SpiderLegIKTarget LegIKTarget
    {
        get => _legIKTarget;
        set
        {
            _legIKTarget = value;
            UpdateLegPosition();
        }
    }

    private SpiderLegIKTarget _legIKTarget;
    private float _totalLength;
    private Vector3 _initialUpperLegLocalPos;
    private Vector3 _initialMiddleLegLocalPos;
    private Vector3 _initialLowerLegLocalPos;
    private Quaternion _lowerLegRotation;

    private Vector3 UpperLegEnd => upperLeg.position + upperLeg.forward * upperLegLength;
    private Vector3 MiddleLegEnd => middleLeg.position + middleLeg.forward * middleLegLength;
    private Vector3 LowerLegEnd => lowerLeg.position + lowerLeg.forward * lowerLegLength;

    private void Awake()
    {
        _initialUpperLegLocalPos = upperLeg.localPosition;
        _initialMiddleLegLocalPos = middleLeg.localPosition;
        _initialLowerLegLocalPos = lowerLeg.localPosition;
        _totalLength = upperLegLength + middleLegLength + lowerLegLength;
    }

    private void Update()
    {
        Vector3 targetPosition = _legIKTarget.Position;
        if (_totalLength <= (targetPosition - upperLeg.position).magnitude)
        {
            StretchLeg();
            return;
        }

        if ((targetPosition - LowerLegEnd).magnitude > delta)
        {
            BackwardIK(ForwardIK());
            // TODO: Freeze leg, follow target when distance is greater, Pole
        }
    }
    
    private void UpdateLegPosition()
    {
        lowerLeg.position = _legIKTarget.Position + _legIKTarget.Normal * lowerLegLength;
        lowerLeg.LookAt(_legIKTarget.Position);
        _lowerLegRotation = lowerLeg.rotation; // Used to freeze rotation of lower leg
    }

    private Vector3[] ForwardIK()
    {
        // Vector3.up can be replaced with the normal of the walked surface
        // Vector3 intermediateLowerLegPos = _targetPosition + Vector3.up * lowerLegLength;
        Vector3 intermediateLowerLegPos = _legIKTarget.Position + _legIKTarget.Normal * lowerLegLength;
        Vector3 intermediateMiddleLegPos = intermediateLowerLegPos -
                                           (intermediateLowerLegPos - middleLeg.position).normalized * middleLegLength;

        return new[] {intermediateMiddleLegPos, intermediateLowerLegPos};
    }

    private void BackwardIK(Vector3[] forwardIKPositions)
    {
        upperLeg.LookAt(forwardIKPositions[0]);
        middleLeg.position = UpperLegEnd;
        middleLeg.LookAt(forwardIKPositions[1]);
        // middleLeg.LookAt(lowerLeg);
        lowerLeg.position = MiddleLegEnd;
        lowerLeg.rotation = _lowerLegRotation;
        // lowerLeg.LookAt(_legIKTarget.Position);
    }

    // To be used when target is out of reach
    private void StretchLeg()
    {
        // TODO: Further experiment
        // upperLeg.LookAt(target);
        // upperLeg.localPosition = _initialUpperLegLocalPos;
        // middleLeg.LookAt(target);
        // middleLeg.localPosition = _initialMiddleLegLocalPos;
        // lowerLeg.LookAt(target);
        // lowerLeg.localPosition = _initialLowerLegLocalPos;
        Vector3 targetPosition = _legIKTarget.Position;
        upperLeg.LookAt(targetPosition);
        middleLeg.position = UpperLegEnd;
        middleLeg.LookAt(targetPosition);
        lowerLeg.position = MiddleLegEnd;
        lowerLeg.LookAt(targetPosition);
    }
}

public struct SpiderLegIKTarget
{
    public Vector3 Position;
    public Vector3 Normal;
}