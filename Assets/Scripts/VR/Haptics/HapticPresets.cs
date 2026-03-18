using UnityEngine;

namespace TripMeta.VR.Haptics
{
    /// <summary>
    /// 触觉预设库
    /// 预定义的触觉模式用于常见场景
    /// </summary>
    public static class HapticPresets
    {
        #region 行走触觉

        /// <summary>
        /// 左脚行走
        /// </summary>
        public static HapticPattern FootstepLeft => new HapticPattern
        {
            type = HapticType.Click,
            amplitude = 0.4f,
            frequency = 100f,
            duration = 0.05f,
            fadeIn = 0f,
            fadeOut = 0.02f
        };

        /// <summary>
        /// 右脚行走
        /// </summary>
        public static HapticPattern FootstepRight => new HapticPattern
        {
            type = HapticType.Click,
            amplitude = 0.4f,
            frequency = 100f,
            duration = 0.05f,
            fadeIn = 0f,
            fadeOut = 0.02f
        };

        /// <summary>
        /// 奔跑 - 左脚
        /// </summary>
        public static HapticPattern RunLeft => new HapticPattern
        {
            type = HapticType.Click,
            amplitude = 0.7f,
            frequency = 150f,
            duration = 0.08f,
            fadeIn = 0f,
            fadeOut = 0.03f
        };

        /// <summary>
        /// 奔跑 - 右脚
        /// </summary>
        public static HapticPattern RunRight => new HapticPattern
        {
            type = HapticType.Click,
            amplitude = 0.7f,
            frequency = 150f,
            duration = 0.08f,
            fadeIn = 0f,
            fadeOut = 0.03f
        };

        /// <summary>
        /// 跳跃
        /// </summary>
        public static HapticPattern Jump => new HapticPattern
        {
            type = HapticType.Rumble,
            amplitude = 0.5f,
            frequency = 80f,
            duration = 0.3f,
            fadeIn = 0.05f,
            fadeOut = 0.15f
        };

        /// <summary>
        /// 落地
        /// </summary>
        public static HapticPattern Landing => new HapticPattern
        {
            type = HapticType.Rumble,
            amplitude = 0.8f,
            frequency = 120f,
            duration = 0.2f,
            fadeIn = 0f,
            fadeOut = 0.1f
        };

        #endregion

        #region 互动触觉

        /// <summary>
        /// 轻触
        /// </summary>
        public static HapticPattern Touch => new HapticPattern
        {
            type = HapticType.Buzz,
            amplitude = 0.2f,
            frequency = 80f,
            duration = 0.03f
        };

        /// <summary>
        /// 抓取物体
        /// </summary>
        public static HapticPattern Grab => new HapticPattern
        {
            type = HapticType.Buzz,
            amplitude = 0.3f,
            frequency = 100f,
            duration = 0.1f,
            fadeIn = 0.02f,
            fadeOut = 0.03f
        };

        /// <summary>
        /// 释放物体
        /// </summary>
        public static HapticPattern Release => new HapticPattern
        {
            type = HapticType.Buzz,
            amplitude = 0.15f,
            frequency = 60f,
            duration = 0.05f
        };

        /// <summary>
        /// 按钮点击
        /// </summary>
        public static HapticPattern ButtonClick => new HapticPattern
        {
            type = HapticType.Click,
            amplitude = 0.5f,
            frequency = 200f,
            duration = 0.02f
        };

        /// <summary>
        /// 开关切换
        /// </summary>
        public static HapticPattern Toggle => new HapticPattern
        {
            type = HapticType.Click,
            amplitude = 0.35f,
            frequency = 150f,
            duration = 0.03f
        };

        #endregion

        #region 战斗触觉

        /// <summary>
        /// 轻微打击
        /// </summary>
        public static HapticPattern HitLight => new HapticPattern
        {
            type = HapticType.Click,
            amplitude = 0.4f,
            frequency = 180f,
            duration = 0.05f
        };

        /// <summary>
        /// 中等打击
        /// </summary>
        public static HapticPattern HitMedium => new HapticPattern
        {
            type = HapticType.Click,
            amplitude = 0.7f,
            frequency = 220f,
            duration = 0.08f
        };

        /// <summary>
        /// 重击
        /// </summary>
        public static HapticPattern HitHeavy => new HapticPattern
        {
            type = HapticType.Rumble,
            amplitude = 1.0f,
            frequency = 150f,
            duration = 0.3f,
            fadeIn = 0f,
            fadeOut = 0.15f
        };

        /// <summary>
        /// 射击 - 手枪
        /// </summary>
        public static HapticPattern ShootPistol => new HapticPattern
        {
            type = HapticType.Click,
            amplitude = 0.6f,
            frequency = 300f,
            duration = 0.05f
        };

