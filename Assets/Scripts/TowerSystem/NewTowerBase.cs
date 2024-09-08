using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.Game;
using Helper;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace.TowerSystem
{
    [SelectionBase]
    public abstract class NewTowerBase : MonoBehaviour
    {
        public TowerScriptableObject Config { get; private set; }

        protected SphereCollider _myCollider;
        [SerializeField] protected GameObject RotateAxis;
        [SerializeField] protected Transform BulletSource;
        protected AudioSource mAudioSource;
        
        protected List<Enemy> TargetList = new List<Enemy>();
        protected Enemy TargetedEnemy;
        protected Enemy LastTargeted;

        protected Coroutine FireRoutine;
        protected bool IsShooting = true;
        protected float BurstTimer = 0;
        protected float AudioTimer = 0;

        protected bool isDisabled = false;
        private float fireTimer = 0;

        public bool IsDisabled
        {
            get => isDisabled;
            set => isDisabled = value;
        }

        private ParticleSystem _particleSystem;

        public virtual void Initiliaze(TowerScriptableObject Config)
        {
            this.Config = Config;
            
            _myCollider = GetComponent<SphereCollider>();
            _myCollider.radius = Config.attackRadius;
            mAudioSource = ComponentCopier.CopyComponent(SoundFXPlayer.Instance.Source, BulletSource.gameObject);
            
            BurstTimer = Time.time;
            _particleSystem = Instantiate(Config.particulOnShoot, BulletSource);
            FireRoutine = StartCoroutine(FireLoopCo());
        }

        private void Update()
        {
            UpdateShootTimer();
        }

        protected virtual void OnFire()
        {
            if (!IsShooting) return;
            
            var spawnPos = BulletSource.position + new Vector3(0, 1, 0);
            var bullet = Instantiate(Config.BulletConfig.prefab, spawnPos, transform.rotation);
            bullet.GetComponent<NewBulletBehaviour>().Initialize(Config.debuffs, TargetedEnemy, Config.BulletConfig);
            
            _particleSystem.Play();
            PlaySoundFX();
        }

        protected bool UpdateShootTimer()
        {
            if (Config.burstDelay == 0)
            {
                IsShooting = true;
                return true;
            }
            
            if (BurstTimer + Config.burstDelay < GameSpeed.GameDeltaTime)
            {
                BurstTimer = GameSpeed.GameDeltaTime;
                IsShooting = !IsShooting;
            }

            return IsShooting;
        }


        private void PlaySoundFX()
        {
            if (AudioTimer + Config.audioCoolDown < GameSpeed.GameDeltaTime)
            {
                var randomClip = Config.firingSfx[Random.Range(0, Config.firingSfx.Count)];
                // SoundFXPlayer.PlaySFX(mAudioSource, randomClip);
                mAudioSource.PlayOneShot(randomClip, AudioManager.SFXVolume);

                AudioTimer = GameSpeed.GameDeltaTime;
            }
        }
        
        protected virtual IEnumerator FireLoopCo()
        {
            while (Application.isPlaying)
            {
                yield return new WaitUntil(() => !isDisabled);
                
                SetNewTarget();

                if (TargetedEnemy != null)
                {
                    RotateToTarget();
                    OnFire();
                }
            
                yield return new WaitUntil(() => GameSpeed.GameTimerCheck(ref fireTimer, Config.fireRate));
            }
        }

        protected void RotateToTarget()
        {
            var direction = TargetedEnemy.mTransform.position - transform.position;
            direction.y = 0;
            var rotation = Quaternion.LookRotation(direction);
            RotateAxis.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y + 90, 0);
        }

        private void SetNewTarget()
        {
            if (TargetList.Count == 0)
            {
                TargetedEnemy = null;
                return;
            }

            switch (Config.myTargetingType)
            {
                case TargetType.RandomSelect:
                    TargetedEnemy = TargetList[Random.Range(0, TargetList.Count)];
                    break;
                case TargetType.FocusOnFirst:
                    TargetedEnemy = TargetList[0];
                    break;
                case TargetType.FocusOnMiddle:
                    TargetedEnemy = TargetList[(int)(TargetList.Count / 2)];
                    break;
                case TargetType.FocusOnLast:
                    TargetedEnemy = TargetList[^1];
                    break;
            }

            if (TargetedEnemy.IsDead)
            {
                TargetList.Remove(TargetedEnemy);
                SetNewTarget();
            }
        }

        private void OnDestroy() => StopCoroutine(FireRoutine);

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Enemy enemey))
                TargetList.Add(enemey);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if(other.TryGetComponent(out Enemy enemey))
                TargetList.Remove(enemey);
        }

        
        private void OnDrawGizmosSelected()
        {
            if (Debugger.ShowTowerRange)
            {
                var radius = GetComponent<SphereCollider>().radius;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, radius);
            }
        }
    }
}