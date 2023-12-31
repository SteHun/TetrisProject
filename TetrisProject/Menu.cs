using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

public class Menu
{
    //References
    private Main main;
    private SpriteBatch spriteBatch;
    public SelectingKeys selectingKeys;

    //Textures
    private Texture2D buttonBegin;
    private Texture2D buttonMiddle;
    private Texture2D buttonEnd;
    public SpriteFont font;
    private Texture2D tile;
    
    //Variables
    public MenuState menuState; //The currently active menu
    public byte menuIndex; //What is selected in the menu
    private int editingControlScheme; //The control scheme that is actively being adjusted (used for changing keybinds)
    private string newProfileName = "";
    private const int maxAmountOfProfiles = 8;
    
    //Toggles
    public byte profileIndex;
    public byte profileIndex2; //Only used in multiplayer game modes
    private readonly string[] gameModeNames = {"Standard", "Tug Of War", "Versus"};
    public byte gameModeIndex;
    
    //Visual variables
    private readonly Vector2 topLeftTopButtonPosition;
    private readonly Vector2 buttonVerticalOffset;
    private readonly int selectedHorizontalOffsetTotal; //Applied to the button that is selected
    private float[] selectedHorizontalOffsets; //Offset of each individual button
    private readonly float buttonAnimationTimeMax;

    public Menu(Main main, SpriteBatch spriteBatch)
    {
        this.main = main;
        this.spriteBatch = spriteBatch;
        menuState = MenuState.MainMenu;
        menuIndex = 0;
        buttonAnimationTimeMax = 0.3f;
        topLeftTopButtonPosition = new Vector2(50, 50);
        buttonVerticalOffset = new Vector2(0, 90);
        selectedHorizontalOffsetTotal = 80;
    }
    
    public void LoadContent(ContentManager content)
    {
        buttonBegin = content.Load<Texture2D>("Button Begin");
        buttonMiddle = content.Load<Texture2D>("Button Middle");
        buttonEnd = content.Load<Texture2D>("Button End");
        font = content.Load<SpriteFont>("Font");
        tile = content.Load<Texture2D>("Square");
        
        if(menuState != MenuState.Explainer)
            GoToMenu(MenuState.MainMenu);
    }

    public void Update(GameTime gameTime)
    {
        //While mapping key(s) to input
        if (menuState == MenuState.MapKeys)
        {
            selectingKeys.Update();
            return;
        }
        
        //Normal menu update
        MenuMovement();
        AnimateMenu(gameTime.ElapsedGameTime.TotalSeconds);
    }

    #region Visual Methods
    //Update horizontal offsets of buttons that are animated based on selected button
    private void AnimateMenu(double deltaTime)
    {
        //Each button has a position and it moves left/right depending on if it is selected, afterwards the values are clamped
        for (int i = 0; i < GetMenuLength(); i++)
        {
            if (i == menuIndex)
            {
                selectedHorizontalOffsets[i] += selectedHorizontalOffsetTotal/buttonAnimationTimeMax * (float)deltaTime;
            }
            else
            {
                selectedHorizontalOffsets[i] -= selectedHorizontalOffsetTotal/buttonAnimationTimeMax * (float)deltaTime;
            }

            selectedHorizontalOffsets[i] = MathHelper.Clamp(selectedHorizontalOffsets[i], 0, selectedHorizontalOffsetTotal);
        }
    }

