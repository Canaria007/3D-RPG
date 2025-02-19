using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    private Image healthSlider;
    private Image expSlider;
    private Text levelText;

    private void Start()
    {
        healthSlider = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        expSlider = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        levelText = transform.GetChild(2).GetComponent<Text>();
    }

    private void Update()
    {
        print("kkkkkkkkk");
        levelText.text = "Level  " + GameManager.Instance.playerStats.characterData.currentLevel.ToString("00");
        print(GameManager.Instance.playerStats.characterData.currentLevel);
        UpdateHealth();
        UpdateExp();
    }

    private void UpdateHealth()
    {
        float sliderPercent = (float)GameManager.Instance.playerStats.CurrentHealth / GameManager.Instance.playerStats.maxHealth;
        healthSlider.fillAmount = sliderPercent;
    }

    private void UpdateExp()
    {
        float sliderPercent = (float)GameManager.Instance.playerStats.characterData.currentExp / GameManager.Instance.playerStats.characterData.baseExp;
        expSlider.fillAmount = sliderPercent;
    }
}
