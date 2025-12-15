using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfTouchKeyboard.Core
{
    /// <summary>
    /// IME（输入法编辑器）辅助类，用于控制 Windows 系统输入法
    /// </summary>
    public static class ImeHelper
    {
        // 输入法相关的 Windows API
        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        private static extern bool ActivateKeyboardLayout(IntPtr hkl, uint Flags);

        [DllImport("user32.dll")]
        private static extern int GetKeyboardLayoutList(int nBuff, IntPtr[] lpList);

        [DllImport("user32.dll")]
        private static extern uint GetKeyboardLayoutName(StringBuilder pwszKLID);

        private const uint KLF_ACTIVATE = 1;
        private const uint KLF_REORDER = 8;
        
        // 输入法布局ID常量（用于识别）
        private const string CHINESE_IME_LAYOUT = "00000804"; // 简体中文输入法布局ID (zh-CN)
        private const string ENGLISH_IME_LAYOUT = "00000409"; // 英文输入法布局ID (en-US)
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;

        /// <summary>
        /// 获取系统中已安装的所有输入法布局
        /// </summary>
        private static List<IntPtr> GetInstalledKeyboardLayouts()
        {
            var layouts = new List<IntPtr>();
            try
            {
                int count = GetKeyboardLayoutList(0, Array.Empty<IntPtr>());
                if (count > 0)
                {
                    var layoutList = new IntPtr[count];
                    GetKeyboardLayoutList(count, layoutList);
                    layouts.AddRange(layoutList);
                }
            }
            catch
            {
                // 如果获取失败，忽略异常
            }
            return layouts;
        }

        /// <summary>
        /// 从布局句柄获取布局ID字符串
        /// </summary>
        private static string GetLayoutIdFromHandle(IntPtr hkl)
        {
            try
            {
                // 布局句柄的低4位是布局ID
                uint layoutId = (uint)hkl.ToInt64() & 0xFFFF;
                return layoutId.ToString("X8");
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 判断布局ID是否是中文输入法（兼容第三方输入法，如搜狗）
        /// </summary>
        private static bool IsChineseLayout(string layoutId)
        {
            if (string.IsNullOrEmpty(layoutId) || layoutId.Length < 4)
                return false;

            // 中文输入法的布局ID通常是 0804 (简体) 或 0404 (繁体)
            // 第三方输入法如搜狗也会挂载到这些布局ID上
            string suffix = layoutId.Substring(layoutId.Length - 4);
            return suffix == "0804" || suffix == "0404" || suffix == "1004";
        }

        /// <summary>
        /// 判断布局ID是否是英文输入法
        /// </summary>
        private static bool IsEnglishLayout(string layoutId)
        {
            if (string.IsNullOrEmpty(layoutId) || layoutId.Length < 4)
                return false;

            string suffix = layoutId.Substring(layoutId.Length - 4);
            return suffix == "0409" || suffix == "0809" || suffix == "0c09";
        }

        /// <summary>
        /// 切换到中文输入法（系统级别，兼容第三方输入法）
        /// </summary>
        public static void SwitchToChineseInput()
        {
            try
            {
                // 先尝试使用标准的简体中文布局ID
                IntPtr hkl = LoadKeyboardLayout(CHINESE_IME_LAYOUT, KLF_ACTIVATE);
                if (hkl != IntPtr.Zero)
                {
                    ActivateKeyboardLayout(hkl, KLF_ACTIVATE);
                    return;
                }

                // 如果失败，尝试从已安装的输入法中找到中文输入法
                var layouts = GetInstalledKeyboardLayouts();
                foreach (var layout in layouts)
                {
                    string layoutId = GetLayoutIdFromHandle(layout);
                    if (IsChineseLayout(layoutId))
                    {
                        ActivateKeyboardLayout(layout, KLF_ACTIVATE);
                        return;
                    }
                }
            }
            catch
            {
                // 如果切换失败，忽略异常
            }
        }

        /// <summary>
        /// 切换到英文输入法（系统级别）
        /// </summary>
        public static void SwitchToEnglishInput()
        {
            try
            {
                // 先尝试使用标准的英文布局ID
                IntPtr hkl = LoadKeyboardLayout(ENGLISH_IME_LAYOUT, KLF_ACTIVATE);
                if (hkl != IntPtr.Zero)
                {
                    ActivateKeyboardLayout(hkl, KLF_ACTIVATE);
                    return;
                }

                // 如果失败，尝试从已安装的输入法中找到英文输入法
                var layouts = GetInstalledKeyboardLayouts();
                foreach (var layout in layouts)
                {
                    string layoutId = GetLayoutIdFromHandle(layout);
                    if (IsEnglishLayout(layoutId))
                    {
                        ActivateKeyboardLayout(layout, KLF_ACTIVATE);
                        return;
                    }
                }
            }
            catch
            {
                // 如果切换失败，忽略异常
            }
        }

        /// <summary>
        /// 获取当前激活的输入法布局ID
        /// </summary>
        public static string GetCurrentInputLayout()
        {
            try
            {
                var sb = new StringBuilder(9);
                GetKeyboardLayoutName(sb);
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 检查当前是否是中文输入法（兼容第三方输入法）
        /// </summary>
        public static bool IsChineseInputActive()
        {
            var currentLayout = GetCurrentInputLayout();
            return IsChineseLayout(currentLayout);
        }

        /// <summary>
        /// 智能切换输入法：如果当前是中文则切换到英文，如果是英文则切换到中文
        /// 这样可以兼容所有输入法，包括第三方输入法
        /// </summary>
        public static void ToggleInputMethod()
        {
            if (IsChineseInputActive())
            {
                SwitchToEnglishInput();
            }
            else
            {
                SwitchToChineseInput();
            }
        }

        /// <summary>
        /// 为目标控件启用中文输入法
        /// </summary>
        public static void EnableChineseIME(FrameworkElement element)
        {
            if (element == null)
                return;

            try
            {
                // 先切换系统输入法到中文
                SwitchToChineseInput();
                
                // 确保控件获得焦点，以便 IME 激活
                if (element is Control control && control.Focusable)
                {
                    if (!control.IsFocused)
                    {
                        control.Focus();
                        // 给一点时间让焦点设置完成
                        System.Threading.Thread.Sleep(30);
                    }
                }
                
                // 使用 WPF 的 InputMethod API 启用 IME
                InputMethod.SetIsInputMethodEnabled(element, true);
                InputMethod.SetPreferredImeState(element, InputMethodState.On);
                
                // 再次确保焦点
                if (element is Control ctrl && ctrl.Focusable && !ctrl.IsFocused)
                {
                    ctrl.Focus();
                }
            }
            catch
            {
                // 如果设置失败，忽略异常
            }
        }

        /// <summary>
        /// 禁用中文输入法，使用英文输入
        /// </summary>
        public static void DisableChineseIME(FrameworkElement element)
        {
            if (element == null)
                return;

            try
            {
                // 先切换系统输入法到英文
                SwitchToEnglishInput();
                
                InputMethod.SetPreferredImeState(element, InputMethodState.Off);
                InputMethod.SetIsInputMethodEnabled(element, false);
            }
            catch
            {
                // 如果设置失败，忽略异常
            }
        }

        /// <summary>
        /// 将字符转换为虚拟键码
        /// </summary>
        private static ushort CharToVirtualKey(char ch)
        {
            // 字母键
            if (ch >= 'a' && ch <= 'z')
                return (ushort)(0x41 + (ch - 'a')); // VK_A = 0x41
            if (ch >= 'A' && ch <= 'Z')
                return (ushort)(0x41 + (ch - 'A'));

            // 数字键
            if (ch >= '0' && ch <= '9')
                return (ushort)(0x30 + (ch - '0')); // VK_0 = 0x30

            return 0;
        }

        /// <summary>
        /// 发送字符输入，使用虚拟键码让系统 IME 处理
        /// </summary>
        public static void SendChar(char ch, bool shiftPressed = false)
        {
            var vk = CharToVirtualKey(ch);
            if (vk == 0)
                return;

            var inputs = new List<INPUT>();

            // 按下 Shift（如果需要）
            if (shiftPressed)
            {
                inputs.Add(new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0x10, // VK_SHIFT
                            wScan = 0,
                            dwFlags = 0,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                });
            }

            // 按下键
            inputs.Add(new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = 0,
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            });

            // 释放键
            inputs.Add(new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            });

            // 释放 Shift（如果需要）
            if (shiftPressed)
            {
                inputs.Add(new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0x10,
                            wScan = 0,
                            dwFlags = KEYEVENTF_KEYUP,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                });
            }

            SendInput((uint)inputs.Count, inputs.ToArray(), INPUT.Size);
        }

        /// <summary>
        /// 发送虚拟键码
        /// </summary>
        public static void SendVirtualKey(ushort virtualKey, bool keyUp = false)
        {
            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        wScan = 0,
                        dwFlags = keyUp ? KEYEVENTF_KEYUP : 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new INPUT[] { input }, INPUT.Size);
        }
    }
}

