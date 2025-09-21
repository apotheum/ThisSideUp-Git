using DG.Tweening;
using UnityEngine;

public class ShutterDoor : MonoBehaviour
{
    [SerializeField] float openSpeed = 1.0f;
    [SerializeField] float closeSpeed = 1.0f;

    void Start()
    {
        GameManager.Instance.GameStartEvent.AddListener(Open);
        GameManager.Instance.GameEndEvent.AddListener(Close);
    }

    void Open()
    {
        Sequence openSequence = DOTween.Sequence();
        openSequence.Append(transform.DOLocalMoveY(5.0f, openSpeed)).SetEase(Ease.InOutCirc);
        openSequence.Play();
    }

    void Close()
    {
        Sequence closeSequence = DOTween.Sequence();
        closeSequence.Append(transform.DOLocalMoveY(0.0f, closeSpeed)).SetEase(Ease.OutBounce);
        closeSequence.Play();
    }

}
