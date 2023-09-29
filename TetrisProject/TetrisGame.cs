using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

public class TetrisGame
{
    //The in-match game logic
    private Field field; //The field in which the game is being played
    private Piece activePiece; //The currently being controlled piece
    private List<byte> pieceQueue = new List<byte>(); //Which pieces come next
    private int nextPieceLength = 5; //The amount of pieces shown in the next piece line
    
    //Sprites
    public Texture2D blockTexture; //Texture of a single block in a piece
    public Texture2D squareTexture; //Used for drawing rectangles with a single color
    
    //File locations
    private const string blockTextureFileName = "BaseBlock";
    private const string squareTextureFileName = "Square";

    public void Instantiate()
    {
        field = new Field(this);
        activePiece = new LinePiece(field, this);
        FillQueue(); //Test
    }

    public void LoadContent(ContentManager content)
    {
        blockTexture = content.Load<Texture2D>(blockTextureFileName);
        squareTexture = content.Load<Texture2D>(squareTextureFileName);
    }

    public void Update(GameTime gameTime)
    {
        activePiece.Update(gameTime);
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        field.Draw(spriteBatch);
        field.DrawPiece(activePiece, spriteBatch);
    }

    //Adds new pieces to the list of pieces the player has to use
    private void FillQueue()
    {
        byte[] pieceOrder = { 0, 1, 2, 3, 4, 5, 6 };
        pieceOrder = Util.ShuffleArray(pieceOrder); //Shuffles the array

        foreach (var pieceByteValue in pieceOrder)
        {
            pieceQueue.Add(pieceByteValue);
        }
    }

    public void NextPiece()
    {
        activePiece = activePiece.GetNextPiece(pieceQueue[0]);
        pieceQueue.Remove(0);

        if (pieceQueue.Count < nextPieceLength+1)
        {
            FillQueue();
        }
    }
}