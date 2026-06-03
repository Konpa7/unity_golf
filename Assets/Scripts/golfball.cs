using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class golfball : MonoBehaviour
{
    public Transform teleportCircleUI;
    public TMPro.TextMeshProUGUI windtextUI;
    public RectTransform windArrowUI;

    public int linesmooth = 4000;//부드러움 정도 -> (선 길이)
    public float curveLength = 50;//커브 걸이 (발사 파워를 정할 것 )
    public float gravity = -60;// 중력
    public float airfriction = 0.005f;//공기저항
    public float bounciness = 0.7f;//나중에 없애고 plane따라 바꿀것
    public float stopThreshold = 0.2f;//멈춤 판단 기준 속도
    public float ballmoveSpeed = 3f;
    
    //공의 반지름
    
    public float angleSpeed = 0.5f;//각도 조절 속도
    float leftRightAngle = 0f;
    float upDownAngle = 45f;//8~15,20~45,45~60각도가 적당 나중에 각도 조절할 때 제한 설정할 것, 또는 각도 값을 고정으로 둘 것

    //debug var (아래 값은 디버그할 때만 쓰일 것)
    float deb_anglechange = 0f;
    public Vector3 debug_val1;
    public float debug_enableFriction_percentage = 100f;//마찰력 적용 여부
    public bool debug_enablebounciness = true;//마찰력 적용 여부

    public Vector3 wind = new Vector3(2f,0f,0f);


    List<Vector3> lines =  new List<Vector3>();
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
    

    LineRenderer lr;
    Vector3 originScale = Vector3.one * 0.02f;
    bool freezeControl = false;

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
        if (!freezeControl) {
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
            

            if (Input.GetKey(KeyCode.A))
            {
                leftRightAngle -= angleSpeed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                leftRightAngle += angleSpeed;
            }
            if (upDownAngle < 60f && Input.GetKey(KeyCode.W))
            {
                upDownAngle += angleSpeed;
            }
            if (upDownAngle > 5f && Input.GetKey(KeyCode.S))
            {
                upDownAngle -= angleSpeed;
            }
            }

        //debug update  
        UpdateWindText();//!! 옮길것

        leftRightAngle += deb_anglechange * Time.deltaTime;//완전 디버그용

        Quaternion yaw = Quaternion.AngleAxis(leftRightAngle, transform.up);
        Quaternion pitch = Quaternion.AngleAxis(-upDownAngle, transform.right);
        debug_val1 = pitch * yaw * transform.forward;
    }

    void MakeLines()
    {
        float simulateTime = Time.fixedDeltaTime * 0.5f;//간격

        lines.RemoveRange(0,lines.Count);


        Quaternion yaw = Quaternion.AngleAxis(leftRightAngle, transform.up);
        Quaternion pitch = Quaternion.AngleAxis(-upDownAngle, transform.right); // -> 유니티에선 right축으로 음수쪽이 윗쪽방향
        

        Vector3 dir = yaw * pitch * transform.forward * curveLength;
        Vector3 pos = transform.position;
        lines.Add(pos);

        for(int i=0;i<linesmooth;i++)
        {
            if (dir.magnitude < stopThreshold) {
                dir = Vector3.zero;
                teleportCircleUI.gameObject.SetActive(true);
                teleportCircleUI.position = pos;
                teleportCircleUI.forward = Vector3.up;
                float distance = (pos - ARAVRInput.LHandPosition).magnitude;
                teleportCircleUI.localScale = originScale * Mathf.Max(1, distance);
                break;
            }
            //공 굴러가게 y속도 제거
            if(pos.y < -10f) {
                break;
            }

            Vector3 lastPos = pos;
            
            float ballRadius = transform.localScale.y / 2;
            RaycastHit groundHit;
            if(Physics.Raycast(pos, Vector3.down, out groundHit, ballRadius)) {//공이 땅에 닿아있을때 -> 굴러가는 상태 따로 적용
                //if(dir.y<0f) dir.y = 0f;
                
                string tag = groundHit.collider.tag;
                if(surfaceFriction.ContainsKey(tag))
                {
                    //Debug.Log($"마찰력 적용, 표면: {tag}, 마찰계수: {surfaceFriction[tag]}, dir before: {dir.magnitude}");
                    dir *= (1f - surfaceFriction[tag] * simulateTime * (debug_enableFriction_percentage * 0.01f));
                }

                //중력 미적용
            }
            else {//공이 공중에 있을 때
                dir.y += gravity * simulateTime;
                dir += wind * simulateTime;//바람은 공중에서만 작용
            }
            
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

            dir *= 1f - (airfriction * simulateTime);//공기저항 적용

            lines.Add(pos);
        }   

        lr.positionCount = lines.Count;
        lr.SetPositions(lines.ToArray());
    }

    private bool CheckHitRay(Vector3 lastPos, ref Vector3 pos, ref Vector3 dir)
    {
        Vector3 rayDir = pos - lastPos;
        Ray ray = new Ray(lastPos, rayDir.normalized);
        RaycastHit hitInfo;
        float ballRadius = transform.localScale.y / 2;
    
        if(Physics.SphereCast(ray, ballRadius, out hitInfo, rayDir.magnitude + ballRadius*2f))
        {
            float radius = transform.localScale.y; // 공의 반지름
            pos = hitInfo.point + hitInfo.normal * (radius / 2f);


            int layer = LayerMask.NameToLayer("Terrain");
            if(hitInfo.transform.gameObject.layer == layer)
            {
                //반사
                if(debug_enablebounciness) {
                   if (dir.magnitude < stopThreshold * 2f) {
                    dir.y = 0f; //구름 시작
                    }
                    else {
                        dir = Vector3.Reflect(dir, hitInfo.normal) * bounciness;
                    } 
                }

                //마찰
                string tag = hitInfo.collider.tag;
                if(surfaceFriction.ContainsKey(tag))
                {
                    dir *= (1f - surfaceFriction[tag] * (debug_enableFriction_percentage * 0.01f));
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
    
    void  UpdateWindText()
    {
        windtextUI.text = $"{wind.magnitude:F1} m/s";

        float angle = Mathf.Atan2(-wind.x, -wind.z) * Mathf.Rad2Deg;//왜 음수인지 모르겠는데 이래야 3D랑 맞음
        windArrowUI.rotation = Quaternion.Euler(0, 0, -angle);//수학은 반시계, 유니티는 시계방향

        return;
    }

    IEnumerator FollowLineSmooth() {
        freezeControl = true;
        Debug.Log("이동 전 rotation: " + transform.rotation.eulerAngles);
        Vector3[] positions = new Vector3[lr.positionCount];
        lr.GetPositions(positions);

        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 start = positions[i];
            Vector3 end = positions[i + 1];
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / Time.fixedDeltaTime * ballmoveSpeed; // 이동 속도 조절
                transform.position = Vector3.Lerp(start, end, t);
                yield return null; // 다음 프레임까지 대기
            }
        }

        Debug.Log("이동 후 rotation: " + transform.rotation.eulerAngles);
        transform.rotation = Quaternion.Euler(0f, 90f, 0f);// 회전 초기화 
        leftRightAngle = 0f;
        upDownAngle = 45f;

        freezeControl = false;
    }

}