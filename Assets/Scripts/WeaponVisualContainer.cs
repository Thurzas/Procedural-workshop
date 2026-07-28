using UnityEngine;

public class WeaponVisualContainer : MonoBehaviour
{
    [Header("Points d'ancrage (Sockets)")]
    public Transform bladeSocket;
    public Transform guardSocket;
    public Transform gripSocket;

    // Références pour stocker les objets instanciés (utile si on veut les détruire/remplacer)
    private GameObject currentBlade;
    private GameObject currentGuard;
    private GameObject currentGrip;

    public void AssembleVisuals(WeaponPartData blade, WeaponPartData guard, WeaponPartData grip)
    {
        // Nettoyage si des pièces existaient déjà
        if (currentBlade != null) Destroy(currentBlade);
        if (currentGuard != null) Destroy(currentGuard);
        if (currentGrip != null) Destroy(currentGrip);

        // Instanciation des nouveaux visuels sur les bons sockets
        if (blade != null && blade.visualPrefab != null)
            currentBlade = Instantiate(blade.visualPrefab, bladeSocket.position, bladeSocket.rotation, bladeSocket);

        if (guard != null && guard.visualPrefab != null)
            currentGuard = Instantiate(guard.visualPrefab, guardSocket.position, guardSocket.rotation, guardSocket);

        if (grip != null && grip.visualPrefab != null)
            currentGrip = Instantiate(grip.visualPrefab, gripSocket.position, gripSocket.rotation, gripSocket);
    }
}