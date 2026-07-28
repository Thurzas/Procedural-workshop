using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralBuilder))]
public class ProceduralBuilderEditor : Editor
{
    private bool isDragging = false;
    private Vector3 startGridPos;
    private Vector3 currentGridPos;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ProceduralBuilder builder = (ProceduralBuilder)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Tout effacer"))
        {
            builder.ClearCurrentBuilding();
        }
        
        GUILayout.Box("MODE CONSTRUCTION RTS :\nMaintiens [Shift + Clic Gauche] et glisse dans la vue Scène pour dessiner une boîte.", GUILayout.ExpandWidth(true));
    }

    // Intercepte les événements de la vue Scène
    private void OnSceneGUI()
    {
        ProceduralBuilder builder = (ProceduralBuilder)target;
        if (builder.theme == null) return;

        Event e = Event.current;
        
        // On n'active le mode "RTS" que si Shift est enfoncé pour ne pas bloquer les outils de base d'Unity
        if (!e.shift) return;

        // Désactive la sélection par défaut d'Unity pour éviter de désélectionner le générateur
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, builder.transform.position);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            // Snap sur la grille du thème
            float size = builder.theme.cellSize;
            Vector3 snappedPoint = new Vector3(
                Mathf.Round(worldPoint.x / size) * size,
                builder.transform.position.y,
                Mathf.Round(worldPoint.z / size) * size
            );

            // Gestion du Drag (Clic, Glisse, Relâche)
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0) // Clic gauche
                    {
                        isDragging = true;
                        startGridPos = snappedPoint;
                        currentGridPos = snappedPoint;
                        e.Use(); // Consomme l'événement
                    }
                    break;

                case EventType.MouseDrag:
                    if (isDragging && e.button == 0)
                    {
                        currentGridPos = snappedPoint;
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (isDragging && e.button == 0)
                    {
                        isDragging = false;
                        // On lance la génération finale !
                        builder.BuildBox(startGridPos, currentGridPos);
                        e.Use();
                    }
                    break;
            }

            // --- RENDER VISUEL DU SÉLECTEUR (Boîte verte transparente) ---
            if (isDragging)
            {
                Vector3 min = Vector3.Min(startGridPos, currentGridPos) - new Vector3(size * 0.5f, 0, size * 0.5f);
                Vector3 max = Vector3.Max(startGridPos, currentGridPos) + new Vector3(size * 0.5f, builder.theme.wallHeight, size * 0.5f);

                // Définition des 8 coins de la boîte de sélection
                Vector3[] verts = new Vector3[8]
                {
                    new Vector3(min.x, min.y, min.z), // 0
                    new Vector3(max.x, min.y, min.z), // 1
                    new Vector3(max.x, min.y, max.z), // 2
                    new Vector3(min.x, min.y, max.z), // 3
                    new Vector3(min.x, max.y, min.z), // 4
                    new Vector3(max.x, max.y, min.z), // 5
                    new Vector3(max.x, max.y, max.z), // 6
                    new Vector3(min.x, max.y, max.z)  // 7
                };

                Color faceColor = new Color(0f, 1f, 0f, 0.15f);
                Color lineColor = Color.green;

                // Dessin des 6 faces de la boîte
                // Dessous
                Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[0], verts[1], verts[2], verts[3] }, faceColor, lineColor);
                // Dessus
                Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[4], verts[5], verts[6], verts[7] }, faceColor, lineColor);
                // Devant
                Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[0], verts[1], verts[5], verts[4] }, faceColor, lineColor);
                // Derrière
                Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[2], verts[3], verts[7], verts[6] }, faceColor, lineColor);
                // Gauche
                Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[0], verts[3], verts[7], verts[4] }, faceColor, lineColor);
                // Droite
                Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[1], verts[2], verts[6], verts[5] }, faceColor, lineColor);

                // Force la vue Scène à se rafraîchir pour éviter les traînées visuelles
                SetSceneDirty();
            }
        }
    }

    private void SetSceneDirty()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll();
        }
    }
}