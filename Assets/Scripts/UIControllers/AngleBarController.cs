using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AngleBarController : MonoBehaviour
{
    private Slider fillBar;
    private PlayerController playerController;
    public float value = 0;
    private bool toRight;
    private float controllSpeed = 0.33f;

    private void Start()
    {
        playerController = GameManager.instance.player.GetComponent<PlayerController>();
        fillBar = gameObject.GetComponent<Slider>();
    }

    public void AngleBarMoving()
    {
        // 3�� ����
        if (fillBar != null)
        {
            if (!toRight)
            {
                float tempValue = fillBar.value;
                fillBar.value = Mathf.Clamp(tempValue + Time.deltaTime * controllSpeed, 0, 1f);
                if (fillBar.value >= 1f)
                {
                    toRight = true;
                }
            }
            else
            {
                float tempValue = fillBar.value;
                fillBar.value = Mathf.Clamp(tempValue - Time.deltaTime * controllSpeed, 0, 1f);
                if (fillBar.value <= 0f) // ��ȸ �Ϸ�
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetAngle()
    {
        // ���� ���� ����
        // value 0~0.6, 0.9~1 : 1���� ����
        // value 0.6~0.7, 0.8~0.9
        // value 0.7~0.8
        var initialZ = (fillBar.value * (playerController.maxAngle - playerController.minAngle)) + playerController.minAngle;
        playerController.initialAngle = Quaternion.Euler(0f, 0f, initialZ);
        //playerController.initialAngle = new Quaternion(0, 0, initialZ < 0.01f ? playerController.minAngle : initialZ, 1);
    }
}
