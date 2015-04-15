using Android.Graphics;
using OpenTK.Graphics.ES11;
using System;

namespace EMU7800.D2D.Interop
{
    public class StaticBitmap : IDisposable
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

        #endregion

        public int HR { get; private set; }

        #region IDisposable Members

        ~StaticBitmap()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteTextures(1, _textureId);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        internal void Draw(RectF rect)
        {
            var sw =  2.0f / _gd.Width;
            var sh = -2.0f / _gd.Height;

            _vertices[0] = rect.Left   * sw - 1.0f;
            _vertices[1] = rect.Bottom * sh + 1.0f;

            _vertices[2] = rect.Right  * sw - 1.0f;
            _vertices[3] = _vertices[1];

            _vertices[4] = _vertices[0];
            _vertices[5] = rect.Top    * sh + 1.0f;

            _vertices[6] = _vertices[2];
            _vertices[7] = _vertices[5];

            GL.BindTexture(All.Texture2D, _textureId[0]);
            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.TextureCoordArray);
            GL.VertexPointer(2, All.Float, 0, _vertices);
            GL.TexCoordPointer(2, All.Float, 0, _uv);
            GL.DrawArrays(All.TriangleStrip, 0, 4);
        }

        internal StaticBitmap(GraphicsDevice gd, byte[] data)
        {
            _gd = gd;

            GL.GenTextures(1, _textureId);
            GL.BindTexture(All.Texture2D, _textureId[0]);

            GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Nearest);

            var options = new BitmapFactory.Options { InScaled = false };
            using (var bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length, options))
            {
                Android.Opengl.GLUtils.TexImage2D(Android.Opengl.GLES10.GlTexture2d, 0, bitmap, 0);
            }
        }
    }
}