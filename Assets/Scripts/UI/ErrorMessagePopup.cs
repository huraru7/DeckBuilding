using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckBuilding.UI
{
    public class ErrorMessagePopup : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Show(string message)
        {
            messageText.text = message;
            panelRoot.SetActive(true);
        }

        public void Hide()
        {
            panelRoot.SetActive(false);
        }
    }
}
