using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public struct Coordinates
{
    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
    }
    public int X;
    public int Y;
}

public class Board : MonoBehaviour {

    private char[][] currentBoard;
    private GameObject[,] currentRenderedBoard;
    private Coordinates? selectedCoords = null;
    //This sneaky boolean doesn't allow user to trigger anything. If this is set to false, it means there is an animation or something happening.
    private bool allowInput = true;

	// Use this for initialization
	void Start () {
        currentRenderedBoard = new GameObject[6, 6];
        currentBoard = new char[6][];
        var theBoard = getBoard();

        currentBoard = theBoard;
        renderBoard(theBoard);        
	}

    void renderBoard(char[][] board)
    {        
        for (int column = 0; column < 6; column++)
        {
            for (int row = 0; row < 6; row++)
            {
                var renderPosition = new Vector3(column, row, 0);
                currentRenderedBoard[column,row] = 
                Instantiate(Resources.Load(board[column][row].ToString()), renderPosition, Quaternion.identity,this.gameObject.transform) as GameObject;
            }
        }
    }

    
    char[][] getBoard()
    {
        char[][] board = new char[6][];
        board[0] = new char[6] { 'x', 'x', 'o', 'x', 'x', 'o' };
        board[1] = new char[6] { 'o', 'x', 'x', 'o', 'x', 'o' };
        board[2] = new char[6] { 'x', 'o', 'o', 'x', 'o', 'x' };
        board[3] = new char[6] { 'x', 'x', 'o', 'x', 'o', 'o' };
        board[4] = new char[6] { 'o', 'o', 'x', 'o', 'x', 'x' };
        board[5] = new char[6] { 'x', 'x', 'o', 'o', 'x', 'o' };        
        return board;       
    }
    //This algorithm assumes that board[0].length == board[1].length. The X can be a different dimension from the y dimension, but will be consistent from row to row.
    List<Coordinates> GetMatching(char[][] board)
    {
        List<Coordinates> matches = new List<Coordinates>();
        for (int x = 0; x < board.Length; x++)
        {
            for (int y = 0; y < board[x].Length; y++)
            {
                var currentObservedChar = board[x][y];
                //Now we need to declare which directions have at least 2 more characters within the bounds of the array.
            }
        }
        if (matches.Any())
        {
            return matches;
        }
        return null;
    }

    void FlipValues(char[][] board,Coordinates first, Coordinates second)
    {
        char firstVal = board[first.X][first.Y];
        board[first.X][first.Y] = board[second.X][second.Y];
        board[second.X][second.Y] = firstVal;
    }

