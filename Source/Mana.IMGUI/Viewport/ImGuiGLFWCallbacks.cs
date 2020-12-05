using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector2 = System.Numerics.Vector2;

namespace Mana.IMGUI.Viewport
{
    public static unsafe class ImGuiGLFWCallbacks
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CreateWindowFunc(ImGuiViewportPtr ptr);
        private static CreateWindowFunc _createWindowDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DestroyWindowFunc(ImGuiViewportPtr ptr);
        private static DestroyWindowFunc _destroyWindowDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ShowWindowFunc(ImGuiViewportPtr ptr);
        private static ShowWindowFunc _showWindowDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetWindowPosFunc(ImGuiViewportPtr ptr, Vector2 pos);
        private static SetWindowPosFunc _setWindowPosDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetWindowPosFunc(out Vector2 vector2, ImGuiViewportPtr ptr);
        private static GetWindowPosFunc _getWindowPosDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetWindowSizeFunc(ImGuiViewportPtr ptr, Vector2 size);
        private static SetWindowSizeFunc _setWindowSizeDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Vector2 GetWindowSizeFunc(ImGuiViewportPtr ptr);
        private static GetWindowSizeFunc _getWindowSizeDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetWindowFocusFunc(ImGuiViewportPtr ptr);
        private static SetWindowFocusFunc _setWindowFocusDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte GetWindowFocusFunc(ImGuiViewportPtr ptr);
        private static GetWindowFocusFunc _getWindowFocusDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte GetWindowMinimizedFunc(ImGuiViewportPtr ptr);
        private static GetWindowMinimizedFunc _getWindowMinimizedDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetWindowAlphaFunc(ImGuiViewportPtr ptr, float alpha);
        private static SetWindowAlphaFunc _setWindowAlphaDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetWindowTitleFunc(ImGuiViewportPtr ptr, char* c);
        private static SetWindowTitleFunc _setWindowTitleDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void RenderWindowFunc(ImGuiViewportPtr ptr, void* _);
        private static RenderWindowFunc _renderWindowDelegate;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SwapBuffersFunc(ImGuiViewportPtr ptr, void* _);
        private static SwapBuffersFunc _swapBuffersDelegate;
        
        private static ImGuiPlatformMonitor[] _monitorData;
        private static GCHandle _monitorDataPin;

        private static ImGuiPlatformMonitorPtr[] _monitorPtrData;
        private static GCHandle _monitorPtrDataPin;

        private static ImVector<ImGuiPlatformMonitor> _monitorsPtrVector;
        private static GCHandle _monitorsPtrVectorPin;

        private static Dictionary<IntPtr, ViewportData> _data = new Dictionary<IntPtr, ViewportData>();

