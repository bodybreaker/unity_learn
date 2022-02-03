using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContorl : MonoBehaviour{
    // Start is called before the first frame update

    [SerializeField]
    private float walkSpeed;
    private Rigidbody myRigid; // 플레이어의 몸

    [SerializeField]
    private float lookSensitivity;// 카메라 민감도

    [SerializeField]
    private Camera theCamera;


    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0;


    void Start(){   
        myRigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update(){
        Move();
        CameraRotation();
        CharactoerRotaition();
    }

    private void Move(){
        float _moveDirX = Input.GetAxisRaw("Horizontal"); // 오른쪽 :1, 왼쪽 -1
        float _moveDirZ = Input.GetAxisRaw("Vertical"); // 정면, 뒤

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal+_moveVertical).normalized * walkSpeed;

        myRigid.MovePosition(transform.position+_velocity * Time.deltaTime);// Time.deltaTime = 0.016
        
    }

    private void CameraRotation(){
        // 상하 카메라 회전
        float _xRotation = Input.GetAxisRaw("Mouse Y"); // 마우스는 x,y 만 존재
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX,-cameraRotationLimit,cameraRotationLimit); // 최소 , 최대값 고정

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX,0f,0f);
    }

    private void CharactoerRotaition(){
        // 좌우 캐릭터 회전
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f,_yRotation,0f)*lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
        Debug.Log(myRigid.rotation);
        Debug.Log(myRigid.rotation.eulerAngles);
    }
}
