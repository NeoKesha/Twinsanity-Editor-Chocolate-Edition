﻿using GlmNet;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TT_Lab.AssetData.Instance;
using TT_Lab.Rendering.Buffers;
using TT_Lab.Rendering.Renderers;
using TT_Lab.Rendering.Shaders;
using TT_Lab.Util;
using TT_Lab.ViewModels;

namespace TT_Lab.Rendering
{
    /// <summary>
    /// A 3D scene to render to
    /// </summary>
    public class Scene : IRenderable
    {
        private Scene? parent = null;

        public Scene? Parent { get => parent; set => parent = value; }
        public float Opacity { get; set; } = 1.0f;
        public IRenderer Renderer { get; private set; }

        // Rendering matrices
        private mat4 projectionMat;
        private mat4 viewMat;
        private mat4 modelMat;
        private mat3 normalMat;
        private vec3 cameraPosition = new vec3(0.0f, 0.0f, 0.0f);
        private vec3 cameraDirection = new vec3(0, 0, -1);
        private vec3 cameraUp = new vec3(0, 1, 0);
        private vec2 resolution = new vec2(0, 0);
        private float cameraSpeed = 1.0f;
        private float cameraZoom = 90.0f;
        private bool canManipulateCamera = true;

        // Scene rendering
        private readonly ShaderProgram? viewModelShader;
        private readonly List<IRenderable> objects = new List<IRenderable>();
        private readonly TextureBuffer colorTextureNT = new TextureBuffer(TextureTarget.Texture2DMultisample);
        private readonly FrameBuffer framebufferNT = new FrameBuffer();
        private readonly RenderBuffer depthRenderbuffer = new RenderBuffer();

        // Misc helper stuff
        private readonly Queue<Action> queuedRenderActions = new Queue<Action>();
        

        /// <summary>
        /// Constructor to setup the matrices
        /// </summary>
        /// <param name="width">Viewport render width</param>
        /// <param name="height">Viewport render height</param>
        private Scene(float width, float height)
        {
            Preferences.PreferenceChanged += Preferences_PreferenceChanged;
            resolution.x = width;
            resolution.y = height;
            projectionMat = glm.perspective(glm.radians(cameraZoom), resolution.x / resolution.y, 0.1f, 1000.0f);
            viewMat = glm.lookAt(cameraPosition, cameraPosition + cameraDirection, cameraUp);
            modelMat = glm.scale(new mat4(1.0f), new vec3(1.0f));
            var modelView = viewMat * modelMat;
            normalMat = modelView.to_mat3();
            ReallocateFramebuffer((int)resolution.x, (int)resolution.y);
            framebufferNT.Bind();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, colorTextureNT.Buffer, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderbuffer.Buffer);
            SetupTransparencyRender();
        }

        /// <summary>
        /// Constructor to setup a simple rendering scene with a custom shader
        /// </summary>
        /// <param name="width">Viewport render width</param>
        /// <param name="height">Viewport render height</param>
        /// <param name="shaderName">Shader file name for both .vert and .frag programs</param>
        /// <param name="shdSetUni">Callback when setting shader's uniforms</param>
        public Scene(float width, float height, string shaderName = "Light",
            Action<ShaderProgram, Scene> shdSetUni = null)
            : this(width, height)
        {
            var passVerShader = ManifestResourceLoader.LoadTextFile($"Shaders\\{shaderName}.vert");
            var passFragShader = ManifestResourceLoader.LoadTextFile($"Shaders\\{shaderName}.frag");
            viewModelShader = new ShaderProgram(passVerShader, passFragShader);
            if (shdSetUni == null)
            {
                viewModelShader.SetUniformsAction(() =>
                {
                    DefaultShaderUniforms();
                });
            }
            else
            {
                viewModelShader.SetUniformsAction(() => shdSetUni(viewModelShader, this));
            }
        }

