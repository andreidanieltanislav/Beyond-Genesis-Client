using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SpiderLegIK : MonoBehaviour
{
    public Transform upperLeg;
    public Transform middleLeg;
    public Transform lowerLeg;

    public Transform target;
    public float delta = 0.01f;

    private readonly float _totalLength = 3f;
    private Vector3 _initialUpperLegLocalPos;
    private Vector3 _initialMiddleLegLocalPos;
    private Vector3 _initialLowerLegLocalPos;

    private Vector3 UpperLegEnd => upperLeg.position + upperLeg.forward * upperLeg.lossyScale.z;
    private Vector3 MiddleLegEnd => middleLeg.position + middleLeg.forward * middleLeg.lossyScale.z;
    private Vector3 LowerLegEnd => lowerLeg.position + lowerLeg.forward * lowerLeg.lossyScale.z;

    private float UpperLegLength => upperLeg.lossyScale.z;
    private float MiddleLegLength => middleLeg.lossyScale.z;
    private float LowerLegLength => lowerLeg.lossyScale.z;

    private void Awake()
    {
        _initialUpperLegLocalPos = upperLeg.localPosition;
        _initialMiddleLegLocalPos = middleLeg.localPosition;
        _initialLowerLegLocalPos = lowerLeg.localPosition;

        EditorApplication.update += Update;
    }

    private void Update()
    {
        // if (_totalLength <= (target.position - upperLeg.position).magnitude)
        // {
        //     StretchLeg();
        //     return;
        // }

        if ((target.position - LowerLegEnd).magnitude > delta)
        {
            BackwardIK(ForwardIK());
        }
    }

    private Vector3[] ForwardIK()
    {
        // Vector3.up can be replaced with the normal of the walked surface
        Vector3 intermediateLowerLegPos = target.position + Vector3.up * LowerLegLength;
        Vector3 intermediateMiddleLegPos = intermediateLowerLegPos -
                                           (intermediateLowerLegPos - middleLeg.position).normalized * MiddleLegLength;
        Vector3 intermediateUpperLegPos = (upperLeg.position - intermediateMiddleLegPos).normalized * UpperLegLength;

        return new[] {intermediateUpperLegPos, intermediateMiddleLegPos, intermediateLowerLegPos};
    }

    private void BackwardIK(Vector3[] forwardIKPositions)
    {
        upperLeg.LookAt(forwardIKPositions[1]);
        middleLeg.position = UpperLegEnd;
        middleLeg.LookAt(forwardIKPositions[2]);
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