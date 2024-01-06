using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Net.Mime;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TwoPlayerTagGame.GameProject
{
    
    public enum LevelState
    {
        Menu,
        Game,
        Gameover,
    }

    public class Level
    {
        public class LevelData
        {
            public int[] data { get; set; }
            public int height { get; set; }
            public int width { get; set; }
            public int x { get; set; }
            public int y { get; set; }
        }

        private string Title = "Two Player Tag";

        private Button PlayButton;
        private Texture2D PlayButtonSprite;

        private Button LevelButton;
        private Texture2D LevelButtonSprite;

        private string Map;
        private string MapName;
        private int MapCode;

        private SpriteFont TextSprite;

        private float tagTimer;
        private float CountdownTime;

        public ContentManager Content { get; private set; }
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }

        private Tile[,] Tiles;
        private LevelState CurrentState;
        private Vector2 StartingPosition1 = new Vector2(400,500);
        private Vector2 StartingPosition2 = new Vector2(800, 500);

        public LevelData Data;

        public Level(IServiceProvider serviceProvider)
        {
            CurrentState = LevelState.Menu;
            CountdownTime = 30f;

            Content = new ContentManager(serviceProvider, "Content");
            TextSprite = Content.Load<SpriteFont>("TextFont");
            PlayButtonSprite = Content.Load<Texture2D>("ButtonBorder");
            LevelButtonSprite = Content.Load<Texture2D>("ButtonBorder");

            #region PlayButton

            PlayButton = new Button(PlayButtonSprite, TextSprite)
            {
                Text = "Play"
            };

            PlayButton.SetPosition(Game1.GameWindowWidth / 2 - 100, Game1.GameWindowHeight / 2);

            PlayButton.Click += (sender, e) =>
            {
                string levelpath = string.Format("C:/Users/Harve/Documents/MonoGame Projects/2PlayerTagGame/{0}", Map);
                var LevelFile = File.ReadAllText(levelpath);
                Data = JsonSerializer.Deserialize<LevelData>(LevelFile);
                LoadTiles();
                CurrentState = LevelState.Game;
            };

            #endregion

            #region LevelButton

            LevelButton = new Button(LevelButtonSprite, TextSprite);

            LevelButton.SetPosition(Game1.GameWindowWidth / 2 + 100, Game1.GameWindowHeight / 2);

            LevelButton.Click += (sender, e) =>
            {
                MapCode++;
            };


            #endregion


            Player1 = new Player(Content, this, StartingPosition1, Keys.A, Keys.D, Keys.W, Keys.S, "Player 1");
            Player1.tagged = true;
            Player2 = new Player(Content, this, StartingPosition2, Keys.J, Keys.L, Keys.I, Keys.K, "Player 2");

        }

        private void LoadTiles()
        {
            Tiles = new Tile[Data.width, Data.height];

            for (int y = 0; y < Data.height; ++y)
            {
                for (int x = 0; x < Data.width; ++x)
                {
                    if (Data.data[x + (y * Data.width)] == 1)
                    {
                        Tiles[x, y] = new Tile(Content.Load<Texture2D>("Tiles/WhiteTile"), TileCollision.Impassable);
                    }
                    else
                    {
                        Tiles[x, y] = new Tile(null, TileCollision.Passable);
                    }
                }
            }
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {

            for (int y = 0; y < Data.height; ++y) 
            {
                for (int x = 0; x < Data.width; ++x)
                {
                    Texture2D texture = Tiles[x, y].Texture;
                    if (texture != null)
                    {
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        public TileCollision GetCollision(int x, int y)
        {
            if (x < 0 || x >= Data.width)
                return TileCollision.Impassable;
            if (y < 0 || y >= Data.height)
                return TileCollision.Passable;

            return Tiles[x, y].Collision;
        }

        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        public void UpdateMenu(GameTime gameTime)
        {
            switch (MapCode)
            {
                case 0:
                    Map = "Map1.json";
                    MapName = "Map 1";
                    break;
                case 1:
                    Map = "Map2.json";
                    MapName = "Map 2";
                    break;
                case 2:
                    Map = "Map3.json";
                    MapName = "Map 3";
                    break;
                case 3:
                    Map = "Map4.json";
                    MapName = "Map 4";
                    break;
                case 4:
                    MapCode = 0;    
                    break;
            }

            LevelButton.Text = MapName;
        }

        public void DrawMenu(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString( // SpriteFont, Text, Position, Colour, Rotation, Origin, Scale, SpriteEffects, LayerDepth
                TextSprite, 
                Title, 
                new Vector2(Game1.GameWindowWidth / 2, Game1.GameWindowHeight / 4),
                Color.Black,
                0f, 
                new Vector2(TextSprite.MeasureString(Title).X / 2, 0),
                2.5f,
                SpriteEffects.None,
                0);
        }

        public void Update(GameTime gameTime)
        {
            if (CurrentState == LevelState.Menu)
            {
                UpdateMenu(gameTime);
                PlayButton.Update(gameTime);
                LevelButton.Update(gameTime);
                return;
            }
            else if (CurrentState == LevelState.Gameover)
            {
                return;
            }

            Player1.Update(gameTime);
            Player2.Update(gameTime);

            tagTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            CountdownTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;           

            if (Player1.BoundingRectangle.Intersects(Player2.BoundingRectangle))
            {
                if(Player1.tagged && tagTimer >= 0.5)
                {
                    Player2.tagged = true;
                    Player1.tagged = false;
                    tagTimer = 0;
                }
                else if (Player2.tagged && tagTimer >= 0.5)
                {
                    Player2.tagged = false;
                    Player1.tagged = true;
                    tagTimer = 0;
                }
            }

            if (CountdownTime < 0)
            {
                CurrentState = LevelState.Gameover;
            }

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (CurrentState == LevelState.Menu)
            {
                DrawMenu(gameTime, spriteBatch);
                PlayButton.Draw(gameTime, spriteBatch);
                LevelButton.Draw(gameTime, spriteBatch);
                return;
            }

            DrawTiles(spriteBatch);
            Player1.Draw(gameTime, spriteBatch);
            Player2.Draw(gameTime, spriteBatch);
            spriteBatch.DrawString(TextSprite, Math.Truncate(CountdownTime).ToString(), new Vector2(100,100), Color.Black);

            if (CountdownTime < 0)
            {
                if (Player1.tagged == true)
                {
                    spriteBatch.DrawString(TextSprite, "Player 2 Wins", new Vector2(500, 300), Color.Black);
                }
                else
                {
                    spriteBatch.DrawString(TextSprite, "Player 1 Wins", new Vector2(500, 300), Color.Black);
                }
            }
        }


     }
}

