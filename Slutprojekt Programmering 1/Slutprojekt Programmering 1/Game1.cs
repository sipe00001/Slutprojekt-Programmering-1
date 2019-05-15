using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Slutprojekt_Programmering_1
{
    //byt namn på dessa.
    static class Constants
    {
        public const int blockCountX = 8;
        public const int blockCountY = 6;
    }


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
                //Hantera Kollision med blocken.
                bool breakflag = false;
                var currentLevelBlocks = gameState.blockarray[gameState.currentLevel()];
                for (int y = 0; y < Constants.blockCountY && !breakflag; y++)
                {
                    for (int x = 0; x < Constants.blockCountX && !breakflag; x++)
                    {
                        var blockhitbox = currentLevelBlocks[x, y].getHitbox();
                        var blocktype = currentLevelBlocks[x, y].type;
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
                            currentLevelBlocks[x, y].Hit(); //replace with gameState.blockarray[x, y].hit();
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
        public void Hit()
        {
            //Make an ENUM for all of the various types of block.
            if (type == 1) {
                type = 0;
                Game1.score += 100;
            } else if (type == 2)
            {
                //define type 2
            }
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
        //returns a list of level indicies to draw.
        public int[] GetView()
        {
            int[] views= new int[2];
            views[0] = level;
            if (level != transit) {
                views[1] = transit;
            }
            else
            {
                views[1] = -1; //ignoreras av draw() funktionen
            }
            return views;
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
        public static int score;
        public static int screenWidth;
        public static int screenHeight;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public List<Block[, ]>  blockarray;
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

        public void blockFill(Block[,] array, int index)
        {
            for (int y = 0; y < Constants.blockCountY; y++)
            {
                for (int x = 0; x < Constants.blockCountX; x++)
                {
                    int type = 1;
                    int levelOffset = index * screenWidth;
                    int paddingX = 7;
                    int paddingY = 30;
                    int spacingX = 69;
                    int spacingY = 37;
                    int xCoordinate = levelOffset + paddingX + spacingX * x; 
                    int yCoordinate = paddingY + spacingY * y;
                    Vector2 pos = new Vector2(xCoordinate, yCoordinate);
                    blockarray[index][x, y] = new Block(blocktexture, pos, type);
                }
            }
        }
        public void blockRandom(Block[,] array, int index)
        {
            Random rng = new Random(index); //seed with index. This creates deterministic generation. Have an actual random seed for a run.
            for (int y = 0; y < Constants.blockCountY; y++)
            {
                for (int x = 0; x < Constants.blockCountX; x++)
                {

                    int type = rng.Next(2); //max 1
                    int levelOffset = index * screenWidth;
                    int paddingX = 7;
                    int paddingY = 30;
                    int spacingX = 69;
                    int spacingY = 37;
                    int xCoordinate = levelOffset + paddingX + spacingX * x;
                    int yCoordinate = paddingY + spacingY * y;
                    Vector2 pos = new Vector2(xCoordinate, yCoordinate);
                    blockarray[index][x, y] = new Block(blocktexture, pos, type);
                }
            }
        }
        public void generateNextLevel()
        {
            int index = blockarray.Count; //0 first time
            blockarray.Add(new Block[Constants.blockCountX, Constants.blockCountY]);
            blockRandom(blockarray[index], index);
        }
        public int currentLevel()
        {
            return cam.GetLevel();
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
            blockarray = new List<Block[,]>();
            //create the first level
            generateNextLevel();
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

            //Generate levels as we go:
            //If the next level is not generated yet we generate it
            if (cam.GetLevel() + 1 >= blockarray.Count)
            {
                generateNextLevel();
            }

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
            // TODO: Add your drawing code here
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            spriteBatch.Begin();
            paddle.Draw(spriteBatch, cam.GetVector());
            ball.Draw(spriteBatch, cam.GetVector(), pointtexture);

            //Draw all potentially visible blocks (at level granularity).
            int[] view = cam.GetView();
            foreach(int level in view) { 
                if (level != -1){ 
                    foreach (Block blk in blockarray[level])
                    {
                        blk.Draw(spriteBatch, cam.GetVector());
                    }
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
