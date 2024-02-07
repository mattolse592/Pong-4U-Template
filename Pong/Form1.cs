/*
 * Description:     A basic PONG simulator
 * Author:      Matthew Olsen     
 * Date:            
 */

#region libraries

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Media;

#endregion

namespace Pong
{
    public partial class Form1 : Form
    {
        #region global values
        Random ranGen = new Random();

        Rectangle pillarTop, pillarBottom;
        bool pillarRight = true;
        int pillarSize = 40;
        int pillarSpeed = 2;
        int pillarSpace;
        int pillarAmount;
        bool pillarCanScore = false;

        //graphics objects for drawing
        SolidBrush whiteBrush = new SolidBrush(Color.White);
        SolidBrush blueBrush = new SolidBrush(Color.Blue);
        SolidBrush redBrush = new SolidBrush(Color.Red);
        SolidBrush greenBrush = new SolidBrush(Color.Green);

        Font drawFont = new Font("Courier New", 10);

        // Sounds for game
        SoundPlayer scoreSound = new SoundPlayer(Properties.Resources.score);
        SoundPlayer collisionSound = new SoundPlayer(Properties.Resources.collision);
        SoundPlayer glitchSound = new SoundPlayer(Properties.Resources.glitch);

        //determines whether a key is being pressed or not
        Boolean wKeyDown, sKeyDown, upKeyDown, downKeyDown;

        // check to see if a new game can be started
        Boolean newGameOk = true;

        //ball values
        Boolean ballMoveRight = true;
        Boolean ballMoveDown = true;
        const int BALL_SPEED = 4;
        const int BALL_WIDTH = 20;
        const int BALL_HEIGHT = 20;
        Rectangle ball;

        //player values
        int player1Speed, player2Speed;
        const int PADDLE_EDGE = 20;  // buffer distance between screen edge and paddle            
        const int PADDLE_WIDTH = 10;
        const int PADDLE_HEIGHT = 40;
        Rectangle player1, player2;

        //player and game scores
        int player1Score = 0;
        int player2Score = 0;
        //int gameWinScore = 2;  // number of points needed to win game

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //check to see if a key is pressed and set is KeyDown value to true if it has
            switch (e.KeyCode)
            {
                case Keys.W:
                    wKeyDown = true;
                    break;
                case Keys.S:
                    sKeyDown = true;
                    break;
                case Keys.Up:
                    upKeyDown = true;
                    break;
                case Keys.Down:
                    downKeyDown = true;
                    break;
                case Keys.Y:
                case Keys.Space:
                    if (newGameOk)
                    {
                        SetParameters();
                    }
                    break;
                case Keys.Escape:
                    if (newGameOk)
                    {
                        Close();
                    }
                    break;
            }
        }

        private void pillarSpawnTimer_Tick(object sender, EventArgs e)
        {
            SpawnPillar();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //check to see if a key has been released and set its KeyDown value to false if it has
            switch (e.KeyCode)
            {
                case Keys.W:
                    wKeyDown = false;
                    break;
                case Keys.S:
                    sKeyDown = false;
                    break;
                case Keys.Up:
                    upKeyDown = false;
                    break;
                case Keys.Down:
                    downKeyDown = false;
                    break;
            }
        }

        /// <summary>
        /// sets the ball and paddle positions for game start
        /// </summary>
        private void SetParameters()
        {
            if (newGameOk)
            {
                player1Score = player2Score = 0;
                player1ScoreLabel.Text = "0";
                player2ScoreLabel.Text = "0";
                newGameOk = false;
                startLabel.Visible = false;
                Refresh();
                gameUpdateLoop.Start();
                pillarSpawnTimer.Start();
            }

            //player start positions
            player1 = new Rectangle(PADDLE_EDGE, this.Height / 2 - PADDLE_HEIGHT / 2, PADDLE_WIDTH, PADDLE_HEIGHT);
            player2 = new Rectangle(this.Width - PADDLE_EDGE - PADDLE_WIDTH, this.Height / 2 - PADDLE_HEIGHT / 2, PADDLE_WIDTH, PADDLE_HEIGHT);

            ball = new Rectangle(this.Width / 2, this.Height / 2, BALL_WIDTH, BALL_HEIGHT);

        }

