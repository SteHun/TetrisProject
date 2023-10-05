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
    private const byte width = 10;
    private const byte height = 20;
    private Pieces[][] blockArray; //Value in array is between 0 and 6 depending on which type of piece it is from so different colors can be used
    
    //Visual variables
    private int blockSize; //How large a block is 
    private int fieldPixelWidth; //How many pixels wide
    private int fieldPixelHeight; //How many pixels high
    private int fieldX; //X value of top left of field
    private int fieldY; //Y value of top left of field
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
    public Field(TetrisGame tetrisGameReference)
    {
        tetrisGame = tetrisGameReference;
        
        //Data setup
        blockArray = new Pieces[height*2][]; //Height of array is double of play height because modern Tetris has a buffer above the playfield
        for (int i = 0; i < blockArray.GetLength(0); i++)
        {
            blockArray[i] = new Pieces[width];
        }
        
        //Visual setup
        SetFieldPixelSizeByWindowHeight(80);
        drawGrid = false; //Adjust in settings later
    }

    //All the methods that are called when a piece is locked into place (in the form of a flowchart check list)
    public void FieldControlFlow()
    {
        // Pattern Phase
        byte[] rowsMarkedForDestruction = PatternPhase();
        string debugString = "d: ";
        foreach (byte b in rowsMarkedForDestruction)
            debugString += $"{b.ToString()}, ";
        Debug.WriteLine(debugString);
        
        // Iterate Phase
        // This may be used for alternative game modes and such
        
        //TODO Animate Phase
        
        // Eliminate Phase
        ClearLines(rowsMarkedForDestruction);
        
        // TODO Completion Phase
        
        // Generation Phase
        tetrisGame.RequestPiece();
    }

    private byte[] PatternPhase()
    {// note that this function uses the index in the entire array, not just the normal matrix.
     // GetBlock won't work with these
        // creates an array of empty blocks to compare against
        List<byte> result = new List<byte>();
        for (byte i = 0; i < height * 2; i++)
        {
            if (!blockArray[i].Contains(Pieces.None))
                result.Add(i);
        }
        return result.ToArray();
    }
    
    private void SetFieldPixelSizeByWindowHeight(int percentage)
    {
        fieldPixelHeight = (int)Math.Round(tetrisGame.WindowSize.Y * (percentage / 100.0));
        fieldPixelWidth = (int)Math.Round((double)fieldPixelHeight / height * width);
        blockSize = (int)Math.Round((double)fieldPixelHeight / Height);
        fieldX = (tetrisGame.WindowSize.X - fieldPixelWidth) / 2;
        fieldY = (tetrisGame.WindowSize.Y - fieldPixelHeight) / 2;
    }
    
    //Handles clearing multiple lines at once
    public void ClearLines(byte[] lines)
    {
        //Make sure lines are sorted top to bottom
        
        foreach (byte line in lines)
        {
            ClearSingleLine(line);
        }
    }

    //Handles clearing a line
    public void ClearSingleLine(byte line)
    {
        //Move all rows above the cleared row down one
        //Because the lowest rows get the higher indices, this loop is a little unorthodox
        //
        for (int i = line; i > 0 * 2; i--)
        {
            //Replace row with row above it
            blockArray[i] = blockArray[i - 1];
        }
        //After a line is cleared, the top row will always be emty
        blockArray[0] = new Pieces[width];
        for (int j = 0; j < width; j++)
            blockArray[0][j] = Pieces.None;

    }

    //Draw all the already placed pieces
    public void Draw(SpriteBatch spriteBatch)
    {
        //Draw field
        
        SetFieldPixelSizeByWindowHeight(80);
        spriteBatch.Draw(tetrisGame.squareTexture, new Rectangle(fieldX, fieldY, fieldPixelWidth, fieldPixelHeight), Color.LightGray * 0.5f); //Temp values
        //Draw blocks
        //For loops for getting blocks in sequence
        for (int i = 0; i < blockArray.GetLength(0); i++)
        {
            for (int j = 0; j < width; j++)
            {
                //Get block color
                Color blockColor;
                switch (blockArray[i][j])
                {
                    case Pieces.None:
                        blockColor = Color.Transparent;
                        break;
                    case Pieces.Block:
                        blockColor = Color.Yellow;
                        break;
                    case Pieces.Line:
                        blockColor = Color.LightBlue;
                        break;
                    case Pieces.T:
                        blockColor = Color.Purple;
                        break;
                    case Pieces.S:
                        blockColor = Color.LightGreen;
                        break;
                    case Pieces.Z:
                        blockColor = Color.Red;
                        break;
                    case Pieces.L:
                        blockColor = Color.Orange;
                        break;
                    case Pieces.J:
                        blockColor = Color.Blue;
                        break;
                    default:
                        blockColor = Color.Green;
                        break;
                }

                Rectangle blockRectangle =
                    new Rectangle(fieldX + blockSize * j, fieldY + blockSize * (i-height), blockSize, blockSize);
                
                if (drawGrid && i < height)
                {
                    //Draws grid cells to make movement and position more clear
                    spriteBatch.Draw(tetrisGame.blockTexture, blockRectangle, Color.DarkGray); //TODO Change texture
                }
                
                //Draw block
                spriteBatch.Draw(tetrisGame.blockTexture, blockRectangle, blockColor);
            }
        }
    }
    
    public void DrawPiece(Piece piece, SpriteBatch spriteBatch)
    {
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

    //Used to get a block in a more intuitive manner
    public Pieces GetBlock(int x, int y)
    {
        return blockArray[y+height][x];
    }

    //Used to set a block in a more intuitive manner
    public void SetBlock(int x, int y, Pieces value)
    {
        blockArray[y+height][x] = value;
    }
    
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
}