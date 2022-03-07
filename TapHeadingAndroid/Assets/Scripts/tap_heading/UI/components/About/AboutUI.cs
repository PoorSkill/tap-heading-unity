using System.Collections.Generic;
using tap_heading.Services.Google;
using UnityEngine;

namespace tap_heading.UI.components.About
{
    public class AboutUI : MonoBehaviour
    {
        [SerializeField] private GameObject aboutPanel;
        [SerializeField] private GameObject[] toHide;

        private List<GameObject> _hidden = new List<GameObject>();

        public void Open()
        {
            Social.ReportProgress(GPGSIds.AchievementThankYou, 0.0f, null);
            GooglePlayServicesManager.Instance.ThankYouAchievement();

            aboutPanel.SetActive(true);

            foreach (var o in toHide)
            {
                if (o.activeSelf)
                {
                    _hidden.Add(o);
                    o.SetActive(false);
                }
            }
        }

        public void Close()
        {
            aboutPanel.SetActive(false);
            foreach (var o in _hidden)
            {
                o.SetActive(true);
            }

            _hidden.Clear();
        }

        public bool IsOpen()
        {
            return aboutPanel.activeSelf;
        }
    }
}