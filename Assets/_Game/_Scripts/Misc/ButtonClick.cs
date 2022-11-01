using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     Play some satisfying click sounds
/// </summary>
public class ButtonClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public void OnPointerDown(PointerEventData eventData) {
        PlayClip("ButtonUp");
    }

    public void OnPointerUp(PointerEventData eventData) {
        PlayClip("ButtonDown");
    }

    private static void PlayClip(string n) {
    }
}