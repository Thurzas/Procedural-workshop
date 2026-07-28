using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GridBuildingSystem))]
public class GridBuildingEditor : Editor
{
    private GridBuildingSystem targetSystem;
    private bool editMode = false;

    private void OnEnable()
    {
        targetSystem = (GridBuildingSystem)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GUILayout.Space(15);
        GUI.backgroundColor = editMode ? Color.green : Color.white;
        if (GUILayout.Button(editMode ? "MODE ÉDITION ACTIF" : "ACTIVER LE PINCEAU DE PLACEMENT", GUILayout.Height(35)))
        {
            editMode = !editMode;
            SceneView.RepaintAll();
        }
        GUI.backgroundColor = Color.white;
    }

    private void OnSceneGUI()
    {
        if (!editMode) return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        // --- FENÊTRE FLOTTANTE ---
        Handles.BeginGUI();
        // Agrandie à 380px de hauteur pour accueillir les outils procéduraux
        GUILayout.BeginArea(new Rect(10, 10, 260, 390), "Éditeur & Générateur", GUI.skin.window);
        
        // Structure & Étages
        GUILayout.Label("構造 Structure", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("↓ Bas") && targetSystem.currentFloor > 0) targetSystem.currentFloor--;
        GUILayout.Box($"Étage : {targetSystem.currentFloor}");
        if (GUILayout.Button("↑ Haut")) targetSystem.currentFloor++;
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Outils Procéduraux (Générateur Rapide)
        GUILayout.Label("Générateur Automatique", EditorStyles.boldLabel);
        targetSystem.houseWidthCells = EditorGUILayout.IntField("Largeur (Cases)", targetSystem.houseWidthCells);
        targetSystem.houseLengthCells = EditorGUILayout.IntField("Longueur (Cases)", targetSystem.houseLengthCells);
        
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Générer la Boîte d'Étage"))
        {
            GenerateProceduralBox();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);

        // Catégories de Peinture
        GUILayout.Label("Pinceau Manuel", EditorStyles.boldLabel);
        string[] categories = { "Murs 1x1", "Murs 2x1", "Murets", "Piliers", "Toits" };
        int newCategory = GUILayout.SelectionGrid(targetSystem.selectedCategory, categories, 2);
        if (newCategory != targetSystem.selectedCategory)
        {
            targetSystem.selectedCategory = newCategory;
            targetSystem.selectedAssetIndex = 0;
        }

        // Sélection d'Asset
        List<GridBuildingSystem.BuildingAsset> currentList = GetCurrentList();
        if (currentList != null && currentList.Count > 0)
        {
            string[] assetNames = new string[currentList.Count];
            for (int i = 0; i < currentList.Count; i++) assetNames[i] = currentList[i].name;
            targetSystem.selectedAssetIndex = EditorGUILayout.Popup(targetSystem.selectedAssetIndex, assetNames);
        }

        // Rotation
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Pivoter 90° (R)")) targetSystem.currentRotation = (targetSystem.currentRotation + 90f) % 360f;
        GUILayout.Box($"{targetSystem.currentRotation}°");
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        if (GUILayout.Button("QUITTER")) editMode = false;

        GUILayout.EndArea();
        Handles.EndGUI();

        // --- LOGIQUE DE DESSIN MANUEL ---
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.R)
        {
            targetSystem.currentRotation = (targetSystem.currentRotation + 90f) % 360f;
            e.Use();
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 cellSize = targetSystem.GetCellSize();
        Plane floorPlane = new Plane(Vector3.up, new Vector3(0, targetSystem.currentFloor * cellSize.y, 0));

        if (floorPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 snapPos = targetSystem.GetGridPosition(hitPoint);

            // Prévisualisation de la cellule ciblée
            Handles.color = Color.cyan;
            Handles.DrawWireCube(snapPos + new Vector3(cellSize.x / 2, cellSize.y / 2, cellSize.z / 2), cellSize);

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                PlaceObject(snapPos, targetSystem.currentRotation);
                e.Use();
            }
            if (e.type == EventType.MouseDown && e.button == 1 && !e.alt)
            {
                DeleteObjectAt(snapPos);
                e.Use();
            }
        }

