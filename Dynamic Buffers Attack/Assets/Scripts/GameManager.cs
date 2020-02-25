using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class GameManager : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    private static GameManager _gameManager;

    public static GameManager Instance => _gameManager;

    public static Entity PfEnemyEntity;
    public static Entity PfAttackEntity;

    public GameObject pFEnemy;
    public GameObject pFAttack;
    
    private void Awake()
    {
        if (!_gameManager)
            _gameManager = this;
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        PfEnemyEntity = conversionSystem.GetPrimaryEntity(pFEnemy);
        PfAttackEntity = conversionSystem.GetPrimaryEntity(pFAttack);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(pFEnemy);
        referencedPrefabs.Add(pFAttack);
    }
}