        public static void InitPlatformInterface()
        {
            ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
            
            var mainWindowHandle = GetMainWindowHandle();
            mainViewport.PlatformHandle = mainWindowHandle;

            var data = ViewportData.Default;
            data.Window = mainWindowHandle;
            IntPtr dataID = BindViewportData(data);
            mainViewport.PlatformUserData = dataID;
            
            var io = ImGui.GetPlatformIO();

            _createWindowDelegate = ImGui_ImplGlfw_CreateWindow;
            io.Platform_CreateWindow = Marshal.GetFunctionPointerForDelegate(_createWindowDelegate);
            
            _destroyWindowDelegate = ImGui_ImplGlfw_DestroyWindow;
            io.Platform_DestroyWindow = Marshal.GetFunctionPointerForDelegate(_destroyWindowDelegate);
            
            _showWindowDelegate = ImGui_ImplGlfw_ShowWindow;
            io.Platform_ShowWindow = Marshal.GetFunctionPointerForDelegate(_showWindowDelegate);
            
            _setWindowPosDelegate = ImGui_ImplGlfw_SetWindowPos;
            io.Platform_SetWindowPos = Marshal.GetFunctionPointerForDelegate(_setWindowPosDelegate);
            
            _getWindowPosDelegate = ImGui_ImplGlfw_GetWindowPos;
            io.Platform_GetWindowPos = Marshal.GetFunctionPointerForDelegate(_getWindowPosDelegate);
            
            _setWindowSizeDelegate = ImGui_ImplGlfw_SetWindowSize;
            io.Platform_SetWindowSize = Marshal.GetFunctionPointerForDelegate(_setWindowSizeDelegate);
            
            _getWindowSizeDelegate = ImGui_ImplGlfw_GetWindowSize;
            io.Platform_GetWindowSize = Marshal.GetFunctionPointerForDelegate(_getWindowSizeDelegate);
            
            _setWindowFocusDelegate = ImGui_ImplGlfw_SetWindowFocus;
            io.Platform_SetWindowFocus = Marshal.GetFunctionPointerForDelegate(_setWindowFocusDelegate);
            
            _getWindowFocusDelegate = ImGui_ImplGlfw_GetWindowFocus;
            io.Platform_GetWindowFocus = Marshal.GetFunctionPointerForDelegate(_getWindowFocusDelegate);
            
            _getWindowMinimizedDelegate = ImGui_ImplGlfw_GetWindowMinimized;
            io.Platform_GetWindowMinimized = Marshal.GetFunctionPointerForDelegate(_getWindowMinimizedDelegate);
            
            _setWindowAlphaDelegate = ImGui_ImplGlfw_SetWindowAlpha;
            io.Platform_SetWindowAlpha = Marshal.GetFunctionPointerForDelegate(_setWindowAlphaDelegate);
            
            _setWindowTitleDelegate = ImGui_ImplGlfw_SetWindowTitle;
            io.Platform_SetWindowTitle = Marshal.GetFunctionPointerForDelegate(_setWindowTitleDelegate);
            
            _renderWindowDelegate = ImGui_ImplGlfw_RenderWindow;
            io.Platform_RenderWindow = Marshal.GetFunctionPointerForDelegate(_renderWindowDelegate);
            
            _swapBuffersDelegate = ImGui_ImplGlfw_SwapBuffers;
            io.Platform_SwapBuffers = Marshal.GetFunctionPointerForDelegate(_swapBuffersDelegate);
             
            SetUpMonitorData();
        }

        private static int _viewportDataID = 1;
        private static IntPtr BindViewportData(ViewportData data)
        {
            var key = new IntPtr(_viewportDataID++);
            _data.Add(key, data);
            return key;
        }
        
        public static IntPtr GetMainWindowHandle()
        {
            return new IntPtr(ManaWindow.MainWindow.WindowPtr);
        }
        
        public static void ImGui_ImplGlfw_WindowCloseCallback(Window* window)
        {
            ImGuiViewport* viewport = ImGui.FindViewportByPlatformHandle(new IntPtr(window));
            if (viewport != null)
            {
                viewport->PlatformRequestClose = 1; // true
            }
        }
        
        // GLFW may dispatch window pos/size events after calling glfwSetWindowPos()/glfwSetWindowSize().
        // However: depending on the platform the callback may be invoked at different time:
        // - on Windows it appears to be called within the glfwSetWindowPos()/glfwSetWindowSize() call
        // - on Linux it is queued and invoked during glfwPollEvents()
        // Because the event doesn't always fire on glfwSetWindowXXX() we use a frame counter tag to only
        // ignore recent glfwSetWindowXXX() calls.
        public static void ImGui_ImplGlfw_WindowPosCallback(Window* window, int _0, int _1)
        {
            ImGuiViewport* viewport = ImGui.FindViewportByPlatformHandle(new IntPtr(window));
            if (viewport != null)
            {
                var data = GetViewportData(viewport);
                bool ignore_event = (ImGui.GetFrameCount() <= data.IgnoreWindowPosEventFrame + 1);
                //data->IgnoreWindowPosEventFrame = -1;
                if (ignore_event)
                    return;
                
                viewport->PlatformRequestMove = 1; // true
            }
        }
        
        public static void ImGui_ImplGlfw_WindowSizeCallback(Window* window, int _0, int _1)
        {
            ImGuiViewport* viewport = ImGui.FindViewportByPlatformHandle(new IntPtr(window));
            if (viewport != null)
            {
                var data = GetViewportData(viewport);
                bool ignore_event = (ImGui.GetFrameCount() <= data.IgnoreWindowSizeEventFrame + 1);
                //data->IgnoreWindowSizeEventFrame = -1;
                if (ignore_event)
                    return;
                
                viewport->PlatformRequestResize = 1; // true
            }
        }

