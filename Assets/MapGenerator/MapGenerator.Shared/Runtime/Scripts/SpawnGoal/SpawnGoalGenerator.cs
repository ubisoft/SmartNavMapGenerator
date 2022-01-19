using System.Transactions;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace LaForge.MapGenerator
{
    public static class IListExtensions
    {
        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        public static void Shuffle<T>(this IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
    }

    [RequireComponent(typeof(NavMeshSurface))]
    public class SpawnGoalGenerator : MonoBehaviour
    {
        [SerializeField]
        private int totalGoals = 0;
        [Header("Options")]
        public bool debugDisplay = false;
        public bool GenerateGoalUniformly = false;
        private bool navMeshGenerated = false;
        public LayerMask layerMask = int.MaxValue;

        private Vector3 goal;
        private Vector3 spawn;

        [Header("Uniform distance Options")]
        public int NbBuckets = 20;
        public int LastBucketDistance = 140;
        public int MaxTries = 100;

        private int bucketSize;
        private int maxPerBucket;

        public int TotalGoals
        {
            get
            {
                return totalGoals;
            }
            set
            {
                totalGoals = value;
            }
        }

        private Dictionary<int, int> buckets;


        private NavMeshSurface navMeshSurface;
        private NavMeshTriangulation navMeshTriangles;
        private float[] navMeshTrianglesWeightsCDF;

        private GameObject cubeSpawnGo = null;
        private GameObject sphereGoalGo = null;
        private List<GameObject> debugGameObjectTriangles = new List<GameObject>();
        private int nbGoalSpawns = 0;

        Material cubeSpawnMaterial;
        Material sphereGoalMaterial;

        private void Awake()
        {
            navMeshSurface = GetComponent<NavMeshSurface>();
        }

        void InitBuckets()
        {
            buckets = new Dictionary<int, int>();
            // create buckets for storing points
            int bucketStart = 0;
            bucketSize = LastBucketDistance / NbBuckets;

            while (bucketStart < LastBucketDistance)
            {
                buckets.Add(bucketStart, 0);
                bucketStart += bucketSize;
            }
            maxPerBucket = totalGoals / NbBuckets;
        }
        public void LogBuckets()
        {
            Debug.Log("LogBuckets");
            foreach (var item in buckets)
            {
                Debug.Log(item.Key + ":" + item.Value);
            }

        }

        public List<Vector3[]> GeneralGoals()
        {
            List<Vector3[]> spawnGoals = new List<Vector3[]>();
            for (int i = 0; i < totalGoals; ++i)
            {
                spawnGoals.Add(Generate());
            }
            // shuffle goals to ensure random ordering when using the uniform distribution method.
            spawnGoals.Shuffle();

            return spawnGoals;
        }

        public Vector3[] Generate()
        {
            if (GenerateGoalUniformly)
            {
                return GenerateUniform();
            }
            else
            {
                return GenerateRandom();
            }
        }

        Vector3[] GenerateRandom()
        {
            spawn = SampleRandomPoint();
            goal = SampleRandomPoint();
            nbGoalSpawns++;

            if (debugDisplay)
            {
                DebugDisplay();
            }

            return new Vector3[] { spawn, goal };
        }

        Vector3[] GenerateUniform()
        {
            // Generate spawn goes with an approximately uniform distribution over distance
            var tries = 0;

            while (tries < MaxTries)
            {
                spawn = SampleRandomPoint();
                goal = SampleRandomPoint();

                var distance = (int)Vector3.Distance(spawn, goal);
                var bucketedDistance = Math.Min(distance - distance % bucketSize, LastBucketDistance - bucketSize);

                if (buckets[bucketedDistance] < maxPerBucket)
                {
                    buckets[bucketedDistance]++;
                    break;
                }
                tries++;

                if (tries == MaxTries)
                {
                    buckets[bucketedDistance]++;
                }
            }

            if (debugDisplay)
            {
                DebugDisplay();
            }

            nbGoalSpawns++;
            return new Vector3[] { spawn, goal };
        }


        public void GenerateNavMesh()
        {
            // Warning: The NavMesh unity returns does not correspond to the one displayed in the editor. 
            // https://forum.unity.com/threads/navmesh-calculatetriangulation-produces-inaccurate-meshes.293894/#post-5570128

            if (!navMeshSurface)
                navMeshSurface = GetComponent<NavMeshSurface>();

            if (navMeshSurface.isActiveAndEnabled)
            {
                if (!navMeshGenerated)
                {
                    navMeshSurface.BuildNavMesh();
                    navMeshTriangles = NavMesh.CalculateTriangulation();

                    // Generate weights of each triangle for sampling goals later on.
                    // We want the sampling to take into account the area defined by triangles
                    // so that we don't oversample areas with a lot of triangles but small surface area.

                    float totalArea = 0.0f;
                    navMeshTrianglesWeightsCDF = new float[navMeshTriangles.areas.Length];
                    for (int index = 0; index < navMeshTrianglesWeightsCDF.Length; index++)
                    {
                        int indiceIndex = 3 * index;
                        Vector3 A = navMeshTriangles.vertices[navMeshTriangles.indices[indiceIndex]];
                        Vector3 B = navMeshTriangles.vertices[navMeshTriangles.indices[indiceIndex + 1]];
                        Vector3 C = navMeshTriangles.vertices[navMeshTriangles.indices[indiceIndex + 2]];

                        Vector3 AB = B - A;
                        Vector3 AC = C - A;

                        float area = Vector3.Cross(AB, AC).magnitude / 2.0f;

                        navMeshTrianglesWeightsCDF[index] = area;
                        totalArea += area;
                    }

                    // Renormalize and compute the CDF for easier sampling
                    float cdf = 0.0f;
                    for (int index = 0; index < navMeshTrianglesWeightsCDF.Length; index++)
                    {
                        cdf += navMeshTrianglesWeightsCDF[index] / totalArea;
                        navMeshTrianglesWeightsCDF[index] = cdf;
                    }
                    navMeshTrianglesWeightsCDF[navMeshTrianglesWeightsCDF.Length - 1] = 1.0f;

                    navMeshGenerated = true;
                }
            }
        }

        public Vector3 SampleRandomPoint()
        {
            // This function sample a point uniformly on the navmesh.
            Debug.Assert(navMeshGenerated);

            float eps = UnityEngine.Random.value;
            int t = 0;
            // Compute inverse CDF(eps) to get index of triangle sampled according to their area.
            bool found = false;
            for (int index = 0; index < navMeshTrianglesWeightsCDF.Length; index++)
            {
                if (navMeshTrianglesWeightsCDF[index] > eps)
                {
                    t = index;
                    found = true;
                    break;
                }
            }

            Debug.Assert(found);

            // Sample in the found triangle. Note: Not the most efficient, but uniform in the triangle.
            int indiceT = 3 * t;
            Vector3 A = navMeshTriangles.vertices[navMeshTriangles.indices[indiceT]];
            Vector3 B = navMeshTriangles.vertices[navMeshTriangles.indices[indiceT + 1]];
            Vector3 C = navMeshTriangles.vertices[navMeshTriangles.indices[indiceT + 2]];

            Vector3[] shuffledTriangleVertices = new Vector3[] { A, B, C };
            ShuffleArray(shuffledTriangleVertices);

            A = shuffledTriangleVertices[0];
            B = shuffledTriangleVertices[1];
            C = shuffledTriangleVertices[2];

            float weight1 = UnityEngine.Random.value;
            float weight2 = UnityEngine.Random.value;

            Vector3 point = weight1 * A + (1 - weight1) * B;
            point = weight2 * point + (1 - weight2) * C;

            if (debugDisplay)
            {
                foreach (Vector3 pointDebug in new List<Vector3> { A, B, C })
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    DestroyImmediate(go.GetComponent<Collider>());
                    go.name = "TriangleVertice_" + pointDebug;
                    Material material = new Material(Shader.Find("Diffuse"));
                    material.color = Color.green;
                    go.GetComponent<Renderer>().sharedMaterial = material;
                    go.transform.localScale = 0.10f * Vector3.one;
                    go.transform.position = pointDebug;
                    go.transform.parent = this.transform;
                    debugGameObjectTriangles.Add(go);
                }
            }

            // Point is now a point randomly sampled on the NavMesh, but sometimes the triangle isn't perfectly aligned with the floor. 
            // We look for the closest point on all the colliders in the scene.

            RaycastHit hit;
            float distanceToClosest = float.PositiveInfinity;
            Vector3 closestPoint = new Vector3();

            Physics.queriesHitBackfaces = true;
            if (Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, layerMask))
            {
                closestPoint = hit.point;
                distanceToClosest = hit.distance;
            }

            if (Physics.Raycast(point, Vector3.up, out hit, Mathf.Infinity, layerMask))
            {
                float distance = hit.distance;
                if (distance < distanceToClosest)
                {
                    distanceToClosest = distance;
                    closestPoint = hit.point;
                }
            }

            if (distanceToClosest == float.PositiveInfinity)
            {
                Debug.Log("ERROR: Haven't found a point when raycasting, not supposed to happen. Point number " + nbGoalSpawns);
                return point;
            }

            return closestPoint;
        }

        void ShuffleArray(Vector3[] array)
        {
            // Knuth shuffle algorithm :: courtesy of Wikipedia :)
            for (int t = 0; t < array.Length; t++)
            {
                Vector3 tmp = array[t];
                int r = UnityEngine.Random.Range(t, array.Length);
                array[t] = array[r];
                array[r] = tmp;
            }
        }

        public void Clear()
        {
            if (!navMeshSurface)
                navMeshSurface = GetComponent<NavMeshSurface>();

            NavMesh.RemoveAllNavMeshData();
            navMeshGenerated = false;

            DestroyImmediate(cubeSpawnGo);
            DestroyImmediate(sphereGoalGo);
            DestroyImmediate(cubeSpawnMaterial);
            DestroyImmediate(sphereGoalMaterial);

            foreach (GameObject go in debugGameObjectTriangles)
            {
                DestroyImmediate(go);
            }
            debugGameObjectTriangles.Clear();

            cubeSpawnGo = null;
            sphereGoalGo = null;

            nbGoalSpawns = 0;

            InitBuckets();
        }

        void DebugDisplay()
        {
            if (cubeSpawnGo == null)
            {
                cubeSpawnGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                DestroyImmediate(cubeSpawnGo.GetComponent<Collider>());
                cubeSpawnGo.name = "Spawn";
                cubeSpawnMaterial = new Material(Shader.Find("Diffuse"));
                cubeSpawnMaterial.color = Color.green;
                cubeSpawnGo.GetComponent<Renderer>().sharedMaterial = cubeSpawnMaterial;
                cubeSpawnGo.transform.localScale = 0.25f * Vector3.one;
                cubeSpawnGo.transform.parent = this.transform;
            }

            if (sphereGoalGo == null)
            {
                sphereGoalGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                DestroyImmediate(sphereGoalGo.GetComponent<Collider>());
                sphereGoalGo.name = "Goal";
                sphereGoalMaterial = new Material(Shader.Find("Diffuse"));
                sphereGoalMaterial.color = Color.red;
                sphereGoalGo.GetComponent<Renderer>().sharedMaterial = sphereGoalMaterial;
                sphereGoalGo.transform.localScale = 0.25f * Vector3.one;
                sphereGoalGo.transform.parent = this.transform;
            }

            cubeSpawnGo.transform.position = spawn;
            sphereGoalGo.transform.position = goal;

            Debug.DrawLine(spawn, goal, Color.blue, 3f, false);
        }
    }
}
