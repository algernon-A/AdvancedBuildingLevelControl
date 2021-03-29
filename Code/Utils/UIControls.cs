using UnityEngine;
using ColossalFramework.UI;


namespace ABLC
{
    /// <summary>
    /// Static utilities class for creating UI controls.
    /// </summary>
    public static class UIControls
    {
        /// <summary>
        /// Adds a simple pushbutton.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="posX">Relative X postion</param>
        /// <param name="posY">Relative Y position</param>
        /// <param name="text">Button text</param>
        /// <param name="width">Button width (default 200)</param>
        /// <param name="height">Button height (default 30)</param>
        /// <param name="scale">Text scale (default 0.9)</param>
        /// <param name="vertPad">Vertical text padding within button (default 4)</param>
        /// <param name="tooltip">Tooltip, if any</param>
        /// <returns>New pushbutton</returns>
        public static UIButton AddButton(UIComponent parent, float posX, float posY, string text, float width = 200f, float height = 30f, float scale = 0.9f, int vertPad = 4, string tooltip = null)
        {
            UIButton button = parent.AddUIComponent<UIButton>();

            // Size and position.
            button.size = new Vector2(width, height);
            button.relativePosition = new Vector2(posX, posY);

            // Appearance.
            button.textScale = scale;
            button.normalBgSprite = "ButtonMenu";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.disabledTextColor = new Color32(128, 128, 128, 255);
            button.canFocus = false;

            // Add tooltip.
            if (tooltip != null)
            {
                button.tooltip = tooltip;
            }

            // Text.
            button.textScale = scale;
            button.textPadding = new RectOffset(0, 0, vertPad, 0);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            button.text = text;

            return button;
        }


        /// <summary>
        /// Adds a checkbox with a descriptive text label immediately to the right.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="xPos">Relative x position</param>
        /// <param name="yPos">Relative y position</param>
        /// <param name="text">Descriptive label text</param>
        /// <param name="textScale">Text scale of label (default 0.8)</param>
        /// <param name="size">Checkbox size (default 16f)</param>
        /// <param name="tooltip">Tooltip, if any</param>
        /// <returns>New UI checkbox with attached labels</returns>
        public static UICheckBox LabelledCheckBox(UIComponent parent, float xPos, float yPos, string text, float size = 16f, float textScale = 0.8f, string tooltip = null)
        {
            // Create base checkbox.
            UICheckBox checkBox = AddCheckBox(parent, xPos, yPos, size, tooltip);

            // Label.
            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.verticalAlignment = UIVerticalAlignment.Middle;
            checkBox.label.textScale = textScale;
            checkBox.label.autoSize = true;
            checkBox.label.text = text;

            // Dynamic width to accomodate label.
            checkBox.width = checkBox.label.width + 21f;
            checkBox.label.relativePosition = new Vector2(21f, ((checkBox.height - checkBox.label.height) / 2f) + 1f);

            return checkBox;
        }


        /// <summary>
        /// Adds a checkbox without a label.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="xPos">Relative x position</param>
        /// <param name="yPos">Relative y position</param>
        /// <param name="size">Checkbox size (default 16f)</param>
        /// <param name="tooltip">Tooltip, if any</param>
        /// <returns>New UI checkbox *without* attached labels</returns>
        public static UICheckBox AddCheckBox(UIComponent parent, float xPos, float yPos, float size = 16f, string tooltip = null)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();

            // Size and position.
            checkBox.width = size;
            checkBox.height = size;
            checkBox.clipChildren = false;
            checkBox.relativePosition = new Vector3(xPos, yPos);

            // Sprites.
            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.spriteName = "check-unchecked";
            sprite.size = new Vector2(size, size);
            sprite.relativePosition = Vector3.zero;

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).spriteName = "check-checked";
            checkBox.checkedBoxObject.size = new Vector2(size, size);
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            // Add tooltip.
            if (tooltip != null)
            {
                checkBox.tooltip = tooltip;
            }

