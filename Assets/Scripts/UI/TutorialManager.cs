using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Texture[] textures;
    int index = 0;
    [SerializeField] RawImage image;
    [SerializeField] GameObject quit;
    [SerializeField] GameObject play;
    [SerializeField] GameObject tutorial;
    [SerializeField] GameObject name;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (index < textures.Length)
        {
            image.texture = textures[index];
            index++;
        }
        else if(index == textures.Length)
        {
            image.gameObject.SetActive(false);
            quit.SetActive(true);
            play.SetActive(true);
            tutorial.SetActive(true);
            name.SetActive(true);
            index = 0;
        }
    }
}
