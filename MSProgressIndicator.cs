using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace milano88.UI.Controls
{
    public class MSProgressIndicator : Control
    {
        private int _circleDiam = 15;
        private int _circleIndex;
        private int _circleCount = 6;
        private readonly SolidBrush _circleBrush = new SolidBrush(Color.Gray);
        private readonly SolidBrush _moveCircleBrush = new SolidBrush(Color.YellowGreen);
        private PointF[] _circlePoints;
        private Size _lastSize;
        private BufferedGraphics _graphicsBuffer;
        private readonly BufferedGraphicsContext _bufferContext = BufferedGraphicsManager.Current;
        private readonly Timer _tmrAnimate = new Timer();
        private UnitVector _unitVector = new UnitVector();

        public MSProgressIndicator()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Size = new Size(90, 90);
            this.Margin = new Padding(0);
            SetCirclePoints();
            _tmrAnimate.Interval = 300;
            _tmrAnimate.Tick +=_tmrAnimate_Tick;
            _tmrAnimate.Start();
        }

        private void _tmrAnimate_Tick(object sender, EventArgs e)
        {
            if (_circleIndex.Equals(0))
                _circleIndex = _circlePoints.Length - 1;
            else
                _circleIndex--;
            this.Invalidate(false);
        }

        private void UpdateGraphicsBuffer()
        {
            if (this.Width > 0 && this.Height > 0)
            {
                _bufferContext.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
                _graphicsBuffer = _bufferContext.Allocate(this.CreateGraphics(), this.ClientRectangle);
                _graphicsBuffer.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }
        }

        private void SetCirclePoints()
        {
            var pointStack = new Stack<PointF>();
            PointF centerPoint = new PointF(this.Width / 2f, this.Height / 2f);
            for (float i = 0; i < 360f; i += 360f / _circleCount)
            {
                _unitVector.SetValues(centerPoint, this.Width / 2 - _circleDiam, i);
                PointF newPoint = _unitVector.EndPoint;
                newPoint = new PointF(newPoint.X - _circleDiam / 2f, newPoint.Y - _circleDiam / 2f);
                pointStack.Push(newPoint);
            }
            _circlePoints = pointStack.ToArray();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            _graphicsBuffer.Graphics.Clear(this.BackColor);
            for (int i = 0; i < _circlePoints.Length; i++) 
            {
                if (_circleIndex == i)
                {
                    _graphicsBuffer.Graphics.FillEllipse(_moveCircleBrush, _circlePoints[i].X,
                        _circlePoints[i].Y, _circleDiam, _circleDiam);
                }
                else
                {
                    _graphicsBuffer.Graphics.FillEllipse(_circleBrush, _circlePoints[i].X,
                        _circlePoints[i].Y, _circleDiam, _circleDiam);
                }
            }

            _graphicsBuffer.Render(e.Graphics);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            LockAspectRatio();
            UpdateGraphicsBuffer();
            SetCirclePoints();
            _lastSize = this.Size;
        }

        private void LockAspectRatio()
        {
            if (_lastSize.Height != this.Height)
            {
                this.Width = this.Height;
            }
            else if (_lastSize.Width != this.Width)
            {
                this.Height = this.Width;
            }
        }

        [Description("The animation speed in a milliseconds interval")]
        [Category("Custom Properties")]
        public int AnimateInterval
        {
            get { return _tmrAnimate.Interval; }
            set { _tmrAnimate.Interval = value; }
        }

        [Category("Custom Properties")]
        public int CircleDiameter
        {
            get { return _circleDiam; }
            set 
            {
                _circleDiam = value;
                SetCirclePoints();
            }
        }

        [Category("Custom Properties")]
        public int CircleCount
        {
            get { return _circleCount; }
            set 
            {
                if (value < 3) _circleCount = 3;
                else _circleCount = value;
                SetCirclePoints();

            }
        }

        [Category("Custom Properties")]
        public Color MovingCircleColor
        {
            get { return _moveCircleBrush.Color; }
            set { _moveCircleBrush.Color = value; }
        }

        [Category("Custom Properties")]
        public Color StaticCircleColor
        {
            get { return _circleBrush.Color; }
            set { _circleBrush.Color = value; }
        }
		
		[Browsable(false)]
        public override Image BackgroundImage { get => base.BackgroundImage; set { } }
        [Browsable(false)]
        public override ImageLayout BackgroundImageLayout { get => base.BackgroundImageLayout; set { } }
        [Browsable(false)]
        public override Font Font { get => base.Font; set { } }
        [Browsable(false)]
        public override string Text { get => base.Text; set { } }
        [Browsable(false)]
        public override Color ForeColor { get => base.ForeColor; set { } }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            _tmrAnimate.Enabled = this.Enabled;
        }
    }

    struct UnitVector
    {
        private double _rise, _run;
        private PointF _startPoint;

        public void SetValues(PointF startPoint, int length, double angleInDegrees)
        {
            _startPoint = startPoint;
            double radian = Math.PI * angleInDegrees / 180.0;
            if (radian > Math.PI * 2) radian = Math.PI * 2;
            if (radian < 0) radian = 0;
            _rise = _run = length;
            _rise = Math.Sin(radian) * _rise;
            _run = Math.Cos(radian) * _run;
        }

        public PointF EndPoint
        {
            get
            {
                float xPos = (float)(_startPoint.Y + _rise);
                float yPos = (float)(_startPoint.X + _run);
                return new PointF(yPos, xPos);
            }
        }
    }
}
