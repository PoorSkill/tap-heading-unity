using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] [Range(0, 1f)] private float maxRandomOffset;
    [SerializeField] private float coinOffsetToBar = .25f;
    [SerializeField] private Transform chunkGroupTransform0;
    [SerializeField] private Transform chunkGroupTransform1;

    [SerializeField] private float speedAdder;

    [SerializeField] private GameObject chunkPrefab;
    private float _chunkHeight;
    private float _halfChunkWidth;
    [SerializeField] private float chunkSpeedBase;
    private float _chunkSpeed;
    private float _chunkYStart;
    private int _amountOfChunksToBuffer;

    [SerializeField] private float yOffsetToChunks;

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float xOffset;

    private readonly List<KeyValuePair<Transform, ChunkManager>> _chunks =
        new List<KeyValuePair<Transform, ChunkManager>>();

    private bool _isRight;

    private float _fistChunkYPosition;

    private bool _isPause;

    private float _pauseChunkSpeed;

    private bool _isFirstChunkGroup = true;

    private void Start()
    {
        _chunkHeight = chunkPrefab.transform.localScale.y;
        var mainCam = Camera.main;
        if (mainCam is { })
        {
            var frustumHeight = 2.0f * mainCam.orthographicSize *
                                Mathf.Tan(mainCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            _chunkYStart = frustumHeight + _chunkHeight + yOffsetToChunks;
        }
        else
        {
            //Backup
            _chunkYStart = _amountOfChunksToBuffer * (_chunkHeight + yOffsetToChunks) / 2f;
        }

        _amountOfChunksToBuffer = (int) (_chunkYStart * 2 / (_chunkHeight + yOffsetToChunks) * .65f) + 1;

        maxRandomOffset = (chunkPrefab.transform.localScale.x - xOffset) * maxRandomOffset;

        _halfChunkWidth = chunkPrefab.transform.localScale.x / 2f;

        GenerateWalls();
    }

    private void GenerateWalls()
    {
        Instantiate(wallPrefab, new Vector3(xOffset, 0, 0), Quaternion.identity);
        Instantiate(wallPrefab, new Vector3(-xOffset, 0, 0), Quaternion.identity);
    }

    internal void StartGame()
    {
        _chunkSpeed = chunkSpeedBase;
        GenerateChunks();
        //just for first generation
        _isFirstChunkGroup = true;
        _isRight = !_isRight;
    }

    private void GenerateChunks()
    {
        var yOffset = 0f;
        for (var i = 0; i < _amountOfChunksToBuffer; ++i)
        {
            GenerateChunk(yOffset);
            yOffset += yOffsetToChunks + _chunkHeight;
        }

        _isFirstChunkGroup = false;
        for (var i = 0; i < _amountOfChunksToBuffer; ++i)
        {
            GenerateChunk(yOffset);
            yOffset += yOffsetToChunks + _chunkHeight;
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void GenerateChunk(float yOffset)
    {
        var chunk = Instantiate(chunkPrefab, _isFirstChunkGroup ? chunkGroupTransform0 : chunkGroupTransform1);
        chunk.transform.position = GetNewChunkPosition(yOffset);


        var chunkManager = chunk.GetComponent<ChunkManager>();
        _chunks.Add(new KeyValuePair<Transform, ChunkManager>(chunk.transform, chunkManager));

        //TODO("fix coin pos -> Random despawn and wrong group")
        var coinPosition = chunk.transform.position + Vector3.right *
            (_isRight
                ? -coinOffsetToBar - _halfChunkWidth
                : coinOffsetToBar + _halfChunkWidth);

        chunkManager.SetUp(_isFirstChunkGroup ? chunkGroupTransform0 : chunkGroupTransform1);
        chunkManager.SpawnCoin(coinPosition);
        //prepare Next
        _isRight = !_isRight;
    }

    private Vector3 GetNewChunkPosition(float yOffset)
    {
        return new Vector3((Random.Range(0, maxRandomOffset) + xOffset) * (_isRight ? 1 : -1), _chunkYStart + yOffset,
            0);
    }

    private void ResetChunk(GameObject chunk)
    {
        chunk.transform.position = new Vector3(Random.Range(0, _isRight ? 3f : -3f), _chunkYStart, 0);
    }

    // Update is called once per frame
    private void Update()
    {
        chunkGroupTransform0.position += Vector3.down * (_chunkSpeed * Time.deltaTime);
        chunkGroupTransform1.position += Vector3.down * (_chunkSpeed * Time.deltaTime);

        if (!_isPause)
        {
            _fistChunkYPosition += _chunkSpeed * Time.deltaTime;
            if (_fistChunkYPosition > (_chunkHeight + yOffsetToChunks) * _amountOfChunksToBuffer)
            {
                int start, stop;
                if (_isFirstChunkGroup)
                {
                    chunkGroupTransform1.position = Vector3.zero;
                    start = _amountOfChunksToBuffer;
                    stop = _chunks.Count;
                }
                else
                {
                    chunkGroupTransform0.position = Vector3.zero;
                    start = 0;
                    stop = _amountOfChunksToBuffer;
                }

                var yOffset = 0f;
                //Resets Position of Chunk in ChunkGroup
                for (var i = start; i < stop; ++i)
                {
                    var newChunkPosition = GetNewChunkPosition(yOffset);
                    _chunks[i].Key.position = newChunkPosition;

                    var coinPosition = newChunkPosition + Vector3.right *
                        (_isRight
                            ? -coinOffsetToBar - _halfChunkWidth
                            : coinOffsetToBar + _halfChunkWidth);

                    _chunks[i].Value.SpawnCoin(coinPosition);
                    //prepare for next
                    _isRight = !_isRight;
                    yOffset += yOffsetToChunks + _chunkHeight;
                }

                _isFirstChunkGroup = !_isFirstChunkGroup;
                Debug.Log("ResetChunks");
                _fistChunkYPosition = 0; //not called in GenerateChunk for easyRead
            }
        }
    }

    internal void AddSpeed()
    {
        _chunkSpeed += speedAdder;
    }

    public void Pause()
    {
        _isPause = true;
        PauseChunks();
    }

    private void PauseChunks()
    {
        _pauseChunkSpeed = _chunkSpeed;
        _chunkSpeed = 0;
    }

    public void Resume()
    {
        _isPause = false;
        ResumeChunks();
    }

    private void ResumeChunks()
    {
        _chunkSpeed = _pauseChunkSpeed;
    }
}