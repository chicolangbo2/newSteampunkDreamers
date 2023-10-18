using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.WSA;

public class StateGliding : BaseState
{
    public Vector3 direction = Vector3.zero;

    private float minAngle;
    private float maxAngle;
    private float minRotSpeed = 5f; // ����ü ȸ�� �ӵ� minimum, ���� ������ ��
    private float standardRotSpeed = 10; // ����ü ȸ�� �ӵ� ���ذ�
    private float maxRotSpeed = 50f; // ����ü ȸ�� �ӵ� maximum, ���� ������ ��
    private float rotSpeed;
    private Vector3 initialPos;

    // Resistance & Speed
    public bool isRotPossible = true;
    private float airResistance;
    private float airResistanceDown = 20f; // ����ü �ޱ��� 50% ������ �� �������� ���ӵ�
    private float airResistanceUp = -50f; // ����ü �ޱ��� 50% �̻��� �� �ö󰡴� ���ӵ�

    private float minFrontSpeed = 5f;
    private float inputLimit = 5f; // frontSpeed�� inputLimit ������ �� Ŭ�� ����
    private float inputLimitRelease = 15f; // frontSpeed�� inputLimitRelease �̻��� �� Ŭ�� ���� ����

    // airflow
    private float airflowFrontRatio = 10; // ��ǳ
    private float airflowReverseRatio = 100; // ��ǳ ( airResistance -= dt*airflowReverseRatio�� ����Ǿ� ���� )

    private bool firstDown;
    private bool jump;
    private float jumpRotateTime = 3f;
    private Quaternion startRotation;
    private Quaternion jumpTargetAngle;
    private float elapsedTime = 0;

    public StateGliding(PlayerController controller) : base(controller)
    {
    }

    public override void OnEnterState()
    {
        // �ʱ� ���� ���� & ���� �����̼� ���ǵ� ����
        jumpTargetAngle = controller.initialAngle;

        // velocity ���� -> �߻�
        if (controller.launchSuccess)
        {
            direction = controller.transform.right;
            initialPos = controller.transform.position;
        }
        controller.frontSpeed = controller.initialSpeed;
        minAngle = controller.minAngle;
        maxAngle = controller.maxAngle;

        startRotation = controller.transform.rotation;
    }     

    public override void OnExitState()
    {
    }

    public override void OnFixedUpdateState()
    {
        if (!jump && controller.launchSuccess)
        {
            Jump(jumpTargetAngle, jumpRotateTime);
        }
    }

    public override void OnUpdateState()
    {
        direction = controller.transform.right;

        if(jump || !controller.launchSuccess)
        {

            if (controller.airshipColiide)
            {
                isRotPossible = false;
            }

            // ����ü �ӵ� ������Ʈ
            controller.frontSpeed += airResistance * Time.deltaTime;
            if(controller.frontSpeed > controller.maxSpeed)
            {
                controller.frontSpeed = controller.maxSpeed;
            }
            else if(controller.frontSpeed < minFrontSpeed)
            {
                controller.frontSpeed = minFrontSpeed;
            }

            SetRotSpeed(controller.transform.eulerAngles);
            RotatePlane(Input.GetMouseButton(0));
            SetResistance(controller.transform.localEulerAngles);
        }

        // distance ������Ʈ
        controller.velocity = direction * ((controller.frontSpeed <= 0f) ? 0f : controller.frontSpeed);
        controller.distance = controller.transform.position.x - initialPos.x;
    }

    public void RotatePlane(bool up)
    {
        if (up && controller.launchSuccess && isRotPossible)
        {
            controller.fuelTimer -= Time.deltaTime;
            if( controller.fuelTimer <= 0f )
            {
                controller.fuelTimer = 0f;
            }
            var change = Vector3.forward * rotSpeed * 1.5f * Time.deltaTime;
            controller.transform.Rotate(change);
        }
        else
        {
            var change = Vector3.forward * -rotSpeed * Time.deltaTime;
            controller.transform.Rotate((change.z < 0) ? change : -change);
        }
        controller.transform.localRotation = Utils.ClampRotation(controller.transform.localEulerAngles, minAngle, maxAngle);
    }

    public void SetResistance(Vector3 localEulerAngle)
    {
        // �ޱ� -> �����
        localEulerAngle.z = Utils.EulerToAngle(localEulerAngle.z);
        var anglePercentage = (localEulerAngle.z - minAngle) / (maxAngle - minAngle) * 100f;

        // �ޱ� - airResistance
        if (anglePercentage >= 50 && firstDown)
        {
            airResistance = (anglePercentage - 50) / 50 * airResistanceUp; // -
        }
        else
        {
            if(firstDown == false && anglePercentage < 49)
            {
                firstDown = true;
            }
            airResistance = (1 - anglePercentage / 50) * airResistanceDown; // +
        }

        // �Է� ����
        if (controller.fuelTimer <= 0 || controller.frontSpeed + airResistance <= inputLimit)
        {
            isRotPossible = false;
        }
        else if(controller.frontSpeed + airResistance >= inputLimitRelease)
        {
            isRotPossible = true;
        }

        // ��� ����
        if (controller.airflows.Count != 0)
        {
            ApplicationAirflow();
        }
    }

    public void SetRotSpeed(Vector3 localEulerAngle)
    {
        localEulerAngle.z = Utils.EulerToAngle(localEulerAngle.z);
        var anglePercentage = (localEulerAngle.z - minAngle) / (maxAngle - minAngle) * 100f;

        if (anglePercentage > 50)
        {
            rotSpeed = standardRotSpeed + (anglePercentage - 50) / 50 * maxRotSpeed; // -
        }
        else
        {
            rotSpeed = standardRotSpeed + (1 - anglePercentage / 50) * minRotSpeed; // +
        }
    }

    public void ApplicationAirflow()
    {
        var airflow = controller.airflows.First.Value;
        if(airflow.airflowType == AirflowType.Front)
        {
            airResistance = Mathf.Lerp(0, controller.maxSpeed * 0.4f, Time.deltaTime * airflowFrontRatio);
        }
        else
        {
            airResistance -= Time.deltaTime * airflowReverseRatio;
        }
    }

    public void Jump(Quaternion targetRotation, float duration)
    {
        if (elapsedTime < duration)
        {
            // ȸ�� ������ �����Ͽ� ������Ʈ
            controller.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
        }
        else
        {
            // ���� ��ǥ ȸ�� ����
            controller.transform.localRotation = Utils.ClampRotation(targetRotation.eulerAngles, minAngle, maxAngle);
            jump = true;
        }
    }
}