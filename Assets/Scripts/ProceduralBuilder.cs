using UnityEngine;

public class ProceduralBuilder : MonoBehaviour
{
    public GridBuildingData theme;

    // Cette fonction sera appelée directement par notre script Editor
    public void BuildBox(Vector3 startPt, Vector3 endPt)
    {
        if (theme == null)
        {
            Debug.LogError("Oups, tu as oublié d'assigner un thème (ScriptableObject) !");
            return;
        }

        // Nettoyage des anciennes constructions sous ce préfab
        ClearCurrentBuilding();

        float size = theme.cellSize;

        // Calcul des coordonnées min/max sur le quadrillage
        int minX = Mathf.RoundToInt(Mathf.Min(startPt.x, endPt.x) / size);
        int maxX = Mathf.RoundToInt(Mathf.Max(startPt.x, endPt.x) / size);
        int minZ = Mathf.RoundToInt(Mathf.Min(startPt.z, endPt.z) / size);
        int maxZ = Mathf.RoundToInt(Mathf.Max(startPt.z, endPt.z) / size);

        // Boucle sur notre échiquier de cellules
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Vector3 cellPos = new Vector3(x * size, transform.position.y, z * size);

                // 1. Génération du Sol
                if (theme.floorPrefab) 
                    SpawnElement(theme.floorPrefab, cellPos, theme.floorPrefab.transform.rotation);

                // 2. Génération du Toit
                if (theme.roofPrefab) 
                    SpawnElement(theme.roofPrefab, cellPos + Vector3.up * theme.wallHeight, theme.roofPrefab.transform.rotation);

                // 3. Gestion des Murs (Périmètre extérieur de la boîte)
                // Mur Nord (Z max)
                if (z == maxZ && theme.wallPrefab)
                    SpawnElement(theme.wallPrefab, cellPos + new Vector3(0, 0, size * 0.5f), Quaternion.LookRotation(Vector3.forward));
                
                // Mur Sud (Z min)
                if (z == minZ && theme.wallPrefab)
                    SpawnElement(theme.wallPrefab, cellPos + new Vector3(0, 0, -size * 0.5f), Quaternion.LookRotation(Vector3.back));

                // Mur Est (X max)
                if (x == maxX && theme.wallPrefab)
                    SpawnElement(theme.wallPrefab, cellPos + new Vector3(size * 0.5f, 0, 0), Quaternion.LookRotation(Vector3.right));

                // Mur Ouest (X min)
                if (x == minX && theme.wallPrefab)
                    SpawnElement(theme.wallPrefab, cellPos + new Vector3(-size * 0.5f, 0, 0), Quaternion.LookRotation(Vector3.left));
            }
        }
    }

    public void ClearCurrentBuilding()
    {
        // Supprime les enfants pour pouvoir reconstruire proprement
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void SpawnElement(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject obj = Instantiate(prefab, position, rotation, transform);
        // Optionnel : Enregistrer l'action pour le Ctrl+Z dans l'éditeur Unity
        #if UNITY_EDITOR
        UnityEditor.Undo.RegisterCreatedObjectUndo(obj, "Procedural Build Element");
        #endif
    }
}