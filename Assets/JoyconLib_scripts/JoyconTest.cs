using UnityEngine;
using System.Collections.Generic;

public class JoyconTest : MonoBehaviour
{
    private List<Joycon> joycons;
    private Joycon joycon;

    private void Start()
    {
        joycons = JoyconManager.Instance.j;

        if (joycons == null || joycons.Count == 0)
        {
            Debug.LogError("Joy-ConÇ™îFéØÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒ");
            return;
        }

        joycon = joycons[0];

        Debug.Log("Joy-ConîFéØê¨å˜ÅI");
    }

    private void Update()
    {
        if (joycon == null)
        {
            return;
        }

        Vector3 gyro = joycon.GetGyro();
        Vector3 accel = joycon.GetAccel();

        Debug.Log(
            "Gyro: " + gyro +
            " / Accel: " + accel
        );
    }
}