using UnityEngine;

public class LogoTransition : MonoBehaviour
{
    public RectTransform logoGroup; // Drag your LogoGroup here
    public float transitionSpeed = 0.5f;
    
    // This value represents how high the logo moves. 
    // You can also use a second 'target' RectTransform to get this position.
    public float targetYOffset = 350f; 

    public void MoveLogoToTop()
    {
        // Simple linear move (we can add easing next)
        Vector2 targetPos = new Vector2(0, targetYOffset);
        
        // If using a Tweening engine like DOTween:
        // logoGroup.DOAnchorPos(targetPos, transitionSpeed).SetEase(Ease.OutCubic);
        
        // If using standard Unity Coroutine:
        StartCoroutine(AnimateLogo(targetPos));
    }

    private System.Collections.IEnumerator AnimateLogo(Vector2 target)
    {
        Vector2 startPos = logoGroup.anchoredPosition;
        float time = 0;
        while (time < transitionSpeed)
        {
            logoGroup.anchoredPosition = Vector2.Lerp(startPos, target, time / transitionSpeed);
            time += Time.deltaTime;
            yield return null;
        }
        logoGroup.anchoredPosition = target;
    }
}