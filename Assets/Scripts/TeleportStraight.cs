using UnityEditor.Profiling;
using UnityEngine;

public class TeleportStraight : MonoBehaviour
{
    public Transform teleportCirclesUI;
    LineRenderer lr;
    Vector3 originScale = Vector3.one * 0.02f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        teleportCirclesUI.gameObject.SetActive(false);

        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(ARAVRInput.Get(ARAVRInput.Button.HandTrigger,ARAVRInput.Controller.LTouch))
        {
            Ray ray = new Ray(ARAVRInput.LHandPosition, ARAVRInput.LHandDirection);
            RaycastHit HitInfo;
            int layer = 1 << LayerMask.NameToLayer("Terrain");

            if(Physics.Raycast(ray, out HitInfo, 200, layer))
            {
                lr.SetPosition(0, ray.origin);
                lr.SetPosition(1, HitInfo.point);

                teleportCirclesUI.gameObject.SetActive(true);
                teleportCirclesUI.position = HitInfo.point;
                teleportCirclesUI.forward = HitInfo.normal;
                teleportCirclesUI.localScale = originScale * Mathf.Max(1,HitInfo.distance);
            }
        }
    }
}