    bool CoordinatesAreNeighbors(Coordinates first, Coordinates second)
    {
        if(first.Y == second.Y && first.X != second.X)
        {
            if (Math.Abs(first.X - second.X) == 1)
                return true;
        }
        else if(first.X == second.X && first.Y != second.Y)
        {
            if (Math.Abs(first.Y - second.Y) == 1)
                return true;
        }
        return false;
    }
    //Procedural animation FTW!!!
    public IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
    }
    //The coroutine that runs when our selected coordinates resulted in a contiguous match. 
    IEnumerator AnimateMatch(Coordinates first, Coordinates second, GameObject[,] renderedBoard, List<Coordinates> coordinatesToDeleteAfterSwap)
    {
        //Stop user from clicking shit when this is false.
        allowInput = false;
        int animateTime = 2;
        var firstPiece = renderedBoard[first.X, first.Y];
        var secondPiece = renderedBoard[second.X, second.Y];
        //Start the individual animations. 
        StartCoroutine(MoveOverSeconds(firstPiece, secondPiece.transform.position, animateTime));
        StartCoroutine(MoveOverSeconds(secondPiece, firstPiece.transform.position, animateTime));
        yield return new WaitForSeconds(animateTime);
        //Now the swap happened, lets delete the matching objects from the rendered board.
        foreach (var item in coordinatesToDeleteAfterSwap)
        {
            Destroy(renderedBoard[item.X, item.Y]);
        }
        //Now it is time for some junk above the created hole to drop down into place.


        allowInput = true;
    }

    Vector3[] getRandomNoiseFromPoint(Vector3 starting, int numberOfShakes, float severity = .3f)
    {
        Vector3[] shakeVectors = new Vector3[numberOfShakes];
        for (int i = 0; i < numberOfShakes; i++)
        {
            shakeVectors[i] = new Vector3(starting.x + UnityEngine.Random.Range(-1 * severity, severity),
                                            starting.y + UnityEngine.Random.Range(-1 * severity, severity));
        }
        return shakeVectors;
    }

    IEnumerator Shake(GameObject piece, Vector3 item, int shakeTime)
    {
        var startingPos = piece.transform.position;
        float elapsedTime = 0;
        while (elapsedTime < shakeTime)
        {
            piece.transform.position = Vector3.Lerp(startingPos, item, (elapsedTime / shakeTime));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator ShakeForSeconds(GameObject piece, int animateTime)
    {
        int amountOfShake = 10;        
        Vector3 startingPos = piece.transform.position;
        Vector3[] shakePositions = getRandomNoiseFromPoint(startingPos, amountOfShake);

        int individualShakeTime = animateTime / amountOfShake;

        foreach (var item in shakePositions)
        {
            yield return StartCoroutine(Shake(piece, item, individualShakeTime));
        }
        piece.transform.position = startingPos;
    }

    IEnumerator AnimateAreNotNeighboring(Coordinates first, Coordinates second, GameObject[,] renderedBoard)
    {
        allowInput = false;
        int animateTime = 1;
        var firstPiece = renderedBoard[first.X, first.Y];
        var secondPiece = renderedBoard[second.X, second.Y];
        StartCoroutine(ShakeForSeconds(firstPiece, animateTime));
        StartCoroutine(ShakeForSeconds(secondPiece, animateTime));
        yield return new WaitForSeconds(animateTime);
        allowInput = true;
    }
	
    void OnGUI()
    {
        //if (GUI.Button(new Rect(10, 10, 200, 200), "Demo Match Animation"))
        //    StartCoroutine(AnimateMatch(new Coordinates(0, 2), new Coordinates(1, 2), currentRenderedBoard, new List<Coordinates> { new Coordinates(0, 0), new Coordinates(0, 1), new Coordinates(0, 2), new Coordinates(0, 3), new Coordinates(0, 4)}));

        if (GUI.Button(new Rect(10, 10, 200, 200), "Demo Shake Animation"))
            StartCoroutine(AnimateAreNotNeighboring(new Coordinates(0, 2), new Coordinates(4, 2), currentRenderedBoard));

       
    }
	// Update is called once per frame
	void Update () {
        if (allowInput)
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    var objectPos = hit.collider.transform.position;

                    if (selectedCoords == null)
                    {
                        hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
                        selectedCoords = new Coordinates((int)objectPos.x, (int)objectPos.y);
                    }
                    else
                    {
                        var proposedSecondCoords = new Coordinates((int)objectPos.x, (int)objectPos.y);

                        if (CoordinatesAreNeighbors(selectedCoords.Value, proposedSecondCoords))
                        {
                            var proposedBoard = currentBoard.Select(a => a.ToArray()).ToArray();

                            //Flip the values of the two 
                            FlipValues(proposedBoard, selectedCoords.Value, proposedSecondCoords);

                            var matches = GetMatching(proposedBoard);
                            //If our second click proposes a new board which has no matches, then we reset our state of what objects have been selected.
                            if (matches == null)
                            {
                                //Animate the back and forth swap to show that the swap resulted in no matches.
                                selectedCoords = null;
                                foreach (var item in this.transform)
                                {

                                }
                            }
                            //This block of code runs if proposed board contains at least one contiguous match of more than 3 of the same item in a row. 
                            else
                            {
                                StartCoroutine(AnimateMatch(selectedCoords.Value, proposedSecondCoords, currentRenderedBoard, matches));
                            }
                        }
                        //Our second selection is does not neighbor the first.
                        else
                        {

                        }
                    }
                }
            }
        }       
	}
}
