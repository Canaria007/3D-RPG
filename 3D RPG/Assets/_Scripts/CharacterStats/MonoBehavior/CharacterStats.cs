using System;
using UnityEngine;

public class CharacterStats:MonoBehaviour
{
    public event Action<int, int> UpdateHealthBarOnAttack;
    public CharacterData_SO characterDataTemplate;
    public CharacterData_SO characterData { get; private set; }
    public AttackData_SO attackData;

    [HideInInspector]
    public bool isCritical;
    #region Read from Data_SO
    public int maxHealth 
    {
        get { if (characterData != null) return characterData.maxHealth; else return 0; }
        set { characterData.maxHealth = value; }
    }

    public int CurrentHealth
    {
        get { if (characterData != null) return characterData.currentHealth; else return 0; }
        set { characterData.currentHealth = value; }
    }

    public int BaseDefence
    {
        get { if (characterData != null) return characterData.baseDefence; else return 0; }
        set { characterData.baseDefence = value; }
    }

    public int CurrentDefence
    {
        get { if (characterData != null) return characterData.currentDefence; else return 0; }
        set { characterData.currentDefence = value; }
    }
    #endregion

    private void Awake()
    {
        if(characterDataTemplate != null)
            characterData = Instantiate(characterDataTemplate);
    }

    #region Character Combat

    public void TakeDamage(CharacterStats attacker, CharacterStats defender)
    {
        int damage = Mathf.Max(attacker.CurrentDamage() - defender.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        if(attacker.isCritical)
        {
            defender.GetComponent<Animator>().SetTrigger("Hit");
        }
        attacker.isCritical = false;

        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int damage, CharacterStats defender)
    {
        int currentDamage = Mathf.Max(0, damage - defender.CurrentDefence);
        CurrentHealth = Mathf.Max(0, CurrentHealth - currentDamage);

        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, maxHealth);
    }

    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);

        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
        }

        return (int)coreDamage;

    }

    #endregion
}
