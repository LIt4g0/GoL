using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2 position;
    Life life;
    public bool alive;
    List<bool> prevStates = new List<bool>(){false,false,false};
    List<bool> secondStates= new List<bool>();
    List<bool> firstStates= new List<bool>();
    Color lerpedColor;
    Color prevColor;
    Color targetColor;

    SpriteRenderer spriteRenderer;
    public int aliveNeighbours;
    public int stableGenerations = 0;
    public int deadCount = 0;
    bool oscilating = false;
    float t;

    public Cell()
    {

    }

    public void SetStartInfo(bool live, Life lifeClass)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.black;
        life = lifeClass;
        SetLife(live);
    }

    public void SetLife(bool lifeIn)
    {
        if (!lifeIn) deadCount ++;
            else deadCount = 0;
        
        prevStates.Insert(0,lifeIn);
        
        bool thisLifeStability = false;
        //clean list at random points to separate performance costs
        if (prevStates.Count > Random.Range(60,120))
        {
            prevStates.RemoveRange(60,prevStates.Count()-60);
            //Debug.Log("Wiped end of lists");
        }
    
        if (deadCount > 5 && spriteRenderer.color == life.deadColor) return;
        // store relvenat states in new list, compare lists:
        int checkLength = 3;
        //Debug.Log(checkLength);
        if (prevStates.Count > checkLength*2 && deadCount < 20)
        {
            firstStates = new List<bool>(prevStates.GetRange(0, checkLength));
            secondStates = new List<bool>(prevStates.GetRange(checkLength,checkLength));
            // Attempt to check
            if (secondStates.SequenceEqual(firstStates))
            {
                thisLifeStability = true;
            }
            else
            {
                checkLength = 15;
                if (prevStates.Count > checkLength*2 && deadCount < 20)
                {
                    firstStates = new List<bool>(prevStates.GetRange(0, checkLength));
                    secondStates = new List<bool>(prevStates.GetRange(checkLength,checkLength));
                    // Attempt to check
                    if (secondStates.SequenceEqual(firstStates))
                    {
                        thisLifeStability = true;
                    }
                }
            }
        }

        // Old system with 1 lookback for stability
        if (lifeIn == alive)
        {
            thisLifeStability = true;
        }

        if (lifeIn == prevStates[2] && lifeIn != prevStates[1])
        {
            thisLifeStability = true;
            oscilating = true;
        }
        else
        {
            oscilating = false;
        }

        if (thisLifeStability) stableGenerations++;
            else stableGenerations = 0;

        //Coloration and more
        if (lifeIn)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = life.aliveColor;
            if (lifeIn == prevStates[1])
                spriteRenderer.color = life.stableColor;
        }
        
        if (alive != lifeIn && !lifeIn)
        {
            spriteRenderer.color = life.dyingColor;
            targetColor = life.deadColor;
        }

        if (oscilating && lifeIn)
        {
            spriteRenderer.color = life.oscilatingColor;
        }
        
        if (!lifeIn)
        {
            t += Time.deltaTime;// incorrect with deltatime but smoother fade life.refreshRate;
            lerpedColor = Color.Lerp(spriteRenderer.color, targetColor, t / life.deadFadeTime);
            spriteRenderer.color = lerpedColor;
        }

        if (alive == lifeIn && !lifeIn && spriteRenderer.color == targetColor)
        {
            spriteRenderer.enabled = false;
            t = 0;
        }

        //primitive on off:
        if (lifeIn)
        {
            spriteRenderer.enabled = true;
            t = 0;
        }


        alive = lifeIn;
    }
}