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
        Fishtank fishtank;
        Transform heldObjectOriginalParent;
        bool isPinching = false;
        SkinnedMeshRenderer renderer;

        string[] grabbableTags = { "Grabbable", "monomer", "dimer", "ring" };

        public Hand(GameObject gameObject, OVRHand.Hand handType, Transform playerTransform, Fishtank fishtank)
        {
            handTransform = gameObject.transform;
            ovrHand = gameObject.GetComponent<OVRHand>();
            laser = gameObject.GetComponent<LineRenderer>();
            this.handType = handType;
            this.playerTransform = playerTransform;
            this.fishtank = fishtank;
            renderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        }
        public void Update()
        {
            if (ovrHand.IsTracked) {
                renderer.enabled = true;
            } else {
                renderer.enabled = false;
                laser.enabled = false;
                return;
            }
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
                            if (hit.transform.parent == null || hit.transform.parent.tag != "Hand") {
                                heldObject = hit.transform;
                                heldObjectOriginalParent = heldObject.parent;
                                heldObject.parent = handTransform;
                                heldObject.GetComponent<Rigidbody>().isKinematic = true;
                            }
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
                heldObject.parent = heldObjectOriginalParent;
                heldObject.GetComponent<Rigidbody>().isKinematic = false;
                heldObject = null;
            }
            if (ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Middle)) {
                if (!isPinching) {
                    fishtank.SwitchMenuUiMode(-1);
                    isPinching = true;
                }
            } else if (ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Ring)) {
                if (!isPinching) {
                    fishtank.SwitchMenuUiMode(1);
                    isPinching = true;
                }
            } else {
                isPinching = false;
            }
        }
    }

    Hand leftHand;
    Hand rightHand;

    // Start is called before the first frame update
    void Start()
    {
        Fishtank fishtank = GameObject.Find("fishtank").GetComponent<Fishtank>();
        leftHand = new Hand(GameObject.Find("OVRCustomHandPrefab_L"), OVRHand.Hand.HandLeft, transform, fishtank);
        rightHand = new Hand(GameObject.Find("OVRCustomHandPrefab_R"), OVRHand.Hand.HandRight, transform, fishtank);
    }

    // Update is called once per frame
    void Update()
    {
        leftHand.Update();
        rightHand.Update();
    }
}
