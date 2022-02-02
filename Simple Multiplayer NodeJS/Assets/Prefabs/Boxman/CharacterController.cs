using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    private Transform chracterBody;

    [SerializeField]
    private Transform cameraArm;

    Animator animator;
    void Start()
    {
        animator = chracterBody.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        LookAround();
    }

    private void LookAround(){

        float deltaX = Input.GetAxis("Mouse X"); // 마우스 좌우로 움직인 수치
        float deltaY = Input.GetAxis("Mouse Y"); // 마우스 상하로 움직인 수치

        Vector2 mouseDelta = new Vector2(deltaX,deltaY);
        Vector3 camAngle = cameraArm.rotation.eulerAngles;

        float x = camAngle.x - mouseDelta.y;

        

        // camangle.x => 상하 카메라 각도, camanble.y => 좌우 카메라 각도
        cameraArm.rotation = Quaternion.Euler(camAngle.x-mouseDelta.y, camAngle.y + mouseDelta.x, camAngle.z);
    }
}