        /// <summary>
        /// Constructor to perform a full chunk render
        /// </summary>
        /// <param name="sceneTree">Tree of chunk resources collision data, positions, cameras, etc.</param>
        /// <param name="width">Viewport render width</param>
        /// <param name="height">Viewport render height</param>
        public Scene(List<AssetViewModel> sceneTree, float width, float height) : this(width, height, "Light")
        {
            // Collision renderer
            var colData = (CollisionData)sceneTree.Find((avm) =>
            {
                return avm.Asset.Type == typeof(Assets.Instance.Collision);
            })!.Asset.GetData();
            var colRender = new Objects.Collision(colData);
            colRender.Parent = this;
            objects.Add(colRender);

            // Positions renderer
            var positions = sceneTree.Find(avm => avm.Alias == "Positions");
            foreach (var pos in positions!.Children)
            {
                var pRend = new Objects.Position((Assets.Instance.Position)pos.Asset);
                pRend.Parent = this;
                objects.Add(pRend);
            }
        }

        public void AddRender(IRenderable renderObj)
        {
            objects.Add(renderObj);
            renderObj.Parent = this;
        }

        public void DefaultShaderUniforms()
        {
            if (viewModelShader != null)
            {
                // Fragment program uniforms
                viewModelShader.SetUniform3("AmbientMaterial", 0.55f, 0.45f, 0.45f);
                viewModelShader.SetUniform3("SpecularMaterial", 0.5f, 0.5f, 0.5f);
                viewModelShader.SetUniform3("LightPosition", cameraPosition.x, cameraPosition.y, cameraPosition.z);
                viewModelShader.SetUniform3("LightDirection", cameraDirection.x, cameraDirection.y, cameraDirection.z);

                // Vertex program uniforms
                SetPVMNShaderUniforms(viewModelShader);
                viewModelShader.SetUniform3("DiffuseMaterial", 0.75f, 0.75f, 0.75f);
            }
        }

        /// <summary>
        /// Sets the matrix uniforms for object's rendering in 3D scene
        /// </summary>
        /// <param name="program"></param>
        public void SetPVMNShaderUniforms(ShaderProgram program)
        {
            program.SetUniformMatrix4("Projection", projectionMat.to_array());
            program.SetUniformMatrix4("View", viewMat.to_array());
            program.SetUniformMatrix4("Model", modelMat.to_array());
            program.SetUniformMatrix3("NormalMatrix", normalMat.to_array());
        }

        public void SetResolution(float width, float height)
        {
            resolution.x = width;
            resolution.y = height;
            ReallocateFramebuffer((int)width, (int)height);
        }

