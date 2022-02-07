using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartWarp : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject heartEffectPrefab;

    public QuizController quizController;

    private bool started;

    void Start()
    {
        started = false;
    }


	void OnCollisionEnter(Collision collision)
	{
		Debug.Log("On Collision >> "+collision.gameObject.name);
        // 싱글플레이라 한사람밖에 없어서 그냥
        // 게임 시작, 효과 발생
        Vector3 colPosition = collision.gameObject.transform.position;
        colPosition.y +=1.5f;
        var startEffect = Instantiate(heartEffectPrefab, colPosition, collision.gameObject.transform.rotation) as GameObject;
        gameObject.SetActive(false);
        quizController.startQuiz(started);
		// Bullet b = bullet.GetComponent<Bullet>();
	}

    public void setStarted(bool val){
        this.started = val;
    }


}
