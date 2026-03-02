using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Enemy.Shady
{
    public class ShadyBattleState : EnemyState
    {
        private Transform player;
        private Enemy_Shady enemy;
        private int moveDir;

        private float defaultSpeed;

        public ShadyBattleState(global::Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Shady _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
        {
            this.enemy = _enemy;
        }

        public override void Enter()
        {
            base.Enter();

            defaultSpeed = enemy.moveSpeed;
            enemy.moveSpeed = enemy.battleStateMoveSpeed;

            // 检查玩家是否存在
            if (PlayerManager.instance != null && PlayerManager.instance.player != null)
            {
                player = PlayerManager.instance.player.transform;
            }
            else
            {
                player = null;
            }

            if (player.GetComponent<PlayerStats>().isDead)
                stateMachine.ChangeState(enemy.moveState);
        }

        public override void Update()
        {
            base.Update();

            // 如果玩家不存在，切换到空闲状态
            if (player == null)
            {
                stateMachine.ChangeState(enemy.idleState);
                return;
            }

            if (enemy.IsPlayerDetected())
            {
                stateTimer = enemy.battleTime;

                if (enemy.IsPlayerDetected().distance < enemy.attackDistance)
                    enemy.stats.killEntity();//this enters dead state which triggers explosion + drop item and souls
            }
            else
            {
                if (stateTimer < 0 || Vector2.Distance(player.position, enemy.transform.position) > 7)
                    stateMachine.ChangeState(enemy.idleState);
            }






            if (player.position.x > enemy.transform.position.x)
                moveDir = 1;
            else if (player.position.x < enemy.transform.position.x)
                moveDir = -1;

            if (enemy.IsPlayerDetected() && enemy.IsPlayerDetected().distance < enemy.attackDistance - .5f)
            {
                enemyBase.anim.SetBool("Move", false);
                enemy.SetZeroVelocity();
            }
            else
            {
                enemy.SetVelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
            }
        }

        public override void Exit()
        {
            base.Exit();

            enemy.moveSpeed = defaultSpeed;
        }

        private bool CanAttack()
        {
            if (Time.time >= enemy.lastTimeAttacked + enemy.attackCooldown)
            {
                enemy.attackCooldown = Random.Range(enemy.minAttackCooldown, enemy.maxAttackCooldown);
                enemy.lastTimeAttacked = Time.time;
                return true;
            }

            return false;
        }
    }
}