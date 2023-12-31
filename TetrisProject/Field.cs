using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetrisProject;

public class Field //The field in which the pieces can be placed
{
    //References
    private TetrisGame tetrisGame;
    
    //Data variables
    private byte width;
    private const byte height = 20;
    public Pieces[][] blockArray; //The blocks that are not being controlled anymore (they are locked in)
    public bool miniTSpin; //Check if a mini-t-spin has been made
    public bool tSpin; //Check if a t-spin has been made
    public bool allClear; //Check if the entire field is wiped after clearing a row
    
    //Visual variables
    public int blockSize; //How large a block is 
    public int fieldPixelWidth; //How many pixels wide
    private int fieldPixelHeight; //How many pixels high
    public int fieldX; //X value of top left of field
    public int fieldY; //Y value of top left of field
    public int fieldHeightOffset; //Extra height above the starting line
    public int fieldCoverSideWidth; //Width of one side of cover outside of field
    public int fieldReceiveWidth; //Width of the receive line (0 if not active)
    private Color themeColor; //Color of the cover
    
    private bool drawGrid;

    public byte Width
    {
        get { return width; }
    }
    
    public byte Height
    {
        get { return height; }
    }

    //Prepare the field to be usable
    public Field(TetrisGame tetrisGameReference, Color themeColor, byte width = 10, int startX = 760, int startY = 140, bool receiveBarActive = false)
    {
        tetrisGame = tetrisGameReference;
        this.themeColor = themeColor;
        this.width = width;
        //Data setup
        Empty();
        
        //Visual setup
        drawGrid = false; //Adjust in settings later
        blockSize = 32;
        fieldPixelWidth = width * blockSize;
        fieldPixelHeight = height * blockSize;
        fieldX = startX;
        fieldY = startY;
        fieldHeightOffset = blockSize * 2;
        fieldCoverSideWidth = 160;
        fieldReceiveWidth = 0;
        if (receiveBarActive)
        {
            fieldReceiveWidth = 36;
        }
    }

    //Create an empty field
    public void Empty()
    {
        blockArray = new Pieces[height*2][]; //Height of array is double of play height because modern Tetris has a buffer above the playfield
        for (int i = 0; i < blockArray.GetLength(0); i++)
        {
            blockArray[i] = new Pieces[width];
        }
    }

