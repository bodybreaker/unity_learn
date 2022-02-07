using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Xwarp : MonoBehaviour
{
    public QuizController quizController;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision)
	{
		Debug.Log("On Collision >> "+collision.gameObject.name);
        quizController.checkAnswer(false);
	}
}