        /// <summary>
        /// 射击 - 步枪
        /// </summary>
        public static HapticPattern ShootRifle => new HapticPattern
        {
            type = HapticType.Click,
            amplitude = 0.8f,
            frequency = 250f,
            duration = 0.08f
        };

        /// <summary>
        /// 射击 - 霰弹枪
        /// </summary>
        public static HapticPattern ShootShotgun => new HapticPattern
        {
            type = HapticType.Rumble,
            amplitude = 0.9f,
            frequency = 200f,
            duration = 0.15f,
            fadeIn = 0f,
            fadeOut = 0.05f
        };

        /// <summary>
        /// 爆炸
        /// </summary>
        public static HapticPattern Explosion => new HapticPattern
        {
            type = HapticType.Rumble,
            amplitude = 1.0f,
            frequency = 100f,
            duration = 0.5f,
            fadeIn = 0f,
            fadeOut = 0.3f
        };

        /// <summary>
        /// 盾牌格挡
        /// </summary>
        public static HapticPattern ShieldBlock => new HapticPattern
        {
            type = HapticType.Buzz,
            amplitude = 0.8f,
            frequency = 180f,
            duration = 0.15f
        };

        #endregion

        #region 环境触觉

        /// <summary>
        /// 微风
        /// </summary>
        public static HapticPattern Wind => new HapticPattern
        {
            type = HapticType.Wave,
            amplitude = 0.2f,
            frequency = 40f,
            duration = 2f,
            fadeIn = 0.5f,
            fadeOut = 0.5f
        };

        /// <summary>
        /// 雨水
        /// </summary>
        public static HapticPattern Rain => new HapticPattern
        {
            type = HapticType.Pulse,
            amplitude = 0.25f,
            frequency = 60f,
            duration = 1f
        };

        /// <summary>
        /// 心跳
        /// </summary>
        public static HapticPattern Heartbeat => new HapticPattern
        {
            type = HapticType.Pulse,
            amplitude = 0.3f,
            frequency = 80f,
            duration = 0.8f
        };

        /// <summary>
        /// 引擎震动
        /// </summary>
        public static HapticPattern Engine => new HapticPattern
        {
            type = HapticType.Continuous,
            amplitude = 0.4f,
            frequency = 120f,
            duration = 3f,
            fadeIn = 0.3f,
            fadeOut = 0.3f
        };

        /// <summary>
        /// 水面漂浮
        /// </summary>
        public static HapticPattern Water => new HapticPattern
        {
            type = HapticType.Wave,
            amplitude = 0.3f,
            frequency = 50f,
            duration = 2f,
            fadeIn = 0.3f,
            fadeOut = 0.3f
        };

        #endregion

        #region UI/反馈触觉

        /// <summary>
        /// 成功提示
        /// </summary>
        public static HapticPattern Success => new HapticPattern
        {
            type = HapticType.Buzz,
            amplitude = 0.4f,
            frequency = 150f,
            duration = 0.1f
        };

        /// <summary>
        /// 错误提示
        /// </summary>
        public static HapticPattern Error => new HapticPattern
        {
            type = HapticType.Buzz,
            amplitude = 0.5f,
            frequency = 100f,
            duration = 0.2f
        };

        /// <summary>
        /// 警告提示
        /// </summary>
        public static HapticPattern Warning => new HapticPattern
        {
            type = HapticType.Pulse,
            amplitude = 0.4f,
            frequency = 120f,
            duration = 0.3f
        };

        /// <summary>
        /// 通知提示
        /// </summary>
        public static HapticPattern Notification => new HapticPattern
        {
            type = HapticType.Buzz,
            amplitude = 0.3f,
            frequency = 140f,
            duration = 0.08f
        };

        /// <summary>
        /// 振动提醒
        /// </summary>
        public static HapticPattern Alert => new HapticPattern
        {
            type = HapticType.Pulse,
            amplitude = 0.6f,
            frequency = 200f,
            duration = 0.5f
        };

        #endregion

        /// <summary>
        /// 创建自定义触觉模式
        /// </summary>
        public static HapticPattern CreateCustom(HapticType type, float amplitude, float duration, float frequency = 100f)
        {
            return new HapticPattern
            {
                type = type,
                amplitude = Mathf.Clamp01(amplitude),
                frequency = frequency,
                duration = duration,
                fadeIn = 0.05f,
                fadeOut = 0.05f
            };
        }

        /// <summary>
        /// 根据强度缩放触觉
        /// </summary>
        public static HapticPattern ScaleIntensity(HapticPattern original, float scale)
        {
            var scaled = original;
            scaled.amplitude = Mathf.Clamp01(original.amplitude * scale);
            return scaled;
        }
    }
}
