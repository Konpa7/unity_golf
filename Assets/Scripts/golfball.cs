using System.Collections.Generic;
using UnityEngine;

public class golfball : MonoBehaviour
{
    public Transform teleportCircleUI;
    LineRenderer lr;
    Vector3 originScale = Vector3.one * 0.02f;
    public float ballRadius = transform.localScale;

    public int linesmooth = 40;//부드러움 정도
    public float curveLength = 50;//커브 걸이 (발사 파워를 정할 것 )
    public float gravity = -60;// 중력
    public float simulateTime = 0.02f;//간격
    public float airfriction = 0.2f;//공기저항
    public float bounciness = 0.8f;//나중에 없애고 plane따라 바꿀것
    public float stopThreshold = 0.5f;//멈춤 판단 기준 속도
    Dictionary<string, float> surfaceFriction = new Dictionary<string, float>()
    {
        {"fairway", 0.2f},
        {"rough", 0.5f}
    };
    

    public float leftRightAngle = 0f;
    public float upDownAngle = 45f;//8~15,20~45,45~60각도가 적당 나중에 각도 조절할 때 제한 설정할 것, 또는 각도 값을 고정으로 둘 것

    //debug var (아래 값은 디버그할 때만 쓰일 것)
    public float deb_anglechange = 0f;
    List<Vector3> lines =  new List<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        teleportCircleUI.gameObject.SetActive(false);

        lr = GetComponent<LineRenderer>();
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger ,ARAVRInput.Controller.LTouch))
        {
            
            lr.enabled = true;
        }
        else if (ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger,ARAVRInput.Controller.LTouch))
        {
            lr.enabled = false;

            if(teleportCircleUI.gameObject.activeSelf)
            {
                Debug.Log("tp");
                GetComponent<CharacterController>().enabled = false;
                transform.position = teleportCircleUI.position + (Vector3.up*0.1f);
                GetComponent<CharacterController>().enabled = true;
            }

            teleportCircleUI.gameObject.SetActive(false);
        }
        else if (ARAVRInput.Get(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.LTouch))
        {
            MakeLines();
        }

        //debug update
        leftRightAngle += deb_anglechange * Time.deltaTime;
    }

    void MakeLines()
    {
        lines.RemoveRange(0,lines.Count);

        Quaternion pitch = Quaternion.AngleAxis(-upDownAngle, transform.right); // -> 유니티에선 right축으로 음수쪽이 윗쪽방향
        Quaternion yaw = Quaternion.AngleAxis(leftRightAngle, transform.up);

        Vector3 dir = pitch * yaw * transform.forward * curveLength;
        Vector3 pos = transform.position;
        lines.Add(pos);

        for(int i=0;i<linesmooth;i++)
        {
            if(pos.y < -10f)
            {
                break;
            }

            Vector3 lastPos = pos;
            dir.y += gravity * simulateTime;
            dir += wind * simulateTime;
            pos += dir * simulateTime;

            if(CheckHitRay(lastPos,ref pos))
            {
                lines.Add(pos);
                break;
            }
            else
            {
                teleportCircleUI.gameObject.SetActive(false);
            }

            lines.Add(pos);
        }

        lr.positionCount = lines.Count;
        lr.SetPositions(lines.ToArray());
    }

    private bool CheckHitRay(Vector3 lastPos, ref Vector3 pos)
    {
        Vector3 rayDir = pos - lastPos;
        Ray ray = new Ray(lastPos, rayDir);
        RaycastHit hitInfo;


        if(Physics.Spherecast(ray,ballRadius, out hitInfo, rayDir.magnitude))
        {
            pos = hitInfo.point + hitInfo.normal * ballRadius;

            int layer = LayerMask.NameToLayer("Terrain");
            if(hitInfo.transform.gameObject.layer == layer)
            {
                float radius = transform.localScale.y;

                //반사
                dir = Vector3.Reflect(dir, hitInfo.normal) * bounciness;

                //마찰
                string tag = hitInfo.collider.tag;
                if(surfaceFriction.ContainsKey(tag))
                {
                    dir *= (1f - surfaceFriction[tag]);
                }

                teleportCircleUI.gameObject.SetActive(true);
                teleportCircleUI.position = pos;
                teleportCircleUI.forward = hitInfo.normal;
                float distance = (pos - ARAVRInput.LHandPosition).magnitude;
                teleportCircleUI.localScale = originScale * Mathf.Max(1, distance);
            }

            return true;
        }    


        return false;
    }
}
