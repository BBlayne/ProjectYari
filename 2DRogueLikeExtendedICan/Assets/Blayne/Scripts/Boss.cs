using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Completed
{
    public class Boss : MovingObject, IDamageable<int>, IKillable, IAttack
    {
        public int playerDamage;                            //The amount of food points to subtract from the player when attacking.
        public AudioClip attackSound1;                      //First of two audio clips to play when attacking the player.
        public AudioClip attackSound2;                      //Second of two audio clips to play when attacking the player.
        public int wallDamage = 5;
        public bool isDead = false;
        private Animator animator;                          //Variable of type Animator to store a reference to the enemy's Animator component.
        private Transform target;                           //Transform to attempt to move toward each turn.
        private bool skipMove;                              //Boolean to determine whether or not enemy should skip a turn or move this turn.
        private int hpRemaining = 2;
        public int HPRemaining
        {
            get
            {
                return hpRemaining;
            }
            set
            {
                this.hpRemaining = value;
                if (this.hpRemaining <= 0)
                    this.Kill();
            }
        }

        //MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
        public void MoveEnemy()
        {
            if (isDead) return;
            //Declare variables for X and Y axis move directions, these range from -1 to 1.
            //These values allow us to choose between the cardinal directions: up, down, left and right.
            int xDir = 0;
            int yDir = 0;

            //If the difference in positions is approximately zero (Epsilon) do the following:
            if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)

                //If the y coordinate of the target's (player) position is greater than the y coordinate of this enemy's position set y direction 1 (to move up). If not, set it to -1 (to move down).
                yDir = target.position.y > transform.position.y ? 1 : -1;

            //If the difference in positions is not approximately zero (Epsilon) do the following:
            else
                //Check if target x position is greater than enemy's x position, if so set x direction to 1 (move right), if not set to -1 (move left).
                xDir = target.position.x > transform.position.x ? 1 : -1;

            //Debug.Log("xDir: " + xDir + "," + "yDir: " + yDir);
            //Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
            AttemptMove<Player>(xDir, yDir);
            Attack(xDir, yDir);
        }

        //OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
        //and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
        protected override void OnCantMove<T>(T component)
        {

            //Declare hitPlayer and set it to equal the encountered component.
            Player hitPlayer = component as Player;

            //Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
            //hitPlayer.LoseFood (playerDamage);
            hitPlayer.TakeDamage(playerDamage);

            //Set the attack trigger of animator to trigger Enemy attack animation.
            animator.SetTrigger("enemyAttack");

            //Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
            SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
        }

        public void Attack(int xDir, int yDir)
        {

        }

        public void TakeDamage(int damageTaken)
        {

        }

	    public void Kill()
        {

        }
    }
}

