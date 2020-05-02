using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudsView : MonoBehaviour
{

    // cloud spawn parameter constants
    public int numberOfCloudsToSpawn = 50; // maximum number of clouds to spawn
    public int framesBetweenCloudSpawns = 2; // dont spawn clouds every frame to lower strain on the system
    public float cloudToCameraDistanceToDespawn = 1000; // despawn clouds outside this view distance from camera
    public float cloudSpawnMaxDistanceFromViewPoint = 1000; // spawn within this width x width area
    public float cloudSpawnMaxDistanceFromElevationLevel = 25f;
    public float cloudSpawnElevation = 100f;

    // cloud movement parameter constants
    public float cloudMoveSpeed = 0.15f;
    public Vector3 cloudMoveDirectionNormalized = new Vector3(1, 0, 1).normalized;

    // private variables
    private int _numberOfCloudsSpawned;
    private float _cloudElevationLevel;
    List<Cloud> _clouds;
    Vector3 _cameraViewCenterPoint;

    private int _framesSinceLastSpawn;

    public class Cloud
    {
        // cloud dimension and offsets constants
        public static float CloudMaxCenterOffsetHorizontal = 15f;
        public static float CloudMinCenterOffsetHorizontal = 5f;

        public static float CloudMaxCenterOffsetVertical = 7f;
        public static float CloudMinCenterOffsetVertical = 2f;

        public static float CloudMaxObjectSizeVertical = 5f;
        public static float CloudMinObjectSizeVertical = 2f;

        public static float CloudMaxObjectSizeHorizontal = 15f;
        public static float CloudMinObjectSizeHorizontal = 3f;
        // cloud object counts
        public static int MaxObjectsPerCloud = 20;
        public static int MinObjectsPerCloud = 5;

        // private variables //

        public int objectsPerCloud { get; }

        public GameObject CloudCenter;
        public Vector3 cloudCenterPos { get { return this.CloudCenter.transform.position; } }

        public Cloud(Transform parent, Vector3 spawnPos)
        {
            objectsPerCloud = (int)UnityEngine.Random.Range(MinObjectsPerCloud, MaxObjectsPerCloud);
            CloudCenter = new GameObject("Cloud");
            CloudCenter.transform.parent = parent;
            CloudCenter.transform.position = spawnPos;
            GenerateCloud(parent);
        }

        private void GenerateCloud(Transform parent)
        {
            for (int i = 0; i < this.objectsPerCloud; i++)
            {
                GameObject cloudObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cloudObject.transform.parent = CloudCenter.transform;
                cloudObject.transform.localPosition =
                                        new Vector3(
                                            Random.Range(CloudMinCenterOffsetHorizontal, CloudMaxCenterOffsetHorizontal),
                                            Random.Range(CloudMinCenterOffsetVertical, CloudMaxCenterOffsetVertical),
                                            Random.Range(CloudMinCenterOffsetHorizontal, CloudMaxCenterOffsetHorizontal)
                                                    );
                cloudObject.transform.localScale = new Vector3(Random.Range(CloudMinObjectSizeHorizontal, CloudMaxObjectSizeHorizontal),
                                                Random.Range(CloudMinObjectSizeVertical, CloudMaxObjectSizeVertical),
                                                Random.Range(CloudMinObjectSizeHorizontal, CloudMaxObjectSizeHorizontal));
            }
        }

        public void Tick(float moveSpeed, Vector3 moveDirection)
        {
            Move(moveSpeed, moveDirection);
        }

        public void Despawn()
        {
            Destroy(CloudCenter);
        }

        private void Move(float moveSpeed, Vector3 moveDirection)
        {
            CloudCenter.transform.position += moveDirection * moveSpeed;
        }
    }

    // Use this for initialization
    void Start()
    {
        _cloudElevationLevel = cloudSpawnElevation;
        _clouds = new List<Cloud>();
        _cameraViewCenterPoint = new Vector3();
        _numberOfCloudsSpawned = 0;
        _framesSinceLastSpawn = 0;
    }

    // Update is called once per frame
    void Update()
    {

        // update camera position
        _cameraViewCenterPoint = new Vector3(0,0,0);
        _cameraViewCenterPoint.y = 0;

        if (_numberOfCloudsSpawned < numberOfCloudsToSpawn && _framesSinceLastSpawn > framesBetweenCloudSpawns)
        {
            // spawn cloud
            SpawnCloud();
            _numberOfCloudsSpawned++;
            _framesSinceLastSpawn = 0;
        }

        _framesSinceLastSpawn++;

        // update clouds
        foreach (Cloud cloud in _clouds)
        {
            // update cloud tick
            cloud.Tick(cloudMoveSpeed, cloudMoveDirectionNormalized);
        }

        // despawn old clouds
        for (int i = 0; i < _clouds.Count; i++)
        {
            Cloud cloud = _clouds[i];
            Vector3 posNoY = cloud.cloudCenterPos;
            posNoY.y = 0;
            if ((_cameraViewCenterPoint - posNoY).magnitude > cloudToCameraDistanceToDespawn)
            {
                DespawnCloud(cloud);
                _clouds.RemoveAt(i);
                i--;
            }
        }
    }

    private void SpawnCloud()
    {
        // compute new cloud position
        Vector3 cloudPos = new Vector3(
            Random.Range(-cloudSpawnMaxDistanceFromViewPoint, cloudSpawnMaxDistanceFromViewPoint),
            _cloudElevationLevel + Random.Range(-cloudSpawnMaxDistanceFromElevationLevel, cloudSpawnMaxDistanceFromElevationLevel),
            Random.Range(-cloudSpawnMaxDistanceFromViewPoint, cloudSpawnMaxDistanceFromViewPoint)
            );
        // create a cloud and add to the list of clouds
        Cloud cloud = new Cloud(transform, _cameraViewCenterPoint + cloudPos);
        _clouds.Add(cloud);
    }

    private void DespawnCloud(Cloud cloud)
    {
        cloud.Despawn();
        _numberOfCloudsSpawned--;
    }
}