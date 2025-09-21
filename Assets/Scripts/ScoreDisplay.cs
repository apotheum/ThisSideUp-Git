using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{

    //Lord forgive me but I'm about to make yet another singleton
    //I've been up 36 hours

    private static ScoreDisplay instance;
    public static ScoreDisplay Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject instanceObject = new GameObject("Score Display Object");
                instance = instanceObject.AddComponent<ScoreDisplay>();
            }

            return instance;
        }
    }

    //Singleton initialization
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return; }
    }



    [SerializeField] private float flySpeed = 1.0f;

    [SerializeField] private TextMeshProUGUI rew_BoxTotalVolume;
    [SerializeField] private TextMeshProUGUI pen_FreeSpace;
    [SerializeField] private TextMeshProUGUI txt_NumFreeSpaces;
    [SerializeField] private TextMeshProUGUI pen_SealedSpace;
    [SerializeField] private TextMeshProUGUI txt_NumSealedSpaces;
    [SerializeField] private TextMeshProUGUI totalScore;

    [SerializeField] private TextMeshProUGUI ranking;

    [SerializeField] private RectTransform scoresRight;

    [SerializeField] private RectTransform continueButtonAnchor;

    public void ClickReturnButton()
    {
        GameManager.Instance.GameResetEvent.Invoke();
        GameManager.Instance.ReturnToTitleEvent.Invoke();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(continueButtonAnchor.DOLocalMoveY(-1080, 1.2f)).SetEase(Ease.OutBounce);
        sequence.Play();

        Sequence flyIn = DOTween.Sequence();
        flyIn.Append(scoresRight.DOLocalMoveX(1400, 0.5f)).SetEase(Ease.InCirc);
        flyIn.Play();

    }


    public void DisplayScore(ScoreEntry lastScore)
    {

        List<ScoreEntry> recentScores = ScoreManager.Instance.scores;

        Debug.Log("Scores size is " + ScoreManager.Instance.scores.Count);

        //ScoreEntry lastScore = recentScores[recentScores.Count - 1];


        if (lastScore != null)
        {
            int boxVolumeBonus = ScoreManager.Instance.rew_filledSpace * lastScore.boxVolume;
            int freeSpacePenalty = ScoreManager.Instance.pen_freeSpace * lastScore.freeSpaces;
            int sealedSpacePenalty = ScoreManager.Instance.pen_sealedSpace * lastScore.sealedSpaces;

            Debug.Log("Boxvolumebonus:" + boxVolumeBonus);

            int totalScoreNum = ScoreManager.Instance.calculateScore(lastScore);

            rew_BoxTotalVolume.text = "+" + boxVolumeBonus + "pts.";
            pen_FreeSpace.text = "-" + freeSpacePenalty + "pts.";
            pen_SealedSpace.text = "-" + sealedSpacePenalty + "pts.";
            totalScore.text = "=" + totalScoreNum + "pts.";

            txt_NumFreeSpaces.text = lastScore.freeSpaces + " Empty Spaces";
            txt_NumSealedSpaces.text = lastScore.sealedSpaces + " Sealed Spaces";

            string rank = "F";
            if (totalScoreNum > 6999) { rank = "P"; }
            if (totalScoreNum > 6750) { rank = "SSS"; }
            if (totalScoreNum > 6500) { rank = "SS"; }
            if (totalScoreNum > 5500) { rank = "S"; }
            if (totalScoreNum > 4000) { rank = "A"; }
            if (totalScoreNum > 3000) { rank = "B"; }
            if (totalScoreNum > 2000) { rank = "C"; }
            if (totalScoreNum > 1250) { rank = "D"; }
            if (totalScoreNum > 500) { rank = "E"; }

            ranking.text = rank;

            Sequence flyOut = DOTween.Sequence();
            flyOut.Append(scoresRight.DOLocalMoveX(615, flySpeed));
            flyOut.Play();

            Sequence buttonFlyUp = DOTween.Sequence();
            buttonFlyUp.Append(continueButtonAnchor.DOLocalMoveY(0, 0.8f)).SetEase(Ease.InOutCirc);
            buttonFlyUp.Play();
        }
        else
        {
            Debug.Log("Last score is null?");
        }


    }

    void HideScores()
    {
        Sequence rightFlyIn = DOTween.Sequence();
        rightFlyIn.Append(scoresRight.DOLocalMoveX(1400, flySpeed));
    }
}
