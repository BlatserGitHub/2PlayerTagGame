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

        private float tagTimer;
        private float CountdownTime;

        private SpriteFont CountdownText;

        public ContentManager Content { get; private set; }
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }

        private Tile[,] Tiles;
        private LevelState CurrentState;
        private Vector2 StartingPosition1= new Vector2(400,500);
        private Vector2 StartingPosition2 = new Vector2(800, 500);

        public LevelData Data;

        public Level(IServiceProvider serviceProvider)
        {
            Content = new ContentManager(serviceProvider, "Content");
            CountdownText = Content.Load<SpriteFont>("TextFont");

            CountdownTime = 30f;

            var LevelFile = File.ReadAllText("Map1.json");
            Data = JsonSerializer.Deserialize<LevelData>(LevelFile);

            LoadTiles();

            Player1 = new Player(Content, this, StartingPosition1, Keys.A, Keys.D, Keys.W, "Player 1");
            Player1.tagged = true;
            Player2 = new Player(Content, this, StartingPosition2, Keys.J, Keys.L, Keys.I, "Player 2");



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

        public void Update(GameTime gameTime)
        {
            if (CurrentState == LevelState.Gameover)
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
            DrawTiles(spriteBatch);
            Player1.Draw(gameTime, spriteBatch);
            Player2.Draw(gameTime, spriteBatch);
            spriteBatch.DrawString(CountdownText, Math.Truncate(CountdownTime).ToString(), new Vector2(100,100), Color.White);

            if (CountdownTime < 0)
            {
                if (Player1.tagged == true)
                {
                    spriteBatch.DrawString(CountdownText, "Player 2 Wins", new Vector2(500, 300), Color.Black);
                }
                else
                {
                    spriteBatch.DrawString(CountdownText, "Player 1 Wins", new Vector2(500, 300), Color.Black);
                }
            }
        }


     }
}

