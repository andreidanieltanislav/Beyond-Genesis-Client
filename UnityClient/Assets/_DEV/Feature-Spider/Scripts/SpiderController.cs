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
        if (!_initialized || Time.realtimeSinceStartup <= 5)
        {
            return;
        }

        AdjustHeight();
    }

    private void AdjustHeight()
    {
        KeyValuePair<Vector3, Vector3> meanPosAndNormal = legsController.GetMeanPosAndNormal();
        Vector3 meanPos = meanPosAndNormal.Key;
        Vector3 meanNormal = meanPosAndNormal.Value;
        Vector3 currentPos = _spine.position;
        Debug.Log($"MeanPos: {meanPos}; MeanNormal: {meanNormal}; height: {_height}");

        _spine.position = Vector3.Lerp(currentPos, meanPos + meanNormal * _height, Time.deltaTime);
    }
}
