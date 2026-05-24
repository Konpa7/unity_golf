using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform bulletImpact;
    ParticleSystem bulletEffect;
    AudioSource bulletAudio;

    public Transform crosshair;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bulletEffect = bulletImpact.GetComponent<ParticleSystem>();
        bulletAudio = bulletImpact.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        ARAVRInput.DrawCrosshair(crosshair);


        if(ARAVRInput.GetDown(ARAVRInput.Button.One))
        {
            bulletAudio.Stop();
            bulletAudio.Play();

            Ray ray = new Ray(ARAVRInput.RHandPosition,ARAVRInput.RHandDirection);

            RaycastHit hitInfo;

            int playerLayer = 1 << LayerMask.NameToLayer("Player");
            int towerLayer = 1 << LayerMask.NameToLayer("Tower");
            int layerMask = playerLayer | towerLayer;

            if (Physics.Raycast(ray, out hitInfo, 200, ~layerMask))
            {
                bulletEffect.Stop();
                bulletEffect.Play();

                bulletImpact.position = hitInfo.point;
                bulletImpact.forward = hitInfo.normal;

                transform.position = hitInfo.point + Vector3.up;


            }


        }
    }
}
