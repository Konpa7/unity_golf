using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class golfball : MonoBehaviour
{
    public Transform teleportCircleUI;
    public TMPro.TextMeshProUGUI windtextUI;
    public RectTransform windArrowUI;

    LineRenderer lr;
    Vector3 originScale = Vector3.one * 0.02f;
    public float ballRadius = 0.1f;

    public int linesmooth = 500;//부드러움 정도 -> (선 길이)
    public float curveLength = 50;//커브 걸이 (발사 파워를 정할 것 )
    public float gravity = -60;// 중력
    public float airfriction = 0.005f;//공기저항
    public float bounciness = 0.7f;//나중에 없애고 plane따라 바꿀것
    public float stopThreshold = 0.2f;//멈춤 판단 기준 속도
    Dictionary<string, float> surfaceFriction = new Dictionary<string, float>()
    {
        {"fairway", 0.2f},
        {"rough", 0.5f}
    };
    Dictionary<string, float> surfaceBounciness = new Dictionary<string, float>()
    {
        {"fairway", 0.7f},
        {"rough", 0.5f}
    };
    

    public float leftRightAngle = 0f;
    public float upDownAngle = 45f;//8~15,20~45,45~60각도가 적당 나중에 각도 조절할 때 제한 설정할 것, 또는 각도 값을 고정으로 둘 것

    //debug var (아래 값은 디버그할 때만 쓰일 것)
    public float deb_anglechange = 0f;

    public Vector3 wind = new Vector3(5f,0f,0f);
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
                //transform.position = teleportCircleUI.position + (Vector3.up*0.1f);
                GetComponent<CharacterController>().enabled = true;

                StartCoroutine(FollowLineSmooth());
            }

            teleportCircleUI.gameObject.SetActive(false);
        }
        else if (ARAVRInput.Get(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.LTouch))
        {
            MakeLines();
        }

        //debug update  
        UpdateWindText();//!! 옮길것

        leftRightAngle += deb_anglechange * Time.deltaTime;//완전 디버그용
    }

    void MakeLines()
    {
        float simulateTime = Time.fixedDeltaTime;//간격

        lines.RemoveRange(0,lines.Count);

        Quaternion pitch = Quaternion.AngleAxis(-upDownAngle, transform.right); // -> 유니티에선 right축으로 음수쪽이 윗쪽방향
        Quaternion yaw = Quaternion.AngleAxis(leftRightAngle, transform.up);

        Vector3 dir = pitch * yaw * transform.forward * curveLength;
        Vector3 pos = transform.position;
        lines.Add(pos);

        for(int i=0;i<linesmooth;i++)
        {
            if (dir.magnitude < stopThreshold) {
                teleportCircleUI.gameObject.SetActive(true);
                teleportCircleUI.position = pos;
                teleportCircleUI.forward = Vector3.up;
                float distance = (pos - ARAVRInput.LHandPosition).magnitude;
                teleportCircleUI.localScale = originScale * Mathf.Max(1, distance);
                break;
            }
            //공 굴러가게 y속도 제거

            Vector3 lastPos = pos;
            dir.y += gravity * simulateTime;
<<<<<<< Updated upstream
            dir += wind * simulateTime;
=======
>>>>>>> Stashed changes
            pos += dir * simulateTime;

            if(CheckHitRay(lastPos, ref pos, ref dir))
            {//선이 바닥과 충돌했을 경우
                lines.Add(pos);
                //break;
            }
            else
            {
                teleportCircleUI.gameObject.SetActive(false);
            }

            dir *= (1f - airfriction * simulateTime);//공기저항 적용

            lines.Add(pos);
        }   

        lr.positionCount = lines.Count;
        lr.SetPositions(lines.ToArray());
    }

    private bool CheckHitRay(Vector3 lastPos, ref Vector3 pos, ref Vector3 dir)
    {
        Vector3 rayDir = pos - lastPos;
        Ray ray = new Ray(lastPos, rayDir);
        RaycastHit hitInfo;

<<<<<<< Updated upstream
        if(Physics.Raycast(ray,out hitInfo, rayDir.magnitude))
        {
            float radius = transform.localScale.y; // 공의 반지름
            pos = hitInfo.point + hitInfo.normal * (radius / 2f);


            int layer = LayerMask.NameToLayer("Terrain");
            if(hitInfo.transform.gameObject.layer == layer)
            {
                //반사
                if (dir.magnitude < stopThreshold * 2f) {
                    dir.y = 0f; //구름 시작
                }
                else {
                    dir = Vector3.Reflect(dir, hitInfo.normal) * bounciness;
                }

                //마찰
                string tag = hitInfo.collider.tag;
                if(surfaceFriction.ContainsKey(tag))
                {
                    dir *= (1f - surfaceFriction[tag]);
                }
=======
        if(Physics.SphereCast(ray, ballRadius, out hitInfo, rayDir.magnitude)) {
            pos = hitInfo.point + hitInfo.normal * ballRadius;

            int layer = LayerMask.NameToLayer("Terrain");
            if(hitInfo.transform.gameObject.layer == layer) {
>>>>>>> Stashed changes
                teleportCircleUI.gameObject.SetActive(true);
                teleportCircleUI.position = pos;
                teleportCircleUI.forward = hitInfo.normal;
                float distance = (pos - ARAVRInput.LHandPosition).magnitude;
                teleportCircleUI.localScale = originScale * Mathf.Max(1, distance);

                //pos += transform.localScale.y / 2f);
            }

            return true;
        }


        return false;
    }
    
    void  UpdateWindText()
    {
        windtextUI.text = $"{wind.magnitude:F1} m/s";

        float angle = Mathf.Atan2(-wind.x, -wind.z) * Mathf.Rad2Deg;//왜 음수인지 모르겠는데 이래야 3D랑 맞음
        windArrowUI.rotation = Quaternion.Euler(0, 0, -angle);//수학은 반시계, 유니티는 시계방향

        return;
    }

    void CalculateReflection(Vector3 hitNormal, ref Vector3 dir)
    {
        Vector3 currpos = transform.position;

        //Vector3 incomVec = currpos - startpos;
        Vector3 normalVec = hitNormal;
    }

    IEnumerator FollowLineSmooth() {

        Vector3[] positions = new Vector3[lr.positionCount];
        lr.GetPositions(positions);

        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 start = positions[i];
            Vector3 end = positions[i + 1];
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / Time.fixedDeltaTime;
                transform.position = Vector3.Lerp(start, end, t);
                yield return null; // 다음 프레임까지 대기
            }
        }
    }

}
