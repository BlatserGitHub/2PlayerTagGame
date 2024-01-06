using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoPlayerTagGame.GameProject
{
    public class Player
    {
        // Constants for Horizontal Movement      
        private const float MaxMoveSpeed = 2000f;
        private const float GroundFriction = 0.60f;
        private const float AirFriction = 0.62f;

        // Constants for Vertical Movement  
        private const float MaxFallSpeed = 1500f;
        private const float MaxJumpTime = 0.35f;
        private const float JumpPower = -4500f;
        private const float JumpControlPower = 0.14f;
        private const float BufferTime = 0.15f;

        // Variables for Horizontal Movement
        private float Movement;
        private float MoveAcceleration;

        // Variables for Vertical Movement
        private float Gravity = 3400f;
        public bool isGrounded;
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;
        public float bufferTimer;
        public int jumpCounter;

        public bool tagged;

        private float previousBottom;

        private SpriteFont TextFont;
        private string PlayerText;
        private Vector2 NamePosition;

        public Texture2D SpriteTexture;
        private Color SpriteColour;


        public Vector2 Velocity;
        public Vector2 Position;


        // KeyboardStates for Controls
        private KeyboardState KeyState;
        private Keys MoveLeftKey;
        private Keys MoveRightKey;
        private Keys MoveUpKey;
        private Keys MoveDownKey;
        public Level Level { get; private set; }

        private Rectangle LocalBounds;
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X) + LocalBounds.X;
                int top = (int)Math.Round(Position.Y) + LocalBounds.Y;

                return new Rectangle(left, top, SpriteTexture.Width, SpriteTexture.Height);
            }
        }

        public Player(ContentManager content, Level level, Vector2 startingPosition, Keys moveLeft, Keys moveRight, Keys moveUp, Keys moveDown, string playerName)
        {
            Level = level;
            Position = startingPosition;

            PlayerText = playerName;

            MoveLeftKey = moveLeft;
            MoveRightKey = moveRight;
            MoveUpKey = moveUp;
            MoveDownKey = moveDown;
           
            LoadContent(content);

        }

        public void LoadContent(ContentManager content)
        {
            TextFont = content.Load<SpriteFont>("TextFont");
            SpriteTexture = content.Load<Texture2D>("PlayerSprites/WhiteSquare");

            SetLocalBounds();
        }



        public void Update(GameTime gameTime)
        {

            KeyState = Keyboard.GetState();

            GetInput(KeyState);
            ApplyMovementPhysics(gameTime);

            if (tagged)
            {
                SpriteColour = Color.Red;
                MoveAcceleration = 35000;
            }
            else
            {
                SpriteColour = Color.White;
                MoveAcceleration = 30000;
            }

            float NamePositionX = (Position.X + (SpriteTexture.Width / 2) - TextFont.MeasureString(PlayerText).X / 2) + 5;
            float NamePositionY = (Position.Y - (SpriteTexture.Height));
            NamePosition = new Vector2(NamePositionX, NamePositionY);

            Movement = 0f;
            isJumping = false;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteTexture, BoundingRectangle, SpriteColour);
            spriteBatch.DrawString(TextFont, PlayerText, NamePosition, Color.Black);
        }

        private void GetInput(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(MoveLeftKey))
            {
                Movement = -1.0f;
            }
            else if (keyboardState.IsKeyDown(MoveRightKey))
            {
                Movement = 1.0f;
            }

            if (keyboardState.IsKeyDown(MoveDownKey))
            {
                Gravity = 8000f;
            }
            else
            {
                Gravity = 3400f;
            }

            isJumping = keyboardState.IsKeyDown(MoveUpKey);
        }

        public void ApplyMovementPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bufferTimer += elapsed;

            Vector2 previousPosition = Position;

            Velocity.X += Movement * MoveAcceleration * elapsed;
            Velocity.Y = MathHelper.Clamp(Velocity.Y + Gravity * elapsed, -MaxFallSpeed, MaxFallSpeed);

            Velocity.Y = Jump(gameTime, Velocity.Y);

            Velocity.X = MathHelper.Clamp(Velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            if (isGrounded)
            {
                Velocity.X *= GroundFriction;
            }
            else
            {
                Velocity.X *= AirFriction;
            }

            Position += Velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            HandleCollision();

            if (Position.X == previousPosition.X)
            {
                Velocity.X = 0;
            }
            if (Position.Y == previousPosition.Y)
            {
                Velocity.Y = 0;
            }
        }

        private float Jump(GameTime gameTime, float velocityY)
        {
            
            if (isJumping)
            {
                if (!wasJumping && jumpCounter < 2 || bufferTimer < BufferTime && isGrounded || jumpTime > 0.0f)
                {
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (jumpTime > 0.0f && jumpTime <= MaxJumpTime)
                {
                    velocityY = JumpPower * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    jumpTime = 0.0f;
                }
            }
            else
            {
                jumpTime = 0.0f;
            }
            
            if (isJumping && !wasJumping)
            {
                jumpCounter ++;
                bufferTimer = 0;
            }

            wasJumping = isJumping;
            

            return velocityY;

        }

        private void HandleCollision()
        {
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling((float)bounds.Right / Tile.Width) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling((float)bounds.Bottom / Tile.Height) - 1;

            isGrounded = false;

            for (int y = topTile; y <= bottomTile; ++y) 
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {                       
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtenstions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            if (absDepthY < absDepthX)
                            {
                                if (previousBottom <= tileBounds.Top)
                                {
                                    isGrounded = true;
                                    jumpCounter = 0;
                                }

                                if (collision == TileCollision.Impassable)
                                {
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable)
                            {
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            previousBottom = bounds.Bottom;
        }
       
        private void SetLocalBounds()
        {
            int width = SpriteTexture.Width;
            int left = 0;
            int height = SpriteTexture.Height;
            int top = 0;
            LocalBounds = new Rectangle(left, top, width, height);
        }

    }
}
