using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SnipMan.Components
{
    /// <summary>
    ///   Defines methods for performing operations on RichTextBox.
    /// </summary>
    ///
    /// <remarks>
    ///   <para>
    ///     The methods in this class could be defined as "extension methods" but
    ///     for efficiency I'd like to retain some state between calls - for
    ///     example the handle on the richtextbox or the buffer and structure for
    ///     the EM_SETCHARFORMAT message, which can be called many times in quick
    ///     succession.
    ///   </para>
    ///
    ///   <para>
    ///     We define these in a separate class for speed and efficiency. For the
    ///     RichTextBox, in order to make a change in format of some portion of
    ///     the text, the app must select the text.  When the RTB has focus, it
    ///     will scroll when the selection is updated.  If we want to retain state
    ///     while highlighting text then, we'll have to restore the scroll state
    ///     after a highlight is applied.  But this will produce an ugly UI effect
    ///     where the scroll jumps forward and back repeatedly.  To avoid that, we
    ///     need to suppress updates to the RTB, using the WM_SETREDRAW message.
    ///   </para>
    ///
    ///   <para>
    ///     As a complement to that, we also have some speedy methods to get and
    ///     set the scroll state, and the selection state.
    ///   </para>
    ///
    /// </remarks>
    [ToolboxBitmap(typeof(RichTextBox))]
    partial class RichTextBoxEx : RichTextBox
    {
        private User32.Charformat _charFormat;
        private IntPtr _lParam1;

        private int _savedScrollLine;
        private int _savedSelectionStart;
        private int _savedSelectionEnd;
        private Pen _borderPen;
        private StringFormat _stringDrawingFormat;

        public RichTextBoxEx()
        {
            _charFormat = new User32.Charformat()
            {
                cbSize = Marshal.SizeOf(typeof(User32.Charformat)),
                szFaceName = new char[32]
            };

            _lParam1 = Marshal.AllocCoTaskMem(_charFormat.cbSize);

            // defaults
            // This should be changed to support the INI file.
            NumberFont = new Font("Courier New",
                                                8.0F,
                                                FontStyle.Regular,
                                                GraphicsUnit.Point, ((byte)(0)));

            NumberColor = Color.FromName("DarkGray");
            NumberLineCounting = LineCounting.Crlf;
            NumberAlignment = StringAlignment.Near;
            NumberBorder = SystemColors.ControlDark;
            NumberBorderThickness = 1;
            NumberPadding = 2;
            NumberBackground1 = SystemColors.Window;
            NumberBackground2 = SystemColors.Window;
            SetStringDrawingFormat();
        }

        ~RichTextBoxEx()
        {
            // Free the allocated memory
            Marshal.FreeCoTaskMem(_lParam1);
        }

        private void SetStringDrawingFormat()
        {
            _stringDrawingFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = NumberAlignment,
                Trimming = StringTrimming.None,
            };
        }


        protected override void OnTextChanged(EventArgs e)
        {
            NeedRecomputeOfLineNumbers();
            base.OnTextChanged(e);
        }

        public void BeginUpdate()
        {
            User32.SendMessage(Handle, (int)User32.Msgs.WmSetredraw, 0, IntPtr.Zero);
        }

        public void EndUpdate()
        {
            User32.SendMessage(Handle, (int)User32.Msgs.WmSetredraw, 1, IntPtr.Zero);
        }


        public IntPtr BeginUpdateAndSuspendEvents()
        {
            // Stop redrawing:
            User32.SendMessage(Handle, (int)User32.Msgs.WmSetredraw, 0, IntPtr.Zero);
            // Stop sending of events:
            IntPtr eventMask = User32.SendMessage(Handle, User32.Msgs.EmGeteventmask, 0, IntPtr.Zero);

            return eventMask;
        }

        public void EndUpdateAndResumeEvents(IntPtr eventMask)
        {
            // turn on events
            User32.SendMessage(Handle, User32.Msgs.EmSeteventmask, 0, eventMask);
            // turn on redrawing
            User32.SendMessage(Handle, User32.Msgs.WmSetredraw, 1, IntPtr.Zero);
            NeedRecomputeOfLineNumbers();
            Invalidate();
        }



        public void GetSelection(out int start, out int end)
        {
            User32.SendMessageRef(Handle, (int)User32.Msgs.EmGetsel, out start, out end);
        }

        public void SetSelection(int start, int end)
        {
            User32.SendMessage(Handle, (int)User32.Msgs.EmSetsel, start, end);
        }

        public void BeginUpdateAndSaveState()
        {
            User32.SendMessage(Handle, (int)User32.Msgs.WmSetredraw, 0, IntPtr.Zero);
            // save scroll position
            _savedScrollLine = FirstVisibleDisplayLine;

            // save selection
            GetSelection(out _savedSelectionStart, out _savedSelectionEnd);
        }

        public void EndUpdateAndRestoreState()
        {
            // restore scroll position
            int line1 = FirstVisibleDisplayLine;
            Scroll(_savedScrollLine - line1);

            // restore the selection/caret
            SetSelection(_savedSelectionStart, _savedSelectionEnd);

            // allow redraw
            User32.SendMessage(Handle, (int)User32.Msgs.WmSetredraw, 1, IntPtr.Zero);

            // explicitly ask for a redraw?
            Refresh();
        }

        private String _sformat;
        private int _ndigits;
        private int _lnw = -1;
        private int LineNumberWidth
        {
            get
            {
                if (_lnw > 0) return _lnw;
                if (NumberLineCounting == LineCounting.Crlf)
                {
                    _ndigits = (CharIndexForTextLine.Length == 0)
                        ? 1
                        : (int)(1 + Math.Log((double)CharIndexForTextLine.Length, 10));
                }
                else
                {
                    int n = GetDisplayLineCount();
                    _ndigits = (n == 0)
                        ? 1
                        : (int)(1 + Math.Log((double)n, 10));
                }
                var s = new String('0', _ndigits);
                var b = new Bitmap(400, 400); // in pixels
                var g = Graphics.FromImage(b);
                SizeF size = g.MeasureString(s, NumberFont);
                g.Dispose();
                _lnw = NumberPadding * 2 + 4 + (int)(size.Width + 0.5 + NumberBorderThickness);
                _sformat = "{0:D" + _ndigits + "}";
                return _lnw;
            }
        }


        public bool LineNumbers;
        public bool ShowLineNumbers
        {
            get
            {
                return LineNumbers;
            }
            set
            {
                if (value == LineNumbers) return;
                SetLeftMargin(value ? LineNumberWidth + Margin.Left : Margin.Left);
                LineNumbers = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        private void NeedRecomputeOfLineNumbers()
        {
            //System.Console.WriteLine("Need Recompute of line numbers...");
            _charIndexForTextLine = null;
            _text2 = null;
            _lnw = -1;

            if (_paintingDisabled) return;

            User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
        }

        private Font _numberFont;
        public Font NumberFont
        {
            get { return _numberFont; }
            set
            {
                if (Equals(_numberFont, value)) return;
                _lnw = -1;
                _numberFont = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        private LineCounting _numberLineCounting;
        public LineCounting NumberLineCounting
        {
            get { return _numberLineCounting; }
            set
            {
                if (_numberLineCounting == value) return;
                _lnw = -1;
                _numberLineCounting = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        private StringAlignment _numberAlignment;
        public StringAlignment NumberAlignment
        {
            get { return _numberAlignment; }
            set
            {
                if (_numberAlignment == value) return;
                _numberAlignment = value;
                SetStringDrawingFormat();
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        private Color _numberColor;
        public Color NumberColor
        {
            get { return _numberColor; }
            set
            {
                if (_numberColor.ToArgb() == value.ToArgb()) return;
                _numberColor = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        private bool _numberLeadingZeroes;
        public bool NumberLeadingZeroes
        {
            get { return _numberLeadingZeroes; }
            set
            {
                if (_numberLeadingZeroes == value) return;
                _numberLeadingZeroes = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        private Color _numberBorder;
        public Color NumberBorder
        {
            get { return _numberBorder; }
            set
            {
                if (_numberBorder.ToArgb() == value.ToArgb()) return;
                _numberBorder = value;
                NewBorderPen();
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        private int _numberPadding;
        public int NumberPadding
        {
            get { return _numberPadding; }
            set
            {
                if (_numberPadding == value) return;
                _lnw = -1;
                _numberPadding = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        private Single _numberBorderThickness;
        public Single NumberBorderThickness
        {
            get { return _numberBorderThickness; }
            set
            {
                if (_numberBorderThickness == value) return;
                _lnw = -1;
                _numberBorderThickness = value;
                NewBorderPen();
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        private Color _numberBackground1;
        public Color NumberBackground1
        {
            get { return _numberBackground1; }
            set
            {
                if (_numberBackground1.ToArgb() == value.ToArgb()) return;
                _numberBackground1 = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        private Color _numberBackground2;
        public Color NumberBackground2
        {
            get { return _numberBackground2; }
            set
            {
                if (_numberBackground2.ToArgb() == value.ToArgb()) return;
                _numberBackground2 = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }


        private bool _paintingDisabled;
        public void SuspendLineNumberPainting()
        {
            _paintingDisabled = true;
        }
        public void ResumeLineNumberPainting()
        {
            _paintingDisabled = false;
        }


        private void NewBorderPen()
        {
            _borderPen = new Pen(NumberBorder) {Width = NumberBorderThickness};
            _borderPen.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);
        }



        private DateTime _lastMsgRecd = new DateTime(1901, 1, 1);

        protected override void WndProc(ref Message m)
        {
            bool handled = false;
            switch (m.Msg)
            {
                case (int)User32.Msgs.WmPaint:
                    //System.Console.WriteLine("{0}", User32.Mnemonic(m.Msg));
                    //System.Console.Write(".");
                    if (_paintingDisabled) return;
                    if (LineNumbers)
                    {
                        base.WndProc(ref m);
                        PaintLineNumbers();
                        handled = true;
                    }
                    break;

                case (int)User32.Msgs.WmChar:
                    // the text is being modified
                    NeedRecomputeOfLineNumbers();
                    break;

                //                 case (int)User32.Msgs.EM_POSFROMCHAR:
                //                 case (int)User32.Msgs.WM_GETDLGCODE:
                //                 case (int)User32.Msgs.WM_ERASEBKGND:
                //                 case (int)User32.Msgs.OCM_COMMAND:
                //                 case (int)User32.Msgs.OCM_NOTIFY:
                //                 case (int)User32.Msgs.EM_CHARFROMPOS:
                //                 case (int)User32.Msgs.EM_LINEINDEX:
                //                 case (int)User32.Msgs.WM_NCHITTEST:
                //                 case (int)User32.Msgs.WM_SETCURSOR:
                //                 case (int)User32.Msgs.WM_KEYUP:
                //                 case (int)User32.Msgs.WM_KEYDOWN:
                //                 case (int)User32.Msgs.WM_MOUSEMOVE:
                //                 case (int)User32.Msgs.WM_MOUSEACTIVATE:
                //                 case (int)User32.Msgs.WM_NCMOUSEMOVE:
                //                 case (int)User32.Msgs.WM_NCMOUSEHOVER:
                //                 case (int)User32.Msgs.WM_NCMOUSELEAVE:
                //                 case (int)User32.Msgs.WM_NCLBUTTONDOWN:
                //                     break;
                //
                //                   default:
                //                       // divider
                //                       var now = DateTime.Now;
                //                       if ((now - _lastMsgRecd) > TimeSpan.FromMilliseconds(850))
                //                           System.Console.WriteLine("------------ {0}", now.ToString("G"));
                //                       _lastMsgRecd = now;
                //
                //                       System.Console.WriteLine("{0}", User32.Mnemonic(m.Msg));
                //                       break;
            }

            if (!handled)
                base.WndProc(ref m);
        }


        int _lastWidth = 0;
        private void PaintLineNumbers()
        {
            //System.Console.WriteLine(">> PaintLineNumbers");
            // To reduce flicker, double-buffer the output

            if (_paintingDisabled) return;

            int w = LineNumberWidth;
            if (w != _lastWidth)
            {
                //System.Console.WriteLine("  WIDTH change {0} != {1}", _lastWidth, w);
                SetLeftMargin(w + Margin.Left);
                _lastWidth = w;
                // Don't bother painting line numbers - the margin isn't wide enough currently.
                // Ask for a new paint, and paint them next time round.
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
                return;
            }

            // Create a Bitmap that is the size of the line numbering margin
            Bitmap buffer = new Bitmap(w, Bounds.Height);
            Graphics g = Graphics.FromImage(buffer);

            Brush forebrush = new SolidBrush(NumberColor);
            // This rect is the size of the entire margin
            var rect = new Rectangle(0, 0, w, Bounds.Height);

            bool wantDivider = NumberBackground1.ToArgb() == NumberBackground2.ToArgb();
            Brush backBrush = (wantDivider)
                ? (Brush)new SolidBrush(NumberBackground2)
                : SystemBrushes.Window;

            // draw background color
            g.FillRectangle(backBrush, rect);

            int n = (NumberLineCounting == LineCounting.Crlf)
                ? NumberOfVisibleTextLines
                : NumberOfVisibleDisplayLines;

            int first = (NumberLineCounting == LineCounting.Crlf)
                ? FirstVisibleTextLine
                : FirstVisibleDisplayLine + 1;

            int py = 0;
            int w2 = w - 2 - (int)NumberBorderThickness;
            //LinearGradientBrush brush;
            Pen dividerPen = new Pen(NumberColor);

            // Do each line.
            for (int i = 0; i <= n; i++)
            {
                int ix = first + i; // Visible line + Top Line
                int c = (NumberLineCounting == LineCounting.Crlf)
                    ? GetCharIndexForTextLine(ix)
                    : GetCharIndexForDisplayLine(ix) - 1;

                var p = GetPosFromCharIndex(c + 1);

                Rectangle r4 = Rectangle.Empty;

                if (i == n) // last line?, this takes up the slack. I don't like this method.
                {
                    if (Bounds.Height <= py) continue;
                    r4 = new Rectangle(1, py, w2, Bounds.Height - py);
                }
                else
                {
                    if (p.Y <= py) continue;
                    r4 = new Rectangle(1, py, w2, p.Y - py);
                }
                
                // Draws Gradiant Look
                //{
                // new brush each time for gradient across variable rect sizes
                //brush = new LinearGradientBrush(r4,
                //                                 NumberBackground1,
                //                                 NumberBackground2,
                //                                 LinearGradientMode.Vertical);
                //g.FillRectangle(brush, r4);
                //}

                if (NumberLineCounting == LineCounting.Crlf) ix++;

                // conditionally slide down
                if (NumberAlignment == StringAlignment.Near)
                    rect.Offset(0, 3);

                // Draw Line Number
                var s = (NumberLeadingZeroes) ? String.Format(_sformat, ix) : ix.ToString();
                g.DrawString(s, NumberFont, forebrush, r4, _stringDrawingFormat);

                py = p.Y;
            }

            // ???
            if (NumberBorderThickness != 0.0)
            {
                int t = (int)(w - (NumberBorderThickness + 0.5) / 2) - 1;
                g.DrawLine(_borderPen, t, 0, t, Bounds.Height);
                //g.DrawLine(_borderPen, w-2, 0, w-2, this.Bounds.Height);
            }

            // paint that buffer to the screen
            Graphics g1 = CreateGraphics();
            g1.DrawImage(buffer, new Point(0, 0)); // This actually draws it to the control.
            g1.Dispose();
            g.Dispose();
        }



        private int GetCharIndexFromPos(int x, int y)
        {
            var p = new User32.Pointl { X = x, Y = y };
            int rawSize = Marshal.SizeOf(typeof(User32.Pointl));
            IntPtr lParam = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(p, lParam, false);
            int r = User32.SendMessage(Handle, (int)User32.Msgs.EmCharfrompos, 0, lParam);
            Marshal.FreeHGlobal(lParam);
            return r;
        }


        private Point GetPosFromCharIndex(int ix)
        {
            int rawSize = Marshal.SizeOf(typeof(User32.Pointl));
            IntPtr wParam = Marshal.AllocHGlobal(rawSize);
            int r = User32.SendMessage(Handle, (int)User32.Msgs.EmPosfromchar, (int)wParam, ix);

            User32.Pointl p1 = (User32.Pointl)Marshal.PtrToStructure(wParam, typeof(User32.Pointl));

            Marshal.FreeHGlobal(wParam);
            var p = new Point { X = p1.X, Y = p1.Y };
            return p;
        }


        private int GetLengthOfLineContainingChar(int charIndex)
        {
            int r = User32.SendMessage(Handle, (int)User32.Msgs.EmLinelength, 0, 0);
            return r;
        }

        private int GetLineFromChar(int charIndex)
        {
            return User32.SendMessage(Handle, (int)User32.Msgs.EmLinefromchar, charIndex, 0);
        }

        private int GetCharIndexForDisplayLine(int line)
        {
            return User32.SendMessage(Handle, (int)User32.Msgs.EmLineindex, line, 0);
        }

        private int GetDisplayLineCount()
        {
            return User32.SendMessage(Handle, (int)User32.Msgs.EmGetlinecount, 0, 0);
        }


        /// <summary>
        ///   Sets the color of the characters in the given range.
        /// </summary>
        ///
        /// <remarks>
        /// Calling this is equivalent to calling
        /// <code>
        ///   richTextBox.Select(start, end-start);
        ///   this.richTextBox1.SelectionColor = color;
        /// </code>
        /// ...but without the error and bounds checking.
        /// </remarks>
        ///
        public void SetSelectionColor(int start, int end, Color color)
        {
            User32.SendMessage(Handle, (int)User32.Msgs.EmSetsel, start, end);

            _charFormat.dwMask = 0x40000000;
            _charFormat.dwEffects = 0;
            _charFormat.crTextColor = ColorTranslator.ToWin32(color);

            Marshal.StructureToPtr(_charFormat, _lParam1, false);
            User32.SendMessage(Handle, (int)User32.Msgs.EmSetcharformat, User32.ScfSelection, _lParam1);
        }


        private void SetLeftMargin(int widthInPixels)
        {
            User32.SendMessage(Handle, (int)User32.Msgs.EmSetmargins, User32.EcLeftmargin,
                               widthInPixels);
        }

        public Tuple<int, int> GetMargins()
        {
            int r = User32.SendMessage(Handle, (int)User32.Msgs.EmGetmargins, 0, 0);
            return Tuple.New(r & 0x0000FFFF, (int)((r >> 16) & 0x0000FFFF));
        }

        public void Scroll(int delta)
        {
            User32.SendMessage(Handle, (int)User32.Msgs.EmLinescroll, 0, delta);
        }


        private int FirstVisibleDisplayLine
        {
            get
            {
                return User32.SendMessage(Handle, (int)User32.Msgs.EmGetfirstvisibleline, 0, 0);
            }
            set
            {
                // scroll
                int current = FirstVisibleDisplayLine;
                int delta = value - current;
                User32.SendMessage(Handle, (int)User32.Msgs.EmLinescroll, 0, delta);
            }
        }

        private int NumberOfVisibleDisplayLines
        {
            get
            {
                int topIndex = GetCharIndexFromPosition(new Point(1, 1));
                int bottomIndex = GetCharIndexFromPosition(new Point(1, Height - 1));
                int topLine = GetLineFromCharIndex(topIndex);
                int bottomLine = GetLineFromCharIndex(bottomIndex);
                int n = bottomLine - topLine + 1;
                return n;
            }
        }

        private int FirstVisibleTextLine
        {
            get
            {
                int c = GetCharIndexFromPos(1, 1);
                for (int i = 0; i < CharIndexForTextLine.Length; i++)
                {
                    if (c < CharIndexForTextLine[i]) return i;
                }
                return CharIndexForTextLine.Length;
            }
        }

        private int LastVisibleTextLine
        {
            get
            {
                int c = GetCharIndexFromPos(1, Bounds.Y + Bounds.Height);
                for (int i = 0; i < CharIndexForTextLine.Length; i++)
                {
                    if (c < CharIndexForTextLine[i]) return i;
                }
                return CharIndexForTextLine.Length;
            }
        }

        private int NumberOfVisibleTextLines
        {
            get
            {
                return LastVisibleTextLine - FirstVisibleTextLine;
            }
        }


        public int FirstVisibleLine
        {
            get
            {
                if (NumberLineCounting == LineCounting.Crlf)
                    return FirstVisibleTextLine;
                else
                    return FirstVisibleDisplayLine;
            }
        }

        public int NumberOfVisibleLines
        {
            get
            {
                if (NumberLineCounting == LineCounting.Crlf)
                    return NumberOfVisibleTextLines;
                else
                    return NumberOfVisibleDisplayLines;
            }
        }

        private int GetCharIndexForTextLine(int ix)
        {
            if (ix >= CharIndexForTextLine.Length) return 0;
            if (ix < 0) return 0;
            return CharIndexForTextLine[ix];
        }



        // The char index is expensive to compute.

        private int[] _charIndexForTextLine;
        private int[] CharIndexForTextLine
        {
            get
            {
                if (_charIndexForTextLine == null)
                {
                    var list = new List<int>();
                    int ix = 0;
                    foreach (var c in Text2)
                    {
                        if (c == '\n') list.Add(ix);
                        ix++;
                    }
                    _charIndexForTextLine = list.ToArray();
                }
                return _charIndexForTextLine;
            }

        }


        private String _text2;
        private String Text2
        {
            get { return _text2 ?? (_text2 = Text); }
        }

        private bool CompareHashes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;  // they are equal
        }



        public enum LineCounting
        {
            Crlf,
            AsDisplayed
        }

    }



    public static class Tuple
    {
        // Allows Tuple.New(1, "2") instead of new Tuple<int, string>(1, "2")
        public static Tuple<T1, T2> New<T1, T2>(T1 v1, T2 v2)
        {
            return new Tuple<T1, T2>(v1, v2);
        }
    }

    public class Tuple<T1, T2>
    {
        public Tuple(T1 v1, T2 v2)
        {
            V1 = v1;
            V2 = v2;
        }

        public T1 V1 { get; set; }
        public T2 V2 { get; set; }
    }
}
