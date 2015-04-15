using System;
using OpenTK;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.Android;
using Android.Content;
using Android.Graphics;
using EMU7800.D2D.Interop;
using System.IO;
using Android.Runtime;
using Android.InputMethodServices;
using Android.Views;

namespace EMU7800.MonoDroid
{
    class GLView1 : AndroidGameView
    {
        GraphicsDevice _graphicsDevice = new GraphicsDevice();

        public GLView1(Context context) : base(context)
        {
        }

        // This gets called when the drawing surface is ready
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //_viewportWidth = Width;
            //_viewportHeight = Height;

            // Run the render loop
            Run();
        }

        protected override void OnResize(EventArgs e)
        {
            _graphicsDevice.UpdateForWindowSizeChange();
        }

        protected override void CreateFrameBuffer()
        {
            _graphicsDevice.Initialize(this);
            base.CreateFrameBuffer();
        }

        public new bool OnKeyDown([GeneratedEnum]Android.Views.Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                case Android.Views.Keycode.DpadLeft:
                    _x -= 2;
                    break;
                case Android.Views.Keycode.DpadRight:
                    _x += 2;
                    break;
                case Android.Views.Keycode.DpadUp:
                    _y -= 2;
                    break;
                case Android.Views.Keycode.DpadDown:
                    _y += 2;
                    break;
                case Android.Views.Keycode.P:
                    _w += 2;
                    _h += 2;
                    break;
                case Android.Views.Keycode.M:
                    _w -= 2;
                    _h -= 2;
                    break;
            }
            return true;
        }

        // This gets called on each frame render
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            //OnRenderOriginalFrame(e);
            //OnRenderExp1(e);
            //OnRenderExp2(e);
            OnRenderExp3(e);
        }

        bool _initialized = false;
        int[] _textureId = new int[2];
        Bitmap _dynBitmap, _dynBitmap2, _dynBitmap3;
        Random _random = new Random();
        int[] _pixels;
        byte[] _pixels2;
        const int pwidth = 320, pheight = 240;

        StaticBitmap _androidLogo;
        DynamicBitmap _snow;
        TextLayout _text;
        float _x, _y, _w, _h;


        void OnRenderExp3(FrameEventArgs e)
        {
            if (_androidLogo == null)
            {
                using (var s = Resources.OpenRawResource(EMU7800.D2D.Resource.Drawable.Icon))
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    _androidLogo = _graphicsDevice.CreateStaticBitmap(ms.ToArray());
                }
                _snow = _graphicsDevice.CreateDynamicBitmap(EMU7800.D2D.Shell.Struct.ToSizeU(320, 240));
                _pixels2 = new byte[320 * 240 * 3];

                _text = _graphicsDevice.CreateTextLayout("Arial", 75, "Hello World!", 600, 200);
            }

            for (var i = 0; i < _pixels2.Length; i += 3)
            {
                var b = (byte)_random.Next(0, 0xff);
                _pixels2[i] = b;
                _pixels2[i + 1] = b;
                _pixels2[i + 2] = b;
            }
            _snow.CopyFromMemory(_pixels2);

            _graphicsDevice.BeginDraw();

            var rect = EMU7800.D2D.Shell.Struct.ToRectF(D2D.Shell.Struct.ToPointF(0, 0), D2D.Shell.Struct.ToSizeF(272 + _w, 272 + _h));
            _graphicsDevice.DrawBitmap(_androidLogo, rect);

            var rect2 = EMU7800.D2D.Shell.Struct.ToRectF(
                D2D.Shell.Struct.ToPointF(100, 300),
                D2D.Shell.Struct.ToSizeF(320 * 2, 240 * 2));
            _graphicsDevice.DrawBitmap(_snow, rect2, D2DBitmapInterpolationMode.Linear);

            var loc = EMU7800.D2D.Shell.Struct.ToPointF(_x, _y);
            _graphicsDevice.DrawText(_text, loc, D2DSolidColorBrush.White);

            _graphicsDevice.EndDraw();

            _graphicsDevice.Present();
        }

        void OnRenderExp1(FrameEventArgs e)
        {
            if (!_initialized)
            {
                GL.Enable(All.Texture2D);
                GL.GenTextures(1, _textureId);
                //GL.ShadeModel(All.Smooth);
                GL.BindTexture(All.Texture2D, _textureId[0]);
                var options = new BitmapFactory.Options();
                options.InScaled = false;
                options.InMutable = false;
                using (var bitmap = BitmapFactory.DecodeResource(Resources, EMU7800.D2D.Resource.Drawable.Icon, options))
                {
                    Android.Opengl.GLUtils.TexImage2D(Android.Opengl.GLES10.GlTexture2d, 0, Android.Opengl.GLES10.GlRgba, bitmap, 0);
                }

                GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
                GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
                _initialized = true;
            }

            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
            GL.Clear((uint)All.ColorBufferBit);
            //GL.Viewport(0, 0, _viewportWidth, _viewportHeight);

            GL.BindTexture(All.Texture2D, _textureId[0]);

            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.TextureCoordArray);
            GL.FrontFace(All.Cw);

            GL.VertexPointer(2, All.Float, 0, square_vertices);
            GL.TexCoordPointer(2, All.Float, 0, uv);

            GL.DrawArrays(All.TriangleStrip, 0, 4);

            SwapBuffers();

        }

        void OnRenderExp2(FrameEventArgs e)
        {
            if (!_initialized)
            {
                GL.Enable(All.Texture2D);
                GL.GenTextures(1, _textureId);
                //GL.ShadeModel(All.Smooth);
                GL.BindTexture(All.Texture2D, _textureId[0]);


                //_dynBitmap = Bitmap.CreateBitmap(_viewportWidth, _viewportHeight, Bitmap.Config.Argb8888);

                Android.Opengl.GLUtils.TexImage2D(Android.Opengl.GLES10.GlTexture2d, 0, Android.Opengl.GLES10.GlRgba, _dynBitmap, 0);

                GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
                GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);

                //GL.BindTexture(All.Texture2D, _textureId[1]);
                _dynBitmap2 = Bitmap.CreateBitmap(pwidth, pheight, Bitmap.Config.Argb8888);
                //Android.Opengl.GLUtils.TexImage2D(Android.Opengl.GLES10.GlTexture2d, 0, Android.Opengl.GLES10.GlRgba, _dynBitmap2, 0);
                //GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
                //GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);


                //GL.BindTexture(All.Texture2D, _textureId[3]);
                var options = new BitmapFactory.Options();
                options.InScaled = false;
                options.InMutable = false;
                _dynBitmap3 = BitmapFactory.DecodeResource(Resources, EMU7800.D2D.Resource.Drawable.Icon, options);
                //{
                //    Android.Opengl.GLUtils.TexImage2D(Android.Opengl.GLES10.GlTexture2d, 0, Android.Opengl.GLES10.GlRgba, bitmap, 0);
                //}

                _pixels = new int[pwidth*pheight];
                _initialized = true;
            }

            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
            GL.Clear((uint)All.ColorBufferBit);
            //GL.Viewport(0, 0, _viewportWidth, _viewportHeight);

            GL.BindTexture(All.Texture2D, _textureId[0]);

            for (var i = 0; i < _pixels.Length; i++)
            {
                var b = _random.Next(0, 0xff);
                var b2 = ((0xff) << 24) | (b << 16) | (b << 8) | b;
                _pixels[i] = unchecked(b2);
            }
            _dynBitmap2.SetPixels(_pixels, 0, pwidth, 0, 0, pwidth, pheight);
            Android.Opengl.GLUtils.TexSubImage2D(Android.Opengl.GLES10.GlTexture2d, 0, 100, 100, _dynBitmap2);
            Android.Opengl.GLUtils.TexSubImage2D(Android.Opengl.GLES10.GlTexture2d, 0, 50, 50, _dynBitmap3);

            //GL.BindTexture(All.Texture2D, _textureId[0]);

            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.TextureCoordArray);

            //GL.FrontFace(All.Cw);

            GL.VertexPointer(2, All.Float, 0, square_vertices2);
            GL.TexCoordPointer(2, All.Float, 0, uv);

            GL.DrawArrays(All.TriangleStrip, 0, 4);

            SwapBuffers();
        }

        void OnRenderOriginalFrame(FrameEventArgs e)
        {
            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0f, 1.0f, -1.5f, 1.5f, -1.0f, 1.0f);
            GL.MatrixMode(All.Modelview);
            GL.Rotate(3.0f, 0.0f, 0.0f, 1.0f);

            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
            GL.Clear((uint)All.ColorBufferBit);

            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.ColorArray);

            GL.VertexPointer(2, All.Float, 0, square_vertices);
            GL.ColorPointer(4, All.UnsignedByte, 0, square_colors);

            GL.DrawArrays(All.TriangleStrip, 0, 4);

            SwapBuffers();
        }

        float[] uv = {
            0, 1,  // bottom-left
            1, 1,  // bottom-right
            0, 0,  // top-left
            1, 0,  // top-right
        };


        float[] square_vertices2 = {
            -1.0f, -1.0f,
             1.0f, -1.0f,
            -1.0f,  1.0f,
             1.0f,  1.0f,
        };

        float[] square_vertices = {
            -0.5f, -0.5f,
             0.5f, -0.5f,
            -0.5f,  0.5f,
             0.5f,  0.5f,
        };

        byte[] square_colors = {
            255, 255,   0, 255,
              0, 255, 255, 255,
              0,   0,   0,  0,
            255,   0, 255, 255,
        };
    }
}
