using UnityEngine;
using UnityEngine.UI;

public class HeatUIController : MonoBehaviour
{
    public Image HeatBar;
    public Gradient heatGradient;
    public SpaceshipController playerShip;

    void Update()
    {
        if (!playerShip) return;

        float thermalNorm = playerShip.GetThermalNorm();
        HeatBar.fillAmount = thermalNorm;
        HeatBar.color = heatGradient.Evaluate(thermalNorm);
    }
}
