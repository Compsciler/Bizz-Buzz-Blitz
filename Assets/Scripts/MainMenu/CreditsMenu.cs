using UnityEngine;

public class CreditsMenu : MonoBehaviour
{
    private string linkUrl;

    public void OpenLink()
    {
        Application.OpenURL(linkUrl);
    }
}