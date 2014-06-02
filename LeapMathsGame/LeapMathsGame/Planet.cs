using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeapMathsGame
{
    class Planet
    {
        private Texture2D texture;
        private Vector2 position;
        private Rectangle boundingRectangle;
        public bool Lock = false;

        public Rectangle BoundingRectangle
        {
            get { return boundingRectangle; }
        }
        private float startX;
        private Vector2 velocity = new Vector2( 0, -0.1f );

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        private int number;

        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        public Planet( Texture2D texture, Vector2 position, int number )
        {
            this.texture = texture;
            this.position = position;
            this.number = number;
            this.startX = position.X;
            this.boundingRectangle = new Rectangle( ( int ) position.X, ( int ) position.Y, this.texture.Width, this.texture.Height );
        }

        public void Update()
        {
            position += velocity;
            if ( position.Y < 70 )
            {
                velocity *= -1;
            }
            else if ( position.Y > 600 )
            {
                velocity *= -1;
            }
            boundingRectangle.X = ( int ) position.X;
            boundingRectangle.Y = ( int ) position.Y;
        }

        public void Draw( SpriteBatch spriteBatch, SpriteFont font )
        {
            if ( MathGame.drawMirrored )
            {
                spriteBatch.Draw( texture, position, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.FlipHorizontally, 0.0f );
                spriteBatch.DrawString( font, number.ToString(), new Vector2( position.X + 32, position.Y + 27 ), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.FlipHorizontally, 0 );
                spriteBatch.DrawString( font, number.ToString(), new Vector2( position.X + 30, position.Y + 25 ), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.FlipHorizontally, 0 );
            }
            else
            {
                spriteBatch.Draw( texture, position, Color.White );
                spriteBatch.DrawString( font, number.ToString(), new Vector2( position.X + 32, position.Y + 27 ), Color.Black );
                spriteBatch.DrawString( font, number.ToString(), new Vector2( position.X + 30, position.Y + 25 ), Color.White );
            }
        }

        internal void Update( GameTime gameTime )
        {
            if ( Lock ) return;
            position.X = startX + (float)Math.Cos( gameTime.TotalGameTime.TotalSeconds ) * 20 / number;
            Update();
        }
    }
}
