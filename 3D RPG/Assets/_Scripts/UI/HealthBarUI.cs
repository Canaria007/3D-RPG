using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI: MonoBehaviour
{
    public GameObject healthBarUIPrefab;
    public Transform healthBarTransform;

    public bool alwaysVisible;
    private float visibleTime = 3.5f;
    private float visibleTimer;

    private Image healthSlider;
    private Transform UIBar;
    private Transform cam;

    CharacterStats currentStats;

    private void Awake()
    {
        currentStats = GetComponent<CharacterStats>();

        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    private void OnEnable()
    {
        cam = Camera.main.transform;

        foreach(Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace)
            {
                UIBar = Instantiate(healthBarUIPrefab, canvas.transform).transform;
                healthSlider = UIBar.GetChild(0).GetComponent<Image>();
                UIBar.gameObject.SetActive(alwaysVisible);
            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
            Destroy(UIBar.gameObject);

        UIBar.gameObject.SetActive(true);

        visibleTimer = visibleTime;

        float sliderPercent = (float)currentHealth / maxHealth;
        healthSlider.fillAmount = sliderPercent;
    }

    private void LateUpdate()
    {
        if(UIBar != null)
        {
            UIBar.position = healthBarTransform.position;
            UIBar.forward = -cam.forward;

            if (visibleTimer <= 0 && !alwaysVisible)
                UIBar.gameObject.SetActive(false);
            else
                visibleTimer -= Time.deltaTime;
        }
    }
}
