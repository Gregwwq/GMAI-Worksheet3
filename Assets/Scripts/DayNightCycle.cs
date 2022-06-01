using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public event System.EventHandler OnChanged;
    public float dayDuration = 10.0f;
    public bool isNight { get; private set; }
    private Color dayColor;
    private Light lightComponent;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        dayColor = lightComponent.color;
    }

    void Update()
    {
        Color nightColor = Color.white * 0.1f;
        float lightIntensity = 0.5f +
                      Mathf.Sin(Time.time * 2.0f * Mathf.PI / dayDuration) / 2.0f;
        if (isNight != (lightIntensity < 0.3))
        {
            isNight = !isNight;
            if (OnChanged != null)
                OnChanged(this, System.EventArgs.Empty);
        }
        lightComponent.color = Color.Lerp(nightColor, dayColor, lightIntensity);
    }
}
