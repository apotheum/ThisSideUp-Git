using DG.Tweening;
using System;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

    private RectTransform rect;

    private void Start()
    {
        GameManager.Instance.GameEndEvent.AddListener(OnGameEnd);
        GameManager.Instance.ReturnToTitleEvent.AddListener(ShowMenu);

        rect = GetComponent<RectTransform>();
        Debug.Log("Rect localPos:" + rect.localPosition + " and " + rect.position);
    }


    void HideMenu()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rect.DOLocalMoveY(1000, 1.0f));
        sequence.Play();
    }

    void ShowMenu()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rect.DOLocalMoveY(0, 1.5f)).SetEase(Ease.OutBounce);
        sequence.Play();
    }

    private void OnGameEnd()
    {
        //throw new NotImplementedException();
    }

    public void ClickPlayButton()
    {
        GameManager.Instance.NewGame();
        HideMenu();
    }
}
