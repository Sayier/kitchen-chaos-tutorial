using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] private Transform iconTemplate;

    private PlateKitchenObject plateKitchenObject;
    private PlateIconSingleUI plateIconSingleUI;

    private void Awake(){
        plateKitchenObject = GetComponentInParent<PlateKitchenObject>();
        iconTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        plateKitchenObject.OnIngredientAddedToPlate += PlateKitchenObject_OnIngredientAddedToPlate;
    }

    private void PlateKitchenObject_OnIngredientAddedToPlate(object sender, PlateKitchenObject.OnIngredientAddedToPlateEventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        foreach(Transform child in transform)
        {
            if (child == iconTemplate) 
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        foreach(KitchenObjectSO kitchenObjectSO in plateKitchenObject.GetKitchenObjectSOOnPlateList())
        {
            Transform iconTransform = Instantiate(iconTemplate, transform);
            iconTransform.GetComponent<PlateIconSingleUI>().SetKitchenObjectSO(kitchenObjectSO);
            iconTransform.gameObject.SetActive(true);
        }
    }
}
