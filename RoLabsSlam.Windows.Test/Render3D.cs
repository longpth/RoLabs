﻿using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using OpenTK.Mathematics;
using OpenTK.WinForms;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.UI.Xaml.Media;

namespace RoLabsSlam.Windows.Test
{
    // Enum for camera movement directions
    public enum CameraDirection
    {
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down
    }

    public class Render3D
    {
        private GLControl _glControl;
        private int _shaderProgram;
        private int _vertexArrayObject;

        private int AxisShader;
        private int PyramidShader;

        private int PyramidVAO;
        private int AxisVAO;
        private int EBO;
        private int PositionBuffer;
        private int ColorBuffer;
        private float _angle = 0.0f;
        Matrix4 _projection;

        private Vector2 _lastMousePosition;
        private Vector3 _cameraPosition = new Vector3(30, 30, 0);  // Position the camera high up on the Y-axis
        private Quaternion _cameraRotation = Quaternion.Identity;
        private Vector3 _mapOrigin = Vector3.Zero;  // Look at the origin
        private Vector3 _camTarget = Vector3.Zero;  // Look at the origin
        private Vector3 _cameraUp = Vector3.UnitY;  // Up direction is along the Z-axis, as Y is used for height
        private float _zoomFactor = 1;

        private List<Matrix4> _pyramidTransformations = new List<Matrix4>();

        Vector3 _pyramidTranslation = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 _pyramidRotation = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 _pyramidScale = new Vector3(1.0f, 1.0f, 1.0f);

        private static readonly Vector3[] VertexData = new Vector3[]
        {
            // Base of the pyramid (square), along the positive X-axis
            new Vector3(-0.5f, -0.5f, 1.0f), // Base vertex 0
            new Vector3(-0.5f,  0.5f, 1.0f), // Base vertex 1
            new Vector3( 0.5f,  0.5f, 1.0f), // Base vertex 2
            new Vector3( 0.5f, -0.5f, 1.0f), // Base vertex 3

            // Apex of the pyramid, at the origin
            new Vector3(0.0f, 0.0f, 0.0f),   // Apex vertex 4
        };

        private static readonly int[] IndexData = new int[]
        {
            // Base edges (square)
            0, 1,  // Edge 1
            1, 2,  // Edge 2
            2, 3,  // Edge 3
            3, 0,  // Edge 4

            // Side edges (connecting apex to base)
            0, 4,  // Side edge 1
            1, 4,  // Side edge 2
            2, 4,  // Side edge 3
            3, 4,  // Side edge 4
        };

        private static readonly Color4[] ColorData = new Color4[]
        {
            // Base edges (e.g., Silver)
            Color4.Silver, Color4.Silver, Color4.Silver, Color4.Silver,

            // Side edges (e.g., Red)
            Color4.IndianRed, Color4.IndianRed, Color4.IndianRed, Color4.IndianRed
        };

        private static readonly Vector3[] AxisVertexData = new Vector3[]
        {
            // X axis (Red)
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(3.0f, 0.0f, 0.0f),

            // Y axis (Green)
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 3.0f, 0.0f),

            // Z axis (Blue)
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 3.0f),
        };

        private static readonly Color4[] AxisColorData = new Color4[]
        {
            Color4.Red,    Color4.Red,    // X axis
            Color4.Yellow,  Color4.Yellow,  // Y axis
            Color4.Blue,   Color4.Blue,   // Z axis
        };

        private const string VertexShaderSource = @"#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec4 aColor;

out vec4 fColor;

uniform mat4 MVP;

void main()
{
    gl_Position = vec4(aPos, 1) * MVP;
    fColor = aColor;
}
";

        private const string FragmentShaderSource = @"#version 330 core

in vec4 fColor;

out vec4 oColor;

