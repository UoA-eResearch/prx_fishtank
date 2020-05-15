using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    class Hand
    {
        Transform handTransform;
        Transform playerTransform;
        OVRHand ovrHand;
        OVRHand.Hand handType;
        LineRenderer laser;
        Transform heldObject;

        string[] grabbableTags = { "Grabbable", "monomer", "dimer", "ring" };

        public Hand(GameObject gameObject, OVRHand.Hand handType, Transform playerTransform)
        {
            handTransform = gameObject.transform;
            ovrHand = gameObject.GetComponent<OVRHand>();
            laser = gameObject.GetComponent<LineRenderer>();
            this.handType = handType;
            this.playerTransform = playerTransform;
        }
        public void Update()
        {
            if (ovrHand.IsPointerPoseValid)
            {
                var origin = handTransform.TransformPoint(laser.GetPosition(0));
                var direction = ovrHand.PointerPose.forward;
                var endPoint = origin + direction * 100000;

                RaycastHit hit;
                if (Physics.Raycast(origin, direction, out hit))
                {
                    endPoint = hit.point;
                    if (ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
                    {
                        if (grabbableTags.Contains(hit.transform.tag) && heldObject == null)
                        {
                            heldObject = hit.transform;
                            heldObject.parent = handTransform;
                            heldObject.GetComponent<Rigidbody>().isKinematic = true;
                        }
                        else if (hit.transform.name == "Ground")
                        {
                            playerTransform.Translate(direction.x * Time.deltaTime, 0, direction.z * Time.deltaTime);
                        }
                    }
                }
                laser.enabled = true;
                //laser.SetPosition(0, origin);
                laser.SetPosition(1, handTransform.InverseTransformPoint(endPoint));
            }
            else
            {
                laser.enabled = false;
            }

            if (!ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Index) && heldObject != null)
            {
                heldObject.parent = null;
                heldObject.GetComponent<Rigidbody>().isKinematic = false;
                heldObject = null;
            }
        }
    }

    Hand leftHand;
    Hand rightHand;

    // Start is called before the first frame update
    void Start()
    {
        leftHand = new Hand(GameObject.Find("OVRCustomHandPrefab_L"), OVRHand.Hand.HandLeft, transform);
        rightHand = new Hand(GameObject.Find("OVRCustomHandPrefab_R"), OVRHand.Hand.HandRight, transform);
    }

    // Update is called once per frame
    void Update()
    {
        leftHand.Update();
        rightHand.Update();
    }
}
