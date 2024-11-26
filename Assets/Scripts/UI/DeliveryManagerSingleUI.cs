using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    public void SetRecipeSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.RecipeName;

        foreach (Transform child in iconContainer)
        {
            if (child == iconTemplate) continue;
            Destroy(child);
        }

        foreach (KitchenObjectSO kitchenObjectSO in recipeSO.KitchenObjectSOList)
        {
            // For every ingredient in recipe
            Transform iconTransform = Instantiate(iconTemplate, iconContainer);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<Image>().sprite = kitchenObjectSO.Sprite;
        }
    }
}
