﻿using System.Collections;
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
        Transform uiSwitches;
        Transform uiSwitchesOriginalParent;

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
            uiSwitches = GameObject.Find("UISwitches").transform;
            uiSwitchesOriginalParent = uiSwitches.parent;
        }
        public void Update()
        {
            if (ovrHand.IsTracked) {
                renderer.enabled = true;
                if (handType == OVRHand.Hand.HandLeft && uiSwitches.parent != uiSwitchesOriginalParent) {
                    uiSwitches.parent = uiSwitchesOriginalParent;
                    uiSwitches.localEulerAngles = new Vector3(0, -90, 0);
                    uiSwitches.localPosition = Vector3.zero;
                }
            } else {
                renderer.enabled = false;
                if (handType == OVRHand.Hand.HandLeft && uiSwitches.parent == uiSwitchesOriginalParent) {
                    uiSwitches.parent = handTransform.parent;
                    uiSwitches.localEulerAngles = new Vector3(0, 0, 0);
                    uiSwitches.localPosition = Vector3.zero;
                }
                Vector2 axis = Vector2.zero;
                if (handType == OVRHand.Hand.HandLeft) {
                    axis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                    if (OVRInput.GetDown(OVRInput.RawButton.Y)) {
                        fishtank.SwitchMenuUiMode(-1);
                    } else if (OVRInput.GetDown(OVRInput.RawButton.X)) {
                        fishtank.SwitchMenuUiMode(1);
                    }
                } else {
                    axis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
                    if (OVRInput.GetDown(OVRInput.RawButton.B)) {
                        fishtank.SwitchMenuUiMode(-1);
                    } else if (OVRInput.GetDown(OVRInput.RawButton.A)) {
                        fishtank.SwitchMenuUiMode(1);
                    }
                }
                Vector3 moveDir = Vector3.zero;
                moveDir += handTransform.forward * axis.y * Time.deltaTime;
                moveDir += handTransform.right * axis.x * Time.deltaTime;
                moveDir.y = 0;
                playerTransform.Translate(moveDir);

                var origin = handTransform.position;
                var direction = handTransform.forward;
                var endPoint = origin + direction * 100000;

                RaycastHit hit;
                if (Physics.Raycast(origin, direction, out hit))
                {
                    endPoint = hit.point;
                    if ((handType == OVRHand.Hand.HandLeft && OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger | OVRInput.Button.PrimaryHandTrigger) || 
                            handType == OVRHand.Hand.HandRight && OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger | OVRInput.Button.SecondaryHandTrigger)) && 
                            grabbableTags.Contains(hit.transform.tag) &&
                            heldObject == null &&
                            (hit.transform.parent == null || hit.transform.parent.tag != "Hand")) {
                        heldObject = hit.transform;
                        heldObjectOriginalParent = heldObject.parent;
                        heldObject.parent = handTransform;
                        heldObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                }
                laser.enabled = true;
                laser.SetPosition(1, handTransform.InverseTransformPoint(endPoint));
                if ((handType == OVRHand.Hand.HandLeft && !OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger | OVRInput.Button.PrimaryHandTrigger) || 
                        handType == OVRHand.Hand.HandRight && !OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger | OVRInput.Button.SecondaryHandTrigger)) && 
                        heldObject != null) {
                    heldObject.parent = heldObjectOriginalParent;
                    heldObject.GetComponent<Rigidbody>().isKinematic = false;
                    heldObject = null;
                }
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
