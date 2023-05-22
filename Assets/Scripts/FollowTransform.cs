using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform targetTransfrom;

    public void SetTargetTransform(Transform newTargetTransfrom)
    {
        targetTransfrom = newTargetTransfrom;
    }

    private void LateUpdate()
    {
        if(targetTransfrom == null)
        {
            return;
        }

        transform.SetPositionAndRotation(targetTransfrom.position, targetTransfrom.rotation);
    }
}
