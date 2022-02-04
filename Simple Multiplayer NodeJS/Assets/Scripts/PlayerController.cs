using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    public Transform chracterBody;

    public Transform cameraArm;

	public Camera cam;

	Animator animator;

	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	public bool isLocalPlayer = false;

	// 타 사용자 애니메이션 효과를 위함
	Vector3 otherOldPosition;
	Vector3 otherCurrentPosition;
	// ###############

	Vector3 oldPosition;
	Vector3 currentPosition;
	Quaternion oldRotation;
	Quaternion currentRotation;

	// 채팅 관련
	public InputField chatInput;
	public Canvas ChatCanvas;
	public Canvas MessageCanvas;

	// Use this for initialization
	void Start () {
		animator = chracterBody.GetComponent<Animator>();
		animator.SetTrigger("Land");

		oldPosition = transform.position;
		currentPosition = oldPosition;
		oldRotation = transform.rotation;
		currentRotation = oldRotation;

		otherOldPosition = transform.position;
		otherCurrentPosition = otherOldPosition;

		// 타 사용자의 캐릭터일 경우
		Transform ct  = ChatCanvas.transform;
		if(!isLocalPlayer){

			for (int i=0;i<ct.childCount;i++){
				Destroy(ct.GetChild(i).gameObject);
			}			
			
		}

	}
	
	// Update is called once per frame
	void Update () {

		// 타 사용자의 캐릭터일 경우
		if (!isLocalPlayer) {
			cam.enabled = false;
			animateOther();
			return;
		}
		if (Input.GetKeyDown (KeyCode.Space) && !chatInput.isFocused) {
			//NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager>();
			//n.CommandShoot();
		}

		if (Input.GetKeyDown (KeyCode.LeftControl) && !chatInput.isFocused) {
			//NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager>();
			//n.CommandShake();
		}

		if(!chatInput.isFocused){
			LookAround();
			Move();
		}
	}
	// 본인 외 타 플레이어들의 시간당 이동 거리 가져와서 에니메이션 효과 주기
	private void animateOther(){
		otherCurrentPosition = transform.position;
		if (otherCurrentPosition != otherOldPosition) {
			otherOldPosition = otherCurrentPosition;
			animator.SetBool("isMove",true);
			animator.SetFloat("MoveSpeed", 100f);
		}else{
			animator.SetBool("isMove",false);
			animator.SetFloat("MoveSpeed", 0f);
		}
	}

	private void Move(){
		Debug.DrawRay(cameraArm.position,new Vector3(cameraArm.forward.x,0f,cameraArm.forward.z).normalized,Color.red);

		//var x = Input.GetAxis ("Horizontal") * Time.deltaTime * 150.0f;
		//var z = Input.GetAxis ("Vertical") * Time.deltaTime * 3.0f;
		//transform.Rotate (0, x, 0);
		//transform.Translate (0, 0, z);

		// currentPosition = transform.position;
		// currentRotation = transform.rotation;

		// if (currentPosition != oldPosition) {
		// 	NetworkManager.instance.GetComponent<NetworkManager>().CommandMove(transform.position);
		// 	oldPosition = currentPosition;
		// }

		// if (currentRotation != oldRotation) {
		// 	NetworkManager.instance.GetComponent<NetworkManager>().CommandTurn(transform.rotation);
		// 	oldRotation = currentRotation;
		// }

		Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
		bool isMove = moveInput.magnitude!=0;
		animator.SetBool("isMove",isMove);
		if(isMove){
			animator.SetFloat("MoveSpeed", 100f);
		}else{
			animator.SetFloat("MoveSpeed", 0f);
		}

		bool test = animator.GetBool("isMove");

		Debug.Log("isMove >> "+test);
		if(isMove){
			Vector3 lookForward = new Vector3(cameraArm.forward.x,0f,cameraArm.forward.z).normalized;
			Vector3 lookRight = new Vector3(cameraArm.right.x,0f,cameraArm.right.z).normalized;
			Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;

			transform.position += moveDir* Time.deltaTime*5f;
			chracterBody.forward = lookForward;
		}
		currentPosition = transform.position;
		currentRotation = chracterBody.rotation;

		if (currentPosition != oldPosition) {
			//NetworkManager.instance.GetComponent<NetworkManager>().CommandMove(transform.position);
			oldPosition = currentPosition;
		}

		if (currentRotation != oldRotation) {
			//NetworkManager.instance.GetComponent<NetworkManager>().CommandTurn(chracterBody.rotation);
			oldRotation = currentRotation;
		}
	}



	
    private void LookAround(){

        float deltaX = Input.GetAxis("Mouse X"); // 마우스 좌우로 움직인 수치
        float deltaY = Input.GetAxis("Mouse Y"); // 마우스 상하로 움직인 수치

        Vector2 mouseDelta = new Vector2(deltaX,deltaY);
        Vector3 camAngle = cameraArm.rotation.eulerAngles;

        float x = camAngle.x - mouseDelta.y;// 상하반전시 +

		if(x<180f){
			x = Mathf.Clamp(x,-1f,70f); // 수평선 이하일 경우 
		}else{
			x = Mathf.Clamp(x,335f,361f);
		}

        // camangle.x => 상하 카메라 각도, camanble.y => 좌우 카메라 각도
        cameraArm.rotation = Quaternion.Euler(x, camAngle.y + mouseDelta.x, camAngle.z);
    }



	public void CmdFire() {
		animator.SetTrigger("Jump");
		// var bullet = Instantiate(bulletPrefab, 
		//                          bulletSpawn.position, 
		//                          bulletSpawn.rotation) as GameObject;
		// Bullet b = bullet.GetComponent<Bullet>();
		// b.playerFrom = this.gameObject;
		// print("setting the velocity");
		// print(bullet.transform.up);
		// bullet.GetComponent<Rigidbody>().isKinematic = false;
		// bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.up * 6, ForceMode.VelocityChange);
		// Destroy(bullet, 2.0f);
	}
	public void CmdShake() {
		animator.SetTrigger("Shake");
	}
}
