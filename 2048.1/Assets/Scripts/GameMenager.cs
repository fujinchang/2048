using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class GameMenager : MonoBehaviour
{
    #region Public variables

    public static int gridWidth = 4, gridHeight = 4;
    public static Transform[,] grid = new Transform[gridWidth, gridHeight];
    public int score = 0;
    public Text scoreValue;

    #endregion
    #region Swipe Variables

    private bool swipeLeft, swipeRight, swipeUp, swipeDown, tap;
    private Vector2 startTouch, swipeDelta;
    private bool isDraging = false;


    public Vector2 SwipeDelta { get { return swipeDelta; } }
    public bool SwipeUp { get { return swipeUp; } }
    public bool SwipeDown { get { return swipeDown; } }
    public bool SwipeLeft { get { return swipeLeft; } }
    public bool SwipeRight { get { return swipeRight; } }
    public bool Tap { get { return tap; } }

    #endregion

    

    public void Start()
    {
        NewPlate(2);
    }
    public void Update()
    {
        tap = swipeDown = swipeLeft = swipeRight = swipeUp = false;

        #region Stand Alone Imputs

        if (Input.GetMouseButtonDown(0))
        {
            isDraging = true;
            tap = true;
            startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDraging = false;
            Reset();
        }

        #endregion
        #region Mobile Imputs

        if (Input.touches.Length > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                isDraging = true;
                tap = true;
                startTouch = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                isDraging = false;
                Reset();
            }
        }

        #endregion

        PLayerInput();
        


    }


    private void Reset()
    {
        startTouch = swipeDelta = Vector2.zero;
        isDraging = false;
    }


    Vector2 RandomPlateLocation()
    {
        List<int> x = new List<int>();
        List<int> y = new List<int>();
        for (int j = 0; j < gridWidth; j++)
        {
            for (int i = 0; i < gridHeight; i++)
            {
                if (grid[j, i] == null)
                {
                    x.Add(j);
                    y.Add(i);
                }
            }
        }
        int randIndex = Random.Range(0, x.Count);

        int randX = x.ElementAt(randIndex);
        int randY = y.ElementAt(randIndex);

        return new Vector2(randX, randY);
    } // ok



    public void PLayerInput()
    {
        //swipe distance
        swipeDelta = Vector3.zero;
        if (isDraging)
        {
            if (Input.touches.Length > 0)
                swipeDelta = Input.touches[0].position - startTouch;
            else if (Input.GetMouseButton(0))
                swipeDelta = (Vector2)Input.mousePosition - startTouch;
        }

        // Cros the Circle
        if (swipeDelta.magnitude > 100)
        {
            // Direction
            float x = swipeDelta.x;
            float y = swipeDelta.y;
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                //horizontal?
                if (x < 0)
                    swipeLeft = true;
                else
                    swipeRight = true;

            }
            else
            {
                //Vertical?
                if (y < 0)
                    swipeDown = true;
                else
                    swipeUp = true;

            }
            Reset();
        }

        if (swipeDown)
        {
            Debug.Log("DOWN!");
            MoveAllPlates(Vector2.down);
        }
        if (swipeUp)
        {
            Debug.Log("UP!");
            MoveAllPlates(Vector2.up);
        }
        if (swipeLeft)
        {
            Debug.Log("LEFT!");
            MoveAllPlates(Vector2.left);

        }
        if (swipeRight)
        {
            Debug.Log("RIGHT!");
            MoveAllPlates(Vector2.right);
        }

    }

    public bool CheckAndFuzion(Transform movingPlate)
    {
        
        Vector2 platePosition = movingPlate.transform.localPosition;
        Transform CollidingPlate = grid[(int)platePosition.x, (int)platePosition.y];

        int movingPlateValue = movingPlate.GetComponent<PlateScript>().plateValue;
        int collidingPlateValue = CollidingPlate.GetComponent<PlateScript>().plateValue;

        if (movingPlateValue == collidingPlateValue)
        {
            Destroy(movingPlate.gameObject);
            Destroy(CollidingPlate.gameObject);

            grid[(int)platePosition.x, (int)platePosition.y] = null;

            string newPlateName = "plate_" + (movingPlateValue * 2);
            GameObject newPlate = (GameObject)Instantiate(Resources.Load(newPlateName, typeof(GameObject)), platePosition, Quaternion.identity);
            newPlate.transform.parent = transform;
            newPlate.GetComponent<PlateScript>().margedThisTurn = true;
            score += movingPlateValue * 2 ;
            ScoreCountValue();
            GridUpdate();

            return true;
        }

        return false;
    }
    
    void NewPlate(int howMany)
    {
        for (int i = 0; i < howMany; i++)
        {
            Vector2 newPlateLocation = RandomPlateLocation();
            float probability = Random.Range(.0f, 1.0f);
            string strPlate = "plate_2";

            if(probability > 0.9f)
            {
                strPlate = "plate_4";
            }

            GameObject newPlate = (GameObject)Instantiate(Resources.Load(strPlate, typeof(GameObject)), newPlateLocation, Quaternion.identity);
            newPlate.transform.parent = transform;

        }

        GridUpdate();
    } // ok

    public void TryAgain()
    {
        grid = new Transform[gridWidth, gridHeight];

    }

    public bool CheckPlateIsInGrid(Vector2 platePosition)
    {
        if(platePosition.x >= 0 && platePosition.x <= gridWidth -1 && platePosition.y >= 0 && platePosition.y <= gridHeight -1)
        {
            return true;
        }
        return false;
    }

    public bool CheckIsPlateInGoodPosition(Vector2 platePosition)
    {
        if(grid[(int)platePosition.x, (int)platePosition.y] == null)
        {
            return true;
        }
        return false;
    } // ok

    public bool MovePlates(Transform plate, Vector2 direction)
    {
        Vector2 startPlatesPosition = plate.localPosition;
        while (true)
        {
            plate.transform.localPosition += (Vector3)direction;
            Vector2 platePosition = plate.transform.localPosition;

            
            if (CheckPlateIsInGrid(platePosition))
            {
                if (CheckIsPlateInGoodPosition(platePosition))
                {
                    GridUpdate();
                }
                else
                {
                    if (!CheckAndFuzion(plate))
                    {
                        plate.transform.localPosition += -(Vector3)direction;
                        if (plate.transform.localPosition == (Vector3)startPlatesPosition)
                            return false;
                        else
                            return true;
                    }
                  
                }
            }
            else
            {
                plate.transform.localPosition += -(Vector3)direction;
                if (plate.transform.localPosition == (Vector3)startPlatesPosition)
                    return false;
                else
                    return true;
            }

            
        }
    } // ok

    public void MoveAllPlates(Vector2 direction)
    {
        int platesMoveCount = 0;
        if (direction == Vector2.left)
        {
            for(int x = 0; x < gridWidth; x++)
            {
                for(int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] != null)
                    {

                        if (MovePlates(grid[x,y], direction))
                            platesMoveCount++;

                    }
                }
            }
        }
        if (direction == Vector2.right)
        {
            for (int x = gridWidth -1; x >= 0; x--)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] != null)
                    {

                        if (MovePlates(grid[x, y], direction))
                            platesMoveCount++;

                    }
                }
            }
        }
        if (direction == Vector2.down)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] != null)
                    {

                        if (MovePlates(grid[x, y], direction))
                            platesMoveCount++;

                    }
                }
            }
        }
        if (direction == Vector2.up)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = gridHeight - 1; y >= 0; y--)
                {
                    if (grid[x, y] != null)
                    {

                        if (MovePlates(grid[x, y], direction))
                            platesMoveCount++;

                    }
                }
            }
        }
        if (platesMoveCount != 0)
            NewPlate(1);
    } 

    public void GridUpdate()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    if (grid[x, y].parent == transform)
                    {
                        grid[x, y] = null;
                    }
                }
            }
        }
        foreach (Transform plate in transform)
        {
            Vector2 newGridValues = new Vector2(Mathf.Round(plate.position.x), Mathf.Round(plate.position.y));
            grid[(int)newGridValues.x, (int)newGridValues.y] = plate;
        }
    } // ok

    public void ScoreCountValue()
    {
        scoreValue.text = score.ToString("00000000");
    }
    

}
