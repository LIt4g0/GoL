using System.Collections;
using System.Collections.Generic;
using System.Linq;

//using System.Numerics;
using UnityEngine;

public class Life : MonoBehaviour
{
    [SerializeField] GameObject refSquare;
    float height;
    float width;
    float scaleX;
    float scaleY;
    [SerializeField] float scale = 1;
    Texture2D refTexture;
    Vector3 refPos;
    [SerializeField] bool[][] xy2 =
    {
        new bool[50], new bool [50]
    };
    [SerializeField] Cell[,] xy;
    [SerializeField] GameObject[,] grid;
  

    void Start()
    {
        Camera cam = Camera.main;
        height = cam.orthographicSize;
        width = height * cam.aspect;
        scaleX = refSquare.transform.localScale.x*0.5f;
        scaleY = refSquare.transform.localScale.y*0.5f;
        refSquare.transform.position = new Vector3(-width, -height,0);
        refPos = refSquare.transform.position;
        refTexture = refSquare.GetComponent<SpriteRenderer>().sprite.texture;
        refSquare.SetActive(false);

        xy = new Cell[25,25];
        grid = new GameObject[25,25];
        
        // for (int x = 0; x < 25; x++)
        // {
        //     for (int y = 0; y < 25; y++)
        //     {
        //         grid[x,y] = new GameObject("Grid :" + x + ", " + y);
        //         //var newO = new GameObject("Cell :" + x + ", " + y);
        //         grid[x,y].transform.position = refPos + new Vector3(x*scale, y*scale,0);
        //         SpriteRenderer newOSprite = grid[x,y].AddComponent<SpriteRenderer>();
        //         newOSprite.sprite = Sprite.Create(refTexture, new Rect(0,0,scale*width,scale*1),new Vector3(0.0f,1.0f,0.5f));
        //         newOSprite.color = Color.gray;
        //     }
        // }


        for (int x = 0; x < 25; x++)
        {
            for (int y = 0; y < 25; y++)
            {
                var newO = new GameObject("Cell :" + x + ", " + y);
                newO.transform.position = refPos;
                SpriteRenderer newOSprite = newO.AddComponent<SpriteRenderer>();
                newOSprite.sprite = Sprite.Create(refTexture, new Rect(0,0,scale*95,scale*95),new Vector3(0.0f,1.0f,0.5f));
                newOSprite.color = Color.white;
                newOSprite.sortingOrder = - 1;
                xy[x,y] = new Cell(newO.transform, x*scale,y*scale);
            }
        }

        // foreach (var x in xy)
        // {
        //     //x.UpdatePos(refPos);
        //     Debug.Log(x.position + " value");
        //     var newO = new GameObject("Square :" + x + ", " + 1);
        //     newO.AddComponent<SpriteRenderer>().sprite = Sprite.Create(refTexture, new Rect(0,0,refTexture.width,refTexture.height),new Vector3(0.5f,0.5f,0.5f));
        //     //newO.transform.position = refPos + new Vector3(x, x, 0);
        // }
       
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            //spawn next
        }
    }

}

public class Cell
{
    public Vector2 position;
    Transform transform;

    //Ball Constructor, called when we type new Ball(x, y);
    public Cell(Transform artHolderGameObject, float x, float y)
    {
        //Set our position when we create the code.
        position = new Vector2(x, y);

        transform = artHolderGameObject;
        transform.position += (Vector3)position;
        //Create the velocity vector and give it a random direction.
        //velocity = Random.insideUnitCircle * 5;
    }

    public void UpdatePos(Vector3 posIn)
    {
        //Update position
        position = posIn;

        //Send new position to the art game object.
        transform.position = position;
    }
}