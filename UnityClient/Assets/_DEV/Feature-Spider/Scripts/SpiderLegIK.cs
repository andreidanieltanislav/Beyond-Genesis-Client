using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SpiderLegIK : MonoBehaviour
{
    public Transform upperLeg;
    public Transform middleLeg;
    public Transform lowerLeg;

    public float upperLegLength = 1f;
    public float middleLegLength = 1f;
    public float lowerLegLength = 0.5f;

    public Transform target;
    public float delta = 0.01f;

    private float _totalLength;
    private Vector3 _initialUpperLegLocalPos;
    private Vector3 _initialMiddleLegLocalPos;
    private Vector3 _initialLowerLegLocalPos;

    private Vector3 UpperLegEnd => upperLeg.position + upperLeg.forward * upperLegLength;
    private Vector3 MiddleLegEnd => middleLeg.position + middleLeg.forward * middleLegLength;
    private Vector3 LowerLegEnd => lowerLeg.position + lowerLeg.forward * lowerLegLength;

    private void Awake()
    {
        _initialUpperLegLocalPos = upperLeg.localPosition;
        _initialMiddleLegLocalPos = middleLeg.localPosition;
        _initialLowerLegLocalPos = lowerLeg.localPosition;
        _totalLength = upperLegLength + middleLegLength + lowerLegLength;

        EditorApplication.update += Update;
    }

    private void Update()
    {
        if (_totalLength <= (target.position - upperLeg.position).magnitude)
        {
            StretchLeg();
            return;
        }

        if ((target.position - LowerLegEnd).magnitude > delta)
        {
            BackwardIK(ForwardIK());
        }
    }

    private Vector3[] ForwardIK()
    {
        // Vector3.up can be replaced with the normal of the walked surface
        Vector3 intermediateLowerLegPos = target.position + Vector3.up * lowerLegLength;
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
        lowerLeg.LookAt(target);
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
        upperLeg.LookAt(target);
        middleLeg.position = UpperLegEnd;
        middleLeg.LookAt(target);
        lowerLeg.position = MiddleLegEnd;
        lowerLeg.LookAt(target);
    }
}