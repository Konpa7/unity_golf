using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class golfball : MonoBehaviour
{
    public CameraControl cameracontrol;
    public static bool FreezeControl = false;
    public static bool fire = false;
    public static float powerval = 1.0f;


    public Transform teleportCircleUI;
    public TMPro.TextMeshProUGUI windtextUI;
    public RectTransform windArrowUI;
    public TMPro.TextMeshProUGUI strokeTextUI;
    public TMPro.TextMeshProUGUI winTextUI;

    public int linesmooth = 2000;//부드러움 정도 -> (선 길이)
    public float power = 50;
    public float gravity = -60;// 중력
    public float airfriction = 0.005f;//공기저항
    //public float bounciness = 0.7f;//나중에 없애고 plane따라 바꿀것
    public float stopThreshold = 0.2f;//멈춤 판단 기준 속도
    public float ballmoveSpeed = 3f;
    
    //공의 반지름
    
    public float angleSpeed = 0.5f;//각도 조절 속도
    float leftRightAngle = 0f;
    float upDownAngle = 45f;//8~15,20~45,45~60각도가 적당 나중에 각도 조절할 때 제한 설정할 것, 또는 각도 값을 고정으로 둘 것

    //debug var (아래 값은 디버그할 때만 쓰일 것)
    float deb_anglechange = 0f;
    public float debug_val1;
    public float debug_enableFriction_percentage = 100f;//마찰력 적용 여부
    public bool debug_enablebounciness = true;//마찰력 적용 여부

    public Vector3 wind = new Vector3(2f,0f,0f);


    //List<Vector3> lines =  new List<Vector3>();
    Dictionary<string, float> surfaceFriction = new Dictionary<string, float>()
    {
        {"fairway", 0.4f},
        {"rough", 0.8f}
    };
    Dictionary<string, float> surfaceBounciness = new Dictionary<string, float>()
    {
        {"fairway", 0.8f},
        {"rough", 0.4f}
    };
    Vector3[] linePositions;
    int lp_cnt = 0;

    LineRenderer lr;
    Vector3 originScale = Vector3.one * 0.02f;
    private int strokeCount = 0;
    private Coroutine move;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wind = new Vector3(Random.Range(-5f,5f),0f,Random.Range(-5f,5f));
        
        
        teleportCircleUI.gameObject.SetActive(false);

        lr = GetComponent<LineRenderer>();
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;

        linePositions = new Vector3[linesmooth * 2];

        UpdateStrokeText();
        UpdateWindText();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!FreezeControl) {
            
            MakeLines();
            if(fire == true)
            {
                
                fire = false;
                
                if(teleportCircleUI.gameObject.activeSelf)
                {
                    Debug.Log("tp");
                    move = StartCoroutine(FollowLineSmooth());

                    
                }
                teleportCircleUI.gameObject.SetActive(false);
            }
            

            if (Input.GetKey(KeyCode.A))
            {
                //leftRightAngle -= angleSpeed;
                transform.rotation *= Quaternion.Euler(0f, -angleSpeed, 0f);
            }
            if (Input.GetKey(KeyCode.D))
            {
                //leftRightAngle += angleSpeed;
                transform.rotation *= Quaternion.Euler(0f, angleSpeed, 0f);
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
        //UpdateWindText();//!! 옮길것

        leftRightAngle += deb_anglechange * Time.deltaTime;//완전 디버그용

        Quaternion yaw = Quaternion.AngleAxis(leftRightAngle, transform.up);
        Quaternion pitch = Quaternion.AngleAxis(-upDownAngle, transform.right);
        debug_val1 = powerval;
    }

    void FixedUpdate()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Holecup")) {
            FreezeControl = true;

            winTextUI.gameObject.SetActive(true);
            if(move != null) {
                StopCoroutine(move);
            }
        }
    }

    void MakeLines()
    {
        float simulateTime = Time.fixedDeltaTime * 0.5f;//간격

        Quaternion yaw = Quaternion.AngleAxis(leftRightAngle, transform.up);
        Quaternion pitch = Quaternion.AngleAxis(-upDownAngle, transform.right); // -> 유니티에선 right축으로 음수쪽이 윗쪽방향  

        float temp = power * powerval;
        Vector3 dir = yaw * pitch * transform.forward * temp;
        Vector3 pos = transform.position;
        
        lp_cnt = 0;
        linePositions[lp_cnt++] = pos;
        
        for(int i=0;i<linesmooth;i++)
        {
            if (i==linesmooth-1 || dir.magnitude < stopThreshold) {
                dir = Vector3.zero;
                teleportCircleUI.gameObject.SetActive(true);
                teleportCircleUI.position = pos;
                teleportCircleUI.forward = Vector3.up;
                float distance = (pos - ARAVRInput.LHandPosition).magnitude;
                teleportCircleUI.localScale = originScale * Mathf.Max(1, distance);
                break;
            }
            if(pos.y < -10f) {
                break;
            }

            Vector3 lastPos = pos;
            
            float ballRadius = transform.localScale.y / 2;
            RaycastHit groundHit;
            if(Physics.Raycast(pos, Vector3.down, out groundHit, ballRadius+0.05f)) {//공이 땅에 닿아있을때 -> 굴러가는 상태 따로 적용
                //if(dir.y<0f) dir.y = 0f;
                
                string tag = groundHit.collider.tag;
                if(surfaceFriction.ContainsKey(tag))
                {
                    //Debug.Log($"마찰력 적용, 표면: {tag}, 마찰계수: {surfaceFriction[tag]}, dir before: {dir.magnitude}");
                    dir *= 1f - surfaceFriction[tag] * simulateTime * (debug_enableFriction_percentage * 0.01f);
                }

                //중력 미적용
            }
            else {//공이 공중에 있을 때
                dir.y += gravity * simulateTime;
                dir += wind * simulateTime;//바람은 공중에서만 작용
            }
            
            pos += dir * simulateTime;

            int val = CheckHitRay(lastPos, ref pos, ref dir);
            if (val == 1)
            {//선이 바닥과 충돌했을 경우
                linePositions[lp_cnt++] = pos;
                //break;
            }
            else if (val == 2)
            {
                linePositions[lp_cnt++] = pos;
                teleportCircleUI.gameObject.SetActive(true);
                teleportCircleUI.position = pos;
                teleportCircleUI.forward = Vector3.up;
                float distance = (pos - ARAVRInput.LHandPosition).magnitude;
                teleportCircleUI.localScale = originScale * Mathf.Max(1, distance);
                break;
            }
            else
            {
                teleportCircleUI.gameObject.SetActive(false);
            }

            dir *= 1f - (airfriction * simulateTime);//공기저항 적용

            linePositions[lp_cnt++] = pos;
        }

        lr.positionCount = lp_cnt;
        lr.SetPositions(linePositions);
    }

    private int CheckHitRay(Vector3 lastPos, ref Vector3 pos, ref Vector3 dir)
    {
        Vector3 rayDir = pos - lastPos;
        Ray ray = new Ray(lastPos, rayDir.normalized);
        RaycastHit hitInfo;
        float ballRadius = transform.localScale.y / 2;

        if(Physics.SphereCast(ray, ballRadius, out hitInfo, rayDir.magnitude + ballRadius*2f))
        {
            float radius = transform.localScale.y; // 공의 반지름
            pos = hitInfo.point + hitInfo.normal * (radius / 2f);

            if(hitInfo.collider.CompareTag("Holecup"))
            {
                return 2;
            }

            int layer = LayerMask.NameToLayer("Terrain");
            if(hitInfo.transform.gameObject.layer == layer)
            {
                //반사
                if(debug_enablebounciness) {
                   if (dir.magnitude < stopThreshold * 10f) {
                    dir.y = 0f; //구름 시작
                    }
                    else {
                        string tg = hitInfo.collider.tag;
                        if(surfaceBounciness.ContainsKey(tg))
                        {
                            dir = Vector3.Reflect(dir, hitInfo.normal) * surfaceBounciness[tg];
                        }
                        
                    } 
                }

                //마찰
                string tag = hitInfo.collider.tag;
                if(surfaceFriction.ContainsKey(tag))
                {
                    //dir *= 1f - surfaceFriction[tag] * (debug_enableFriction_percentage * 0.01f);
                }
            }

            return 1;
        }    


        return 0;
    }
    
    void  UpdateWindText()
    {
        windtextUI.text = $"{wind.magnitude:F1} m/s";

        float angle = Mathf.Atan2(-wind.x, -wind.z) * Mathf.Rad2Deg;//왜 음수인지 모르겠는데 이래야 3D랑 맞음
        windArrowUI.rotation = Quaternion.Euler(0, 0, -angle);//수학은 반시계, 유니티는 시계방향
    }

    void  UpdateStrokeText()
    {
        strokeTextUI.text = "Stroke: " + strokeCount;
    }

    IEnumerator FollowLineSmooth() {
        FreezeControl = true;
        lr.enabled = false;
        cameracontrol.cameramode = 2;

        strokeCount++;
        UpdateStrokeText();

        //Debug.Log("이동 전 rotation: " + transform.rotation.eulerAngles);
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

        //Debug.Log("이동 후 rotation: " + transform.rotation.eulerAngles);

        yield return new WaitForSeconds(0.5f);

        transform.rotation = Quaternion.Euler(0f, 90f, 0f);// 회전 초기화 
        leftRightAngle = 0f;
        upDownAngle = 45f;
        powerval = 1.0f;

        cameracontrol.cameramode = 1;
        
        wind = new Vector3(Random.Range(-4f,4f),0f,Random.Range(-4f,4f));
        UpdateWindText();

        lr.enabled = true;
        FreezeControl = false;
    }

}