    //All the methods that are called when a piece is locked into place (in the form of a flowchart check list)
    public void FieldControlFlow()
    {
        // Pattern Phase
        byte[] rowsMarkedForDestruction = PatternPhase();
        
        // Iterate Phase
        
        //Animate Phase
        AnimationPhase(rowsMarkedForDestruction);
        
        // Eliminate Phase
        int scoringLines = ClearLines(rowsMarkedForDestruction);
        
        //Completion Phase
        tetrisGame.HandleScore(scoringLines);

        //Check if the player is in stress (they are close to a game over)
        bool anyBlockAtTop = false;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < (int)MathF.Round((float)Height / 5); y++)
            {
                if (GetBlock(x, y) != Pieces.None)
                {
                    anyBlockAtTop = true;
                    break;
                }
            }
        }

        tetrisGame.isInStress = anyBlockAtTop;
        
        // Generation Phase
        tetrisGame.RequestPiece();

        //Update checks
        miniTSpin = false;
        tSpin = false;
        allClear = false;
    }

    //Check for any completed rows
    private byte[] PatternPhase()
    {
        // note that this function uses the index in the entire array, not just the normal matrix.
        // GetBlock won't work with these
        // creates an array of empty blocks to compare against
        List<byte> result = new List<byte>();
        for (byte i = 0; i < height * 2; i++)
        {
            //Check if a row is full
            if (!blockArray[i].Contains(Pieces.None))
                result.Add(i);
        }
        return result.ToArray();
    }
    
    //Animations
    private void AnimationPhase(byte[] markedLines)
    {
        Random rng = new Random();
        
        //Screen shake on getting a tetris
        if(markedLines.Length == 4)
            tetrisGame.gameHandler.mainReference.StartScreenShake();
        
        Vector2 rowSize = new Vector2(blockSize * width, blockSize);
        foreach (byte y in markedLines)
        {
            //Color row red if a line is cleared
            Vector2 rowPosition = new Vector2(fieldX, fieldY + blockSize * (y - height));
            AnimationManager.PlayAnimation(new FadingRectangle(rowPosition, rowSize, Color.Red, 
                tetrisGame.squareTexture), 0);
            
            //Make blocks fall if line is cleared
            for (int x = 0; x < width; x++)
            {
                Vector2 blockPosition = new Vector2(fieldX + blockSize * x,
                    fieldY + blockSize * (y - height));
                AnimationManager.PlayAnimation(new FallingBlockAnimation(blockPosition, new Vector2(rng.Next(-200, 200), -800), 
                    tetrisGame.blockTexture, rng.Next(-4, 4), color: GetColor(GetBlock(x, y - height)),
                    size: new Vector2(blockSize, blockSize)), 4);
            }

        }
    }

    //Handles clearing multiple lines at once
    private int ClearLines(byte[] lines)
    {
        //Keeps track of lines that aren't garbage in versus mode to decide how many to send back
        int nonGarbageLines = 0;
        
        //Make sure lines are sorted top to bottom
        foreach (byte line in lines)
        {
            if (blockArray[line][0] != Pieces.Garbage && blockArray[line][1] != Pieces.Garbage)
            {
                nonGarbageLines++;
            }
            //Clear line
            ClearSingleLine(line);
        }

        //Check for an all clear (awards bonus in scoring)
        if (lines.Length != 0)
        {
            allClear = CheckAllClear();
        }

        return nonGarbageLines;
    }
    
    //Handles clearing a line
    private void ClearSingleLine(byte line)
    {
        //Move all rows above the cleared row down one
        //Because the lowest rows get the higher indices, this loop is a little unorthodox
        for (int i = line; i > 0; i--)
        {
            //Replace row with row above it
            blockArray[i] = blockArray[i - 1]; 
        }
        
        //After a line is cleared, the top row will always be empty
        blockArray[0] = new Pieces[width];
        for (int j = 0; j < width; j++)
            blockArray[0][j] = Pieces.None;
    }
    
    private bool CheckAllClear()
    {
        //Check for empty rows
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                //If a single block is not air there is no all clear
                if (blockArray[i+height][j] != Pieces.None)
                {
                    return false;
                }
            }
        }

        //If all rows are empty it's an all clear
        return true;
    }

    private static Color GetColor(Pieces piece)
    {
        //Get block color
        switch (piece)
        {
            case Pieces.None:
                return Color.Transparent;
            case Pieces.Block:
                return Color.Yellow;
            case Pieces.Line:
                return Color.LightBlue;
            case Pieces.T:
                return Color.Purple;
            case Pieces.S:
                return Color.LightGreen;
            case Pieces.Z:
                return Color.Red;
            case Pieces.L:
                return Color.Orange;
            case Pieces.J:
                return Color.Blue;
            case Pieces.Ghost:
                return Color.White * 0.5f;
            case Pieces.Garbage:
                return Color.Gray;
            default: //Default should never be called
                return Color.Green;
        }
    }

    //Draw all the already placed pieces
    public void Draw(SpriteBatch spriteBatch)
    {
        //Draw field background
        spriteBatch.Draw(tetrisGame.squareTexture, new Rectangle(fieldX, fieldY - fieldHeightOffset, fieldPixelWidth, fieldPixelHeight + fieldHeightOffset), Color.Black * 0.1f);
        
        //Draw field cover
        if (fieldReceiveWidth != 0) //Receive bar is active
        {
            spriteBatch.Draw(tetrisGame.coverReceiveBarTexture, new Vector2(fieldX-fieldReceiveWidth, fieldY-fieldHeightOffset), themeColor);
        }
        spriteBatch.Draw(tetrisGame.coverLeftTexture, new Vector2(fieldX-tetrisGame.coverLeftTexture.Width - fieldReceiveWidth, fieldY - fieldHeightOffset), themeColor);
        spriteBatch.Draw(tetrisGame.coverMiddleTexture, new Rectangle(fieldX, fieldY - fieldHeightOffset, fieldPixelWidth, tetrisGame.coverMiddleTexture.Height), themeColor);
        spriteBatch.Draw(tetrisGame.coverRightTexture, new Vector2(fieldX+fieldPixelWidth, fieldY - fieldHeightOffset), themeColor);
        
        //Draw starting line
        spriteBatch.Draw(tetrisGame.squareTexture, new Rectangle(fieldX, fieldY, fieldPixelWidth, 10), themeColor * 0.5f);
        
        //Receive bar blocks
        for (int i = 0; i < tetrisGame.blocksBeingAdded; i++)
        {
            //Only 8 lines can be added at once (the rest is queued up), those 8 lines are colored red
            if (tetrisGame.blocksBeingAdded - i <= 8)
            {
                spriteBatch.Draw(tetrisGame.blockTexture, new Vector2(fieldX-fieldReceiveWidth, fieldY+(height-i-1)*blockSize), GetColor(Pieces.Z));
            }
            else
            {
                //Queued up lines are colored gray
                spriteBatch.Draw(tetrisGame.blockTexture, new Vector2(fieldX-fieldReceiveWidth, fieldY+(height-i-1)*blockSize), GetColor(Pieces.Ghost));
            }
        }
        
        //Draw blocks
        //For loops for getting blocks in sequence
        for (int i = 0; i < blockArray.GetLength(0); i++)
        {
            for (int j = 0; j < width; j++)
            {
                //Get block color
                Color blockColor = GetColor(blockArray[i][j]);

                //Get position of block
                Rectangle blockRectangle =
                    new Rectangle(fieldX + blockSize * j, fieldY + blockSize * (i-height), blockSize, blockSize);
                
                if (drawGrid && i < height)
                {
                    //Draws grid cells to make movement and position more clear
                    spriteBatch.Draw(tetrisGame.blockTexture, blockRectangle, Color.DarkGray);
                }
                
                //Draw block
                spriteBatch.Draw(tetrisGame.blockTexture, blockRectangle, blockColor);
            }
        }
    }
    
    //Get the entire 4x4 matrix of a piece and draw the parts that match the rotation
    public void DrawPiece(Piece piece, SpriteBatch spriteBatch)
    {
        //Draw the ghost piece, with the color of the piece that it represents
        if (piece.GetType() != typeof(GhostPiece))
            DrawPiece(new GhostPiece(this, tetrisGame, tetrisGame.controls, piece), spriteBatch);
        
        //Draw the normal piece
        for (int y = 0; y < Piece.hitboxSize; y++)
        {
            for (int x =  0; x < Piece.hitboxSize; x++)
            {
                //Check if the is a block in that part of the piece (in the 4x4 matrix of possible hitbox points)
                if (!piece.Hitbox[x, y])
                    continue;

                //Draw individual block of a piece
                Rectangle blockRectangle =
                    new Rectangle(fieldX + blockSize * (x + piece.Position.X), fieldY + blockSize * (piece.Position.Y - y), blockSize, blockSize);
                spriteBatch.Draw(tetrisGame.blockTexture, blockRectangle, piece.Color);
            }
        }
    }

    //Used to check T-spins and mini-T-spins
    public bool TSpinCheck(int x, int y)
    {
        //Return true if out of bounds
        if (x < 0 || x >= width || y >= height)
        {
            return true;
        }
        
        return GetBlock(x,y) != Pieces.None;
    }
    
    //Used to get a block in a more intuitive manner
    private Pieces GetBlock(int x, int y)
    {
        //Return if out of bounds
        if (x < 0 || x >= width || y >= height)
        {
            return Pieces.None;
        }
        
        //Return piece
        return blockArray[y+height][x];
    }

    //Used to set a block in a more intuitive manner
    public void SetBlock(int x, int y, Pieces value)
    {
         blockArray[y+height][x] = value;
    }
    
    //Check for vertical collision
    public bool CollidesVertical(bool[,] hitbox, Point position)
    {
        for (int y = 0; y < hitbox.GetLength(1); y++)
        {
            for (int x = 0; x < hitbox.GetLength(0); x++)
            {
                if (hitbox[x, y] && (position.Y - y >= Height || GetBlock(x + position.X, position.Y - y) != 0))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    //Check for horizontal collision
    public bool CollidesHorizontal(bool[,] hitbox, Point position)
    {
        for (int y = 0; y < hitbox.GetLength(1); y++)
        {
            for (int x = 0; x < hitbox.GetLength(0); x++)
            {
                if (!hitbox[x, y])
                    continue;
                if (x + position.X >= Width || x + position.X < 0 || (GetBlock(x + position.X, position.Y - y) != 0))
                    return true; 
            }
        }
        
        return false;
    }

    public void HoldPiece(Piece piece)
    {
        tetrisGame.HoldPiece(piece);
    }

    public void GameOver()
    {
        tetrisGame.GameOver();
    }

    public void PlayGameOverAnimation()
    {
        Random rng = new Random();
        
        //Play explosion
        AnimationManager.PlayAnimation(new ExplosionAnimation(new Vector2(fieldX, fieldY), 
            new Vector2(blockSize * Width, blockSize * Height), tetrisGame.explosionTextures));
        
        //Play animation of all blocks falling down
        for (int y = -Height;  y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (GetBlock(x, y) != Pieces.None)
                {
                    Vector2 blockPosition = new Vector2(fieldX + blockSize * x,
                        fieldY + blockSize * y);
                    AnimationManager.PlayAnimation(new FallingBlockAnimation(blockPosition, new Vector2(rng.Next(-200, 200), -800), 
                        tetrisGame.blockTexture, rng.Next(-4, 4), color: GetColor(GetBlock(x, y)),
                        size: new Vector2(blockSize, blockSize)), 0);
                }
            }
        }
    }
}