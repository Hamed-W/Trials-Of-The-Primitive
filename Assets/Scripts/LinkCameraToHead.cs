using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkCameraToHead : MonoBehaviour
{
    public Vector3 cameraOffset;
    public Transform headBonePos;

    void LateUpdate()
    {
        if (headBonePos != null) transform.position = headBonePos.position + headBonePos.TransformDirection(cameraOffset);
    }
}
