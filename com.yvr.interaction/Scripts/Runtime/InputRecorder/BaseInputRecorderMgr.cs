using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using YVR.Core;
using YVR.Utilities;

namespace YVR.Interaction.Runtime
{
    [RequireComponent(typeof(InputRecorder))]
    public class BaseInputRecorderMgr : MonoBehaviour
    {
        protected InputRecorder inputRecorder;
        private string m_InputRecorderFilePath;

        private void Awake()
        {
            inputRecorder = this.GetComponent<InputRecorder>();
            m_InputRecorderFilePath = Application.persistentDataPath + "/InputRecorderCache";
            inputRecorder.changeEvent.AddListener(OnInputRecorderChange);
        }

        public virtual void SetSavePath(string fileName)
        {
            m_InputRecorderFilePath = fileName;
        }

        protected virtual void Capture(bool enable)
        {
            if (enable)
            {
                inputRecorder.ClearCapture();
                inputRecorder.StartCapture();
            }
            else
            {
                inputRecorder.StopCapture();
                if (File.Exists(m_InputRecorderFilePath))
                    File.Delete(m_InputRecorderFilePath);

                inputRecorder.SaveCaptureToFile(m_InputRecorderFilePath);
            }
        }

        protected virtual void Replay(bool enable)
        {
            if (enable)
            {
                inputRecorder.LoadCaptureFromFile(m_InputRecorderFilePath);
                inputRecorder.StartReplay();
            }
            else
            {
                inputRecorder.StopReplay();
            }
        }

        protected virtual void OnInputRecorderChange(InputRecorder.Change change)
        {
            switch (change)
            {
                case InputRecorder.Change.ReplayStarted:
                    YVRPlugin.Instance.SetBlockInteractionData(true);
                    break;
                case InputRecorder.Change.ReplayStopped:
                    YVRPlugin.Instance.SetBlockInteractionData(false);
                    break;
            }
        }

    }
}