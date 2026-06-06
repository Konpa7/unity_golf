using Unity.VisualScripting;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class CameraControl : MonoBehaviour
{
    public golfball golfball;
    public Camera maincam;

    public float transitionSpeed = 2f;
    public float cameramode = 1;
    private Vector3 targetpos;
    private Quaternion targetrot;


    private Camera activecam;
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        activecam = maincam;
    }

    
    void FixedUpdate()
    {   
        if(cameramode == 1)//maincam
        {
            targetpos = golfball.transform.position 
            + Vector3.up * 3f 
            + golfball.transform.forward * -10f;

            targetrot = golfball.transform.rotation;
        }
        else if (cameramode == 2)//golfballcam
        {
            targetpos = golfball.transform.position 
                + Vector3.up * 20f
                + golfball.transform.forward * -40f;

            targetrot = golfball.transform.rotation;
        }


        if(cameramode != 0 && Vector3.Distance(activecam.transform.position,targetpos) >= 0.1f)
            activecam.transform.position = Vector3.Lerp(activecam.transform.position,targetpos,Time.deltaTime * transitionSpeed);
            activecam.transform.rotation = Quaternion.Slerp(activecam.transform.rotation,targetrot,Time.deltaTime * transitionSpeed);
            
            if(Vector3.Distance(activecam.transform.position,targetpos) < 0.1f)
            {
                activecam.transform.position = targetpos;
                activecam.transform.rotation = targetrot;

                //istransitioning = false;
            }
        
    }
    /*
    public void SetActiveCamera(Camera cam,Vector3 targetvec)
    {
        foreach (Camera c in Camera.allCameras)
        {
            c.gameObject.SetActive(false);
        }

        istransitioning = true;
        activecam = cam;
    }*/
}
