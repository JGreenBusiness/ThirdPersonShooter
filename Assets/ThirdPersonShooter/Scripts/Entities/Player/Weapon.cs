using JetBrains.Annotations;

using System;
using System.Collections;

using ThirdPersonShooter.AI;
using ThirdPersonShooter.Utilities;
using ThirdPersonShooter.VFX;

using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPersonShooter.Entities.Player
{
	public class Weapon : MonoBehaviour
	{
		public float MaxAmmo => maxAmmo;
		public float Ammo => ammo;
		
		public Action<int> onAmmoChanged;
		
		[SerializeField] private InputActionReference shootAction;
		[SerializeField] private InputActionReference reloadAction;
		[SerializeField] private Transform shootPoint;
		[SerializeField] private BulletLine bulletLine;

		[CanBeNull] private IEntity player;

		private bool canShoot = true;

		[SerializeField] private int maxAmmo = 5;
		private int ammo = 5;
		private float reloadRate = 1;

		private void Start()
		{
			ammo = maxAmmo;
		}

		public void SetPlayer(IEntity _player) => player = _player;

		private void Update()
		{
			if(player != null && canShoot)
			{
				if( shootAction.action.IsDown())
				{
					if(ammo > 0)
					{
						Shoot();
						ammo--;
					}
					else
					{
						Reload();
					}
					onAmmoChanged.Invoke(ammo);

					
				}
				
				if(reloadAction.action.IsDown())
				{
					Reload();

				}
			}
		}

		private void Reload()
		{
			StartCoroutine(ReloadCoolDown_CR());
		}

		private void Shoot()
		{
			bool didHit = Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit hit, player.Stats.Range);

			if(didHit && hit.collider.TryGetComponent(out EnemyEntity entity))
			{
				entity.Stats.TakeDamage(player.Stats.Damage);
			}
					
			BulletLine newLine = Instantiate(bulletLine);
			newLine.Play(shootPoint.position, didHit ? hit.point : shootPoint.position + shootPoint.forward * player.Stats.Range, didHit);
			StartCoroutine(ShootCoolDown_CR());
		}

		private IEnumerator ShootCoolDown_CR()
		{
			canShoot = false;

			if(player != null)
			{
				yield return new WaitForSeconds(player.Stats.AttackRate);
				canShoot = true;
			}
		}

		private IEnumerator ReloadCoolDown_CR()
		{
			canShoot = false;
			if(ammo != maxAmmo)
			{
				yield return new WaitForSeconds(reloadRate);
				ammo = maxAmmo;
				canShoot = true;
				onAmmoChanged.Invoke(ammo);
			}
		}

	}
}