    public void Draw(GameTime gameTime)
    {
        //Background
        DrawBackground(gameTime);
        
        if (menuState == MenuState.MapKeys)
        {
            selectingKeys.Draw(spriteBatch, tile, font);
            return;
        }

        //All buttons and text, based on what menu the player is on
        switch (menuState)
        {
            case MenuState.Explainer:
                int textVerticalSpacing = 40;
                //Text background
                spriteBatch.Draw(tile, new Rectangle(30, 30, Main.WorldWidth-60,  76+textVerticalSpacing*20+20), Color.Black);
                
                //Text wall
                spriteBatch.DrawString(font, "Hey, looks like you're new around here", new Vector2(50, 50), Color.White);
                spriteBatch.DrawString(font, "This is our take on modern Tetris", new Vector2(50, 50 + textVerticalSpacing), Color.White);
                spriteBatch.DrawString(font, "Press f11 to toggle fullscreen", new Vector2(50, 50 + textVerticalSpacing*2), Color.White);
                spriteBatch.DrawString(font, "Use the up and down arrow keys to move through the menu", new Vector2(50, 50 + textVerticalSpacing*3), Color.White);
                spriteBatch.DrawString(font, "You can press enter to select a button", new Vector2(50, 50 + textVerticalSpacing*4), Color.White);
                spriteBatch.DrawString(font, "Use the left and right arrow keys to cycle through values", new Vector2(50, 50 + textVerticalSpacing*5), Color.White);
                spriteBatch.DrawString(font, "You can press escape to move back one menu", new Vector2(50, 50 + textVerticalSpacing*6), Color.White);
                spriteBatch.DrawString(font, "If you don't like the default controls, create a profile", new Vector2(50, 50 + textVerticalSpacing*7), Color.White);
                spriteBatch.DrawString(font, "Profiles store your controls to easily be able to play how you want", new Vector2(50, 50 + textVerticalSpacing*8), Color.White);
                spriteBatch.DrawString(font, "Profiles also allow you to assign multiple keys to one action", new Vector2(50, 50 + textVerticalSpacing*9), Color.White);
                spriteBatch.DrawString(font, "Customize the settings and profiles as much as you want", new Vector2(50, 50 + textVerticalSpacing*10), Color.White);
                spriteBatch.DrawString(font, "Your settings and profiles are saved automatically", new Vector2(50, 50 + textVerticalSpacing*11), Color.White);
                spriteBatch.DrawString(font, "The default controls are as follows", new Vector2(50, 50 + textVerticalSpacing*12), Color.White);
                spriteBatch.DrawString(font, "Movement + soft drop: arrow keys or A/S/D", new Vector2(50, 50 + textVerticalSpacing*13), Color.White);
                spriteBatch.DrawString(font, "Clockwise rotation: Up arrow/X/E", new Vector2(50, 50 + textVerticalSpacing*14), Color.White);
                spriteBatch.DrawString(font, "Counterclockwise rotation: Z/Q", new Vector2(50, 50 + textVerticalSpacing*15), Color.White);
                spriteBatch.DrawString(font, "Hard drop: space", new Vector2(50, 50 + textVerticalSpacing*16), Color.White);
                spriteBatch.DrawString(font, "Hold: shift", new Vector2(50, 50 + textVerticalSpacing*17), Color.White);
                spriteBatch.DrawString(font, "Pause: escape", new Vector2(50, 50 + textVerticalSpacing*18), Color.White);
                spriteBatch.DrawString(font, "Now let's get to playing, press enter to close this message", new Vector2(50, 50 + textVerticalSpacing*19), Color.White);
                break;
            
            case MenuState.MainMenu:
                DrawButton("Play", 0);
                DrawButton(gameModeNames[gameModeIndex], 0, "Play");
                DrawButton("Settings", 1);
                DrawButton("Quit", 2);
                break;
            
            case MenuState.LobbyStandard:
                DrawButton("Start", 0);
                DrawButton("Profile", 1);
                DrawButton(main.settings.controlProfiles[profileIndex].controlName, 1, "Profile");
                DrawButton("Starting Level", 2);
                DrawButton(main.settings.game.startingLevel.ToString(), 2, "Starting Level");
                DrawButton("Gravity Multiplier", 3);
                DrawButton($"{MathF.Round((float)main.settings.game.gravityMultiplier*10)/10}x", 3, "Gravity Multiplier");
                DrawButton("Width", 4);
                DrawButton(main.settings.game.width.ToString(), 4, "Width");
                DrawButton("Back", 5);
                break;
            
            case MenuState.LobbyTugOfWar:
                DrawButton("Start", 0);
                DrawButton("Profile 1", 1);
                DrawButton(main.settings.controlProfiles[profileIndex].controlName, 1, "Profile 1");
                DrawButton("Profile 2", 2);
                DrawButton(main.settings.controlProfiles[profileIndex2].controlName, 2, "Profile 2");
                DrawButton("Lines to win", 3);
                DrawButton(main.settings.game.linesToWin.ToString(), 3, "Lines to win");
                DrawButton("Starting Level", 4);
                DrawButton(main.settings.game.startingLevel.ToString(), 4, "Starting Level");
                DrawButton("Gravity Multiplier", 5);
                DrawButton($"{MathF.Round((float)main.settings.game.gravityMultiplier*10)/10}x", 5, "Gravity Multiplier");
                DrawButton("Width", 6);
                DrawButton(main.settings.game.width.ToString(), 6, "Width");
                DrawButton("Back", 7);
                
                //Check for conflicting keybinds
                if (CheckConflictingKeybinds(main.settings.controlProfiles[profileIndex],
                        main.settings.controlProfiles[profileIndex2]))
                {
                    spriteBatch.DrawString(font, "Warning: There are conflicting keybinds", new Vector2(620, 62), Color.Red);
                }
                break;
            
            case MenuState.LobbyVersus:
                DrawButton("Start", 0);
                DrawButton("Profile 1", 1);
                DrawButton(main.settings.controlProfiles[profileIndex].controlName, 1, "Profile 1");
                DrawButton("Profile 2", 2);
                DrawButton(main.settings.controlProfiles[profileIndex2].controlName, 2, "Profile 2");
                DrawButton("Garbage Multiplier", 3);
                DrawButton($"{MathF.Round((float)main.settings.game.garbageMultiplier*10)/10}x", 3, "Garbage Multiplier");
                DrawButton("Starting Level", 4);
                DrawButton(main.settings.game.startingLevel.ToString(), 4, "Starting Level");
                DrawButton("Gravity Multiplier", 5);
                DrawButton($"{MathF.Round((float)main.settings.game.gravityMultiplier*10)/10}x", 5, "Gravity Multiplier");
                DrawButton("Width", 6);
                DrawButton(main.settings.game.width.ToString(), 6, "Width");
                DrawButton("Back", 7);

                //Check for conflicting keybinds
                if (CheckConflictingKeybinds(main.settings.controlProfiles[profileIndex],
                        main.settings.controlProfiles[profileIndex2]))
                {
                    spriteBatch.DrawString(font, "Warning: There are conflicting keybinds", new Vector2(620, 62), Color.Red);
                }
                break;
            
            case MenuState.Settings:
                DrawButton("Master Volume", 0);
                DrawButton($"{main.settings.masterVolume}%", 0, "Master Volume");
                DrawButton("Music Volume", 1);
                DrawButton($"{main.settings.musicVolume}%", 1, "Music Volume");
                DrawButton("Sfx Volume", 2);
                DrawButton($"{main.settings.soundEffectVolume}%", 2, "Sfx Volume");
                string musicMode = main.settings.useClassicMusic ? "Classic" : "Modern";
                DrawButton("Music", 3);
                DrawButton(musicMode, 3, "Music");
                DrawButton("Controls", 4);
                DrawButton("Back", 5);
                break;
            
            case MenuState.ControlProfiles:
                //Draw all control profiles
                
                for (int i = 0; i < main.settings.controlProfiles.Count+3; i++)
                {
                    //Draw button at the bottom of the list
                    if (i == main.settings.controlProfiles.Count)
                    {
                        DrawButton("Create Profile", i);
                        if (newProfileName != "")
                        {
                            DrawButton(newProfileName, i, "Create Profile");
                        }
                        else
                        {
                            DrawButton("Start typing to enter name", i, "Create Profile");
                        }
                        continue;
                    }
                    //Draw button at the bottom of the list
                    if (i == main.settings.controlProfiles.Count+1)
                    {
                        DrawButton("Delete Profile", i);
                        DrawButton(main.settings.controlProfiles[profileIndex].controlName, i, "Delete Profile");
                        continue;
                    }
                    //Draw button at the bottom of the list
                    if (i == main.settings.controlProfiles.Count+2)
                    {
                        DrawButton("Back", i);
                        continue;
                    }
                    
                    //Otherwise draw button representing that control scheme
                    DrawButton(main.settings.controlProfiles[i].controlName, i);
                }
                break;
            
            case MenuState.Controls:
                Controls profile = main.settings.controlProfiles[editingControlScheme];
                DrawButton("Move Left", 0);
                DrawButton(ArrayListedAsString(profile.leftKey), 0, "Move Left");
                DrawButton("Move Right", 1);
                DrawButton(ArrayListedAsString(profile.rightKey), 1, "Move Right");
                DrawButton("Soft Drop", 2);
                DrawButton(ArrayListedAsString(profile.softDropKey), 2, "Soft Drop");
                DrawButton("Hard Drop", 3);
                DrawButton(ArrayListedAsString(profile.hardDropKey), 3, "Hard Drop");
                DrawButton("Rotate Clockwise", 4);
                DrawButton(ArrayListedAsString(profile.rotateClockWiseKey), 4, "Rotate Clockwise");
                DrawButton("Rotate Counterclockwise", 5);
                DrawButton(ArrayListedAsString(profile.rotateCounterClockWiseKey), 5, "Rotate Counterclockwise");
                DrawButton("Hold", 6);
                DrawButton(ArrayListedAsString(profile.holdKey), 6, "Hold");
                DrawButton("Back", 7);

                break;
        }
    }

