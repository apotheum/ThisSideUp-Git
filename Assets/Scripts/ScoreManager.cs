using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour {

    public List<ScoreEntry> scores=new List<ScoreEntry>();

    private string highestScoreKey = "TSU_HighestScore";
    private string scoreKey = "TSU_HiScores";

    public int rew_filledSpace = 20;
    public int pen_sealedSpace = 10;
    public int pen_freeSpace = 5;

    public UnityEvent ScoreUpdateEvent=new UnityEvent();


    private static ScoreManager instance;
    public static ScoreManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject instanceObject = new GameObject("Score Manager Object");
                instance = instanceObject.AddComponent<ScoreManager>();
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


    public int calculateScore(ScoreEntry entry)
    {

        //Score is the total number of boxes you placed
        //      -Free Spaces (each is worth 5)
        //      -Sealed Spaces (each is worth 10)

        int score = 0;
        score =
            (entry.boxVolume * rew_filledSpace) -
            (entry.freeSpaces * pen_freeSpace) -
            (entry.sealedSpaces * pen_sealedSpace);

        return score;
    }

    public void AddScore(ScoreEntry newEntry)
    {
        if (newEntry != null)
        {

            scores.Add(newEntry);

            SaveScores();

            ScoreUpdateEvent.Invoke();
        }
        else
        {
            Debug.Log("New score entry is null");
        }


    }

    public bool IsNewHighScore(ScoreEntry newEntry)
    {
        bool newHighScore = true;

        ScoreEntry highestScore = HighestScore();
        if (highestScore != null)
        {
            int thisScore = calculateScore(newEntry);
            int highest = calculateScore(highestScore);

            if (thisScore > highest)
            {
                highestScore = newEntry;
            }
            else
            {
                newHighScore = false;
            }
        }

        if (newHighScore)
        {
            string highScoreJson = JsonUtility.ToJson(highestScore);
            PlayerPrefs.SetString(highestScoreKey, highScoreJson);
            ScoreUpdateEvent.Invoke();
        }

        return newHighScore;
    }


    public ScoreEntry HighestScore()
    {
        ScoreEntry entry = null;

        if (PlayerPrefs.HasKey(highestScoreKey))
        {
            string scoreJson=PlayerPrefs.GetString(highestScoreKey);
            entry=JsonUtility.FromJson<ScoreEntry>(scoreJson);
        }

        return entry;
    }

    public void SaveScores()
    {
        string listJson = JsonUtility.ToJson(scores);
        PlayerPrefs.SetString(scoreKey, listJson);

        Debug.Log("Saved scores to file");
    }

    public void LoadScores()
    {
        if (PlayerPrefs.HasKey(scoreKey))
        {
            string scoreJson=PlayerPrefs.GetString(scoreKey);

            scores=JsonUtility.FromJson<List<ScoreEntry>>(scoreJson);

            Debug.Log("Loaded scores from file");
        }
    }


}

[System.Serializable]
public class ScoreEntry
{
    public string date;
    public int freeSpaces;
    public int sealedSpaces;
    public int boxVolume;

    public ScoreEntry(string currentDate, int free, int sealedSpace, int boxVol)
    {
        date = currentDate;
        freeSpaces = free;
        sealedSpaces = sealedSpace;
        boxVolume = boxVol;


    }

}
