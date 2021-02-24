using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    public SpiderLegsController legsController;

    private float _height;
    private Transform _spine;
    private Vector3 _spineBasePosition;
    private float _baseAnimationHeightOffset;
    private bool _initialized;

    public void Init(SpiderLegsController legsController, float height, Transform spine, float baseAnimationHeightOffset)
    {
        this.legsController = legsController;

        _height = height;
        _spine = spine;
        _spineBasePosition = spine.position;
        _baseAnimationHeightOffset = baseAnimationHeightOffset;
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
        BaseAnimation();
    }

    private void AdjustHeight()
    {
        KeyValuePair<Vector3, Vector3> meanPosAndNormal = legsController.GetMeanLegsPosAndNormal();
        Vector3 meanPos = meanPosAndNormal.Key;
        Vector3 meanNormal = meanPosAndNormal.Value;
        Vector3 currentPos = _spineBasePosition;

        _spineBasePosition = Vector3.Lerp(currentPos, meanPos + meanNormal * _height, Time.deltaTime * 2);
    }

    private void AdjustRotation()
    {
        KeyValuePair<Vector3, Vector3> meanLeftAndRightPos = legsController.GetMeanLeftAndRightLegsPos();
        Vector3 leftPos = meanLeftAndRightPos.Key;
        Vector3 rightPos = meanLeftAndRightPos.Value;

        Vector3 right = (rightPos - leftPos).normalized;
        Vector3 newUp = Vector3.Cross(transform.forward, right);
        _spine.up = Vector3.Lerp(_spine.up, newUp, Time.deltaTime);
    }

    private void BaseAnimation()
    {
        // Move up and down a bit
        float offset = _baseAnimationHeightOffset * _height / 10;
        _spine.position = _spineBasePosition + _spine.up * (Mathf.Sin(Time.time * 2) * offset);
    }
}