using System.Collections.Generic;
using UnityEngine;

public class GridBuildingSystem : MonoBehaviour
{
    [System.Serializable]
    public struct BuildingAsset
    {
        public string name;
        public GameObject prefab;
    }

    [Header("Dimensions des Murs Normaux (1x1)")]
    public Vector3 wallSize = new Vector3(3f, 3f, 0.2f); // Largeur, Hauteur, Épaisseur

    [Header("Dimensions des Murs Doubles (2x1)")]
    public Vector3 doubleWallSize = new Vector3(6f, 3f, 0.2f);

    [Header("Dimensions des Murets")]
    public Vector3 lowWallSize = new Vector3(3f, 1f, 0.2f);

    [Header("Dimensions des Piliers")]
    public Vector3 pillarSize = new Vector3(0.5f, 3f, 0.5f);

    [Header("Dimensions des Toits (Empreinte au sol)")]
    public Vector3 roofSize = new Vector3(3f, 2f, 3f);

    [Header("Catalogues d'Assets")]
    public List<BuildingAsset> normalWalls = new List<BuildingAsset>();
    public List<BuildingAsset> doubleWalls = new List<BuildingAsset>();
    public List<BuildingAsset> lowWalls = new List<BuildingAsset>();
    public List<BuildingAsset> pillars = new List<BuildingAsset>();
    public List<BuildingAsset> roofs = new List<BuildingAsset>();

    [HideInInspector] public int currentFloor = 0;
    [HideInInspector] public int selectedCategory = 0; 
    [HideInInspector] public int selectedAssetIndex = 0;
    [HideInInspector] public float currentRotation = 0f;

    // Dimensions de la boîte de génération procédurale (en nombre de cellules)
    [Header("Paramètres Génération Procédurale")]
    public int houseWidthCells = 4;  // Largeur (Axe X)
    public int houseLengthCells = 5; // Longueur (Axe Z)

    // Retourne la taille de grille par défaut (basée sur le mur normal)
    public Vector3 GetCellSize()
    {
        return new Vector3(wallSize.x, wallSize.y, wallSize.x); // La profondeur de cellule est égale à la largeur d'un mur
    }

    public Vector3 GetGridPosition(Vector3 worldPosition)
    {
        Vector3 cell = GetCellSize();
        int x = Mathf.RoundToInt(worldPosition.x / cell.x);
        int z = Mathf.RoundToInt(worldPosition.z / cell.z);
        float y = currentFloor * cell.y; 

        return new Vector3(x * cell.x, y, z * cell.z);
    }
}