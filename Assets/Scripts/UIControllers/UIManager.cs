using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // �̱��� ���ٿ� ������Ƽ
    public static UIManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<UIManager>(); // ���� ��ġ���ִ� ��� ���ӿ�����Ʈ ��ȸ�ϱ� ������ ��� X
            }

            return m_instance;
        }
    }

    private static UIManager m_instance; // �̱����� �Ҵ�� ����

    public TextMeshProUGUI distance;
    public TextMeshProUGUI velocity;
    public TextMeshProUGUI altitude;
    public GameObject gameoverUI; // ���� ������ Ȱ��ȭ�� UI 

    public void UpdateDistanceText(float d)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(d.ToString("F2"));
        sb.Append(" m");
        distance.text = sb.ToString();
    }

    public void UpdateVelocityText(float v)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(v.ToString("F2"));
        sb.Append(" m/s");
        velocity.text = sb.ToString();
    }

    public void UpdateAltitudeText(float a)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(a.ToString("F2"));
        sb.Append(" m");
        altitude.text = sb.ToString();
    }

    // ���� ���� UI Ȱ��ȭ
    public void SetActiveGameoverUI(bool active)
    {
        gameoverUI.SetActive(active);
    }

    // ���� �����
    public void GameRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
