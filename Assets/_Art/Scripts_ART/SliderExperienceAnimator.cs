using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Controls a complex UI animation sequence including slider, text, glow, and scale effects, all triggered by a single button.
/// </summary>
public class SliderExperienceAnimator : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button triggerButton;
    [SerializeField] private Slider targetSlider;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Main Animation")]
    [SerializeField] private float animationDuration = 2.0f;
    [SerializeField] private float resetDelay = 4.0f;
    [SerializeField] private int maxTextValue = 2000;
    
    [Header("Button Click Animation")]
    [SerializeField] private float buttonPunchDuration = 0.3f;
    [SerializeField] private AnimationCurve buttonPunchCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(0.5f, 1.1f), new Keyframe(1, 1f));

    [Header("Glow Effect")]
    [SerializeField] private Image glowImage;
    [SerializeField] private float glowAnimationDuration = 1.5f;
    [SerializeField] private AnimationCurve alphaCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    [Header("Completion Scale Effect")]
    [SerializeField] private RectTransform objectToScale;
    [SerializeField] private float scaleAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 1.5f), new Keyframe(1, 1));

    [Header("Effects")]
    [SerializeField] private AudioSource completionSound;

    private bool isAnimating = false;
    private Coroutine mainAnimationCoroutine;

    private void Start()
    {
        if (triggerButton != null)
        {
            triggerButton.onClick.AddListener(PlayAnimationSequence);
        }
        else
        {
            Debug.LogError("Trigger Button is not assigned.", this);
        }
        
        Debug.Log("Initializing Slider Experience.", this);
        ResetToInitialState();
    }

    private void OnDestroy()
    {
        if (triggerButton != null)
        {
            triggerButton.onClick.RemoveListener(PlayAnimationSequence);
        }
    }
    
    public void PlayAnimationSequence()
    {
        if (isAnimating)
        {
            Debug.LogWarning("Animation is already playing.", this);
            return;
        }
        
        // Start the button punch animation.
        StartCoroutine(AnimateButtonClickRoutine());

        // Stop any previous main coroutine just in case, before starting a new one.
        if(mainAnimationCoroutine != null)
        {
            StopCoroutine(mainAnimationCoroutine);
        }
        mainAnimationCoroutine = StartCoroutine(AnimateMainRoutine());
    }

    private IEnumerator AnimateMainRoutine()
    {
        isAnimating = true;
        triggerButton.interactable = false;

        if (glowImage != null)
        {
            StartCoroutine(AnimateGlowRoutine());
        }
        
        targetSlider.fillRect.gameObject.SetActive(true);
        
        Debug.Log("Animation Started.", this);

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / animationDuration);

            // Update slider value and text based on progress.
            targetSlider.value = progress;
            valueText.text = Mathf.FloorToInt(progress * maxTextValue).ToString();
            
            yield return null; // Wait for the next frame.
        }

        // Ensure final values are set precisely.
        targetSlider.value = 1;
        valueText.text = maxTextValue.ToString();

        Debug.Log("Animation Completed. Playing Sound.", this);
        if (completionSound != null)
        {
            completionSound.Play();
        }
        else
        {
            Debug.LogWarning("Completion Sound (AudioSource) is not assigned in the Inspector.", this);
        }

        if (objectToScale != null)
        {
            StartCoroutine(AnimateScaleRoutine());
        }

        // Countdown sequence
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            float countdown = resetDelay;
            while(countdown > 0)
            {
                countdownText.text = Mathf.CeilToInt(countdown).ToString();
                yield return new WaitForSeconds(1f);
                countdown--;
            }
        }
        else
        {
            // If no countdown text is provided, just wait.
            yield return new WaitForSeconds(resetDelay);
        }

        Debug.Log("Resetting to initial state.", this);
        ResetToInitialState();
    }

    private void ResetToInitialState()
    {
        targetSlider.value = 0;
        valueText.text = "0";
        targetSlider.fillRect.gameObject.SetActive(false);
        
        if(countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        triggerButton.interactable = true;
        isAnimating = false;

        if (glowImage != null)
        {
            Color initialColor = glowImage.color;
            initialColor.a = alphaCurve.Evaluate(0f);
            glowImage.color = initialColor;
        }

        if (objectToScale != null)
        {
            objectToScale.localScale = Vector3.one;
        }
    }

    private IEnumerator AnimateButtonClickRoutine()
    {
        float elapsedTime = 0f;
        Transform buttonTransform = triggerButton.transform;
        
        while (elapsedTime < buttonPunchDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / buttonPunchDuration);
            
            float scale = buttonPunchCurve.Evaluate(progress);
            buttonTransform.localScale = new Vector3(scale, scale, scale);
            
            yield return null;
        }
        
        // Ensure the final scale is set precisely to what the curve dictates at its end.
        buttonTransform.localScale = new Vector3(buttonPunchCurve.Evaluate(1f), buttonPunchCurve.Evaluate(1f), buttonPunchCurve.Evaluate(1f));
    }

    private IEnumerator AnimateScaleRoutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < scaleAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / scaleAnimationDuration);

            float scaleMultiplier = scaleCurve.Evaluate(progress);
            objectToScale.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);

            yield return null;
        }
        
        float finalScale = scaleCurve.Evaluate(1f);
        objectToScale.localScale = new Vector3(finalScale, finalScale, finalScale);
    }

    private IEnumerator AnimateGlowRoutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < glowAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / glowAnimationDuration);

            float newAlpha = alphaCurve.Evaluate(progress);

            Color currentColor = glowImage.color;
            currentColor.a = newAlpha;
            glowImage.color = currentColor;

            yield return null;
        }

        Color finalColor = glowImage.color;
        finalColor.a = alphaCurve.Evaluate(1f);
        glowImage.color = finalColor;
    }
}

// ScriptRole: Manages a complex UI animation sequence including slider, text, glow, and scale effects, all triggered by a single button.
// Dependencies: Requires multiple UI components (Button, Slider, Image, RectTransform etc.) and an AudioSource.
// NeedsSetup: All serialized fields must be assigned in the Inspector. The triggerButton's OnClick event should call PlayAnimationSequence(). 