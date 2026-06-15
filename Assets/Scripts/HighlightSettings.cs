using UnityEngine;

[CreateAssetMenu(fileName = "HighlightSettings", menuName = "Reaco/Highlight Settings")]
public class HighlightSettings : ScriptableObject
{
    [Tooltip("Couleur de surbrillance au survol")]
    public Color couleur = new Color(1f, 0.85f, 0.3f, 1f);
    [Tooltip("Intensité de l'émission (0 = aucune, 1 = normale, >1 = très brillant)")]
    [Range(0f, 2f)]
    public float intensite = 0.3f;
}
