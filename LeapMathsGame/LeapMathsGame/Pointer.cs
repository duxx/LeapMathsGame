using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeapMathsGame
{
    class Pointer
    {
        public event SelectionHandler Selection;
        public EventArgs e = null;
        public delegate void SelectionHandler( Pointer p, EventArgs e );

        private Texture2D texture;
        private Rectangle drawRect;
        private Rectangle destinationRectangle;
        public Rectangle DestinationRectangle
        {
            get { return destinationRectangle; }
        }
        private Vector2 position;
        private int currentFrame = 0;
        private const int numFrames = 10;
        public bool Lock = false;
        private bool isPlaying = false;
        private double oldGameTime;
        public bool IsPlaying
        {
            get { return isPlaying; }
            set { isPlaying = value; }
        }

        public Pointer(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.position = position;
            this.drawRect = new Rectangle( (int)position.X, (int)position.Y, 68, 68 );
            this.destinationRectangle = drawRect;
            this.oldGameTime = 0;
        }

        public void Update( GameTime gameTime, Vector2 position )
        {
            if ( Lock ) return;
            this.position = position;
            this.destinationRectangle.X = ( int ) position.X;
            this.destinationRectangle.Y = ( int ) position.Y;

            if ( isPlaying )
            {
                if ( gameTime.TotalGameTime.TotalMilliseconds > oldGameTime + 200 )
                {
                    drawRect.X = currentFrame * 68;
                    if ( currentFrame < numFrames - 1 )
                    {
                        currentFrame++;
                    }
                    else
                    {
                        currentFrame = 0;
                        isPlaying = false;
                        Selection( this, e );
                    }
                    oldGameTime = gameTime.TotalGameTime.TotalMilliseconds;
                }
            }
            else
            {
                drawRect.X = 0;
                currentFrame = 0;
            }
        }

        public void Draw( SpriteBatch spriteBatch )
        {
            spriteBatch.Draw( texture, destinationRectangle, drawRect, Color.White );
            //spriteBatch.Draw( MathGame.temp, destinationRectangle, Color.Yellow );
        }
    }
}