        public static void ImGui_ImplGlfw_CreateWindow(ImGuiViewportPtr viewport)
        {
            // ImGuiViewportDataGlfw* data = CreateViewportData();
            // viewport->PlatformUserData = data;
            // GLFW 3.2 unfortunately always set focus on glfwCreateWindow() if GLFW_VISIBLE is set, regardless of GLFW_FOCUSED
            // With GLFW 3.3, the hint GLFW_FOCUS_ON_SHOW fixes this problem

            GLFW.WindowHint(WindowHintBool.Visible, true);
            GLFW.WindowHint(WindowHintBool.Focused, true);
            GLFW.WindowHint(WindowHintBool.FocusOnShow, true);
            GLFW.WindowHint(WindowHintBool.Decorated, !viewport.Flags.HasFlag(ImGuiViewportFlags.NoDecoration));
            GLFW.WindowHint(WindowHintBool.Floating, viewport.Flags.HasFlag(ImGuiViewportFlags.TopMost));
            
            Window* share_window = (Window*)GetMainWindowHandle().ToPointer();
            
            var window = GLFW.CreateWindow((int)viewport.Size.X, (int)viewport.Size.Y, "No Title Yet", null, share_window);
            
            // data->Window = window;
            // data->WindowOwned = true;
            viewport.PlatformHandle = new IntPtr(window);
            
            GLFW.SetWindowPos(window, (int)viewport.Pos.X, (int)viewport.Pos.Y);

            // GLFW.SetMouseButtonCallback(data->Window, ImGui_ImplGlfw_MouseButtonCallback);
            // GLFW.SetScrollCallback(data->Window, ImGui_ImplGlfw_ScrollCallback);
            // GLFW.SetKeyCallback(data->Window, ImGui_ImplGlfw_KeyCallback);
            // GLFW.SetCharCallback(data->Window, ImGui_ImplGlfw_CharCallback);
            GLFW.SetWindowCloseCallback(window, ImGui_ImplGlfw_WindowCloseCallback);
            GLFW.SetWindowPosCallback(window, ImGui_ImplGlfw_WindowPosCallback);
            GLFW.SetWindowSizeCallback(window, ImGui_ImplGlfw_WindowSizeCallback);
            
            GLFW.MakeContextCurrent(window);
            GLFW.SwapInterval(0);
        }

        public static void ImGui_ImplGlfw_DestroyWindow(ImGuiViewportPtr viewport)
        {
            var data = GetViewportData(viewport);
            
            if (data.WindowOwned)
            {
                GLFW.DestroyWindow((Window*)data.Window);
            }

            _data.Remove(viewport.PlatformUserData);

            viewport.PlatformUserData = viewport.PlatformHandle = IntPtr.Zero;
        }

        public static void ImGui_ImplGlfw_ShowWindow(ImGuiViewportPtr viewport)
        {
            var window = GetWindow(viewport);
            GLFW.ShowWindow(window);
        }

        public static void ImGui_ImplGlfw_GetWindowPos(out Vector2 position, ImGuiViewportPtr viewport)
        {
            var window = GetWindow(viewport);
            GLFW.GetWindowPos(window, out int x, out int y);
            position = new Vector2(x, y);
        }

        public static void ImGui_ImplGlfw_SetWindowPos(ImGuiViewportPtr viewport, Vector2 pos)
        {
            var window = GetWindow(viewport);
            GLFW.SetWindowPos(window, (int)pos.X, (int)pos.Y);
        }

        public static Vector2 ImGui_ImplGlfw_GetWindowSize(ImGuiViewportPtr viewport)
        {
            var window = GetWindow(viewport);
            GLFW.GetWindowSize(window, out int w, out int h);
            return new Vector2(w, h);
        }

        public static void ImGui_ImplGlfw_SetWindowSize(ImGuiViewportPtr viewport, Vector2 size)
        {
            var data = GetViewportData(viewport);
            data.IgnoreWindowSizeEventFrame = ImGui.GetFrameCount();
            GLFW.SetWindowSize((Window*)data.Window, (int)size.X, (int)size.Y);
        }

