using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image barImage;
    [SerializeField] private GameObject hasProgressGameObject;
    [SerializeField] private GameObject[] visualProgressBarGameObjectArray;

    private IHasProgress hasProgress;

    private void Start()
    {
        hasProgress = hasProgressGameObject.GetComponent<IHasProgress>();
        if (hasProgress == null)
        {
            Debug.LogError($"Game Object {hasProgressGameObject} does not have a component that implements IHasGameObject");
        }
        hasProgress.OnProgressChanged += HasProgress_OnProgressChanged;

        barImage.fillAmount = 0f;
        HideProgressBar();
    }

    private void HasProgress_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        barImage.fillAmount = e.progressNormalized;
        if (barImage.fillAmount <= 0f || barImage.fillAmount >= 1f)
        {
            HideProgressBar();
        }
        else
        {
            ShowProgressBar();
        }
    }

    private void ShowProgressBar()
    {
        foreach(GameObject visualProgressBar in visualProgressBarGameObjectArray)
        {
            visualProgressBar.SetActive(true);
        }
    }

    private void HideProgressBar()
    {
        foreach (GameObject visualProgressBar in visualProgressBarGameObjectArray)
        {
            visualProgressBar.SetActive(false);
        }
    }
}
