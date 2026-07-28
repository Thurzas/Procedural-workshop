using UnityEngine;

public enum PartType { Blade, Guard, Grip, Pommel }

[CreateAssetMenu(fileName = "NewWeaponPart", menuName = "Forge/Weapon Part")]
public class WeaponPartData : ScriptableObject
{
    public string partName;
    public PartType type;
    public GameObject visualPrefab;

    [Header("Statistiques fournies par cette pièce")]
    public float baseDamage;
    public float weight;
    public float attackSpeedModifier; // Ex: 0.9 pour plus lent, 1.1 pour plus rapide
    
    [Header("Matériau (Optionnel pour ton jeu de forge)")]
    public string materialType; // Ex: Fer, Acier, Titane...
}