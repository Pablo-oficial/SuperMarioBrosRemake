using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public GameObject image;
    public bool isFirstSelected = false; 

    private bool isSelected = false;

    void Start()
    {
        if (isFirstSelected)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(gameObject);
            ExecuteEvents.Execute(gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.selectHandler);
        }
    }

    void Update()
    {
        if (isSelected && (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Submit")))
        {
            SceneManager.LoadScene("Level1-1");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetImageVisible(true);
        EventSystem.current.SetSelectedGameObject(gameObject); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        SetImageVisible(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        SetImageVisible(false);
    }

    private void SetImageVisible(bool visible)
    {
        if (image != null)
            image.SetActive(visible);
    }
}
