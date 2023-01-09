// <copyright file="DistrictPanelManager.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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
        private static GameObject s_gameObject;
        private static DistrictPanel s_panel;

        /// <summary>
        /// Gets the active instance.
        /// </summary>
        internal static DistrictPanel Panel => s_panel;

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
                districtInfoPanel.eventVisibilityChanged += (c, isVisible) =>
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
        /// <param name="parentTransform">Parent transform.</param>
        internal static void Create(Transform parentTransform)
        {
            try
            {
                // If no instance already set, create one.
                if (s_gameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    s_gameObject = new GameObject("ABLCDistrictPanel");
                    s_gameObject.transform.parent = parentTransform;

                    s_panel = s_gameObject.AddComponent<DistrictPanel>();

                    // Set up and show panel.
                    Panel.transform.parent = parentTransform;

                    // Set position according to setting.
                    if (ModSettings.OnRight)
                    {
                        // On right of info panel.
                        s_panel.relativePosition = new Vector2(s_panel.parent.width + 10f, 0f);
                    }
                    else
                    {
                        // On left of info panel.
                        s_panel.relativePosition = new Vector2(-(s_panel.width + 10f), 0f);
                    }

                    Panel.Show();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception creating DistrictPanel");
            }
        }

        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        internal static void Close()
        {
            GameObject.Destroy(s_panel);
            GameObject.Destroy(s_gameObject);

            s_panel = null;
            s_gameObject = null;
        }
    }
}