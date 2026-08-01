using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverTextScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private RectTransform scaleTarget;

    [Header("Hover")]
    [SerializeField] private Color hoverTextColor = Color.yellow;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float transitionSpeed = 12f;

    [Header("Audio")]
    [SerializeField] private bool playHoverSound = true;
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private float sfxVolume = 1f;

    private Color _normalTextColor;
    private Vector3 _normalScale;
    private bool _isHovered;

    private void Awake()
    {
        if (labelText != null)
            _normalTextColor = labelText.color;

        if (scaleTarget != null)
            _normalScale = scaleTarget.localScale;
    }

    private void OnDisable()
    {
        _isHovered = false;

        if (labelText != null)
            labelText.color = _normalTextColor;

        if (scaleTarget != null)
            scaleTarget.localScale = _normalScale;
    }

    private void Update()
    {
        float t = 1f - Mathf.Exp(-transitionSpeed * Time.unscaledDeltaTime);

        if (labelText != null)
        {
            Color targetColor = _isHovered ? hoverTextColor : _normalTextColor;
            labelText.color = Color.Lerp(labelText.color, targetColor, t);
        }

        if (scaleTarget != null)
        {
            Vector3 targetScale = _isHovered ? _normalScale * hoverScale : _normalScale;
            scaleTarget.localScale = Vector3.Lerp(scaleTarget.localScale, targetScale, t);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;

        if (playHoverSound && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(Sfx.UIHover, sfxVolume);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClickSound && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(Sfx.UIClick, sfxVolume);
    }
}