            return checkBox;
        }


        /// <summary>
        /// Creates a dropdown menu with an attached text label.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="xPos">Relative x position</param>
        /// <param name="yPos">Relative y position</param>
        /// <param name="text">Text label</param>
        /// <param name="width">Dropdown menu width, excluding label (default 220f)</param>
        /// <param name="itemTextScale">Text scaling (default 0.7f)</param>
        /// <param name="height">Dropdown button height (default 25f)</param>
        /// <param name="itemHeight">Dropdown menu item height (default 20)</param>
        /// <param name="itemVertPadding">Dropdown menu item vertical text padding (default 8)</param>
        /// <param name="accomodateLabel">True (default) to move menu to accomodate text label width, false otherwise</param>
        /// <param name="tooltip">Tooltip, if any</param>
        /// <returns>New dropdown menu with an attached text label and enclosing panel</returns>
        public static UIDropDown AddLabelledDropDown(UIComponent parent, float xPos, float yPos, string text, float width = 220f, float height = 25f, float itemTextScale = 0.7f, int itemHeight = 20, int itemVertPadding = 8, bool accomodateLabel = true, string tooltip = null)
        {
            // Create dropdown.
            UIDropDown dropDown = AddDropDown(parent, xPos, yPos, width, height, itemTextScale, itemHeight, itemVertPadding, tooltip);

            // Add label.
            UILabel label = dropDown.AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.text = text;

            // Get width and position.
            float labelWidth = label.width + 10f;

            label.relativePosition = new Vector2(-labelWidth, (height - label.height) / 2f);

            // Move dropdown to accomodate label if that setting is set.
            if (accomodateLabel)
            {
                dropDown.relativePosition += new Vector3(labelWidth, 0f);
            }

            return dropDown;
        }


        /// <summary>
        /// Creates a dropdown menu without text label or enclosing panel.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="xPos">Relative x position (default 20)</param>
        /// <param name="yPos">Relative y position (default 0)</param>
        /// <param name="width">Dropdown menu width, excluding label (default 220f)</param>
        /// <param name="height">Dropdown button height (default 25f)</param>
        /// <param name="itemTextScale">Text scaling (default 0.7f)</param>
        /// <param name="itemHeight">Dropdown menu item height (default 20)</param>
        /// <param name="itemVertPadding">Dropdown menu item vertical text padding (default 8)</param>
        /// <param name="tooltip">Tooltip, if any</param>
        /// <returns>New dropdown menu *without* an attached text label or enclosing panel</returns>
        public static UIDropDown AddDropDown(UIComponent parent, float xPos, float yPos, float width = 220f, float height = 25f, float itemTextScale = 0.7f, int itemHeight = 20, int itemVertPadding = 8, string tooltip = null)
        {
            // Create dropdown menu.
            UIDropDown dropDown = parent.AddUIComponent<UIDropDown>();
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
            dropDown.textFieldPadding = new RectOffset(8, 0, itemVertPadding, 0);
            dropDown.itemPadding = new RectOffset(14, 0, itemVertPadding, 0);

            dropDown.relativePosition = new Vector3(xPos, yPos);

            // Dropdown size parameters.
            dropDown.size = new Vector2(width, height);
            dropDown.listWidth = (int)width;
            dropDown.listHeight = 500;
            dropDown.itemHeight = itemHeight;
            dropDown.textScale = itemTextScale;

            // Create dropdown button.
            UIButton button = dropDown.AddUIComponent<UIButton>();
            dropDown.triggerButton = button;
            button.size = dropDown.size;
            button.text = "";
            button.relativePosition = new Vector2(0f, 0f);
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

            // Add tooltip.
            if (tooltip != null)
            {
                dropDown.tooltip = tooltip;
            }

            return dropDown;
        }


        /// <summary>
        /// Creates a vertical scrollbar
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="scrollPanel">Panel to scroll</param>
        /// <returns>New vertical scrollbar linked to the specified scrollable panel, immediately to the right</returns>
        public static UIScrollbar AddScrollbar(UIComponent parent, UIScrollablePanel scrollPanel)
        {
            // Basic setup.
            UIScrollbar newScrollbar = parent.AddUIComponent<UIScrollbar>();
            newScrollbar.orientation = UIOrientation.Vertical;
            newScrollbar.pivot = UIPivotPoint.TopLeft;
            newScrollbar.minValue = 0;
            newScrollbar.value = 0;
            newScrollbar.incrementAmount = 50f;
            newScrollbar.autoHide = true;

            // Location and size.
            newScrollbar.width = 10f;
            newScrollbar.relativePosition = new Vector2(scrollPanel.relativePosition.x + scrollPanel.width, scrollPanel.relativePosition.y);
            newScrollbar.height = scrollPanel.height;

            // Tracking sprite.
            UISlicedSprite trackSprite = newScrollbar.AddUIComponent<UISlicedSprite>();
            trackSprite.relativePosition = Vector2.zero;
            trackSprite.autoSize = true;
            trackSprite.anchor = UIAnchorStyle.All;
            trackSprite.size = trackSprite.parent.size;
            trackSprite.fillDirection = UIFillDirection.Vertical;
            trackSprite.spriteName = "ScrollbarTrack";
            newScrollbar.trackObject = trackSprite;

            // Thumb sprite.
            UISlicedSprite thumbSprite = trackSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = thumbSprite.parent.width;
            thumbSprite.spriteName = "ScrollbarThumb";
            newScrollbar.thumbObject = thumbSprite;

            // Event handler to handle resize of scroll panel.
            scrollPanel.eventSizeChanged += (component, newSize) =>
            {
                newScrollbar.relativePosition = new Vector2(scrollPanel.relativePosition.x + scrollPanel.width, scrollPanel.relativePosition.y);
                newScrollbar.height = scrollPanel.height;
            };

            // Attach to scroll panel.
            scrollPanel.verticalScrollbar = newScrollbar;

            return newScrollbar;
        }
    }
}