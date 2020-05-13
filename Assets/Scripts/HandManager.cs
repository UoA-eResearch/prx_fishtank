using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{

    public OVRHand leftHand;
    public LineRenderer leftLaser;
    public OVRHand rightHand;
    public LineRenderer rightLaser;
    private Transform leftHeld;
    private Transform rightHeld;

    // Start is called before the first frame update
    void Start()
    {
    }

    void UpdateHand(OVRHand hand, string handType) {
        if (hand.IsPointerPoseValid) {
            var origin = hand.PointerPose.position;
            var direction = hand.PointerPose.forward;
            var endPoint = origin + direction * 100000;
            
            RaycastHit hit;
            if (Physics.Raycast(origin,direction,out hit)) {
                endPoint = hit.point;
                if (hit.transform.tag == "Grabbable") {
                    if (hand.GetFingerIsPinching(OVRHand.HandFinger.Index)) {
                        if (handType == "left") {
                            leftHeld = hit.transform;
                            hit.transform.parent = hand.transform;
                        } else if (handType == "right") {
                            rightHeld = hit.transform;
                            hit.transform.parent = hand.transform;
                        }
                    }
                } else if (hit.transform.name == "Ground") {

                }
            }
            
            if (handType == "left") {
                leftLaser.enabled = true;
                leftLaser.SetPosition(0, origin);
                leftLaser.SetPosition(1, endPoint);
            } else if (handType == "right") {
                rightLaser.enabled = true;
                rightLaser.SetPosition(0, origin);
                rightLaser.SetPosition(1, endPoint);
            }
        } else if (handType == "left") {
            leftLaser.enabled = false;
        } else if (handType == "right") {
            rightLaser.enabled = false;
        }
        
        if (!hand.GetFingerIsPinching(OVRHand.HandFinger.Index)) { // release
            if (handType == "left" && leftHeld != null) {
                leftHeld.parent = null;
            }
            if (handType == "right" && rightHeld != null) {
                rightHeld.parent = null;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHand(leftHand, "left");
        UpdateHand(rightHand, "right");
    }
}
