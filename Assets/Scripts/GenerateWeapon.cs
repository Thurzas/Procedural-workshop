using UnityEngine;

public class GeneratedWeapon : MonoBehaviour
{
    public string weaponName;
    
    // Les données des pièces qui composent cette arme spécifique
    public WeaponPartData bladePart;
    public WeaponPartData guardPart;
    public WeaponPartData gripPart;
    // Stats finales calculées
    public float FinalDamage { get; private set; }
    public float FinalWeight { get; private set; }
    public float FinalAttackSpeed { get; private set; }

    public void InitializeWeapon(WeaponPartData b, WeaponPartData gu, WeaponPartData gr)
    {
        bladePart = b;
        guardPart = gu;
        gripPart = gr;

        CalculateFinalStats();
        GenerateDynamicName();

        // Demande au composant visuel de s'assembler
        GetComponent<WeaponVisualContainer>().AssembleVisuals(b, gu, gr);
    }

    private void CalculateFinalStats()
    {
        // Logique combinatoire à la Borderlands
        // Le dommage de base vient surtout de la lame, altéré par les autres pièces
        float rawDamage = (bladePart ? bladePart.baseDamage : 0) + (guardPart ? guardPart.baseDamage : 0);
        
        // Le poids s'additionne simplement
        FinalWeight = (bladePart ? bladePart.weight : 0) + 
                      (guardPart ? guardPart.weight : 0) + 
                      (gripPart ? gripPart.weight : 0);
        // La vitesse d'attaque est un multiplicateur croisé
        float speedMod = (bladePart ? bladePart.attackSpeedModifier : 1) * 
                         (gripPart ? gripPart.attackSpeedModifier : 1);        
        FinalDamage = rawDamage;
        FinalAttackSpeed = speedMod;
    }

    private void GenerateDynamicName()
    {
        // Exemple simple de nommage procédural selon les matériaux ou les pièces
        string material = bladePart ? bladePart.materialType : "Fer";
        string style = guardPart ? guardPart.partName : "Simple";
        
        weaponName = $"Épée en {material} {style}";
    }
}