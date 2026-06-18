using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(DoorInteractable))]
public class EndDoor : MonoBehaviour
{
    [Tooltip("Nom exact de la scène à charger à l'ouverture de cette porte")]
    public string sceneACharger;

    private DoorInteractable porte;
    private bool sceneChargee;

    void Awake()
    {
        porte = GetComponent<DoorInteractable>();
        porte.Animee += (vaOuvrir, duree) => { if (vaOuvrir) ChargerScene(); };
    }

    void ChargerScene()
    {
        if (sceneChargee) return;
        sceneChargee = true;

        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeOutThenIn(() => SceneManager.LoadScene(sceneACharger));
        else
            SceneManager.LoadScene(sceneACharger);
    }
}
