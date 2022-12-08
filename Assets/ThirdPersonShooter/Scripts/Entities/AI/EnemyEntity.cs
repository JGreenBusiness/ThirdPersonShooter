﻿using System;
using System.Collections;

using ThirdPersonShooter.Entities;
using ThirdPersonShooter.Entities.Player;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;

namespace ThirdPersonShooter.AI
{
	[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
	public class EnemyEntity : MonoBehaviour, IEntity
	{
		private static readonly int deadHash = Animator.StringToHash("Dead");
		private static readonly int playerDeadHash = Animator.StringToHash("PlayerDead");

		public ref Stats Stats => ref stats;
		public Vector3 Position => transform.position;

		[SerializeField] private Stats stats;
		[SerializeField] private int value = 1;
		

		[Header("Components")] 
		[SerializeField] private Animator animator;
		[SerializeField] private AudioSource hurtSource;
		[SerializeField] private AudioSource deathSource;

		[Header("Debug")]
		[SerializeField] private bool skipPathFinding;

		private PlayerEntity player;
		private NavMeshAgent agent;

		private bool isAttacking;
		private bool isPlayerDead;

		private new Collider collider;

		private void Start()
		{
			stats.Start();
			stats.onDeath += OnDied;
			stats.onHealthChanged += OnDamaged;

			agent = gameObject.GetComponent<NavMeshAgent>();
			agent.speed = stats.Speed;

			collider = gameObject.GetComponent<CapsuleCollider>();

			player = GameManager.IsValid() ? GameManager.Instance.Player : FindObjectOfType<PlayerEntity>();
			player.Stats.onDeath += OnPlayerDied;

		}

		private void OnDestroy()
		{
			stats.onDeath -= OnDied;
			stats.onHealthChanged -= OnDamaged;
			player.Stats.onDeath -= OnPlayerDied;

		}

		private void Update()
		{
			if(isPlayerDead || skipPathFinding)
			{
				return;
			}

			agent.SetDestination(player.Position);

			if(Vector3.Distance(player.Position, transform.position) < stats.Range)
			{
				if(!isAttacking)
				{
					player.Stats.TakeDamage(stats.Damage);
					StartCoroutine(AttackCoolDown_CR());
				}
			}
			
		}

		private IEnumerator AttackCoolDown_CR()
		{
			isAttacking = true;
			yield return new WaitForSeconds(stats.AttackRate);
			isAttacking = false;
		}

		private void OnDamaged(float _health) => hurtSource.Play();

		private void OnDied()
		{
			animator.SetTrigger(deadHash);
			collider.enabled = false;
			deathSource.Play();
			player.AddScore(value);
		}

		private void OnPlayerDied()
		{
			animator.SetTrigger(playerDeadHash);
			isPlayerDead = true;
			agent.ResetPath();
		}
	}
}