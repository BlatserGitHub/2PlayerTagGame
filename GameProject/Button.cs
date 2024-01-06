using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoPlayerTagGame
{
    public class Button
    {
        #region Fields

        private Texture2D Texture;
        private SpriteFont Font;
        private MouseState CurrentMouseState;
        private MouseState PreviousMouseState;
        private bool isHovering;

        #endregion

        #region Properties

        public Vector2 ButtonPosition { get; set; }
        public Color ButtonColor { get; set; }
        public Color TextColor { get; set; }
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)ButtonPosition.X, (int)ButtonPosition.Y, Texture.Width, Texture.Height);
            }
        }
        public string Text { get; set; }
        public bool IsClicked { get; private set; }

        #endregion  

        public event EventHandler Click;

        public Button(Texture2D texture, SpriteFont spriteFont)
        {
            Texture = texture;
            Font = spriteFont;

            TextColor = Color.Black;
        }

        public void Update(GameTime gameTime) 
        {
            CurrentMouseState = Mouse.GetState();

            var MouseRectangle = new Rectangle(CurrentMouseState.X, CurrentMouseState.Y, 1, 1);

            isHovering = false;

            if(MouseRectangle.Intersects(Rectangle))
            {
                isHovering = true;

                if(CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed) 
                {
                    Click?.Invoke(this, EventArgs.Empty);
                }
            }

            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch) 
        {
            ButtonColor = Color.White;

            if(isHovering)
            {
                ButtonColor = Color.Gray;
            }

            spriteBatch.Draw(Texture, Rectangle, ButtonColor);

            if(!string.IsNullOrEmpty(Text)) 
            {
                var TextPositionX = (Rectangle.X + (Rectangle.Width / 2)) - (Font.MeasureString(Text).X  /2);
                var TextPositionY = (Rectangle.Y + (Rectangle.Height / 2)) - (Font.MeasureString(Text).Y / 2);

                spriteBatch.DrawString(Font, Text, new Vector2(TextPositionX, TextPositionY), TextColor);
            }

        }

        public void SetPosition(int X, int Y)
        {
            ButtonPosition = new Vector2( X - (Texture.Width / 2), Y - (Texture.Height / 2) );
        }

    }
}
