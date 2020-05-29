using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;


public class MoveSphere : MonoBehaviour
{
    [SerializeField] private bool useJobs;
    [SerializeField] private int nbEntities;
    [SerializeField] private int sizeArea;

    [SerializeField] private Transform sphere;

    private Transform[] transformArray; 
    
    private NativeArray<float3> positionArray;
    private NativeArray<float> tabMoveX;
    private NativeArray<float> tabMoveY;
    private NativeArray<float> tabMoveZ;
    

    // L'affichage est très couteux d'ou le faible gain quite à l'utilisation de job et burst
    
    [BurstCompile]
    public struct MoveSphereJob : IJobParallelFor
    {
            
        public NativeArray<float3> positionArray;
        public NativeArray<float> tabMoveX;
        public NativeArray<float> tabMoveY;
        public NativeArray<float> tabMoveZ;
        
        public float deltaTime;
        public float sizeArea;
        
        
        public void Execute(int index)
        {
            if (positionArray[index].x >= sizeArea || positionArray[index].x <= -sizeArea) tabMoveX[index] = -tabMoveX[index];

            if (positionArray[index].y >= sizeArea || positionArray[index].y <= -sizeArea) tabMoveY[index] = -tabMoveY[index];

            if (positionArray[index].z >= sizeArea || positionArray[index].z <= -sizeArea) tabMoveZ[index] = -tabMoveZ[index];
                
            positionArray[index] += new float3(tabMoveX[index] * deltaTime, tabMoveY[index] * deltaTime, tabMoveZ[index] * deltaTime);
        }
    }

    public void Start()
    {
        transformArray = new Transform[nbEntities];
        
        positionArray = new NativeArray<float3>(nbEntities, Allocator.Persistent);
        tabMoveX = new NativeArray<float>(nbEntities, Allocator.Persistent);
        tabMoveY = new NativeArray<float>(nbEntities, Allocator.Persistent);
        tabMoveZ = new NativeArray<float>(nbEntities, Allocator.Persistent);
        
        for (int i = 0; i < nbEntities; i++)
        {
            var pos = new Vector3(Random.Range(-sizeArea,sizeArea),Random.Range(-sizeArea,sizeArea),Random.Range(-sizeArea,sizeArea));
            transformArray[i] = Instantiate(sphere, pos, Quaternion.identity).GetComponent<Transform>();
            positionArray[i] = pos;
            
            tabMoveX[i] = Random.Range(-1f, 1f);
            tabMoveY[i] = Random.Range(-1f, 1f);
            tabMoveZ[i] = Random.Range(-1f, 1f);
        }
    }

    public void Update()
    {
        if (useJobs)
        {
            var moveSphereJob = new MoveSphereJob()
            {
                positionArray = positionArray,
                tabMoveX = tabMoveX,
                tabMoveY = tabMoveY,
                tabMoveZ = tabMoveZ,
                deltaTime = Time.deltaTime,
                sizeArea = sizeArea
            };
            moveSphereJob.Schedule(nbEntities, nbEntities/8).Complete();;

            
            for (int i = 0; i < nbEntities; i++)
            {
                transformArray[i].position = positionArray[i];
            }
            
            
        }
        else
        {
            for (int i=0;i<nbEntities;i++)
            {
                if (positionArray[i].x >= sizeArea || positionArray[i].x <= -sizeArea) tabMoveX[i] = -tabMoveX[i];

                if (positionArray[i].y >= sizeArea || positionArray[i].y <= -sizeArea) tabMoveY[i] = -tabMoveY[i];

                if (positionArray[i].z >= sizeArea || positionArray[i].z <= -sizeArea) tabMoveZ[i] = -tabMoveZ[i];

                positionArray[i] += new float3(tabMoveX[i] * Time.deltaTime, tabMoveY[i] * Time.deltaTime, tabMoveZ[i] * Time.deltaTime);
                transformArray[i].position = positionArray[i];
            }
        }
    }

    public void OnDestroy()
    {
        positionArray.Dispose();
        tabMoveX.Dispose();
        tabMoveY.Dispose();
        tabMoveZ.Dispose();
    }
}
