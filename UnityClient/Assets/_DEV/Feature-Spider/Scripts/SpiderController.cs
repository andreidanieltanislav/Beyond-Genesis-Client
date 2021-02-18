using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    public SpiderLegsController legsController;

    private Vector3 _initialUp;
    private float _height;
    private Transform _spine;
    private bool _initialized;

    public void Init(SpiderLegsController legsController, float height, Transform spine)
    {
        this.legsController = legsController;

        _initialUp = transform.up;
        _height = height;
        _spine = spine;
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized)
        {
            return;
        }

        AdjustHeight();
        AdjustRotation();
    }

    private void AdjustHeight()
    {
        KeyValuePair<Vector3, Vector3> meanPosAndNormal = legsController.GetMeanLegsPosAndNormal();
        Vector3 meanPos = meanPosAndNormal.Key;
        Vector3 meanNormal = meanPosAndNormal.Value;
        Vector3 currentPos = _spine.position;
        // Debug.Log($"MeanPos: {meanPos}; MeanNormal: {meanNormal}; height: {_height}");

        _spine.position = Vector3.Lerp(currentPos, meanPos + meanNormal * _height, Time.deltaTime * 2);
    }

    private void AdjustRotation()
    {
        KeyValuePair<Vector3, Vector3> meanLeftAndRightPos = legsController.GetMeanLeftAndRightLegsPos();
        Vector3 leftPos = meanLeftAndRightPos.Key;
        Vector3 rightPos = meanLeftAndRightPos.Value;

        Vector3 right = (rightPos - leftPos).normalized;
        Vector3 newUp = Vector3.Cross(transform.forward, right);
        // Debug.Log($"new right: {right}; currentForward: {transform.forward}, new up: {newUp}");
        _spine.up = Vector3.Lerp(_spine.up, newUp, Time.deltaTime);
    }
}