        private void SpawnPillar()
        {
            pillarCanScore = true;

            pillarSpace = ranGen.Next(10, this.Height - 200);

            pillarTop = new Rectangle(this.Width / 2 - pillarSize / 2, 0, pillarSize, pillarSpace);
            pillarBottom = new Rectangle(this.Width / 2 - pillarSize / 2, pillarSpace + 200, pillarSize, this.Height - 200);

            pillarRight = !pillarRight;

            if (ballMoveRight == true)
            {
                pillarRight = false;
            }
            else
            {
                pillarRight = true;
            }

            pillarAmount++;
            if (pillarAmount == 5)
            {
                pillarAmount = 0;
                pillarSpeed++;
            }
        }
        /// <summary>
        /// This method is the game engine loop that updates the position of all elements
        /// and checks for collisions.
        /// </summary>
        private void gameUpdateLoop_Tick(object sender, EventArgs e)
        {
            #region update ball position
            if (ballMoveRight == true)
            {
                ball.X = ball.X + BALL_SPEED;
            }
            else
            {
                ball.X = ball.X - BALL_SPEED;
            }

            if (ballMoveDown == true)
            {
                ball.Y = ball.Y + BALL_SPEED;
            }
            else
            {
                ball.Y = ball.Y - BALL_SPEED;
            }

            #endregion

            #region update paddle positions
            if (wKeyDown == true && player1Speed < 10)
            {
                player1Speed = 10;
                wKeyDown = false;
            }
            if (upKeyDown == true && player2Speed < 10)
            {
                player2Speed = 10;
                upKeyDown = false;
            }

            if (player1.Y < this.Height - player1.Height && player1.Y > 0)
            {
                player1.Y = player1.Y - player1Speed;
            }
            if (player2.Y < this.Height - player2.Height && player2.Y > 0)
            {
                player2.Y = player2.Y - player2Speed;
            }

            if (player1.Y <= 0)
            {
                player1.Y = 2;
            }
            else if (player1.Y >= this.Height - player1.Height)
            {
                player1.Y = this.Height - player1.Height - 2;
            }
            if (player2.Y <= 0)
            {
                player2.Y = 2;
            }
            else if (player2.Y >= this.Height - player2.Height)
            {
                player2.Y = this.Height - player2.Height - 2;
            }


            if (player1Speed > -5)
            {
                player1Speed -= 1;
            }
            if (player2Speed > -5)
            {
                player2Speed -= 1;
            }

            #endregion

            #region ball collision with top and bottom lines

            if (ball.Y < 0) // if ball hits top line
            {
                ballMoveDown = true;
                collisionSound.Play();
            }
            else if (ball.Y + BALL_HEIGHT > this.Height)
            {
                ballMoveDown = false;
            }

            #endregion

            #region ball collision with paddles

            if (ball.IntersectsWith(player1) || ball.IntersectsWith(player2))
            {
                ballMoveRight = !ballMoveRight;
                collisionSound.Play();
            }

            #endregion

            #region ball collision with side walls (point scored)

            if (ball.X < 0)  // ball hits left wall logic
            {
                scoreSound.Play();
                player2Score++;
                player2ScoreLabel.Text = Convert.ToString(player2Score);

                ball.X = this.Width / 2;
                ball.Y = this.Height / 2;

                if (player2Score >= 5 && player2Score > player1Score)
                {
                    GameOver("Player 2");
                }
            }

            if (ball.X >= this.Width - ball.Width)  // ball hits right wall logic
            {
                scoreSound.Play();
                player1Score++;
                player1ScoreLabel.Text = Convert.ToString(player1Score);

                ball.X = this.Width / 2;
                ball.Y = this.Height / 2;

                if (player1Score >= 5 && player1Score > player2Score)
                {
                    GameOver("Player 1");
                }
            }

            #endregion

            #region pillar update
            if (pillarRight == true)
            {
                pillarTop.X = pillarTop.X + pillarSpeed;
                pillarBottom.X = pillarBottom.X + pillarSpeed;
            }
            else
            {
                pillarTop.X = pillarTop.X - pillarSpeed;
                pillarBottom.X = pillarBottom.X - pillarSpeed;
            }
            #endregion

            #region ball collision with pillar
            if (ball.IntersectsWith(pillarBottom) || ball.IntersectsWith(pillarTop))
            {
                ballMoveRight = !ballMoveRight;
            }

            collisionSound.Play();

            #endregion

            #region player collision with pillar
            if (player1.IntersectsWith(pillarTop) || player1.IntersectsWith(pillarBottom))
            {
                glitchSound.Play();
                player1.Y = this.Height / 2;

                if (pillarCanScore == true)
                {
                    player2Score++;
                    player2ScoreLabel.Text = $"{player1Score}";
                }

                pillarCanScore = false;
            }
            if (player2.IntersectsWith(pillarTop) || player2.IntersectsWith(pillarBottom))
            {
                glitchSound.Play();
                player2.Y = this.Height / 2;

                if (pillarCanScore == true)
                {
                    player1Score++;
                    player1ScoreLabel.Text = $"{player1Score}";
                }
                pillarCanScore = false;
            }
            #endregion


            //refresh the screen, which causes the Form1_Paint method to run
            this.Refresh();
        }

        /// <summary>
        /// Displays a message for the winner when the game is over and allows the user to either select
        /// to play again or end the program
        /// </summary>
        /// <param name="winner">The player name to be shown as the winner</param>
        private void GameOver(string winner)
        {
            gameUpdateLoop.Enabled = false;
            startLabel.Text = $"{winner} Wins!";
            startLabel.Visible = true;
            newGameOk = true;
            Refresh();

            startLabel.Text += " Do you want to play again? Press Space to start or ESC to exit";

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(blueBrush, player1);
            e.Graphics.FillRectangle(redBrush, player2);
            e.Graphics.FillRectangle(greenBrush, pillarTop);
            e.Graphics.FillRectangle(greenBrush, pillarBottom);

            e.Graphics.FillRectangle(whiteBrush, ball);
        }

    }
}
