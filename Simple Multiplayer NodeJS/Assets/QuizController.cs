using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;



public class QuizController : MonoBehaviour
{
    // Start is called before the first frame update
    
    private class Quiz{
        private string quizString;
        private bool answer;

        private string desc;

        public Quiz(string _quizString,bool _answer, string _desc){
            this.quizString = _quizString;
            this.answer = _answer;
            this.desc = _desc;
        }
        public string getQuizString(){
            return quizString;
        }
        public bool getAnswer(){
            return answer;
        }
        public string getDesc(){
            return desc;
        }

    }
    public GameObject correctPrefab;
    public GameObject wrongPrefab;
    public GameObject Owarp;
    public GameObject Xwarp;

    public GameObject player;

    public GameObject StartWarp;

    public Text text;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private GameObject cam;
    private Vector3 startCameraPosition;
    private Quaternion startCameraRotation;

    private List<Quiz> quizList;

    private Quiz currentQuiz;
    private int currentIdx;
    private int length;

    private static string[] messages = {
        "<b><color=red>O</color> <color=blue>X</color> 퀴즈를 시작합니다</b>",
        "문제를 잘 보시고\n <color=red>O</color> <color=blue>X</color> 를 선택해 주세요 \n\n <color=red>O</color> <-           -> <color=blue>X</color>",
        "자...\n 이제 시작합니다..!",
    };
    

    void Start()
    {
        Owarp.SetActive(false);
        Xwarp.SetActive(false);
        startPosition = player.transform.position;
        startRotation = player.transform.rotation;

        cam = player.transform.Find("CameraArm").transform.Find("Camera").gameObject;

        startCameraPosition = cam.transform.position;
        startCameraRotation = cam.transform.rotation;
        text.supportRichText = true;
        initQuiz();
    }

    private void initQuiz(){
        quizList = new List<Quiz>();

        quizList.Add(new Quiz(
            "대구은행의 설립연도는 1966년이다",
            false,
            "대구은행은 1966년 10월 7일에 설립되었습니다."
        ));

        quizList.Add(new Quiz(
            "DGB의 슬로건은 [Digital & Global Bank] 이다.",
            false,
            "DGB 의 슬로건은 Digital & Global Bank 입니다."
        ));

        quizList.Add(new Quiz(
            "DGB의 핵심 가치는 인간중심, 주인의식, 고객우선 이다.",
            false,
            "DGB의 핵심 가치 3가지는 성과중심, 주인의식, 고객우선 입니다. "
        ));


        length = quizList.Count;

    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void startQuiz(bool isStarted){

        if(!isStarted){
            currentIdx = 0;
            StartCoroutine(showDurationMessage(3f));
            StartWarp.GetComponent<StartWarp>().setStarted(true);
        }else{
            currentIdx++;

            if(currentIdx+1>length){
                player.transform.position = startPosition;
                player.transform.rotation = startRotation;
                cam.transform.position = startCameraPosition;
                cam.transform.rotation = startCameraRotation;

                text.text="<b> 퀴즈를 모두 다 푸셨습니다! </b> \n\n<i>기존 방으로 돌아가시려면 시작위치로 이동해 주세요</i>";


                StartWarp.SetActive(true);
                Debug.Log("퀴즈 종료");
            }else{
                nextQuiz();
                Debug.Log("다음 문제 진행");
            }
            
        }
    }


    private void nextQuiz(){
        setWarpActive(true);
        currentQuiz = quizList[currentIdx];
        int num = currentIdx+1;
        text.text="<b>["+num+"] 번 문제</b>\n\n"+currentQuiz.getQuizString();
    }

    private IEnumerator showDurationMessage(float sec){
        for(int i=0; i< messages.Length;i++){
            text.text = messages[i];
            yield return new WaitForSeconds(sec);  
        }
        nextQuiz();     
    }

    private void setWarpActive(bool onOff){
        Owarp.SetActive(onOff);
        Xwarp.SetActive(onOff);
    }

    public void checkAnswer(bool answer){

        player.transform.position = startPosition;
        player.transform.rotation = startRotation;
        cam.transform.position = startCameraPosition;
        cam.transform.rotation = startCameraRotation;
        
        setWarpActive(false);

        Vector3 effectPosition = startPosition;
        effectPosition.y +=1.5f;

        if(currentQuiz.getAnswer() == answer){
            var correctEffect= Instantiate(correctPrefab, effectPosition, startRotation) as GameObject;
            text.text ="<b><color=green>정답입니다!</color></b>"+"\n\n <i>다음문제로 이동하시려면 시작위치로 이동해 주세요</i>";
        }else{
            var wrongEffect= Instantiate(wrongPrefab, effectPosition, startRotation) as GameObject;
            text.text ="<b><color=red>오답입니다!</color></b>\n\n"+currentQuiz.getDesc()+"\n\n <i>다음문제로 이동하시려면 시작위치로 이동해 주세요</i>";
        }

        StartWarp.SetActive(true);
    }

    
}
