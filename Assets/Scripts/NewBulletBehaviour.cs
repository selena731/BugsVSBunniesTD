using System.Collections.Generic;
using DefaultNamespace.Game;
using DefaultNamespace.OnDeathEffects;
using DefaultNamespace.TowerSystem;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


public interface IPoolable
{
    public int ID { get; }
    public GameObject Prefab { get; }
    public void Dispose();
}

namespace DefaultNamespace
{
    public class NewBulletBehaviour : MonoBehaviour
    {
        private List<Debuff> _debuffs;
        private IEnemyUnit _target;
        private BulletConfig _bulletConfig;
        
        //Spawns + Entangles with enemies when killed (Bubbles foreach enemy etc.)
        public GameObject EntangleWhenKillEnemy;

        private int _hitCount;
        private Vector3 _lastDirection;

        private float durationTimer = 0;
        
        public void Initialize(List<Debuff> debuffs, IEnemyUnit target, BulletConfig bulletConfig)
        {
            _debuffs = debuffs;
            _target = target;
            _bulletConfig = bulletConfig;

            _lastDirection = _target.mTransform.position - transform.position;
        }
       
        private void Update()
        {
            durationTimer += GameSpeed.GameDeltaTime;
            if (durationTimer > 10) // Total Life Duration
                Dispose();

            if (!CheckTargetAvaliablity())
            {
                transform.position += _lastDirection.normalized * _bulletConfig.speed * GameSpeed.GameDeltaTime;
                return;
            }

            _lastDirection = _target.mTransform.position - transform.position;
            transform.position = Vector3.MoveTowards(transform.position, _target.mTransform.position,
                _bulletConfig.speed * GameSpeed.GameDeltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hitCount >= _bulletConfig.maxHit)
            {
                Dispose();
                return;
            }

            if (other.TryGetComponent(out IDebuffable debuffable))
            {
                foreach (var debuff in _debuffs)
                {
                    debuffable.ApplyDebuff(debuff);
                }
            }
            
            if (other.TryGetComponent(out IEnemyUnit enemyUnit))
            {
                if (enemyUnit.TakeDamage(_bulletConfig.damage, false))
                {
                    ApplySpecialDeath(enemyUnit);
                    enemyUnit.Kill(true);
                }

                _hitCount++;
            }
        }

        private bool CheckTargetAvaliablity()
        {
            if (_target == null) return false;
            if (_target.IsDead) return false;
            if (_target.mTransform == null) return false;
            if (!_target.mTransform.gameObject.activeInHierarchy) return false;

            return true;
        }

        private void ApplySpecialDeath(IEnemyUnit enemyUnit)
        {
            var copiedMesh = new Mesh();
            var skinnedMesh = enemyUnit.mTransform.transform.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMesh.BakeMesh(copiedMesh);
            
            GameObject newMeshObject = new GameObject("CopiedMesh");
            MeshFilter meshFilter = newMeshObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = newMeshObject.AddComponent<MeshRenderer>();
            meshFilter.mesh = copiedMesh;
            meshRenderer.material = skinnedMesh.material;
            
            
            var spawn = Instantiate(EntangleWhenKillEnemy, enemyUnit.mTransform.position, quaternion.identity);
            newMeshObject.transform.SetParent(spawn.transform);
            newMeshObject.transform.localPosition = Vector3.zero; // TODO Add Offsett
            
            spawn.transform.localScale *= enemyUnit.mTransform.localScale.x;
            spawn.GetComponent<Rigidbody>().AddForce(_lastDirection, ForceMode.Impulse);
            spawn.GetComponent<Rigidbody>().AddTorque(new Vector3().RandomDirection() * 10, ForceMode.Impulse);
        }
        
        public void Dispose()
        {
            Destroy(gameObject);
        }
    }
}