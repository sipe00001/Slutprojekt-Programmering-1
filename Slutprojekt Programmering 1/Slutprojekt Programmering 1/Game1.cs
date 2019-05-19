using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Slutprojekt_Programmering_1
{
    /// <summary>
    /// Konstanter för spelet
    /// </summary>
    static class Constants
    {
        public const int blockCountX = 8;
        public const int blockCountY = 6;
        
    }
    /// <summary>
    /// Hanterar input när man skriver in namnet vid highscore och uppdaterar och ber scoreboard att sortera/spara score. 
    /// </summary>
    class KeyboardInput
    {
        static private KeyboardState oldKeyState;
        static private KeyboardState keyState;
        static private String name="";
        
        static public void Update(Game1 gameState)
        {
            oldKeyState = keyState;
            keyState = Keyboard.GetState();
            foreach (Keys key in keyState.GetPressedKeys())
            {
                if (oldKeyState.IsKeyUp(key))
                {

                    if (key == Keys.Back)
                    {
                        if (name.Length > 0) { 
                            name = name.Remove(name.Length - 1, 1);
                        }
                    }
                    else if (key == Keys.Enter)
                    {
                        System.Diagnostics.Trace.WriteLine(name); 
                        Scoreboard.scorelistNames.Add(name);
                        Scoreboard.scorelistScores.Add(Scoreboard.score);
                        Scoreboard.Sort();
                        Scoreboard.Clean();
                        Scoreboard.SaveScore();
                        gameState.Reset();
                        name = "";
                    }
                    else if (name.Length>11){
                        continue;
                    }
                    else if (!(64 < (int)key && (int)key < 91) ||
                       !(60 < (int)key && (int)key < 123)
                       )
                    {
                        continue;
                    }
                    else
                    {
                        name += key.ToString();
                    }
                }
            }


        }
        static public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Scoreboard.font, "Name:", new Vector2(163, 600), Color.Black);
            spriteBatch.DrawString(Scoreboard.font, name, new Vector2(223, 600), Color.Black);
        }
    }
    /// <summary>
    /// Hanterar hi-score och ritar ut hi-score på skärmen.
    /// </summary>
    class Scoreboard
    {
        static public int score;
        static public SpriteFont font;
        static public Texture2D background;
        static public List<int> scorelistScores = new List<int>();
        static public List<string> scorelistNames = new List<string>();
        public static void Update()
        {
            

        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, new Vector2(50, 130), Color.White);
            for (var i = 0; i < scorelistScores.Count; i++) 
            {
                spriteBatch.DrawString(font, scorelistNames[i],             new Vector2(75, 150 + i * 18), Color.Black);
                spriteBatch.DrawString(font, scorelistScores[i].ToString(), new Vector2(250, 150 + i * 18), Color.Black);
            }
        }
        public static void LoadScore()
        {
            string text = System.IO.File.ReadAllText(@".\score.txt");
            foreach(var line in text.Split('\n'))
            {
                if (line == "")
                {
                    continue;
                }
                var splitLine=line.Split(':');
                scorelistNames.Add(splitLine[0]);
                scorelistScores.Add(Convert.ToInt32(splitLine[1]));
                Scoreboard.Sort();
            }
        }
        public static void Sort()
        {

            Sorting.QuickSort(scorelistScores, scorelistNames, scorelistScores.Count);
            scorelistScores.Reverse();
            scorelistNames.Reverse();
        }
        public static void Clean()
        {
            while (scorelistNames.Count > 23)
            {
                scorelistNames.RemoveAt(scorelistNames.Count - 1);
                scorelistScores.RemoveAt(scorelistScores.Count - 1);
            }
        }
        public static void SaveScore()
        {
            string text = "";
            for(var i = 0; i < scorelistScores.Count; i++)
            {
                text += scorelistNames[i] + ':' + scorelistScores[i] + '\n';
            }
            System.IO.File.WriteAllText(@".\score.txt", text);
        }


    }
    /// <summary>
    /// Bollen i spelet.
    /// </summary>
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
            // Om bollen sitter fast på paddeln.
            if (attached) {
                hitbox.Center.X = (float)Util.Clamp(paddlehitbox.Left, hitbox.Center.X, paddlehitbox.Right);
                hitbox.Center.Y = paddlehitbox.Y - hitbox.Radius;
                velocity.X = 0;
                velocity.Y = 1;
            }
            // Vanlig bollfysik
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
                    Paddle.attached = true;
                    Paddle.life -= 1;
                    
                }
                //Hantera Kollision med blocken.
                bool breakflag = false;
                var currentLevelBlocks = gameState.blocklist[gameState.currentLevel()];
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
                            currentLevelBlocks[x, y].Hit(); //replace with gameState.blocklist[x, y].hit();
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
    /// <summary>
    /// Alla blocken i spelet. 
    /// Det enda som gör skillnad på blocken är dess typ vilket avgör hur de reagerar på att bli träffade av bollen och vilken färg de har. 
    /// </summary>
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
            //Kanske skapa konstanter så vi kan ha namn iställlet för bara siffror.
            if (type == 1) { //vanliga block
                type = 0;
                Scoreboard.score -= 100;
            }
            else if (type == 2) //Blå/poäng givande block
            {
                type = 0;
                Scoreboard.score += 800;
            }
            else if (type == 3) //Röda/farliga block
            {
                type = 0;
                Paddle.life--;
            }
            else if (type == 4) //Gröna/medic block
            {
                type = 0;
                Paddle.life++;
            }
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            if(type == 1) //vanliga block
            { 
                spriteBatch.Draw(texture, new Vector2(hitbox.X, hitbox.Y) - offset, Color.White);
            }
            else if (type == 2) //poäng givande block
            {
                spriteBatch.Draw(texture, new Vector2(hitbox.X, hitbox.Y) - offset, Color.Blue);
            }
            else if (type == 3) //farliga block
            {
                spriteBatch.Draw(texture, new Vector2(hitbox.X, hitbox.Y) - offset, Color.Red);
            }
            else if (type == 4)//medic block
            {
                spriteBatch.Draw(texture, new Vector2(hitbox.X, hitbox.Y) - offset, Color.Green);
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
        /// <summary>
        /// Returnerar en lista med level index som ska ritas.
        /// </summary>
        public int[] GetView()
        {
            int[] views= new int[2];
            views[0] = level;
            if (level != transit) {
                views[1] = transit;
            }
            else
            {
                views[1] = -1; //Markerar att blocken ska ignoreras av draw() funktionen
            }
            return views;
        }


        ///<summary>Utvärderar vilken offset vektor som alla draw-objekten ska använda.</summary>
        ///<returns>Ger tillbaka en vektor som säger hut mycket man ska förskuta alla objekten i spelvärlden.</returns>
        public Vector2 GetVector() {
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
                Paddle.attached = true;
                //När man åker över till en annan nivå så förlorar man 200 poäng. Detta är gjort för att man inte ska fuska.
                Scoreboard.score -= 200;
            }
            time += dt;
            time = Math.Min(time, 1.0f);

        }
        
    }
    /// <summary>
    /// Detta är paddeln. Den kan flyttas av spelaren till höger och vänster. 
    /// Den håller sig på skärmen ifall man inte trycker space. 
    /// </summary>
    class Paddle
    {
        Texture2D texture;
        Rectangle hitbox;
        private bool releaseKeyWasDown;
        public static bool attached;
        public static int life;
        public Paddle(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            hitbox = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            Paddle.attached = true;
            life = 3;
        }

        public Rectangle getHitbox() { return hitbox; }

        public void Update(float targetLevelLeftEdge, bool transitionDone)
        {
            //Styrning
            KeyboardState state = Keyboard.GetState();
            var speedmultiplier = 1;
            if (state.IsKeyDown(Keys.LeftShift))
                speedmultiplier = 2;
            if (state.IsKeyDown(Keys.Right))
                hitbox.X += 5 * speedmultiplier;
            if (state.IsKeyDown(Keys.Left))
                hitbox.X -= 5 * speedmultiplier;
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
            spriteBatch.DrawString(Scoreboard.font, "Score: " + Scoreboard.score.ToString(), new Vector2(10, 3), Color.White);
            spriteBatch.DrawString(Scoreboard.font, "Lives: "+life.ToString(), new Vector2(Game1.screenWidth-90, 3), Color.White);
            
        }
    }
    
    public class Game1 : Game
    {
        public static int screenWidth;
        public static int screenHeight;
        public static bool gameover = false;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public List<Block[, ]>  blocklist;
        Texture2D blocktexture;
        Texture2D circletexture;
        Texture2D pointtexture;
        Texture2D paddletexture;
        Texture2D backgroundtexture;
        Paddle paddle;
        Ball ball;
        Cam cam;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 562;  //Fönstrets bredd
            graphics.PreferredBackBufferHeight = 800;   //Fönstrets höjd
            graphics.ApplyChanges();
            screenWidth = graphics.PreferredBackBufferWidth;
            screenHeight = graphics.PreferredBackBufferHeight;
        }
        
        protected override void Initialize()
        {
            base.Initialize();
        }
        public void Reset()
        {
            blocklist = new List<Block[,]>();
            //create the first level
            generateNextLevel();
            generateNextLevel();
            Vector2 paddlepos = new Vector2(graphics.PreferredBackBufferWidth * 0.5f, graphics.PreferredBackBufferHeight * 0.9f);
            paddle = new Paddle(paddletexture, paddlepos);
            ball = new Ball(circletexture, new Vector2(paddlepos.X + 24, paddlepos.Y - 16));
            cam = new Cam();
            Scoreboard.score = 0;
        }
        /// <summary>
        /// Genererar blocken för en del av världen (en skärm) 
        /// </summary>
        public void blockRandom(Block[,] array, int index)
        {
            Random rng = new Random(index + DateTime.Now.Millisecond); //Randomiserar seed för rng så att blocken inte blir samma varenda spel
            for (int y = 0; y < Constants.blockCountY; y++)
            {
                for (int x = 0; x < Constants.blockCountX; x++)
                {
                    int type = 0;
                    int randnum=rng.Next(101); 
                    if (randnum < 10)
                    {
                        type = 2;
                    }
                    else if (randnum < 15)
                    {
                        type = 3;
                    }
                    else if (randnum < 16)
                    {
                        type = 4;
                    }
                    else if (randnum < 70)
                    {
                        type = 1;
                    }
                    int levelOffset = index * screenWidth;
                    int paddingX = 7;
                    int paddingY = 30;
                    int spacingX = 69;
                    int spacingY = 37;
                    int xCoordinate = levelOffset + paddingX + spacingX * x;
                    int yCoordinate = paddingY + spacingY * y;
                    Vector2 pos = new Vector2(xCoordinate, yCoordinate);
                    blocklist[index][x, y] = new Block(blocktexture, pos, type);
                }
            }
        }
        /// <summary>
        /// Genererar en level till i spelvärlden.
        /// </summary>
        public void generateNextLevel()
        {
            int index = blocklist.Count; //är en större än sista index i array
            blocklist.Add(new Block[Constants.blockCountX, Constants.blockCountY]);
            blockRandom(blocklist[index], index);
            //man förlorar poäng när nya banor skapas
            Scoreboard.score -= 800;
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
            Scoreboard.LoadScore();
            Scoreboard.font = Content.Load<SpriteFont>("score");
            Scoreboard.background = Content.Load<Texture2D>("scoreboardbackground");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            blocktexture = Content.Load<Texture2D>("block");
            backgroundtexture = Content.Load<Texture2D>("background");
            paddletexture = Content.Load<Texture2D>("paddle");
            circletexture = Content.Load<Texture2D>("circle");
            pointtexture= Content.Load<Texture2D>("point");
            blocklist = new List<Block[,]>();
            //Skapar två nivåer från början för att simplifiera logiken.
            generateNextLevel();
            generateNextLevel();
            Vector2 paddlepos = new Vector2(graphics.PreferredBackBufferWidth * 0.5f, graphics.PreferredBackBufferHeight * 0.9f);
            paddle = new Paddle(paddletexture, paddlepos);
            ball = new Ball(circletexture, new Vector2(paddlepos.X+24, paddlepos.Y-16));
            cam = new Cam();
            Scoreboard.score = 0;
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
            
            //Genererar levelar allteftersom man rör sig:
            //Om man inte har genererat nästa nivå ännu så genereras den.
            if (cam.GetLevel() + 1 >= blocklist.Count)
            {
                generateNextLevel();
            }
            gameover = Paddle.life <= 0;

            if (gameover)
            {
                KeyboardInput.Update(this);
            }
            else
            {
                cam.Update(paddle, dt);
                paddle.Update(cam.GetTargetLevelLeftEdge(), cam.GetTransitionTime() >= 1.0f);
                ball.Update(this, paddle.getHitbox(), Paddle.attached, cam.GetLevel());
                base.Update(gameTime);
            }
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
            spriteBatch.Draw(backgroundtexture, new Vector2(-640,0), Color.White);
            paddle.Draw(spriteBatch, cam.GetVector());
            ball.Draw(spriteBatch, cam.GetVector(), pointtexture);

            //Ritar alla potentiellt synliga block.
            int[] view = cam.GetView();
            foreach(int level in view) { 
                if (level != -1){ 
                    foreach (Block block in blocklist[level])
                    {
                        block.Draw(spriteBatch, cam.GetVector());
                    }
                }
            }
            if (gameover) {
                Scoreboard.Draw(spriteBatch);
                KeyboardInput.Draw(spriteBatch);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
