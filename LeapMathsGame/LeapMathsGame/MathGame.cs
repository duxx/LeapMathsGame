using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpOSC;

namespace LeapMathsGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MathGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        /* UI Elements */
        Texture2D backgroundTexture;
        Texture2D questionBarTexture;
        SpriteFont uiFont;

        /* Game objects */
        Texture2D planetBlueTexture;
        Texture2D planetPurpleTexture;
        Texture2D planetYellowTexture;
        Texture2D ballRedTexture;
        List<Planet> planets;
        Pointer pointer;

        /* Audio */
        SoundEffect powerUpSound;
        SoundEffect backgroundMusic;
        SoundEffectInstance powerUpSoundInstance;
        SoundEffectInstance backgroundMusicInstance;
        SoundEffect correctSound;
        SoundEffect explosionSound;

        /* Game Logic */
        bool isColliding;
        bool hasWon = false;
        bool hasLost = false;
        byte lostTextAlpha = 255;
        int collideTargetNum = 0;
        public static bool drawMirrored = true;
        KeyboardState keyboardState;
        KeyboardState oldKeyboardState;

        /* Leap Stuff */
        Vector2 playerPosition = Vector2.Zero;
        UDPListener listener = null;
        static int MOUSE_X = 0;
        static int MOUSE_Y = 0;
        static int TIP_VELOCITY = 0; //Not really needed

        public MathGame()
        {
            graphics = new GraphicsDeviceManager( this );
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;
            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            HandleOscPacket callback = delegate( OscPacket packet )
            {
                var message = ( OscMessage ) packet;
                MOUSE_X = Int32.Parse( message.Arguments[0].ToString() );
                MOUSE_Y = Int32.Parse( message.Arguments[1].ToString() );
                TIP_VELOCITY = Int32.Parse( message.Arguments[2].ToString() );
            };

            listener = new UDPListener( 55555, callback );

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch( GraphicsDevice );

            /* UI Elements */
            backgroundTexture = Content.Load<Texture2D>( @"UiElements\spaceBackground" );
            questionBarTexture = Content.Load<Texture2D>( @"UiElements\questionBar" );
            uiFont = Content.Load<SpriteFont>( @"Fonts\contentFont" );

            /* Game Objects Textures */
            planetBlueTexture = Content.Load<Texture2D>( @"GameObjects\planetBlue" );
            planetPurpleTexture = Content.Load<Texture2D>( @"GameObjects\planetPurple" );
            planetYellowTexture = Content.Load<Texture2D>( @"GameObjects\planetYellow" );
            ballRedTexture = Content.Load<Texture2D>( @"GameObjects\ballRedAnimated" );

            /* Audio files */
            powerUpSound = Content.Load<SoundEffect>( @"Sounds\powerUp" );
            powerUpSoundInstance = powerUpSound.CreateInstance();
            powerUpSoundInstance.Volume = 0.3f;
            backgroundMusic = Content.Load<SoundEffect>( @"Sounds\backgroundMusic" );
            backgroundMusicInstance = backgroundMusic.CreateInstance();
            backgroundMusicInstance.IsLooped = true;
            backgroundMusicInstance.Volume = 0.4f;
            backgroundMusicInstance.Play();
            correctSound = Content.Load<SoundEffect>( @"Sounds\correct" );
            explosionSound = Content.Load<SoundEffect>( @"Sounds\explosion" );

            /* Game Objects internal */
            pointer = new Pointer( ballRedTexture, new Vector2( 0, 0 ) );
            pointer.Selection += pointer_Selection;
            planets = new List<Planet>();
            planets.Add( new Planet( planetBlueTexture, new Vector2( 110, 110 ), 4 ) );
            planets.Add( new Planet( planetPurpleTexture, new Vector2( 600, 350 ), 2 ) );
            planets.Add( new Planet( planetYellowTexture, new Vector2( 900, 500 ), 1 ) );
            oldKeyboardState = Keyboard.GetState();
            keyboardState = oldKeyboardState;
        }

        void pointer_Selection( Pointer p, EventArgs e )
        {
            //pointer.Selection -= pointer_Selection;
            powerUpSoundInstance.Stop();
            //pointer.Lock = true;
            

            if ( collideTargetNum == 2 )
            {
                correctSound.Play();
                hasWon = true;
            }
            else
            {
                explosionSound.Play();
                hasLost = true;
                Planet toDelete = null;
                foreach ( var planet in planets )
                {
                    if ( planet.Number == collideTargetNum )
                    {
                        toDelete = planet;
                    }
                }
                planets.Remove( toDelete );
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update( GameTime gameTime )
        {
            keyboardState = Keyboard.GetState();

            if ( keyboardState.IsKeyDown( Keys.Escape ) )
            {
                this.Exit();
            }

            if ( keyboardState.IsKeyDown( Keys.Space ) && oldKeyboardState.IsKeyUp( Keys.Space ) )
            {
                drawMirrored = !drawMirrored;
            }

            oldKeyboardState = keyboardState;

            /* Update position from Leap */
            playerPosition.X = MathHelper.Clamp( ( int ) ( ( MOUSE_X / 1600.0f ) * 1280.0f ),
                0, graphics.PreferredBackBufferWidth - 68 );
            if ( drawMirrored )
            {
                playerPosition.X *= -1;
                playerPosition.X += 1200;
            }
            playerPosition.Y = MathHelper.Clamp( ( ( MOUSE_Y / 900.0f ) * 800.0f ),
                0, graphics.PreferredBackBufferHeight - 68 );

            pointer.Update( gameTime, playerPosition );

            if(hasLost)
            {
                lostTextAlpha--;
                if ( lostTextAlpha <= 0 ) hasLost = false;
            }

            if ( !hasWon )
            {
                isColliding = false;
                collideTargetNum = 0;
                foreach ( var planet in planets )
                {
                    if ( pointer.DestinationRectangle.Intersects( planet.BoundingRectangle ) )
                    {
                        isColliding = true;
                        collideTargetNum = planet.Number;
                    }

                    planet.Update( gameTime );
                }

                if ( isColliding )
                {
                    pointer.IsPlaying = true;
                    if ( powerUpSoundInstance.State == SoundState.Stopped )
                    {
                        powerUpSoundInstance.Play();
                    }
                }
                else
                {
                    pointer.IsPlaying = false;
                    powerUpSoundInstance.Stop();
                }
            }

            base.Update( gameTime );
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw( GameTime gameTime )
        {
            GraphicsDevice.Clear( Color.CornflowerBlue );

            spriteBatch.Begin();

            spriteBatch.Draw( backgroundTexture, new Vector2( 0, 0 ), Color.White );
            spriteBatch.Draw( questionBarTexture, new Vector2( 0, graphics.PreferredBackBufferHeight - questionBarTexture.Height), Color.White );

            foreach ( var planet in planets )
            {
                planet.Draw( spriteBatch, uiFont );
            }
            
            pointer.Draw( spriteBatch );

            if ( !hasWon )
            {
                if ( drawMirrored )
                {
                    spriteBatch.DrawString( uiFont, "13 + ? = 15", new Vector2( 451, graphics.PreferredBackBufferHeight - 59 ), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.FlipHorizontally, 0 );
                    spriteBatch.DrawString( uiFont, "13 + ? = 15", new Vector2( 450, graphics.PreferredBackBufferHeight - 60 ), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.FlipHorizontally, 0 );
                }
                else
                {
                    spriteBatch.DrawString( uiFont, "13 + ? = 15", new Vector2( 451, graphics.PreferredBackBufferHeight - 59 ), Color.Black );
                    spriteBatch.DrawString( uiFont, "13 + ? = 15", new Vector2( 450, graphics.PreferredBackBufferHeight - 60 ), Color.White );
                }
                
            }

            if ( hasWon )
            {
                if ( drawMirrored )
                {
                    spriteBatch.DrawString( uiFont, "CORRECT!", new Vector2( 451, graphics.PreferredBackBufferHeight - 249 ), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.FlipHorizontally, 0 );
                    spriteBatch.DrawString( uiFont, "CORRECT!", new Vector2( 450, graphics.PreferredBackBufferHeight - 250 ), Color.Gold, 0, Vector2.Zero, 1.0f, SpriteEffects.FlipHorizontally, 0 );
                    spriteBatch.DrawString( uiFont, "13 + 2 = 15", new Vector2( 451, graphics.PreferredBackBufferHeight - 59 ), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.FlipHorizontally, 0 );
                    spriteBatch.DrawString( uiFont, "13 + 2 = 15", new Vector2( 450, graphics.PreferredBackBufferHeight - 60 ), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.FlipHorizontally, 0 );
                }
                else
                {
                    spriteBatch.DrawString( uiFont, "CORRECT!", new Vector2( 451, graphics.PreferredBackBufferHeight - 249 ), Color.Black );
                    spriteBatch.DrawString( uiFont, "CORRECT!", new Vector2( 450, graphics.PreferredBackBufferHeight - 250 ), Color.Gold );
                    spriteBatch.DrawString( uiFont, "13 + 2 = 15", new Vector2( 451, graphics.PreferredBackBufferHeight - 59 ), Color.Black );
                    spriteBatch.DrawString( uiFont, "13 + 2 = 15", new Vector2( 450, graphics.PreferredBackBufferHeight - 60 ), Color.White );
                }
            }

            if ( hasLost )
            {
                if ( drawMirrored )
                {
                    spriteBatch.DrawString( uiFont, "WRONG!", new Vector2( 451, graphics.PreferredBackBufferHeight - 349 + lostTextAlpha ), new Color( 0, 0, 0, lostTextAlpha ), 0, Vector2.Zero, 1.0f, SpriteEffects.FlipHorizontally, 0 );
                    spriteBatch.DrawString( uiFont, "WRONG!", new Vector2( 450, graphics.PreferredBackBufferHeight - 350 + lostTextAlpha ), new Color( 255, 0, 0, lostTextAlpha ), 0, Vector2.Zero, 1.0f, SpriteEffects.FlipHorizontally, 0 );
                }
                else
                {
                    spriteBatch.DrawString( uiFont, "WRONG!", new Vector2( 451, graphics.PreferredBackBufferHeight - 349 + lostTextAlpha ), new Color( 0, 0, 0, lostTextAlpha ) );
                    spriteBatch.DrawString( uiFont, "WRONG!", new Vector2( 450, graphics.PreferredBackBufferHeight - 350 + lostTextAlpha ), new Color( 255, 0, 0, lostTextAlpha ) );
                }
            }

            spriteBatch.End();

            base.Draw( gameTime );
        }
    }
}
