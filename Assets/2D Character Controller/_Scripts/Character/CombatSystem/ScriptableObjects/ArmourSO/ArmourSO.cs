using UnityEngine;

[CreateAssetMenu(fileName = "New Armour", menuName = "Create New Armour", order = 1)]
public class ArmourSO : ScriptableObject {

    [Tooltip("The name of the armour")]
    [SerializeField] private string armourName;
    [Tooltip("The icon of the armour")]
    [SerializeField] private Sprite armourIcon;
    [Space(10)]
    [Tooltip("Current armour points of the character/object")]
    [SerializeField] private float armourPoints;
    [Tooltip("This constant determines the effectiveness of the armor. Lower values increase the armor's effectiveness, " +
        "while higher values reduce its effectiveness.")]
    [SerializeField] private float armourEffectiveness;

    public string ArmourName => armourName;
    public Sprite ArmourIcon => armourIcon;
    public float ArmourPoints => armourPoints;
    public float ArmourEffectiveness => armourEffectiveness;
}
