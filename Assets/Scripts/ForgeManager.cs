using UnityEngine;

public class ForgeManager : MonoBehaviour
{
    [Header("Pièces actuellement forgées par le joueur")]
    public WeaponPartData currentForgedBlade;
    public WeaponPartData currentForgedGuard;
    public WeaponPartData currentForgedGrip;

    [Header("Préfab de base de l'arme")]
    public GameObject weaponTemplatePrefab; 
    public Transform spawnPointTable; // La table où l'arme finale apparaît

    public void Start()
    {
        AssembleFinalWeapon();
    }
    // Appelée quand le joueur termine la garde à l'enclume
    public void OnGuardForgingComplete(WeaponPartData finishedGuard)
    {
        currentForgedGuard = finishedGuard;
        CheckForgeStatus();
    }
    // Appelée quand le joueur termine la lame à l'enclume
    public void OnBladeForgingComplete(WeaponPartData finishedBlade)
    {
        currentForgedBlade = finishedBlade;
        CheckForgeStatus();
    }

    // Appelée quand le joueur a fini d'usiner le pommeau sur sa machine
    public void OnPommelMachiningComplete(WeaponPartData finishedGrip)
    {
        currentForgedGrip = finishedGrip;
        CheckForgeStatus();
    }

    // Étape finale : Assemblage
    public void AssembleFinalWeapon()
    {
        if(currentForgedBlade == null || currentForgedGuard == null || currentForgedGrip == null)
        {
            Debug.LogWarning("Il manque des pièces pour assembler l'arme !");
            return;
        }

        // 1. On instancie le squelette de l'arme
        GameObject newWeaponObj = Instantiate(weaponTemplatePrefab, spawnPointTable.position, spawnPointTable.rotation);
        
        // 2. On l'initialise avec les pièces façonnées par le joueur
        GeneratedWeapon weaponScript = newWeaponObj.GetComponent<GeneratedWeapon>();
        weaponScript.InitializeWeapon(currentForgedBlade, currentForgedGuard, currentForgedGrip);

        Debug.Log($"Arme créée avec succès : {weaponScript.weaponName} ! Dégâts : {weaponScript.FinalDamage}");
        
        // Nettoyage de la forge pour la prochaine arme
        ResetForge();
    }

    private void CheckForgeStatus() { /* Activer un bouton "Assembler" dans l'UI si tout est prêt */ }
    private void ResetForge() { currentForgedBlade = null; currentForgedGuard = null; currentForgedGrip = null; }
}