
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class powergauge : MonoBehaviour
{

    public float speed = 100f;
    bool active = false;
    public RectTransform powergaugeUI;
    public RectTransform arrow;
    float lx,ly;
    float startx,endx,boxwidth,pt;
    Vector3 dir = Vector3.left;

    void Start()
    {   
        arrow.gameObject.SetActive(false);
        
        lx = powergaugeUI.rect.width * powergaugeUI.localScale.x / 2f;
        ly = powergaugeUI.rect.height / 2f * powergaugeUI.localScale.y + arrow.rect.height / 2f * arrow.localScale.y;

        startx = powergaugeUI.position.x - (lx);
        endx = powergaugeUI.position.x + (lx);
        boxwidth = powergaugeUI.rect.width * powergaugeUI.localScale.x / 8f;//powergauge = 1024x64-> 128x64 * 16, max칸 = 128*64

        arrow.position = new Vector3(powergaugeUI.position.x + lx,powergaugeUI.position.y + ly,0f);
    }

    void Update()
    {
        if(!golfball.FreezeControl)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Increase the scale of the power gauge
                if(!active) {
                    active = true;
                    arrow.gameObject.SetActive(true);

                    arrow.position = new Vector3(powergaugeUI.position.x + lx,powergaugeUI.position.y + ly,0f);
                }
                else
                {

                    golfball.fire = true;
                    //golfball.power = transform.position.x;

                    active = false;
                }
            }

            if(active)
            {
                arrow.position += dir * speed * Time.deltaTime;
                if(arrow.position.x <= startx )
                {
                    dir = Vector3.right;
                }
                else if(arrow.position.x >= endx )
                {
                    dir = Vector3.left;
                }

                pt = arrow.position.x;
                if(pt < startx + boxwidth)
                    {
                        golfball.powerval = Mathf.InverseLerp(startx-(boxwidth*3), startx+boxwidth, pt);//startx -> 0.75
                        //golfball.powerval = Math.Max(golfball.powerval,0.5f);
                    }
                    else if (pt <= startx + boxwidth*2)
                    {
                        golfball.powerval = 1.0f;
                    }
                    else
                    {
                        golfball.powerval = 1 - Mathf.InverseLerp(startx+(boxwidth*2), endx , pt);
                        golfball.powerval = Math.Max(golfball.powerval,0.1f);
                    }
            }   
        }
        
        



    }

    
}