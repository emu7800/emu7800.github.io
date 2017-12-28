using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace EMU7800.WP.View
{
    public class InputBox
    {
        #region Fields

        readonly Texture2D _texture;
        readonly Rectangle _textureDestRect;
        readonly Color _colorOn, _colorOff;

        Rectangle _inputRectangle;
        int _pressedTouchLocationId;

        #endregion

        public Action Pressed { get; set; }
        public Action Released { get; set; }

        public int InputRectangleNewWidth
        {
            set { ChangeInputRectangleHorizontally(value); }
        }

        public int InputRectangleNewHeight
        {
            set { ChangeInputRectangleVertically(value); }
        }

        public int InputRectangleNewWidthAndHeight
        {
            set
            {
                InputRectangleNewWidth = value;
                InputRectangleNewHeight = value;
            }
        }

        public void HandleTouchLocationInput(TouchLocation tl)
        {
            switch (tl.State)
            {
                case TouchLocationState.Invalid:
                    break;
                case TouchLocationState.Released:
                    if (tl.Id == _pressedTouchLocationId)
                    {
                        _pressedTouchLocationId = -1;
                        if (Released != null)
                            Released();
                    }
                    break;
                case TouchLocationState.Moved:
                    if (DoesIntersectWithInputRectangle(tl.Position))
                    {
                        if (_pressedTouchLocationId >= 0)
                            break;
                        _pressedTouchLocationId = tl.Id;
                        if (Pressed != null)
                            Pressed();
                    }
                    else if (tl.Id == _pressedTouchLocationId)
                    {
                        _pressedTouchLocationId = -1;
                        if (Released != null)
                            Released();
                    }
                    break;
                case TouchLocationState.Pressed:
                    if (_pressedTouchLocationId >= 0)
                        break;
                    if (DoesIntersectWithInputRectangle(tl.Position))
                    {
                        _pressedTouchLocationId = tl.Id;
                        if (Pressed != null)
                            Pressed();
                    }
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_texture == null)
                return;
            var color = (_pressedTouchLocationId >= 0) ? _colorOn : _colorOff;
            spriteBatch.Draw(_texture, _textureDestRect, color);
        }

        #region Constructors

        public InputBox(int x, int y, int size, Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");

            _texture = texture;
            _textureDestRect = new Rectangle(x, y, size, size);

            _colorOff = Color.FromNonPremultiplied(0xff, 0xff, 0xff, 24);
            _colorOn = Color.White;

            _inputRectangle = _textureDestRect;
            _pressedTouchLocationId = -1;
        }

        #endregion

        #region Helpers

        bool DoesIntersectWithInputRectangle(Vector2 position)
        {
            return position.X >= _inputRectangle.Left && position.X <= _inputRectangle.Right && position.Y >= _inputRectangle.Top && position.Y <= _inputRectangle.Bottom;
        }

        void ChangeInputRectangleHorizontally(int newWidth)
        {
            var r = _inputRectangle;

            var dx = (r.Width >> 1) - (newWidth >> 1);
            r.X += dx;
            r.Width = newWidth;

            _inputRectangle = r;
        }

        void ChangeInputRectangleVertically(int newHeight)
        {
            var r = _inputRectangle;

            var dy = (r.Height >> 1) - (newHeight >> 1);
            r.Y += dy;
            r.Height = newHeight;

            _inputRectangle = r;
        }

        #endregion
    }
}
