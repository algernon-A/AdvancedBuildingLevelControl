namespace ABLC
{
    using System;
    using AlgernonCommons;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Static class to manage the ABLC district panel.
    /// </summary>
    internal static class DistrictPanelManager
    {
        // Instance references.
        private static GameObject uiGameObject;
        private static ABLCDistrictPanel _panel;
        internal static ABLCDistrictPanel Panel => _panel;


        /// <summary>
        /// Adds event handler to show/hide district panel as appropriate (in line with DistrictWorldInfoPanel).
        /// </summary>
        internal static void Hook()
        {

            UIComponent districtInfoPanel = UIView.library.Get<DistrictWorldInfoPanel>(typeof(DistrictWorldInfoPanel).Name)?.component;
            if (districtInfoPanel == null)
            {
                Logging.Error("couldn't hook district info panel");
            }
            else
            {
                districtInfoPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    if (isVisible)
                    {
                        Create(districtInfoPanel.transform);
                    }
                    else
                    {
                        Close();
                    }
                };
            }
        }


        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        internal static void Create(Transform parentTransform)
        {
            try
            {
                // If no instance already set, create one.
                if (uiGameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    uiGameObject = new GameObject("ABLCDistrictPanel");
                    uiGameObject.transform.parent = parentTransform;

                    _panel = uiGameObject.AddComponent<ABLCDistrictPanel>();

                    // Set up and show panel.
                    Panel.transform.parent = parentTransform;
                    Panel.Setup();
                    Panel.Show();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception creating ABLCDistrictPanel");
            }
        }


        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        internal static void Close()
        {
            GameObject.Destroy(_panel);
            GameObject.Destroy(uiGameObject);
        }
    }
}