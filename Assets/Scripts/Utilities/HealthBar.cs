using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
    }

    public void UpdateHealth(float current, float max)
    {
        if (_fillImage != null)
        {
            _fillImage.fillAmount = current / max;
            
            _fillImage.color = Color.Lerp(Color.red, Color.green, _fillImage.fillAmount);
        }
    }

    void LateUpdate()
    {
        if(_cam != null)
        {
            transform.LookAt(transform.position + _cam.transform.forward);
        }
    }
}