        public static void ImGui_ImplGlfw_SetWindowTitle(ImGuiViewportPtr viewport, char* title)
        {
            string titleString = Marshal.PtrToStringAnsi(new IntPtr(title));
            var window = GetWindow(viewport);
            GLFW.SetWindowTitle(window, titleString);
        }

        public static void ImGui_ImplGlfw_SetWindowFocus(ImGuiViewportPtr viewport)
        {
        }

        public static byte ImGui_ImplGlfw_GetWindowFocus(ImGuiViewportPtr viewport)
        {
            var window = GetWindow(viewport);
            return (byte)(GLFW.GetWindowAttrib(window, WindowAttributeGetBool.Focused) ? 1 : 0);
        }

        public static byte ImGui_ImplGlfw_GetWindowMinimized(ImGuiViewportPtr viewport)
        {
            var window = GetWindow(viewport);
            return (byte)(GLFW.GetWindowAttrib(window, WindowAttributeGetBool.Iconified) ? 1 : 0);
        }

        public static void ImGui_ImplGlfw_SetWindowAlpha(ImGuiViewportPtr viewport, float alpha)
        {
            var window = GetWindow(viewport);
            GLFW.SetWindowOpacity(window, alpha);
        }

        public static void ImGui_ImplGlfw_RenderWindow(ImGuiViewportPtr viewport, void* _)
        {
            var window = GetWindow(viewport);
            GLFW.MakeContextCurrent(window);
        }

        public static void ImGui_ImplGlfw_SwapBuffers(ImGuiViewportPtr viewport, void* _)
        {
            var window = GetWindow(viewport);
            GLFW.MakeContextCurrent(window);
            GLFW.SwapBuffers(window);
        }

        private static ViewportData GetViewportData(ImGuiViewportPtr viewport)
        {
            if (viewport.PlatformUserData == IntPtr.Zero)
                throw new Exception("viewport.PlatformUserData was null.");
            
            if (_data.ContainsKey(viewport.PlatformUserData))
                return _data[viewport.PlatformUserData];
            
            throw new Exception("_data does not contain a key for viewport.PlatformUserData");
        }

        private static Window* GetWindow(ImGuiViewportPtr viewport)
        {
            ViewportData data = GetViewportData(viewport);
            return (Window*)data.Window;
        }
        
        private static void SetUpMonitorData()
        {
            ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();
            
            var platformMonitors = new List<ImGuiPlatformMonitor>();

            {
                Monitor*[] monitors = GLFW.GetMonitors();

                for (int i = 0; i < monitors.Length; i++)
                {
                    Monitor* monitor = monitors[i];
                    ImGuiPlatformMonitor m = new ImGuiPlatformMonitor();

                    var vidMode = GLFW.GetVideoMode(monitor);
                    GLFW.GetMonitorPos(monitor, out int x, out int y);
                    m.MainPos = m.WorkPos = new Vector2(x, y);
                    m.MainSize = m.WorkSize = new Vector2(vidMode->Width, vidMode->Height);
                    GLFW.GetMonitorContentScale(monitor, out float xScale, out float yScale);
                    m.DpiScale = xScale;

                    platformMonitors.Add(m);
                }
            }

            _monitorData = platformMonitors.ToArray();
            _monitorDataPin = GCHandle.Alloc(_monitorData, GCHandleType.Pinned);
            
            var platformMonitorsPtrs = new ImGuiPlatformMonitorPtr[_monitorData.Length];

            for (int i = 0; i < _monitorData.Length; i++)
            {
                fixed (ImGuiPlatformMonitor* ptr = &_monitorData[i])
                {
                    platformMonitorsPtrs[i] = new ImGuiPlatformMonitorPtr(ptr);
                }
            }

            _monitorPtrData = platformMonitorsPtrs;
            _monitorPtrDataPin = GCHandle.Alloc(_monitorPtrData, GCHandleType.Pinned);
            _monitorsPtrVector = new ImVector<ImGuiPlatformMonitor>(_monitorPtrData.Length, 
                                                                    _monitorPtrData.Length, 
                                                                    _monitorDataPin.AddrOfPinnedObject());

            {
                ImGuiPlatformIO* ptr = platformIO;
                ImVector v = Unsafe.As<ImVector<ImGuiPlatformMonitor>, ImVector>(ref _monitorsPtrVector);
                ptr->Monitors = v;
            }
        }
    }
}