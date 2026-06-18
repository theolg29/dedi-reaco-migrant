using UnityEngine;

// Désactive un GameObject et active un autre — typiquement pour basculer entre deux setups d'éclairage
public class GameObjectSwapper : MonoBehaviour
{
    [Tooltip("GameObject désactivé au moment du basculement")]
    public GameObject gameObjectADesactiver;
    [Tooltip("GameObject activé au moment du basculement")]
    public GameObject gameObjectAActiver;

    public void Basculer()
    {
        if (gameObjectADesactiver != null)
            gameObjectADesactiver.SetActive(false);
        if (gameObjectAActiver != null)
            gameObjectAActiver.SetActive(true);
    }
}