        SceneView.currentDrawingSceneView.Repaint();
    }

    // --- LE MOTEUR PROCÉDURAL ---
    private void GenerateProceduralBox()
    {
        Vector3 cell = targetSystem.GetCellSize();
        float yPos = targetSystem.currentFloor * cell.y;

        if (targetSystem.normalWalls.Count == 0 || targetSystem.pillars.Count == 0)
        {
            EditorUtility.DisplayDialog("Erreur", "Tu dois ajouter au moins un Mur 1x1 et un Pilier.", "OK");
            return;
        }

        GameObject wallPrefab = targetSystem.normalWalls[0].prefab;
        GameObject pillarPrefab = targetSystem.pillars[0].prefab;

        Undo.IncrementCurrentGroup();

        // Récupération des dimensions réelles définies dans ton inspecteur
        float wallThickness = targetSystem.wallSize.z; // L'épaisseur du mur (ex: 0.2)
        float pillarWidth = targetSystem.pillarSize.x;   // La largeur du pilier (ex: 0.5)

        // Décalage pour aligner le mur sur le rebord intérieur de la dalle (selon ton schéma Paint)
        float wallOffset = (cell.z / 2f) - (wallThickness / 2f);

        // 1. MURS NORD & SUD (Poussés vers les rebords extérieurs de leurs cellules respectives)
        for (int x = 0; x < targetSystem.houseWidthCells; x++)
        {
            float xPos = x * cell.x + (cell.x / 2f);

            // Mur Sud
            Vector3 posSud = new Vector3(xPos, yPos, 0f + wallOffset);
            SpawnProceduralPart(wallPrefab, posSud, 90f);

            // Mur Nord
            Vector3 posNord = new Vector3(xPos, yPos, (targetSystem.houseLengthCells - 1) * cell.z - wallOffset);
            SpawnProceduralPart(wallPrefab, posNord, 270f);
        }

        // 2. MURS EST & OUEST (Ils vont désormais de 0 à MAX pour fermer les angles et croiser les murs N/S)
        for (int z = 0; z < targetSystem.houseLengthCells; z++)
        {
            float zPos = z * cell.z + (cell.z / 2f);

            // Mur Ouest
            Vector3 posOuest = new Vector3(0f + wallOffset, yPos, zPos);
            SpawnProceduralPart(wallPrefab, posOuest, 180f);

            // Mur Est
            Vector3 posEst = new Vector3((targetSystem.houseWidthCells - 1) * cell.x - wallOffset, yPos, zPos);
            SpawnProceduralPart(wallPrefab, posEst, 0f);
        }

        // 3. LES 4 PILIERS DE COIN (Ramenés pile sur les intersections intérieures pour lier les murs)
        float firstX = 0f + wallOffset;
        float firstZ = 0f + wallOffset;
        float lastX = (targetSystem.houseWidthCells - 1) * cell.x - wallOffset;
        float lastZ = (targetSystem.houseLengthCells - 1) * cell.z - wallOffset;

        // Ajuste les rotations si tes poteaux ont un sens (ici laissés à 0, 90, 180, 270 pour épouser les coins)
        SpawnProceduralPart(pillarPrefab, new Vector3(firstX, yPos, firstZ), 0f);       // Coin Sud-Ouest
        SpawnProceduralPart(pillarPrefab, new Vector3(lastX, yPos, firstZ), 90f);      // Coin Sud-Est
        SpawnProceduralPart(pillarPrefab, new Vector3(lastX, yPos, lastZ), 180f);      // Coin Nord-Est
        SpawnProceduralPart(pillarPrefab, new Vector3(firstX, yPos, lastZ), 270f);     // Coin Nord-Ouest

        Undo.SetCurrentGroupName("Génération Boîte Coins Fermés");
    }    // Petite fonction helper simplifiée pour le spawn procédural direct sans décalage parasite
    private void SpawnProceduralPart(GameObject prefab, Vector3 spawnPos, float rotationY)
    {
        GameObject spawned = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        spawned.transform.position = spawnPos;
        spawned.transform.rotation = Quaternion.Euler(0, rotationY, 0);
        spawned.transform.SetParent(targetSystem.transform);

        Undo.RegisterCreatedObjectUndo(spawned, "Procedural Box Part");
    }
    private void SpawnPart(GameObject prefab, Vector3 gridPos, float rotation, Vector3 cellSize)
    {
        GameObject spawned = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        
        // Calcule le centre de la cellule actuelle
        Vector3 centerOffset = new Vector3(cellSize.x / 2f, 0, cellSize.z / 2f);
        spawned.transform.position = gridPos + centerOffset;
        spawned.transform.rotation = Quaternion.Euler(0, rotation, 0);
        spawned.transform.SetParent(targetSystem.transform);

        Undo.RegisterCreatedObjectUndo(spawned, "Procedural Spawn");
    }

    private void PlaceObject(Vector3 position, float rotation)
    {
        var list = GetCurrentList();
        if (list == null || list.Count == 0 || targetSystem.selectedAssetIndex >= list.Count) return;

        GameObject prefab = list[targetSystem.selectedAssetIndex].prefab;
        if (prefab == null) return;

        SpawnPart(prefab, position, rotation, targetSystem.GetCellSize());
    }

    private void DeleteObjectAt(Vector3 position)
    {
        Vector3 cell = targetSystem.GetCellSize();
        Vector3 targetCenter = position + new Vector3(cell.x / 2, cell.y / 2, cell.z / 2);
        Collider[] colliders = Physics.OverlapBox(targetCenter, cell * 0.45f);
        
        foreach (var col in colliders)
        {
            if (col.transform.IsChildOf(targetSystem.transform))
            {
                Undo.DestroyObjectImmediate(col.gameObject);
            }
        }
    }

    private List<GridBuildingSystem.BuildingAsset> GetCurrentList()
    {
        return targetSystem.selectedCategory switch
        {
            0 => targetSystem.normalWalls,
            1 => targetSystem.doubleWalls,
            2 => targetSystem.lowWalls,
            3 => targetSystem.pillars,
            4 => targetSystem.roofs,
            _ => null
        };
    }
}