    //Draw moving tile background
    private void DrawBackground(GameTime gameTime)
    {
        int tileCountHorizontal = 10; //Amount of tiles in horizontal direction on screen at once
        int tileSize = Main.WorldWidth / tileCountHorizontal;
        int aspectRatio = Main.WorldWidth / Main.WorldHeight;
        double timeFrame = gameTime.TotalGameTime.TotalSeconds % 2.5/2.5; //Used to make background move
        
        //Draw tiles of background
        for (int i = -1; i < tileCountHorizontal; i++)
        {
            for (int j = -1; j < tileCountHorizontal/aspectRatio + 1; j++)
            {
                //Decide color based on checkerboard pattern
                Color tileColor = Color.ForestGreen;
                if ((j * (tileCountHorizontal + 1) + i) % 2 == 0)
                {
                    tileColor = Color.DarkOliveGreen;
                }
                
                //Draw tile
                spriteBatch.Draw(tile, new Rectangle((int)((i + timeFrame)*tileSize), (int)((j + timeFrame)*tileSize), tileSize, tileSize), tileColor);
            }
        } 
    }
    #endregion
    
    #region Menu Movement
    //What to do when a key is pressed
    private void MenuMovement()
    {
        //Check if a key was pressed
        if (Util.GetKeysPressed().Length == 0)
        {
            return;
        }
        
        if (Util.GetKeyPressed(Keys.Up))
        {
            if (menuIndex != 0)
            {
                menuIndex--;
            }
            else //Loop around
            {
                menuIndex = (byte)(GetMenuLength() - 1);
            }

            return;
        }
        
        if (Util.GetKeyPressed(Keys.Down))
        {
            menuIndex++;
            
            if (menuIndex == GetMenuLength()) //Loop around
            {
                menuIndex = 0;
            }
            
            return;
        }

        if (Util.GetKeyPressed(Keys.Enter))
        {
            MenuFunction(InputType.Select);
            return;
        }
        
        if (Util.GetKeyPressed(Keys.Left))
        {
            MenuFunction(InputType.MoveLeft);
            return;
        }
        
        if (Util.GetKeyPressed(Keys.Right))
        {
            MenuFunction(InputType.MoveRight);
            return;
        }
        
        if (Util.GetKeyPressed(Keys.Back))
        {
            MenuFunction(InputType.Back);
            return;
        }
        
        //Other key was pressed
        MenuFunction(InputType.Text);
    }
    #endregion

