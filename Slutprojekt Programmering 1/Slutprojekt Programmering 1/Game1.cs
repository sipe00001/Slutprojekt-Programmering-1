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

        public void Update(Game1 gameState, Rectangle paddlehitbox, bool attached, int currentlevel)
        {
            //Om bollen sitter fast på paddeln.
            if (attached) {
                hitbox.Center.X = (float)Util.Clamp(paddlehitbox.Left, hitbox.Center.X, paddlehitbox.Right);
                hitbox.Center.Y = paddlehitbox.Y - hitbox.Radius;
                velocity.X = 0;
                velocity.Y = 1;
            }
            /*** Vanlig bollfysik***/
            else
            {              
                //Hantera kollision med paddeln
                hitbox.Center += velocity;
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
                //Hantera Kollision med skärmkantena.
                var screenLeftEdge= currentlevel * Game1.screenWidth;
                var screenRightEdge = (currentlevel + 1) * Game1.screenWidth;
                if(hitbox.Center.X-hitbox.Radius <= screenLeftEdge)
                {
                    var distancePastLeftEdge = screenLeftEdge - (hitbox.Center.X - hitbox.Radius);
                    hitbox.Center.X += distancePastLeftEdge;
                    velocity.X = -velocity.X;
                }
                if (hitbox.Center.X + hitbox.Radius >= screenRightEdge)
                {
                    var distancePastRightEdge = screenRightEdge - (hitbox.Center.X + hitbox.Radius);
                    hitbox.Center.X += distancePastRightEdge;
                    velocity.X = -velocity.X;
                }
                //Top koordinat är 0
                if (hitbox.Center.Y - hitbox.Radius <= 0)
                {
                    hitbox.Center.Y = 0 + hitbox.Radius;
                    velocity.Y = -velocity.Y;
                }
                if (hitbox.Center.Y - hitbox.Radius >= Game1.screenHeight)
                {
                    hitbox.Center.Y = Game1.screenHeight - hitbox.Radius;
                    velocity.Y = -velocity.Y;
                }
                //Hantera kollision med blocken.
                bool breakflag = false;
                for (int y = 0; y < gameState.blockarray.GetLength(1) && !breakflag; y++)
                {
                    for (int x = 0; x < gameState.blockarray.GetLength(0) && !breakflag; x++)
                    {
                        var blockhitbox = gameState.blockarray[x, y].getHitbox();
                        var blocktype = gameState.blockarray[x, y].type;
                        if (blocktype != 0 && this.hitbox.Intersect(blockhitbox))
                        {
                            //Kollision
                            var closePoint = Circle.ClosestPoint(this.hitbox.Center, blockhitbox);
                            if (blockhitbox.Top == closePoint.Y || blockhitbox.Bottom == closePoint.Y)
                            {
                                this.velocity.Y = -this.velocity.Y;
                            }
                            else if (blockhitbox.Left == closePoint.X || blockhitbox.Right == closePoint.X)
                            {
                                this.velocity.X = -this.velocity.X;
                            }
                            gameState.blockarray[x, y].type = 0; //replace with gameState.blockarray[x, y].hit();
                            breakflag = true;
                        }
                    }
                }
            }

        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset, Texture2D point)
        {
            Vector2 radiusvec = new Vector2(texture.Width, texture.Height)/2;
            spriteBatch.Draw(texture, new Vector2(hitbox.Center.X, hitbox.Center.Y) - radiusvec - offset, Color.White);
            spriteBatch.Draw(point, new Vector2(hitbox.Center.X, hitbox.Center.Y) - offset, Color.White);
        }
    }
    public class Block
    {
        Texture2D texture;
        Rectangle hitbox;
        public int type;
        public Block(Texture2D texture, Vector2 position, int type)
        {
            this.texture = texture;
            hitbox = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            this.type = type;
        }

        public Rectangle getHitbox() { return hitbox; }

        public void Update()
        {

        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            if(type != 0) { 
                spriteBatch.Draw(texture, new Vector2(hitbox.X, hitbox.Y) - offset, Color.White);
            }
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
        public Cam()
        {
        }
        public int GetLevel()
        {
            return level;
        }
        public Vector2 GetVector() {
			//Ger tillbaka offset för att rita saker på rätt plats
			//Utvärderar vilken offset vektor som alla draw-objekten ska använda.
            Vector2 transitVector = new Vector2(transit * Game1.screenWidth, 0);
            Vector2 levelVector = new Vector2(level * Game1.screenWidth, 0);

            return Vector2.Lerp(transitVector, levelVector, time);
        }
        public float GetTransitionTime()
        {
            return time;
        }
        public float GetTargetLevelLeftEdge()
        {
            return level * Game1.screenWidth;
        }
        public void Update(Paddle paddle, float dt)
        {
            float paddlePos = paddle.getHitbox().Center.X;
            int nextLevel = (int)(paddlePos / Game1.screenWidth); 
            if (level != nextLevel && time>=1.0f)
            {
                transit = level;
                time = 0.0f;
                level = nextLevel;
                paddle.attached = true;
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
        private bool releaseKeyWasDown;
        public bool attached;
        public Paddle(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            hitbox = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
        }

        public Rectangle getHitbox() { return hitbox; }

        public void Update(float targetLevelLeftEdge, bool transitionDone)
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
            float rightEdgeBorder = targetLevelLeftEdge + Game1.screenWidth - paddlewidth / 2 - 1;
            if (hitbox.X >= rightEdgeBorder && !allowTransition)
            {
                hitbox.X = (int)(rightEdgeBorder);
            }

            if (releaseKeyWasDown == true) {
                if (state.IsKeyDown(Keys.Up) == false)
                {
                    attached = false;
                }
            }
            releaseKeyWasDown = state.IsKeyDown(Keys.Up);

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
        public static int screenWidth;
        public static int screenHeight;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public Block[,] blockarray;
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
            screenWidth = graphics.PreferredBackBufferWidth;
            screenHeight = graphics.PreferredBackBufferHeight;

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
                    int type = 1;
                    Vector2 pos = new Vector2(7 + 69 * x, 30 + 37 * y);
                    blockarray[x,y] = new Block(blocktexture, pos, type);
                }
            }
            Vector2 paddlepos = new Vector2(graphics.PreferredBackBufferWidth * 0.5f, graphics.PreferredBackBufferHeight * 0.9f);
            paddle = new Paddle(blocktexture, paddlepos);
            ball = new Ball(circletexture, new Vector2(paddlepos.X+24, paddlepos.Y-16));
            cam = new Cam();
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
            paddle.Update(cam.GetTargetLevelLeftEdge(), cam.GetTransitionTime()>=1.0f);
            ball.Update(this, paddle.getHitbox(), paddle.attached, cam.GetLevel());
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
