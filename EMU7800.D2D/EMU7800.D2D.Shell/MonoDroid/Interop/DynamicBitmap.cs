using Android.Graphics;
using OpenTK.Graphics.ES11;
using System;

namespace EMU7800.D2D.Interop
{
    public sealed class DynamicBitmap : IDisposable
    {
        #region Fields

        readonly GraphicsDevice _gd;

        readonly int[] _textureId = new int[1];

        readonly float[] _uv =
        {
            0.0f, 1.0f,  // bottom-left
            1.0f, 1.0f,  // bottom-right
            0.0f, 0.0f,  // top-left
            1.0f, 0.0f,  // top-right
        };

        readonly float[] _vertices = new float[4 * 2];

        readonly int[] _copybuf;

        readonly Bitmap _bitmap;

        #endregion

        public int HR { get; private set; }

        public int CopyFromMemory(byte[] data)
        {
            for (int i = 0, j = 0; i < data.Length && j < _copybuf.Length; j++)
            {
                _copybuf[j] = (0xff << 24) | (data[i + 2] << 16) | (data[i + 1] << 8) | data[i];
                i += 3;
            }
            _bitmap.SetPixels(_copybuf, 0, _bitmap.Width, 0, 0, _bitmap.Width, _bitmap.Height);

            GL.BindTexture(All.Texture2D, _textureId[0]);
            Android.Opengl.GLUtils.TexSubImage2D(Android.Opengl.GLES10.GlTexture2d, 0, 0, 0, _bitmap);
            return 0;
        }

        #region IDisposable Members

        ~DynamicBitmap()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteTextures(1, _textureId);
                using (_bitmap)
                {
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        internal void Draw(RectF rect, D2DBitmapInterpolationMode interpolationMode)
        {
            var sw = 2.0f / _gd.Width;
            var sh = -2.0f / _gd.Height;

            _vertices[0] = rect.Left * sw - 1.0f;
            _vertices[1] = rect.Bottom * sh + 1.0f;

            _vertices[2] = rect.Right * sw - 1.0f;
            _vertices[3] = _vertices[1];

            _vertices[4] = _vertices[0];
            _vertices[5] = rect.Top * sh + 1.0f;

            _vertices[6] = _vertices[2];
            _vertices[7] = _vertices[5];

            GL.BindTexture(All.Texture2D, _textureId[0]);

            switch (interpolationMode)
            {
                case D2DBitmapInterpolationMode.Linear:
                    GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Linear);
                    GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);

                    break;
                case D2DBitmapInterpolationMode.NearestNeighbor:
                    GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
                    GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Nearest);
                    break;
            }

            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.TextureCoordArray);
            GL.VertexPointer(2, All.Float, 0, _vertices);
            GL.TexCoordPointer(2, All.Float, 0, _uv);
            GL.DrawArrays(All.TriangleStrip, 0, 4);
        }

        internal DynamicBitmap(GraphicsDevice gd, SizeU size)
        {
            _gd = gd;

            var w = (int)size.Width;
            var h = (int)size.Height;
            _copybuf = new int[w * h];
            _bitmap = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);

            GL.GenTextures(1, _textureId);
            GL.BindTexture(All.Texture2D, _textureId[0]);
            Android.Opengl.GLUtils.TexImage2D(Android.Opengl.GLES10.GlTexture2d, 0, _bitmap, 0);
        }
    };
}