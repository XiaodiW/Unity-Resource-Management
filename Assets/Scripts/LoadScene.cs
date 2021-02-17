using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour {

    private GameController _gameController;

    private void Start() {
        _gameController = GetComponent<GameController>();

    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        _gameController.ToReleaseMemory(_gameController.isUnloadUnusedAssets, _gameController.isGCCollect);
    }
}
