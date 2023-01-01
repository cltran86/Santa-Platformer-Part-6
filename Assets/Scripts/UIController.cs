using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Text scoreText;

    private ScoreKeeper _sk;

    // Start is called before the first frame update
    void Start()
    {
        _sk = GameObject.Find("ScoreKeeper").GetComponent<ScoreKeeper>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = _sk.Score > 0 ? _sk.Score.ToString() : "0";
    }
}
