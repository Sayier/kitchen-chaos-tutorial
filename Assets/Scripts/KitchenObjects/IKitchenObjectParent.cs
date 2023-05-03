using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKitchenObjectParent
{
    public Transform GetKitchenObjectFollowTransform();

    public KitchenObject GetKitchenObject();

    public void SetKitchenObject(KitchenObject newKitchenObject);

    public void ClearKitchenObject();

    public bool HasKitchenObject();
}