        public void Render()
        {
            foreach (var a in queuedRenderActions)
            {
                a.Invoke();
            }
            queuedRenderActions.Clear();
            UpdateMatrices();
            Bind();
            GL.CullFace(CullFaceMode.FrontAndBack);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Multisample);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);
            // Opaque rendering color
            framebufferNT.Bind();
            Bind();
            float[] clearColorNT = System.Drawing.Color.LightGray.ToArray();
            float clearDepth = 1f;
            GL.ClearBuffer(ClearBuffer.Color, 0, clearColorNT);
            GL.ClearBuffer(ClearBuffer.Depth, 0, ref clearDepth);
            // Transparency rendering
            Renderer.Render(objects);

            // Reset modes
            GL.CullFace(CullFaceMode.Back);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Multisample);
            Unbind();
        }

        public void Bind()
        {
            viewModelShader?.Bind();
            viewModelShader?.SetUniforms();
        }

        public void SetCameraSpeed(float s)
        {
            cameraSpeed = s;
        }

        public void DisableCameraManipulation()
        {
            canManipulateCamera = false;
        }

        private vec2 yaw_pitch;
        public void RotateView(Vector2 rot)
        {
            if (!canManipulateCamera) return;

            rot.Normalize();
            yaw_pitch.x += rot.X;
            yaw_pitch.y += rot.Y;

            if (yaw_pitch.y > 89f)
            {
                yaw_pitch.y = 89f;
            }
            if (yaw_pitch.y < -89f)
            {
                yaw_pitch.y = -89f;
            }

            vec3 direction;
            direction.x = (float)Math.Cos(glm.radians(yaw_pitch.x)) * (float)Math.Cos(glm.radians(yaw_pitch.y));
            direction.y = (float)Math.Sin(glm.radians(yaw_pitch.y));
            direction.z = (float)Math.Sin(glm.radians(yaw_pitch.x)) * (float)Math.Cos(glm.radians(yaw_pitch.y));

            cameraDirection = glm.normalize(direction);
        }

        public void ZoomView(float z)
        {
            if (!canManipulateCamera) return;

            z *= -0.01f;
            cameraZoom = (z + cameraZoom).Clamp(10.0f, 100.0f);
        }

        public void Move(List<Keys> keysPressed)
        {
            if (!canManipulateCamera) return;

            var camSp = cameraSpeed;

            if (keysPressed.Contains(Keys.ShiftKey))
            {
                camSp *= 5;
            }

            foreach (var keyPressed in keysPressed)
            {
                switch (keyPressed)
                {
                    case Keys.W:
                        cameraPosition += camSp * cameraDirection;
                        break;
                    case Keys.S:
                        cameraPosition -= camSp * cameraDirection;
                        break;
                    case Keys.A:
                        cameraPosition -= camSp * glm.cross(cameraDirection, cameraUp);
                        break;
                    case Keys.D:
                        cameraPosition += camSp * glm.cross(cameraDirection, cameraUp);
                        break;
                }
            }
        }

        public void Unbind()
        {
            viewModelShader?.Unbind();
        }

        public void Delete()
        {
            viewModelShader?.Delete();
            colorTextureNT.Delete();
            framebufferNT.Delete();
            depthRenderbuffer.Delete();
            Renderer.Delete();
            foreach (var @object in objects)
            {
                @object.Delete();
            }
            objects.Clear();
            Preferences.PreferenceChanged -= Preferences_PreferenceChanged;
        }

        public void PreRender()
        {
            foreach (var @object in objects)
            {
                @object.PreRender();
            }
        }

        public void PostRender()
        {
            foreach (var @object in objects)
            {
                @object.PostRender();
            }
        }

        private void UpdateMatrices()
        {
            projectionMat = glm.perspective(glm.radians(cameraZoom), resolution.x / resolution.y, 0.1f, 1000.0f);
            viewMat = glm.lookAt(cameraPosition, cameraPosition + cameraDirection, cameraUp);
            var modelView = viewMat * modelMat;
            normalMat = modelView.to_mat3();
        }

        private void ReallocateFramebuffer(int width, int height)
        {
            colorTextureNT.Bind();
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, 4, PixelInternalFormat.Rgb10A2, width, height, true);
            depthRenderbuffer.Bind();
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, 4, RenderbufferStorage.DepthComponent, width, height);
            Renderer?.ReallocateFramebuffer(width, height);
        }

        private void Preferences_PreferenceChanged(Object? sender, Preferences.PreferenceChangedArgs e)
        {
            if (e.PreferenceName == Preferences.TranslucencyMethod)
            {
                queuedRenderActions.Enqueue(() =>
                {
                    SetupTransparencyRender();
                });
            }
        }

        private void SetupTransparencyRender()
        {
            var method = Preferences.GetPreference<RenderSwitches.TranslucencyMethod>(Preferences.TranslucencyMethod);
            Renderer?.Delete();
            switch (method)
            {
                case RenderSwitches.TranslucencyMethod.WBOIT:
                    Renderer = new WBOITRenderer(depthRenderbuffer, resolution.x, resolution.y);
                    break;
                case RenderSwitches.TranslucencyMethod.DDP:
                default:
                    Renderer = new DDPRenderer(resolution.x, resolution.y);
                    break;
            }
            Renderer.Scene = this;
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
        }
    }
}
