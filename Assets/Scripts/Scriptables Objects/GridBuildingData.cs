using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingTheme", menuName = "Procedural/Building Theme")]
public class GridBuildingData : ScriptableObject
{
    [Header("Sols et Toits")]
    public GameObject floorPrefab;
    public GameObject roofPrefab;

    [Header("Murs")]
    public GameObject wallPrefab;
    
    [Tooltip("Taille d'une cellule de ton quadrillage (ex: 3 pour des cases de 3x3 mètres)")]
    public float cellSize = 3f;
    [Tooltip("Hauteur d'un étage")]
    public float wallHeight = 3f;
}