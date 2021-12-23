using UnityEngine;

namespace CustomRP
{
    [DisallowMultipleComponent,RequireComponent(typeof(Camera))]
    public class CustomRenderPipelinesCamera : MonoBehaviour
    {
        [SerializeField]
        private CameraSettings m_Setting = default;

        public CameraSettings Settings
        {
            get
            {
                if (m_Setting != null)
                {
                    return m_Setting;
                }

                m_Setting = new CameraSettings();
                return m_Setting;
            }
        }
    }
}