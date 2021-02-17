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

    public bool manuallyInitialized;
    private bool _initialized;

    private Vector3 UpperLegEnd => upperLeg.position + upperLeg.forward * upperLegLength;
    private Vector3 MiddleLegEnd => middleLeg.position + middleLeg.forward * middleLegLength;
    private Vector3 LowerLegEnd => lowerLeg.position + lowerLeg.forward * lowerLegLength;

    public void Init(Transform upperLeg, Transform middleLeg, Transform lowerLeg,
        float upperLegLength, float middleLegLength, float lowerLegLength,
        float delta, int iterations)
    {
        this.upperLeg = upperLeg;
        this.middleLeg = middleLeg;
        this.lowerLeg = lowerLeg;
        this.upperLegLength = upperLegLength;
        this.middleLegLength = middleLegLength;
        this.lowerLegLength = lowerLegLength;
        this.delta = delta;
        this.iterations = iterations;
        
        _totalLength = upperLegLength + middleLegLength + lowerLegLength;
        _previousIKTarget = new SpiderLegIKTarget {Normal = Vector3.zero, Position = Vector3.zero};
        _intermediateIKTarget = _previousIKTarget;
        Debug.Log($"Intermediate {_intermediateIKTarget.Position}");
        
        _initialized = true;
    }

    private void Awake()
    {
        if (manuallyInitialized)
        {
            _totalLength = upperLegLength + middleLegLength + lowerLegLength;
            _previousIKTarget = new SpiderLegIKTarget {Normal = Vector3.zero, Position = Vector3.zero};
            _intermediateIKTarget = _previousIKTarget;
            Debug.Log($"Intermediate {_intermediateIKTarget.Position}");
        }
    }

    private void Update()
    {
        if (!_initialized && !manuallyInitialized)
        {
            return;
        }
        
        Vector3 targetPosition = _currentIKTarget.Position;
        if (_totalLength <= (targetPosition - upperLeg.position).magnitude)
        {
            StretchLeg();
            return;
        }

        SolveIK();
        // TODO: Pole
    }

    private void UpdateLegPosition()
    {
        lowerLeg.position = _currentIKTarget.Position + _currentIKTarget.Normal * lowerLegLength;
        lowerLeg.LookAt(_currentIKTarget.Position);
        _lowerLegRotation = lowerLeg.rotation; // Used to freeze rotation of lower leg
        SolveIK();
    }

    private void SolveIK()
    {
        int ikIterations = iterations;
        // while (ikIterations > 0)
        // Do at least one step, even if delta is still the same, in order to correct limbs orientation
        do
        {
            BackwardIK(ForwardIK());
            ikIterations--;
        } while ((_currentIKTarget.Position - LowerLegEnd).magnitude > delta && ikIterations > 0);
    }

    private Vector3[] ForwardIK()
    {
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