    #region Menu functions
    //What effect a type of input has on the menu
    private void MenuFunction(InputType inputType)
    {
        //First check what menu the player is in, then check what button is selected, finally check what to do with the specified input
        switch (menuState)
        {
            case MenuState.Explainer:
                if (inputType == InputType.Select)
                {
                    GoToMenu(MenuState.MainMenu);
                }
                break;
            
            case MenuState.MainMenu:
                switch (menuIndex)
                {
                    case (byte)MainMenu.Play:
                        if (inputType == InputType.Select)
                        {
                            switch (gameModeIndex)
                            {
                                default:
                                    GoToMenu(MenuState.LobbyStandard);
                                    break;
                                case (byte)GameMode.TugOfWar:
                                    GoToMenu(MenuState.LobbyTugOfWar);
                                    break;
                                case (byte)GameMode.Versus:
                                    GoToMenu(MenuState.LobbyVersus);
                                    break;
                            }
                        }
                        if (inputType == InputType.MoveRight) gameModeIndex = ToggleNext(gameModeNames, gameModeIndex);
                        if (inputType == InputType.MoveLeft) gameModeIndex = TogglePrevious(gameModeNames, gameModeIndex);
                        break;
                    case (byte)MainMenu.Settings:
                        if (inputType == InputType.Select) GoToMenu(MenuState.Settings);
                        break;
                    case (byte)MainMenu.Quit:
                        if (inputType == InputType.Select) main.QuitGame();
                        break;
                }
                break;
            
            case MenuState.LobbyStandard:
                switch (menuIndex)
                {
                    case (byte)LobbyStandard.Start:
                        if (inputType == InputType.Select) main.gameState = GameState.Playing;
                        break;
                    case (byte)LobbyStandard.Profile:
                        if (inputType == InputType.Select) profileIndex = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex);
                        if (inputType == InputType.MoveRight) profileIndex = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex);
                        if (inputType == InputType.MoveLeft) profileIndex = TogglePrevious(main.settings.controlProfiles.ToArray(), profileIndex);
                        break;
                    case (byte)LobbyStandard.StartingLevel:
                        if (inputType == InputType.Select) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, 1, 1, 15);
                        if (inputType == InputType.MoveRight) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, 1, 1, 15);
                        if (inputType == InputType.MoveLeft) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, -1, 1, 15);
                        break;
                    case (byte)LobbyStandard.GravityMultiplier:
                        if (inputType == InputType.Select) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, 0.1, 0.1, 5);
                        if (inputType == InputType.MoveRight) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, 0.1, 0.1, 5);
                        if (inputType == InputType.MoveLeft) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, -0.1, 0.1, 5);
                        break;
                    case (byte)LobbyStandard.Width:
                        if (inputType == InputType.Select) main.settings.game.width = (byte)Increment(main.settings.game.width, 1, 4, 12);
                        if (inputType == InputType.MoveRight) main.settings.game.width = (byte)Increment(main.settings.game.width, 1, 4, 12);
                        if (inputType == InputType.MoveLeft) main.settings.game.width = (byte)Increment(main.settings.game.width, -1, 4, 12);
                        break;
                    case (byte)LobbyStandard.Back:
                        if (inputType == InputType.Select) GoToMenu(MenuState.MainMenu);
                        break;
                }
                break;
            
            case MenuState.LobbyTugOfWar:
                switch (menuIndex)
                {
                    case (byte)LobbyTugOfWar.Start:
                        if (inputType == InputType.Select) main.gameState = GameState.Playing;
                        break;
                    case (byte)LobbyTugOfWar.Profile1:
                        if (inputType == InputType.Select) profileIndex = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex);
                        if (inputType == InputType.MoveRight) profileIndex = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex);
                        if (inputType == InputType.MoveLeft) profileIndex = TogglePrevious(main.settings.controlProfiles.ToArray(), profileIndex);
                        break;
                    case (byte)LobbyTugOfWar.Profile2:
                        if (inputType == InputType.Select) profileIndex2 = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex2);
                        if (inputType == InputType.MoveRight) profileIndex2 = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex2);
                        if (inputType == InputType.MoveLeft) profileIndex2 = TogglePrevious(main.settings.controlProfiles.ToArray(), profileIndex2);
                        break;
                    case (byte)LobbyTugOfWar.LinesToWin:
                        if (inputType == InputType.Select) main.settings.game.linesToWin = Increment(main.settings.game.linesToWin, 1, 5, 50);
                        if (inputType == InputType.MoveRight) main.settings.game.linesToWin = Increment(main.settings.game.linesToWin, 1, 5, 50);
                        if (inputType == InputType.MoveLeft) main.settings.game.linesToWin = Increment(main.settings.game.linesToWin, -1, 5, 50);
                        break;
                    case (byte)LobbyTugOfWar.StartingLevel:
                        if (inputType == InputType.Select) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, 1, 1, 15);
                        if (inputType == InputType.MoveRight) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, 1, 1, 15);
                        if (inputType == InputType.MoveLeft) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, -1, 1, 15);
                        break;
                    case (byte)LobbyTugOfWar.GravityMultiplier:
                        if (inputType == InputType.Select) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, 0.1, 0.1, 5);
                        if (inputType == InputType.MoveRight) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, 0.1, 0.1, 5);
                        if (inputType == InputType.MoveLeft) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, -0.1, 0.1, 5);
                        break;
                    case (byte)LobbyTugOfWar.Width:
                        if (inputType == InputType.Select) main.settings.game.width = (byte)Increment(main.settings.game.width, 1, 4, 12);
                        if (inputType == InputType.MoveRight) main.settings.game.width = (byte)Increment(main.settings.game.width, 1, 4, 12);
                        if (inputType == InputType.MoveLeft) main.settings.game.width = (byte)Increment(main.settings.game.width, -1, 4, 12);
                        break;
                    case (byte)LobbyTugOfWar.Back:
                        if (inputType == InputType.Select) GoToMenu(MenuState.MainMenu);
                        break;
                }
                break;
            
            case MenuState.LobbyVersus:
                    switch (menuIndex)
                    {
                        case (byte)LobbyVersus.Start:
                            if (inputType == InputType.Select) main.gameState = GameState.Playing;
                            break;
                        case (byte)LobbyVersus.Profile1:
                            if (inputType == InputType.Select) profileIndex = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex);
                            if (inputType == InputType.MoveRight) profileIndex = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex);
                            if (inputType == InputType.MoveLeft) profileIndex = TogglePrevious(main.settings.controlProfiles.ToArray(), profileIndex);
                            break;
                        case (byte)LobbyVersus.Profile2:
                            if (inputType == InputType.Select) profileIndex2 = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex2);
                            if (inputType == InputType.MoveRight) profileIndex2 = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex2);
                            if (inputType == InputType.MoveLeft) profileIndex2 = TogglePrevious(main.settings.controlProfiles.ToArray(), profileIndex2);
                            break;
                        case (byte)LobbyVersus.GarbageMultiplier:
                            if (inputType == InputType.Select) main.settings.game.garbageMultiplier = Increment(main.settings.game.garbageMultiplier, 0.1, 0.1, 5);
                            if (inputType == InputType.MoveRight) main.settings.game.garbageMultiplier = Increment(main.settings.game.garbageMultiplier, 0.1, 0.1, 5);
                            if (inputType == InputType.MoveLeft) main.settings.game.garbageMultiplier = Increment(main.settings.game.garbageMultiplier, -0.1, 0.1, 5);
                            break;
                        case (byte)LobbyVersus.StartingLevel:
                            if (inputType == InputType.Select) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, 1, 1, 15);
                            if (inputType == InputType.MoveRight) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, 1, 1, 15);
                            if (inputType == InputType.MoveLeft) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, -1, 1, 15);
                            break;
                        case (byte)LobbyVersus.GravityMultiplier:
                            if (inputType == InputType.Select) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, 0.1, 0.1, 5);
                            if (inputType == InputType.MoveRight) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, 0.1, 0.1, 5);
                            if (inputType == InputType.MoveLeft) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, -0.1, 0.1, 5);
                            break;
                        case (byte)LobbyVersus.Width:
                            if (inputType == InputType.Select) main.settings.game.width = (byte)Increment(main.settings.game.width, 1, 4, 12);
                            if (inputType == InputType.MoveRight) main.settings.game.width = (byte)Increment(main.settings.game.width, 1, 4, 12);
                            if (inputType == InputType.MoveLeft) main.settings.game.width = (byte)Increment(main.settings.game.width, -1, 4, 12);
                            break;
                        case (byte)LobbyVersus.Back:
                            if (inputType == InputType.Select) GoToMenu(MenuState.MainMenu);
                            break;
                    }
                    break;
            
            case MenuState.Settings:
                switch (menuIndex)
                {
                    case (byte)SettingsMenu.MasterVolume:
                        if (inputType == InputType.Select) { main.settings.masterVolume = Increment(main.settings.masterVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveRight) { main.settings.masterVolume = Increment(main.settings.masterVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveLeft) { main.settings.masterVolume = Increment(main.settings.masterVolume, -10, 0, 100); main.UpdateVolume();}
                        break;
                    case (byte)SettingsMenu.MusicVolume:
                        if (inputType == InputType.Select) { main.settings.musicVolume = Increment(main.settings.musicVolume,  10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveRight) { main.settings.musicVolume = Increment(main.settings.musicVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveLeft) { main.settings.musicVolume = Increment(main.settings.musicVolume, -10, 0, 100); main.UpdateVolume();}
                        break;
                    case (byte)SettingsMenu.SfxVolume:
                        if (inputType == InputType.Select) { main.settings.soundEffectVolume = Increment(main.settings.soundEffectVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveRight) { main.settings.soundEffectVolume = Increment(main.settings.soundEffectVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveLeft) { main.settings.soundEffectVolume = Increment(main.settings.soundEffectVolume, -10, 0, 100); main.UpdateVolume();}
                        break;
                    case (byte)SettingsMenu.Music:
                        if (inputType == InputType.Select || 
                            inputType == InputType.MoveLeft || 
                            inputType == InputType.MoveRight) {main.settings.useClassicMusic = !main.settings.useClassicMusic;}
                        break;
                    case (byte)SettingsMenu.Controls:
                        if(inputType == InputType.Select) GoToMenu(MenuState.ControlProfiles);
                        break;
                    case (byte)SettingsMenu.Back:
                        if (inputType == InputType.Select) GoToMenu(MenuState.MainMenu);
                        break;
                }
                break;
            
            case MenuState.ControlProfiles:
                //ControlProfiles menu has a dynamic length based on the amount of profiles that exist
                if (inputType == InputType.Select)
                {
                    //Create new profile button
                    if (menuIndex == GetMenuLength() - 3)
                    {
                        //Only make new control profile if name is not empty
                        if (newProfileName != "" && main.settings.controlProfiles.Count < maxAmountOfProfiles)
                        {
                            //Only make new control profile if name is not in use yet
                            foreach (var profiles in main.settings.controlProfiles)
                            {
                                if (string.Equals(profiles.controlName, newProfileName, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    return;
                                }
                            }
                            
                            //Create new set of controls
                            Controls newControls = new Controls();
                            newControls.controlName = newProfileName;
                            main.settings.controlProfiles.Add(newControls);
                            editingControlScheme = main.settings.controlProfiles.Count-1; //Add() adds to end of list, set control scheme to end of list
                            GoToMenu(MenuState.Controls);
                        }
                        break;
                    }
                    
                    //Delete profile button
                    if (menuIndex == GetMenuLength() - 2)
                    {
                        //Can't delete default
                        if (profileIndex != 0)
                        {
                            main.settings.controlProfiles.RemoveAt(profileIndex);
                        }
                        
                        //Update index position (because one less button means the selected button would shift)
                        GoToMenu(MenuState.ControlProfiles, (byte)(GetMenuLength() - 2));
                        break;
                    }
                    
                    //Back button
                    if (menuIndex == GetMenuLength() - 1)
                    {
                        GoToMenu(MenuState.Settings);
                        break;
                    }

                    //If not back button select the control scheme and go to menu page
                    editingControlScheme = menuIndex;
                    GoToMenu(MenuState.Controls);
                }

                //If hovering over create profile and typing text
                if (inputType == InputType.Text && menuIndex == GetMenuLength() - 3)
                {
                    //Add text to string, with a limit on what characters can be user, not case sensitive
                    newProfileName += FilterInput(Util.GetKeysPressed(), "abcdefghijklmnopqrstuvwxyz");
                }

                //If hovering over create profile and deleting text
                if (inputType == InputType.Back && menuIndex == GetMenuLength() - 3 && newProfileName.Length != 0)
                {
                    //Delete last letter of string
                    newProfileName = newProfileName.Substring(0, newProfileName.Length - 1);
                }

                //Only be able to toggle through list profiles if there is more than 1, because otherwise edge case causes error
                if (main.settings.controlProfiles.Count > 1)
                {
                    if (inputType == InputType.MoveLeft && menuIndex == GetMenuLength() - 2) { profileIndex = TogglePrevious(main.settings.controlProfiles.ToArray(), profileIndex);}
                    if (inputType == InputType.MoveRight && menuIndex == GetMenuLength() - 2) { profileIndex = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex);}
                }
                break;
            
            case MenuState.Controls:
                if (inputType == InputType.Select)
                {
                    //Back button
                    if (menuIndex == (byte)ControlsMenu.Back)
                    {
                        GoToMenu(MenuState.ControlProfiles);
                        break;
                    }
                    
                    //If not back button go to selecting keybind
                    selectingKeys = new SelectingKeys(main, this, editingControlScheme, (ControlsMenu)menuIndex);
                    GoToMenu(MenuState.MapKeys);
                }
                break;
        }
    }
    #endregion
    
    #region Extra functions
    //Only parse allowed characters to string
    private string FilterInput(Keys[] keys, string filter)
    {
        //Filter works as a whitelist
        string text = "";
        
        //Store all filtered characters in a list
        List<string> filterList = new List<string>();
        for (int i = 0; i < filter.Length; i++)
        {
            filterList.Add(filter[i].ToString().ToUpper()); 
        }

        //Check for each input if it is in the filter or not
        foreach (var key in keys)
        {
            if (filterList.Contains(key.ToString()))
            {
                text += key.ToString();
            }
        }

        return text;
    }
    
    public void GoToMenu(MenuState state, byte goToIndex = 0)
    {
        //Save settings
        main.SaveSettings();
        
        //Special cases for these menus
        if (menuState is MenuState.ControlProfiles or MenuState.Settings)
        {
            newProfileName = "";
            profileIndex = 0;
            
            //Don't show default profile in delete list
            if (main.settings.controlProfiles.Count > 1)
            {
                profileIndex = 1;
            }
        }
        else if(menuState is MenuState.MainMenu or MenuState.LobbyStandard or MenuState.LobbyTugOfWar or MenuState.LobbyVersus)
        {
            profileIndex = 0;
            profileIndex2 = 0;
        }
        
        //Don't reset index if in controls, because this is way nicer when changing the entire list as is typically done
        if (menuState != MenuState.Controls)
        {
            menuIndex = goToIndex;
        }
        
        menuState = state;
        
        //Extra check to avoid some edge case errors
        if (menuState == MenuState.ControlProfiles)
        {
            menuIndex = goToIndex;
        }
        
        //Update the horizontal offsets of the buttons
        selectedHorizontalOffsets = new float[GetMenuLength()];
        selectedHorizontalOffsets[menuIndex] = selectedHorizontalOffsetTotal;
    }
    
    //Toggle to next in list
    private byte ToggleNext<T>(T[] array, byte index)
    {
        //If on delete profile
        if (menuState == MenuState.ControlProfiles && menuIndex == GetMenuLength() - 2)
        {
            //Skip default profile
            if (index == array.Length - 1)
            {
                return 1;
            }
        }
        
        //Loop around
        if (index == array.Length - 1)
        {
            return 0;
        }

        return (byte)(index + 1);
    }

    //Toggle to previous in list
    private byte TogglePrevious<T>(T[] array, byte index)
    {
        //If on delete profile
        if (menuState == MenuState.ControlProfiles && menuIndex == GetMenuLength() - 2)
        {
            //Skip default profile
            if (index == 1)
            {
                return (byte)(array.Length-1);
            }
        }
        
        //Loop around
        if (index == 0)
        {
            return (byte)(array.Length - 1);
        }

        return (byte)(index - 1);
    }

    //Increase / decrease by a fixed amount
    private int Increment(int value, int increment, int min, int max)
    {
        value += increment;
        return MathHelper.Clamp(value, min, max);
    }
    
    private double Increment(double value, double increment, double min, double max)
    {
        value += increment;
        return Util.Clamp(value, min, max);
    }

    //Gets the length of a menu (the amount of buttons with a different Y value), used for looping and horizontal offset of buttons
    private int GetMenuLength()
    {
        switch (menuState)
        {
            case MenuState.MainMenu:
                return Enum.GetNames(typeof(MainMenu)).Length;
            case MenuState.LobbyStandard:
                return Enum.GetNames(typeof(LobbyStandard)).Length;
            case MenuState.LobbyTugOfWar:
                return Enum.GetNames(typeof(LobbyTugOfWar)).Length;
            case MenuState.LobbyVersus:
                return Enum.GetNames(typeof(LobbyVersus)).Length;
            case MenuState.Settings:
                return Enum.GetNames(typeof(SettingsMenu)).Length;
            case MenuState.ControlProfiles:
                return main.settings.controlProfiles.Count+3; //3 extra for create new, delete, and back button
            case MenuState.Controls:
                return Enum.GetNames(typeof(ControlsMenu)).Length;
            case MenuState.MapKeys:
                return 8;
            default: //Error value
                return 0;
        }
    }

    //Button color is based on index and is colored with a color of the tetris pieces
    private Color GetButtonColor(int index)
    {
        if (menuIndex == index)
        {
            switch (index % 7)
            {
                case 0:
                    return Color.LightBlue;
                case 1:
                    return Color.Blue;
                case 2:
                    return Color.Orange;
                case 3:
                    return Color.Yellow;
                case 4:
                    return Color.Red;
                case 5:
                    return Color.Purple;
                case 6:
                    return Color.LightGreen;
            }
        }

        //If button is not selected
        return Color.White;
    }

    //List out an entire array as a string
    public string ArrayListedAsString(Keys[] keys)
    {
        string text = "[";

        for (int i = 0; i < keys.Length; i++)
        {
            if (i > 0)
            {
                text += ", ";
            }
            text += keys[i].ToString();
        }

        text += "]";

        return text;
    }

    private bool CheckConflictingKeybinds(Controls controls1, Controls controls2)
    {
        //Get all controls from first control profile
        List<Keys> allKeysControls1 = new();
        allKeysControls1.AddRange(controls1.leftKey.ToList());
        allKeysControls1.AddRange(controls1.rightKey.ToList());
        allKeysControls1.AddRange(controls1.softDropKey.ToList());
        allKeysControls1.AddRange(controls1.hardDropKey.ToList());
        allKeysControls1.AddRange(controls1.rotateClockWiseKey.ToList());
        allKeysControls1.AddRange(controls1.rotateCounterClockWiseKey.ToList());
        allKeysControls1.AddRange(controls1.holdKey.ToList());
        
        //Get all controls from second control profile
        List<Keys> allKeysControls2 = new();
        allKeysControls2.AddRange(controls2.leftKey.ToList());
        allKeysControls2.AddRange(controls2.rightKey.ToList());
        allKeysControls2.AddRange(controls2.softDropKey.ToList());
        allKeysControls2.AddRange(controls2.hardDropKey.ToList());
        allKeysControls2.AddRange(controls2.rotateClockWiseKey.ToList());
        allKeysControls2.AddRange(controls2.rotateCounterClockWiseKey.ToList());
        allKeysControls2.AddRange(controls2.holdKey.ToList());

        //Check for matches
        foreach (var key in allKeysControls1)
        {
            if (allKeysControls2.Contains(key))
            {
                return true;
            }
        }

        //If no matches return false
        return false;
    }

    //Gets the length of a button based on what text it holds
    private int GetButtonLength(string text)
    {
        //Assumes buttons are 80 pixels tall (and begin and end are also 80 pixels wide)
        if (text == null)
        {
            return 160;
        }
        
        return 160 + (int)font.MeasureString(text).X;
    }

    //Draw a button with a dynamic length and spacing
    private void DrawButton(string text, int index, string previousString = null)
    {
        //Get button spacing
        int horizontalOffset = GetButtonLength(previousString);

        //Extra horizontal offset if button is selected
        horizontalOffset += (int)selectedHorizontalOffsets[index];

        //Extra spacing so buttons aren't glued together
        if (previousString != null)
        {
            horizontalOffset += 100;
        }

        //Creates a button with a dynamic length based on the length of the string
        spriteBatch.Draw(buttonBegin, new Vector2(horizontalOffset, 0) + topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
        spriteBatch.Draw(buttonMiddle, new Vector2(horizontalOffset + 80,0) + topLeftTopButtonPosition + buttonVerticalOffset * index, null, GetButtonColor(index), 0f, Vector2.Zero, new Vector2(font.MeasureString(text).X, 1), SpriteEffects.None, 0f);
        spriteBatch.Draw(buttonEnd, new Vector2( horizontalOffset + 80 + font.MeasureString(text).X,0) + topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
        
        //Draw text on button
        spriteBatch.DrawString(font, text, new Vector2(horizontalOffset + 80,12) + topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
    }
    #endregion
    
    //What type of input is done on a button
    private enum InputType
    {
        Select,
        MoveLeft,
        MoveRight,
        Back,
        Text,
    }
}

#region Enums
//Enums for all menus (MenuState being what menu the player is on and every other enum what that menu holds
public enum MenuState
{
    MainMenu,
    LobbyStandard,
    LobbyTugOfWar,
    LobbyVersus,
    Settings,
    ControlProfiles,
    Controls,
    MapKeys,
    Explainer,
}

public enum MainMenu
{
    Play,
    Settings,
    Quit,
}

public enum LobbyStandard
{
    Start,
    Profile,
    StartingLevel,
    GravityMultiplier,
    Width,
    Back,
}

public enum LobbyTugOfWar
{
    Start,
    Profile1,
    Profile2,
    LinesToWin,
    StartingLevel,
    GravityMultiplier,
    Width,
    Back,
}

public enum LobbyVersus
{
    Start,
    Profile1,
    Profile2,
    GarbageMultiplier,
    StartingLevel,
    GravityMultiplier,
    Width,
    Back,
}

public enum SettingsMenu
{
    MasterVolume,
    MusicVolume,
    SfxVolume,
    Music,
    Controls,
    Back,
}

public enum ControlsMenu
{
    MoveLeft,
    MoveRight,
    SoftDrop, 
    HardDrop,
    RotateClockWise,
    RotateCounterClockWise,
    Hold,
    Back
}
#endregion