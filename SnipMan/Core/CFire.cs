using System;
using System.Drawing;

namespace SnipMan.Core
{
	public class CFire
	{
        public bool LessFire;
        public bool IsSmoke;
        public bool Filter;

        byte[] _scrn;
		Bitmap _scrnBmp;
		int[] _mtrx;

        const int Width = 100;
        const int Height = 100;        

		public CFire(){
            _scrn = new byte[(Height * Width) * 4];//[40000];
            _scrnBmp = new Bitmap(Width, Height);//new Bitmap(100,100);
            LessFire = false;
            IsSmoke = false;
            Filter = false;
		    _mtrx = new int[9] { -1, 0, 1, -101, -100, -99, 99, 100, 101}; //, 496, 400, 404};
			return;
		}
		public Image GetImage(){
			MemToBmp();
			return (Image)_scrnBmp;
		}
		public Image ByteArrayToImage()
		{
			return (Image)_scrnBmp;
		}
		public void MemToBmp(){
		    for(int x = 0; x < _scrn.Length; x+=4){
					int loc = x / 4;
                    int lx = (loc % Width);
                    int ly = (((loc) - lx) / Height);
				    int j1;
				    if(ly == 0){
					        j1 = 255 << 24;
					        j1 |= (int)0;
                            _scrnBmp.SetPixel(lx, ly, Color.FromArgb(j1));
					        continue; }
                    if (ly < Height - 3 &&
                        (_scrn[(loc * 4) + 2] <= 70) && Filter)
                    {
                        j1 = 255 << 24;
                    }
                    else
                    {
                        j1 = 255 << 24;
					    j1 |= ((int)(_scrn[x + 1])) << 16;
					    j1 |= ((int)(_scrn[x + 2])) << 8;
					    j1 |= ((int)(_scrn[x + 3])) << 0;
                    }
					_scrnBmp.SetPixel(lx, ly, Color.FromArgb(j1));
				}
				return;
		}
		public Bitmap GetBitmap(){
			return _scrnBmp;
		}
		public void SetFire(){
			Random rnd = new Random();
			int randomColor = rnd.Next(256);
			for(int g = 0; g < Width * 4; g += 4){
				_scrn[39600 + g] = (Byte)255;
                _scrn[39600 + (g + 1)] = (Byte)(IsSmoke ? randomColor : 255);
				_scrn[39600 + (g + 2)] = (Byte)randomColor;
                _scrn[39600 + (g + 3)] = (Byte)(IsSmoke ? randomColor : 0);
				randomColor = rnd.Next(256);
			}
			return;
		}
		public void Update(){
			SetFire();
            for (int g = 0; g < _scrn.Length; g += 4)
			{
                if ((g / 4) % Width == 0 || (g / 4) % Width == (Width - 1))
                {
                    _scrn[g] = (byte)255;
                    _scrn[(g + 1)] = (byte)0;
                    _scrn[(g + 2)] = (byte)0;
                    _scrn[(g + 3)] = (byte)0;
                    continue;
                }
                Color temp = ProcessPixel(g / 4);
                _scrn[g] = (byte)255;
                _scrn[(g + 1)] = temp.R;
                _scrn[(g + 2)] = temp.G;
                _scrn[(g + 3)] = temp.B;
			}
			return;
		}
		private Color ProcessPixel(int p){
			int[] j = new int[6];
			int finalColor = 0;
			p += Width;
			for(int i = 0; i < _mtrx.Length; i++)
			{
                if (((p + _mtrx[i]) * 4) > ((Height * Width) * 4) - 5 || ((p + _mtrx[i]) * 4) < 0)
				{
                    j[0] = 0; j[1] = 0; j[2] = 0;
				}else{
                    j[0] = ((int)(_scrn[((p + _mtrx[i]) * 4) + 1]));
                    j[1] = ((int)(_scrn[((p + _mtrx[i]) * 4) + 2]));
                    j[2] = ((int)(_scrn[((p + _mtrx[i]) * 4) + 3]));
				}
                j[3] += j[0]; j[4] += j[1]; j[5] += j[2];
			}
            j[3] /= _mtrx.Length + (LessFire ? 1 : 0);
            j[4] /= _mtrx.Length + (LessFire ? 1 : 0);
            j[5] /= _mtrx.Length + (LessFire ? 1 : 0);
            finalColor |= 255 << 24;
            finalColor |= j[3] << 16;
            finalColor |= j[4] << 8;
            finalColor |= j[5] << 0;
			return Color.FromArgb(finalColor);			
		}
	}
}
