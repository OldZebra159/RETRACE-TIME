using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ToolTip : MonoBehaviour
{
    /// <summary>
    /// 提示框所在的逻辑区域
    /// </summary>
    private enum ToolTipArea
    {
        /// <summary>
        /// 默认：根据鼠标相对屏幕中心的位置自动左右 / 上下翻转
        /// </summary>
        Default,

        /// <summary>
        /// Stat：提示框在屏幕 X 轴 50%（垂直中线）上，
        ///       X 固定在 50%，Y 跟随鼠标位置变化
        /// </summary>
        Stat,

        /// <summary>
        /// Item：提示框限制在屏幕右下象限，
        ///       也就是 X、Y 都在 50% ~ 100% 的范围内
        /// </summary>
        Item
    }

    [Header("位置设置")]
    [SerializeField] private ToolTipArea area = ToolTipArea.Default;

    [Header("默认模式偏移（仅 Default 使用）")]
    [SerializeField] private float xOffset = 150;
    [SerializeField] private float yOffset = 150;

    /// <summary>
    /// 调整提示框位置
    /// </summary>
    public virtual void AdjustPositon()
    {
        // 当前鼠标在屏幕空间中的坐标（左下角为 (0,0)）
        Vector2 mousePosition = Input.mousePosition;

        // 计算后的目标位置
        Vector2 targetPos = mousePosition;

        switch (area)
        {
            case ToolTipArea.Stat:
                {
                    // Stat：X 固定在屏幕宽度的 50%，Y 跟随鼠标
                    float centerX = Screen.width * 0.5f;
                    targetPos = new Vector2(centerX, mousePosition.y);
                    break;
                }

            case ToolTipArea.Item:
                {
                    // Item：提示框出现在“鼠标左上方”
                    // 使用公共的 xOffset / yOffset，向左、向上偏移
                    float offsetX = Mathf.Abs(xOffset);
                    float offsetY = Mathf.Abs(yOffset);

                    targetPos = new Vector2(
                        mousePosition.x - offsetX,   // 向左
                        mousePosition.y + offsetY    // 向上
                    );

                    // 不再把 Y 强行夹在某条“界限线上”，
                    // 让下面一排物品的 tip 也能比上面几排更靠下，保持跟随鼠标的感觉
                    break;
                }

            default:
                {
                    // Default：根据鼠标在屏幕中心的左右 / 上下自动选择偏移方向
                    float xLimit = Screen.width * 0.5f;
                    float yLimit = Screen.height * 0.5f;

                    float newXOffset = mousePosition.x > xLimit ? -Mathf.Abs(xOffset) : Mathf.Abs(xOffset);
                    float newYOffset = mousePosition.y > yLimit ? -Mathf.Abs(yOffset) : Mathf.Abs(yOffset);

                    targetPos = new Vector2(mousePosition.x + newXOffset, mousePosition.y + newYOffset);
                    break;
                }
        }

        // 最后统一做一次屏幕边缘限制，避免超出屏幕
        float padding = 10f;
        float minClampX = padding;
        float maxClampX = Screen.width - padding;
        float minClampY = padding;
        float maxClampY = Screen.height - padding;

        targetPos.x = Mathf.Clamp(targetPos.x, minClampX, maxClampX);
        targetPos.y = Mathf.Clamp(targetPos.y, minClampY, maxClampY);

        transform.position = targetPos;
    }
}
