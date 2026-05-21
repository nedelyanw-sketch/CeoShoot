using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CeoShootMain
{
    public class SetupForm : Form
    {
        public bool IsAutostartEnabled { get; private set; } = true;

        private readonly Color _bgColor = Color.FromArgb(20, 20, 23);
        private readonly Color _panelColor = Color.FromArgb(28, 28, 32);
        private readonly Color _accentColor = Color.FromArgb(88, 101, 242);
        private readonly Color _accentHover = Color.FromArgb(71, 82, 196);
        private readonly Color _textColor = Color.FromArgb(240, 240, 245);
        private readonly Color _mutedText = Color.FromArgb(140, 140, 150);

        private Rectangle _closeBtnRect;
        private Rectangle _toggleRect;
        private Rectangle _actionBtnRect;

        private bool _isCloseHovered = false;
        private bool _isActionHovered = false;
        private bool _isToggleHovered = false;
        private bool _isDragging = false;
        private Point _dragStart;

        public SetupForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(480, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = _bgColor;
            this.DoubleBuffered = true;

            _closeBtnRect = new Rectangle(this.Width - 40, 12, 25, 25);
            _toggleRect = new Rectangle(40, 185, 46, 24);
            _actionBtnRect = new Rectangle(this.Width - 180, 245, 140, 42);

            this.MouseDown += SetupForm_MouseDown;
            this.MouseMove += SetupForm_MouseMove;
            this.MouseUp += SetupForm_MouseUp;
            this.MouseLeave += (s, e) => ResetHoverStates();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (Pen borderPen = new Pen(Color.FromArgb(45, 45, 50), 1))
            {
                g.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
            }

            using (Font titleFont = new Font("Segoe UI", 18, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(_textColor))
            {
                g.DrawString("CEOSHOOT", titleFont, textBrush, 35, 30);
            }

            using (SolidBrush dotBrush = new SolidBrush(_accentColor))
            {
                g.FillEllipse(dotBrush, 182, 46, 6, 6);
            }

            Color closeColor = _isCloseHovered ? Color.FromArgb(240, 71, 71) : _mutedText;
            using (Font closeFont = new Font("Segoe UI", 11, FontStyle.Bold))
            using (SolidBrush closeBrush = new SolidBrush(closeColor))
            {
                g.DrawString("✕", closeFont, closeBrush, _closeBtnRect.X + 4, _closeBtnRect.Y + 2);
            }

            Rectangle infoCard = new Rectangle(35, 85, this.Width - 70, 75);
            DrawRoundedRectangle(g, infoCard, 8, _panelColor);

            string description = "Press [PrintScreen] to capture anything instantly.\nDrag to select target area, then press Ctrl+C or Ctrl+S.";
            using (Font descFont = new Font("Segoe UI", 9.5f, FontStyle.Regular))
            using (SolidBrush descBrush = new SolidBrush(_mutedText))
            {
                g.DrawString(description, descFont, descBrush, infoCard.X + 15, infoCard.Y + 18);
            }

            Color currentToggleBg = IsAutostartEnabled ? _accentColor : Color.FromArgb(60, 60, 65);
            if (_isToggleHovered && !IsAutostartEnabled) currentToggleBg = Color.FromArgb(75, 75, 80);
            DrawRoundedRectangle(g, _toggleRect, 12, currentToggleBg);

            int thumbX = IsAutostartEnabled ? _toggleRect.Right - 21 : _toggleRect.X + 3;
            Rectangle thumbRect = new Rectangle(thumbX, _toggleRect.Y + 3, 18, 18);
            g.FillEllipse(Brushes.White, thumbRect);

            using (Font labelFont = new Font("Segoe UI Semibold", 10, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(_textColor))
            {
                g.DrawString("Launch CEOSHOOT on Windows startup", labelFont, textBrush, _toggleRect.Right + 15, _toggleRect.Y + 2);
            }

            Color currentBtnColor = _isActionHovered ? _accentHover : _accentColor;
            DrawRoundedRectangle(g, _actionBtnRect, 6, currentBtnColor);

            using (Font btnFont = new Font("Segoe UI", 10, FontStyle.Bold))
            using (SolidBrush btnTextBrush = new SolidBrush(Color.White))
            {
                string btnText = "LET'S SHOOT";
                Size textSize = TextRenderer.MeasureText(btnText, btnFont);
                int tx = _actionBtnRect.X + (_actionBtnRect.Width - textSize.Width) / 2;
                int ty = _actionBtnRect.Y + (_actionBtnRect.Height - textSize.Height) / 2;
                g.DrawString(btnText, btnFont, btnTextBrush, tx, ty);
            }
        }

        private void DrawRoundedRectangle(Graphics g, Rectangle rect, int radius, Color color)
        {
            using (GraphicsPath path = new GraphicsPath())
            using (SolidBrush brush = new SolidBrush(color))
            {
                int diameter = radius * 2;
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseAllFigures();
                g.FillPath(brush, path);
            }
        }

        private void SetupForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_closeBtnRect.Contains(e.Location))
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
                else if (_toggleRect.Contains(e.Location) || new Rectangle(_toggleRect.Right, _toggleRect.Y, 250, _toggleRect.Height).Contains(e.Location))
                {
                    IsAutostartEnabled = !IsAutostartEnabled;
                    this.Invalidate();
                }
                else if (_actionBtnRect.Contains(e.Location))
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    _isDragging = true;
                    _dragStart = e.Location;
                }
            }
        }

        private void SetupForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - _dragStart.X, p.Y - _dragStart.Y);
                return;
            }

            bool closeChanged = UpdateHoverState(ref _isCloseHovered, _closeBtnRect.Contains(e.Location));
            bool actionChanged = UpdateHoverState(ref _isActionHovered, _actionBtnRect.Contains(e.Location));
            bool toggleChanged = UpdateHoverState(ref _isToggleHovered, _toggleRect.Contains(e.Location));

            if (_isCloseHovered || _isActionHovered || _isToggleHovered || new Rectangle(_toggleRect.Right, _toggleRect.Y, 250, _toggleRect.Height).Contains(e.Location))
            {
                this.Cursor = Cursors.Hand;
            }
            else
            {
                this.Cursor = Cursors.Default;
            }

            if (closeChanged || actionChanged || toggleChanged)
            {
                this.Invalidate();
            }
        }

        private void SetupForm_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private bool UpdateHoverState(ref bool stateVariable, bool isCurrentlyInside)
        {
            if (stateVariable != isCurrentlyInside)
            {
                stateVariable = isCurrentlyInside;
                return true;
            }
            return false;
        }

        private void ResetHoverStates()
        {
            _isCloseHovered = false;
            _isActionHovered = false;
            _isToggleHovered = false;
            this.Invalidate();
        }
    }
}