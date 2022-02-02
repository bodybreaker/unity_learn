using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public Transform chracterBody;

    public Transform cameraArm;

	Animator animator;

	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	public bool isLocalPlayer = false;

	Vector3 oldPosition;
	Vector3 currentPosition;
	Quaternion oldRotation;
	Quaternion currentRotation;

	// Use this for initialization
	void Start () {
		oldPosition = transform.position;
		currentPosition = oldPosition;
		oldRotation = transform.rotation;
		currentRotation = oldRotation;
	}
	
	// Update is called once per frame
	void Update () {

		animator = chracterBody.GetComponent<Animator>();
		if (!isLocalPlayer) {
			return;
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager>();
			n.CommandShoot();
		}
		LookAround();
		Move();
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
			Vector3 lookForward = new Vector3(cameraArm.forward.x,0f,cameraArm.forward.z).normalized;
			Vector3 lookRight = new Vector3(cameraArm.right.x,0f,cameraArm.right.z).normalized;
			Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;

			transform.position += moveDir* Time.deltaTime*5f;
			chracterBody.forward = lookForward;
		}
		currentPosition = transform.position;
		currentRotation = chracterBody.rotation;

		if (currentPosition != oldPosition) {
			NetworkManager.instance.GetComponent<NetworkManager>().CommandMove(transform.position);
			oldPosition = currentPosition;
		}

		if (currentRotation != oldRotation) {
			NetworkManager.instance.GetComponent<NetworkManager>().CommandTurn(transform.rotation);
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
		var bullet = Instantiate(bulletPrefab, 
		                         bulletSpawn.position, 
		                         bulletSpawn.rotation) as GameObject;
		Bullet b = bullet.GetComponent<Bullet>();
		b.playerFrom = this.gameObject;
		print("setting the velocity");
		print(bullet.transform.up);
		bullet.GetComponent<Rigidbody>().isKinematic = false;
		bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.up * 6, ForceMode.VelocityChange);
		Destroy(bullet, 2.0f);
	}
}
