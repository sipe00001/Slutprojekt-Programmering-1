using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Slutprojekt_Programmering_1
{

    

    class Ball
    {
        Texture2D texture;
        Circle hitbox;
        Vector2 velocity;
        public Ball(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            hitbox = new Circle(position, 8);
            velocity = new Vector2(5, -5);
        }

        public Circle getHitbox() { return hitbox; }

        public void Update(Rectangle paddlehitbox, bool attached)
        {
            if (attached) {
                hitbox.Center.X = (float)Util.Clamp(paddlehitbox.Left, hitbox.Center.X, paddlehitbox.Right);
                hitbox.Center.Y = paddlehitbox.Y - hitbox.Radius;
                velocity.X = 0;
                velocity.Y = 1;
            }
            else
            {
                if (hitbox.Intersect(paddlehitbox))
                {
                    var offsetFromPaddleCenter = hitbox.Center.X - paddlehitbox.Center.X;
                    var maxRight = 0.8;
                    var maxLeft = -0.8;
                    var cos = Util.Clamp(maxLeft, (offsetFromPaddleCenter / paddlehitbox.Width) * 2, maxRight); //normaliserar [-0.5, 0.5] till [-1.0, 1.0]  
                    var sin = -Math.Sin(Math.Acos(cos));
                    var vel = new Vector2((float)cos, (float)sin)*5;
                    velocity = vel;
                }
                hitbox.Center += velocity;
                
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset, Texture2D point)
        {
            Vector2 radiusvec = new Vector2(texture.Width, texture.Height)/2;
            spriteBatch.Draw(texture, new Vector2(hitbox.Center.X, hitbox.Center.Y) - radiusvec - offset, Color.White);
            spriteBatch.Draw(point, new Vector2(hitbox.Center.X, hitbox.Center.Y) - offset, Color.White);
        }
    }
    class Block
    {
        Texture2D texture;
        Rectangle hitbox;

        public Block(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            hitbox = new Rectangle((int)position.X, (int)position.Y, 100, 100);
        }

        public Rectangle getHitbox() { return hitbox; }

        public void Update()
        {

        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            spriteBatch.Draw(texture, new Vector2(hitbox.X, hitbox.Y) - offset, Color.White);
        }
    }
	
/// <summary>
/// Denna klassen skapar en punkt som bestämmer var saker ska ritas i relation till spelarens vy. 
/// Kamerans plats uppdateras när paddeln går utanför skärmen. Som om man byter level. 
/// Detta görs med en animation (Vector2.Lerp).
/// </summary>
    class Cam
    {
        int level;
        int transit;
        float time;
        private float screenWidth;
        public Cam(float scrWidth)
        {
            screenWidth = scrWidth;
        }
        public Vector2 GetVector() {
			//Ger tillbaka offset för att rita saker på rätt plats
			//Utvärderar vilken offset vektor som alla draw-objekten ska använda.
            Vector2 transitVector = new Vector2(transit * screenWidth, 0);
            Vector2 levelVector = new Vector2(level * screenWidth, 0);

            return Vector2.Lerp(transitVector, levelVector, time);
        }
        public float GetTransitionTime()
        {
            return time;
        }
        public float GetTargetLevelLeftEdge()
        {
            return level * screenWidth;
        }
        public void Update(Paddle paddle, float dt)
        {
            float paddlePos = paddle.getHitbox().Center.X;
            int nextLevel = (int)(paddlePos / screenWidth); 
            if (level != nextLevel && time>=1.0f)
            {
                transit = level;
                time = 0.0f;
                level = nextLevel;
            }
            time += dt;
            time = Math.Min(time, 1.0f);


        }
        
    }
/// <summary>
/// Detta är paddeln. Den kan flyttas av spelaren till höger och vänster. 
/// Den håller sig på skärmen ifall man inte trycker space. 
/// Den ritas just nu ut som ett block.
/// </summary>
    class Paddle
    {
        Texture2D texture;
        Rectangle hitbox;
        public bool attached;
        public Paddle(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            hitbox = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
        }

        public Rectangle getHitbox() { return hitbox; }

        public void Update(float targetLevelLeftEdge, bool transitionDone, float screenWidth)
        {
            //Styrning
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Right))
                hitbox.X += 10;
            if (state.IsKeyDown(Keys.Left))
                hitbox.X -= 10;
            if (hitbox.X <= 0.0f)
            {
                hitbox.X = 0;
            }
            float paddlewidth = hitbox.Width;
            //Kod för att se till att man stannar på nuvarande nivå tills man vill byta (trycker space)
            bool allowTransition = transitionDone && state.IsKeyDown(Keys.Space);
            float leftEdgeBorder = targetLevelLeftEdge - paddlewidth / 2;
            if (hitbox.X <= leftEdgeBorder && !allowTransition)
            {
                hitbox.X = (int)leftEdgeBorder;
            }
            float rightEdgeBorder = targetLevelLeftEdge + screenWidth - paddlewidth / 2 - 1;
            if (hitbox.X >= rightEdgeBorder && !allowTransition)
            {
                hitbox.X = (int)(rightEdgeBorder);
            }

            attached = state.IsKeyDown(Keys.Up);
            


        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            spriteBatch.Draw(texture, new Vector2(hitbox.X, hitbox.Y) - offset, Color.White);
        }
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    /// 

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Block[,] blockarray;
        Texture2D blocktexture;
        Texture2D circletexture;
        Texture2D pointtexture;
        Paddle paddle;
        Ball ball;
        Cam cam;
        public Game1()
        {
            
            
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 562;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 800;   // set this value to the desired height of your window
            graphics.ApplyChanges();
            

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            blocktexture = Content.Load<Texture2D>("block");
            circletexture = Content.Load<Texture2D>("circle");
            pointtexture= Content.Load<Texture2D>("point");
            // TODO: use this.Content to load your game content here
            blockarray = new Block[8, 6];
            for (int y = 0; y < blockarray.GetLength(1); y++)
            {
                for (int x = 0; x < blockarray.GetLength(0); x++)
                {
                        Vector2 pos = new Vector2(7 + 69 * x, 30 + 37 * y);
                        blockarray[x,y] = new Block(blocktexture, pos);
                }
            }
            Vector2 paddlepos = new Vector2(graphics.PreferredBackBufferWidth * 0.5f, graphics.PreferredBackBufferHeight * 0.9f);
            paddle = new Paddle(blocktexture, paddlepos);
            ball = new Ball(circletexture, new Vector2(paddlepos.X+24, paddlepos.Y-16));
            cam = new Cam(graphics.PreferredBackBufferWidth);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            cam.Update(paddle, dt);
            paddle.Update(cam.GetTargetLevelLeftEdge(), cam.GetTransitionTime()>=1.0f, graphics.PreferredBackBufferWidth);
            ball.Update(paddle.getHitbox(), paddle.attached);
            base.Update(gameTime);
            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
           GraphicsDevice.Clear(Color.CornflowerBlue);
            
            spriteBatch.Begin();
            paddle.Draw(spriteBatch, cam.GetVector());
            ball.Draw(spriteBatch, cam.GetVector(), pointtexture);
            foreach (Block blk in blockarray)
            {
                blk.Draw(spriteBatch, cam.GetVector());
            }
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
