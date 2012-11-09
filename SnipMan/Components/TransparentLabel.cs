using System.Windows.Forms;
using System.Drawing;

namespace SnipMan.Components
{
    public partial class TransparentLabel : Label
    {
        public TransparentLabel()
        {
            InitializeComponent();
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
        }

        protected override CreateParams CreateParams {
            get 
            {
                CreateParams parms = base.CreateParams;
                parms.ExStyle |= 0x20;  // Turn on WS_EX_TRANSPARENT
                return parms;
            }
        }
    }
    public partial class TransparentLinkLabel : LinkLabel
    {
        public TransparentLinkLabel()
        {
            //InitializeComponent();
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parms = base.CreateParams;
                parms.ExStyle |= 0x20;  // Turn on WS_EX_TRANSPARENT
                return parms;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle border = new Rectangle(0, 0, e.ClipRectangle.Width-1, e.ClipRectangle.Height-1);
            Pen whitePen = new Pen(Color.White, 1);
            e.Graphics.DrawRectangle(whitePen, border);
        }
    }
}