using UnityEngine;
using ColossalFramework.UI;
using HarmonyLib;

namespace ABLC
{
    public class UIUtils
    {
        // Original utils code by SamsamTS, altered slightly by algernon.
        // SamsamTS's comments:
        // Figuring all this was a pain (no documentation whatsoever)
        // So if your are using it for your mod consider thanking me (SamsamTS)
        // Extended Public Transport UI's code helped me a lot so thanks a lot AcidFire
        //
        // So, thank you, SamsamTS!


        /// <summary>
        /// Creates a dropdown menu.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Text label</param>
        /// <param name="xPos">Relative x position (default 20)</param>
        /// <param name="yPos">Relative y position (default 0)</param>
        /// <returns></returns>
        public static UIDropDown CreateDropDown(UIComponent parent, string text, float xPos = 20f, float yPos = 0f )
        {
            // Constants.
            const float Width = 60f;
            const float Height = 25f;
            const int ItemHeight = 20;


            // Add container at specified position.
            UIPanel container = parent.AddUIComponent<UIPanel>();
            container.height = 25;
            container.relativePosition = new Vector3(xPos, yPos);

            // Add label.
            UILabel label = container.AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.text = text;
            label.relativePosition = new Vector3(15f, 6f);

            // Create dropdown menu.
            UIDropDown dropDown = container.AddUIComponent<UIDropDown>();
            dropDown.listBackground = "GenericPanelLight";
            dropDown.itemHover = "ListItemHover";
            dropDown.itemHighlight = "ListItemHighlight";
            dropDown.normalBgSprite = "ButtonMenu";
            dropDown.disabledBgSprite = "ButtonMenuDisabled";
            dropDown.hoveredBgSprite = "ButtonMenuHovered";
            dropDown.focusedBgSprite = "ButtonMenu";
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            dropDown.popupColor = new Color32(45, 52, 61, 255);
            dropDown.popupTextColor = new Color32(170, 170, 170, 255);
            dropDown.zOrder = 1;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.textFieldPadding = new RectOffset(8, 0, 8, 0);
            dropDown.itemPadding = new RectOffset(14, 0, 8, 0);

            // Dropdown relative position.
            dropDown.relativePosition = new Vector3(130f, 0f);

            // Dropdown size parameters.
            dropDown.size = new Vector2(Width, Height);
            dropDown.listWidth = (int)Width;
            dropDown.listHeight = 500;
            dropDown.itemHeight = ItemHeight;
            dropDown.textScale = 0.7f;

            // Create dropdown button.
            UIButton button = dropDown.AddUIComponent<UIButton>();
            dropDown.triggerButton = button;
            button.size = dropDown.size;
            button.text = "";
            button.relativePosition = new Vector3(0f, 0f);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.normalFgSprite = "IconDownArrow";
            button.hoveredFgSprite = "IconDownArrowHovered";
            button.pressedFgSprite = "IconDownArrowPressed";
            button.focusedFgSprite = "IconDownArrowFocused";
            button.disabledFgSprite = "IconDownArrowDisabled";
            button.spritePadding = new RectOffset(3, 3, 3, 3);
            button.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.zOrder = 0;

            return dropDown;
        }


        /// <summary>
        /// Adds a checkbox with a descriptive text label immediately to the right.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Descriptive label text</param>
        /// <param name="xPos">Relative x position (default 0)</param>
        /// <param name="yPos">Relative y position (default 0)</param>
        /// <returns>New UI checkbox with attached labels</returns>
        public static UICheckBox AddCheckBox(UIComponent parent, string text, float xPos = 20f, float yPos = 0f)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();

            // Size and position.
            checkBox.height = 20f;
            checkBox.clipChildren = true;
            checkBox.relativePosition = new Vector3(xPos, yPos);

            // Sprites.
            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.spriteName = "check-unchecked";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = Vector3.zero;

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).spriteName = "check-checked";
            checkBox.checkedBoxObject.size = new Vector2(16f, 16f);
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            // Label.
            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.relativePosition = new Vector3(21f, 2f);
            checkBox.label.height = 20f;
            checkBox.label.textScale = 0.8f;
            checkBox.label.autoSize = true;
            checkBox.label.text = text;

            // Dynamic width to accomodate label.
            checkBox.width = checkBox.label.width + 21f;

            return checkBox;
        }


        /// <summary>
        /// Creates a pushbutton.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Text label</param>
        /// <param name="width">Width (default 100)</param>
        /// <param name="xPos">Relative x position (default 0)</param>
        /// <param name="yPos">Relative y position (default 0)</param>
        /// <returns></returns>
        public static UIButton CreateButton(UIComponent parent, string text, float width = 100f, float xPos = 0f, float yPos = 0f)
        {
            // Constants.
            const float Height = 30f;


            // Create button.
            UIButton button = parent.AddUIComponent<UIButton>();
            button.normalBgSprite = "ButtonMenu";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.disabledTextColor = new Color32(128, 128, 128, 255);
            button.canFocus = false;

            // Button size parameters.
            button.relativePosition = new Vector3(xPos, yPos);
            button.size = new Vector2(width, Height);
            button.textScale = 0.9f;

            // Label.
            button.text = text;

            return button;
        }
    }
}