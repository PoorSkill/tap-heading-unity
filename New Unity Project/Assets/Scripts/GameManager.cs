using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerMovementScript playerMovementScript;
    [SerializeField] private LevelGeneratorScript levelGeneratorScript;
    [SerializeField] private UIManager uiManager;

    private bool _isRunning = false;

    private int score;

    void Start()
    {
        playerMovementScript.SetManager(this);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            if (_isRunning)
            {
                playerMovementScript.CallChangeDirection();
            }
            else
            {
                _isRunning = true;
                levelGeneratorScript.StartGame();
            }
    }

    internal void CoinPickedUpCallback()
    {
        //Todo()
        uiManager.UpdateScoreText(++score);
    }
}