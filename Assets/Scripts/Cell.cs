using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cell : MonoBehaviour
{
    Life life;
    public bool alive;
    List<bool> prevStates = new List<bool>(){false,false,false};
    Color lerpedColor;
    SpriteRenderer spriteRenderer;
    public int aliveNeighbours;
    public int stableGenerations = 0;
    int deadCount = 0;
    bool oscilating = false;
    float t;

    public void SetStartInfo(bool live, Life lifeClass)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        life = lifeClass;
        spriteRenderer.color = life.deadColor;
        SetLife(live);
    }

    public void SetLife(bool lifeIn)
    {
        if (!lifeIn) deadCount ++;
            else deadCount = 0;
        
        if (deadCount > 5 && !spriteRenderer.enabled) return;
        prevStates.Insert(0,lifeIn);
        
        bool thisLifeStability = false;
        //clean list at random points to separate performance costs
        if (prevStates.Count > 31)
        {
            prevStates.RemoveRange(31,prevStates.Count()-31);
        }
    
        // store relvenat states in new list, compare lists:
        int checkLength = 3;
        if (prevStates.Count > checkLength*2)
        {
            if (prevStates.GetRange(0, checkLength).SequenceEqual(prevStates.GetRange(checkLength,checkLength)))
            {
                thisLifeStability = true;
            }
            else
            {
                checkLength = 15;
                if (prevStates.Count > checkLength*2)
                {
                    if (prevStates.GetRange(0, checkLength).SequenceEqual(prevStates.GetRange(checkLength,checkLength)))
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

        //Coloration and spriterenderer
        ColorAndLife(lifeIn);

        alive = lifeIn;
    }

    void ColorAndLife(bool lifeIn)
    {
        if (lifeIn)
        {
            spriteRenderer.enabled = true;
            t = 0;
            if (oscilating)
            {
                spriteRenderer.color = life.oscilatingColor;
                return;
            }
            if (lifeIn == prevStates[1])
            {
                spriteRenderer.color = life.stableColor;
                return;
            }
            spriteRenderer.color = life.aliveColor;
            return;
        }

        if (alive == lifeIn && !lifeIn && t >= life.deadFadeTime && spriteRenderer.enabled || life.deadFadeTime == 0)
        {
            spriteRenderer.enabled = false;
            t = 0;
            return;
        }

        if (alive != lifeIn && !lifeIn)
        {
            spriteRenderer.color = life.dyingColor;
        }

        if (!lifeIn && t < life.deadFadeTime)
        {
            t += Time.deltaTime;// incorrect with deltatime but smoother fade life.refreshRate;
            lerpedColor = Color.Lerp(spriteRenderer.color, life.deadColor, t / life.deadFadeTime);
            spriteRenderer.color = lerpedColor;
        }
    }
}