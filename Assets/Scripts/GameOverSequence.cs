using DG.Tweening;
using System;
using System.Collections.Generic;
using ThisSideUp.Boxes.Core;
using UnityEngine;

public class GameOverSequence : MonoBehaviour
{

    //The first part of the Game Over sequence, called by GameManager, shrinks the offending box until nothing remains.
    public void StartSequence(BoxInstance offendingBoxInstance)
    {
        //Prevent inventory clicks
        BlockInventory.Instance.ChangeSelectionAllowed(false);

        //Shrink the offending box in a humiliating way
        Transform instanceTransform = offendingBoxInstance.gameObject.transform;

        Sequence humiliation = DOTween.Sequence();
        humiliation.Append(instanceTransform.DOMoveY(2.0f, 3)).SetEase(Ease.OutCirc);
        humiliation.Append(instanceTransform.DOScale(new Vector3(0, 0, 0), 0.4f).SetEase(Ease.OutCirc));
        humiliation.OnComplete(() => Phase2());
        humiliation.Play();
    }

    //We then calculate the score and store it in the next ScoreObject.
    void Phase2()
    {
        Debug.Log("Phase 2 of game end");

        //Every free space subtracts 5 points
        int freeSpaces = GridManager.Instance.unsealedSpaceCache.Count-49; //This calculation goes outside the map bounds

        //Every sealed space substracts 10 points
        int sealedSpaces = GridManager.Instance.sealedSpaceCache.Count-1;

        float gridWH = GridManager.Instance.gridWidthHeight+1;
        float gridLength = GridManager.Instance.highestGridZ;

        float totalVolume = gridWH * gridWH * gridLength;

        //Every occupied space gives 20 points
        int occupiedSpaces = (((int)totalVolume) - freeSpaces) - sealedSpaces;

        Debug.Log("FINAL SCORE! From total volume "+totalVolume+", sealed spaces: "+sealedSpaces+", free spaces: "+freeSpaces+", total iccupied volume: "+ occupiedSpaces);

        DateTime date = DateTime.Now;
        String nowString=date.ToString();

        Debug.Log("date:" + nowString);

        ScoreEntry score = new ScoreEntry(nowString, freeSpaces, sealedSpaces, occupiedSpaces);

        if (score != null)
        {
            Debug.Log("Score is not null HEHEHE");
        }

        ScoreDisplay.Instance.DisplayScore(score);
        
        ScoreManager.Instance.AddScore(score);

        Phase3();
    }

    void Phase3()
    {
        List<BoxInstance> placedBlocks = GridManager.Instance.placedBoxes;

        if (placedBlocks.Count > 0)
        {
            BoxInstance instance=placedBlocks[0];

            Sequence deletion = DOTween.Sequence();
            deletion.Append(instance.gameObject.transform.DOScale(new Vector3(0, 0, 0), 0.2f)).SetEase(Ease.OutCirc);
            deletion.OnComplete(() => PrepareIndividualPhase3(instance));
            deletion.Play();
        } else
        {
            Phase4();
        }
    }

    void PrepareIndividualPhase3(BoxInstance instance)
    {

        GridManager.Instance.RemovePlacedBox(instance);

        Destroy(instance.gameObject);

        Phase3();
    }

    //Invoke GameEndEvent in GameManager, which changes a lot of behavior for the camera, lighting, etc
    void Phase4()
    {
        GameManager.Instance.GameEndEvent.Invoke();
    }


    //Phase 1: Shrink the offending block slowly.



    //SCORING:
    //Score:
    //Amount of Boxes Placed (higher volume = higher points)
    //  - Space Left (5pts per free space.)
    //  - Sealed Space (x2 penalty; 10pts per sealed space.)



    //Phase 2: Delete every existing block in sequence using loop. Add points.


}