void main()
{
    oColor = fColor;
}
";

        #region public method
        public Render3D(GLControl glControl)
        {
            _glControl = glControl;

            // Attach event handlers
            _glControl.Paint += glControl_Paint;
            _glControl.Resize += glControl_Resize;

            // Attach mouse event handlers
            _glControl.MouseDown += glControl_MouseDown;
            _glControl.MouseMove += glControl_MouseMove;
            _glControl.MouseWheel += glControl_MouseWheel;
        }

        public void glControl_Load(object sender, EventArgs e)
        {
            // Ensure that the viewport and projection matrix are set correctly initially.
            glControl_Resize(_glControl, EventArgs.Empty);

            setupPyramid();
            setupAxis();
        }

        public void AddPyramidTransformation(Matrix4 tranformation)
        {
            // Add the new transformation to the list
            _pyramidTransformations.Add(tranformation);

            // Trigger a repaint to update the view
            _glControl.Invalidate();
        }

        // Method to update the camera position based on direction
        public void UpdateCameraPosition(float delta, CameraDirection direction)
        {
            // Calculate the direction vector from the camera to the map origin
            var cameraZDirection = Vector3.Normalize(_cameraPosition - _camTarget);
            var cameraXDirection = Vector3.Normalize(Vector3.Cross(_cameraUp, cameraZDirection));
            var cameraYDirection = Vector3.Cross(cameraZDirection, cameraXDirection);

            switch (direction)
            {
                case CameraDirection.Forward:
                    // Move forward toward the target
                    _cameraPosition -= cameraZDirection * delta;
                    break;

                case CameraDirection.Backward:
                    // Move backward away from the target
                    _cameraPosition += cameraZDirection * delta;
                    break;

                case CameraDirection.Left:
                    // Move left relative to the direction to the target 
                    _camTarget -= cameraXDirection * delta;
                    _cameraPosition -= cameraXDirection * delta;
                    break;

                case CameraDirection.Right:
                    // Move right relative to the direction to the target
                    _camTarget += cameraXDirection * delta;
                    _cameraPosition += cameraXDirection * delta;
                    break;

                case CameraDirection.Up:
                    // Move up relative to the direction to the target 
                    _camTarget -= cameraYDirection * delta;
                    _cameraPosition -= cameraYDirection * delta;
                    break;

                case CameraDirection.Down:
                    // Move down relative to the direction to the target 
                    _camTarget += cameraYDirection * delta;
                    _cameraPosition += cameraYDirection * delta;
                    break;
            }

            // Trigger the scene to be redrawn with the new camera position
            _glControl.Invalidate();
        }
        #endregion

        #region private method
        private void setupPyramid()
        {
            PyramidShader = CompileProgram(VertexShaderSource, FragmentShaderSource);

            PyramidVAO = GL.GenVertexArray();
            GL.BindVertexArray(PyramidVAO);

            EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, IndexData.Length * sizeof(int), IndexData, BufferUsageHint.StaticDraw);

            PositionBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, PositionBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, VertexData.Length * sizeof(float) * 3, VertexData, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);

            ColorBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, ColorData.Length * sizeof(float) * 4, ColorData, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
        }

        private void setupAxis()
        {
            AxisShader = CompileProgram(VertexShaderSource, FragmentShaderSource);

            AxisVAO = GL.GenVertexArray();
            GL.BindVertexArray(AxisVAO);

            PositionBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, PositionBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, AxisVertexData.Length * sizeof(float) * 3, AxisVertexData, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);

            ColorBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, AxisColorData.Length * sizeof(float) * 4, AxisColorData, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
        }

        private void glControl_Resize(object? sender, EventArgs e)
        {
            _glControl.MakeCurrent();

            if (_glControl.ClientSize.Height == 0)
                _glControl.ClientSize = new System.Drawing.Size(_glControl.ClientSize.Width, 1);

            GL.Viewport(0, 0, _glControl.ClientSize.Width, _glControl.ClientSize.Height);

            float aspect_ratio = Math.Max(_glControl.ClientSize.Width, 1) / (float)Math.Max(_glControl.ClientSize.Height, 1);
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 64);
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }

        // Function to create the view matrix based on yaw, pitch, and target
        private Matrix4 GetViewMatrix()
        {
            // Create the view matrix
            return Matrix4.LookAt(_cameraPosition, _camTarget, _cameraUp);
        }

        private void Render()
        {
            _glControl.MakeCurrent();

            GL.ClearColor(Color4.MidnightBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

            // Get the view matrix based on the current yaw, pitch, and camera position
            Matrix4 viewMatrix = GetViewMatrix();

            //Console.WriteLine("viewMatrix");
            //Console.WriteLine(viewMatrix);

            // Set up the common transformation matrices
            Matrix4 projectionMatrix = _projection;

            // Draw the original Pyramid
            GL.UseProgram(PyramidShader);

            // Original pyramid transformation (no transformation)
            Matrix4 translationMatrix = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);

            // Apply the translation to the original pyramid's model matrix
            Matrix4 originalPyramidModelMatrix = translationMatrix;

            Matrix4 originalPyramidMVP = originalPyramidModelMatrix * viewMatrix * projectionMatrix;
            GL.UniformMatrix4(GL.GetUniformLocation(PyramidShader, "MVP"), true, ref originalPyramidMVP);
            GL.BindVertexArray(PyramidVAO);
            GL.DrawElements(PrimitiveType.Lines, IndexData.Length, DrawElementsType.UnsignedInt, 0);

            // Draw each additional pyramid with its own transformation
            foreach (var pyramidTransformation in _pyramidTransformations)
            {
                //Console.WriteLine("*************************************");

                Matrix4 newPyramid = pyramidTransformation * originalPyramidModelMatrix;

                //Console.WriteLine(transformedPyramidMVP);
                //Console.WriteLine(pyramidTransformation);
                //Console.WriteLine(viewMatrix);
                //Console.WriteLine(projectionMatrix);

                //Console.WriteLine("*************************************");

                Matrix4 originalPyramidMVPNew = newPyramid * viewMatrix * projectionMatrix;
                GL.UniformMatrix4(GL.GetUniformLocation(PyramidShader, "MVP"), true, ref originalPyramidMVPNew);
                GL.BindVertexArray(PyramidVAO);
                GL.DrawElements(PrimitiveType.Lines, IndexData.Length, DrawElementsType.UnsignedInt, 0);
            }

            // Draw the Axis
            Matrix4 axisMVP = viewMatrix * projectionMatrix;
            GL.UseProgram(AxisShader);
            GL.UniformMatrix4(GL.GetUniformLocation(AxisShader, "MVP"), true, ref axisMVP);
            GL.BindVertexArray(AxisVAO);
            GL.DrawArrays(PrimitiveType.Lines, 0, AxisVertexData.Length);

            _glControl.SwapBuffers();
        }

        private int CompileProgram(string vertexShader, string fragmentShader)
        {
            int program = GL.CreateProgram();

            int vert = CompileShader(ShaderType.VertexShader, vertexShader);
            int frag = CompileShader(ShaderType.FragmentShader, fragmentShader);

            GL.AttachShader(program, vert);
            GL.AttachShader(program, frag);

            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string log = GL.GetProgramInfoLog(program);
                throw new Exception($"Could not link program: {log}");
            }

            GL.DetachShader(program, vert);
            GL.DetachShader(program, frag);

            GL.DeleteShader(vert);
            GL.DeleteShader(frag);

            return program;

            static int CompileShader(ShaderType type, string source)
            {
                int shader = GL.CreateShader(type);

                GL.ShaderSource(shader, source);
                GL.CompileShader(shader);

                GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
                if (status == 0)
                {
                    string log = GL.GetShaderInfoLog(shader);
                    throw new Exception($"Failed to compile {type}: {log}");
                }

                return shader;
            }
        }

        private void glControl_MouseDown(object? sender, MouseEventArgs e)
        {
            _lastMousePosition = new Vector2(e.X, e.Y);
        }

        private void glControl_MouseMove(object? sender, MouseEventArgs e)
        {
            // Calculate the current mouse position
            var thisMousePosition = new Vector2(e.X, e.Y);

            if (_lastMousePosition != null)
            {
                // Calculate the change in mouse position (delta)
                var delta = thisMousePosition - _lastMousePosition;

                if (e.Button == MouseButtons.Left)
                {
                    // Calculate the rotation quaternions based on mouse movement
                    float yawAngle = delta.X * 0.1f;  // Adjust sensitivity as needed
                    float pitchAngle = delta.Y * 0.1f; // Adjust sensitivity as needed

                    // Create quaternions for yaw and pitch
                    Quaternion yawRotation = Quaternion.FromAxisAngle(_cameraUp, MathHelper.DegreesToRadians(yawAngle));
                    Quaternion pitchRotation = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(-pitchAngle));

                    // Combine the rotations and apply them to the camera's position
                    _cameraRotation = Quaternion.Normalize(yawRotation * pitchRotation);

                    //Console.WriteLine("MouseMove");
                    //Console.WriteLine(_cameraRotation);

                    _cameraPosition = Vector3.Transform(_cameraPosition - _mapOrigin, _cameraRotation) + _mapOrigin;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    // TODO
                }
            }

            // Update last mouse position for the next move event
            _lastMousePosition = thisMousePosition;

            // Trigger a repaint to update the view
            _glControl.Invalidate();
        }

        private void glControl_MouseWheel(object? sender, MouseEventArgs e)
        {
            // Zoom in or out by adjusting the camera position along the direction vector
            Vector3 direction = Vector3.Normalize(_mapOrigin - _cameraPosition);
            _cameraPosition += direction * (e.Delta * 0.01f);

            _zoomFactor += e.Delta * 0.01f;

            // Trigger a repaint to update the view
            _glControl.Invalidate();
        }
        #endregion
    }
}
