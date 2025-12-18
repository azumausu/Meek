using UnityEngine;

namespace Demo
{
    public class SplashModel
    {
        public SplashModel()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
        }
    }
}