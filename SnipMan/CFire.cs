using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace FireBlt
{
	public class CFire
	{
        public bool LessFire;
        public bool IsSmoke;
        public bool Filter;

        byte[] _scrn; // = new byte[3000]; // 100x100 with 3 byte color.
		Bitmap _scrnBMP; // = new Bitmap(100, 100);
		int[] _mtrx;
		public CFire(){
			_scrn = new byte[40000];
			_scrnBMP = new Bitmap(100,100);
            LessFire = false;
            IsSmoke = false;
            Filter = false;
			//_mtrx = new int[9] { -4, 0, 4, 296, 300, 304, 596, 600, 604 };
            //_mtrx = new int[9] { -396, -400, -404, 396, 400, 404, 696, 700, 704 };
            //_mtrx = new int[4] { 296, 0, 300, 596}; //, 300, 304, 596, 600, 604 };
            //_mtrx = new int[9] { -4, 0, 4, 496, 400, 404, 796, 800, 804 };
		_mtrx = new int[9] { -1, 0, 1, -101, -100, -99, 99, 100, 101}; //, 496, 400, 404};
		//_mtrx = new int[6] { -1, 0, 1, 101, -100, 99 };	    
//_mtrx = new int[9] { -4, 0, 4, -396, -400, -404, 396, 400, 404 };
			return;
		}
		public Image getImage(){
			//return Bitmap.FromHbitmap(_scrnBMP.GetHbitmap());
			//return Bitmap.FromHbitmap(_scrnBMP.GetHbitmap());
			MemToBMP();
			return (Image)_scrnBMP;
		}
		public Image byteArrayToImage()
		{
			return (Image)_scrnBMP;
			//i.PixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
			//Image returnImage = Image.FromHbitmap(i.GetHbitmap());
			//return returnImage;
		}
		public void MemToBMP(){
			//using(Bitmap j = new Bitmap(100,100)){
				int j1;
				for(int x = 0; x < _scrn.Length; x+=4){
					int loc = x / 4;
					int lx = (loc % 100);
					int ly = (((loc)-lx) / 100);
					if(ly == 0){
					        j1 = 255 << 24;
					        j1 |= (int)0;
                            _scrnBMP.SetPixel(lx, ly, Color.FromArgb(j1));
					        continue; }
                    if (ly < 97 &&
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
					_scrnBMP.SetPixel(lx, ly, Color.FromArgb(j1));
				}
				//return _scrnBMP;
				return;
			//}
		}
		public Bitmap getBitmap(){
			return _scrnBMP;
		}
		public void SetFire(){
			Random rnd = new Random();
			int randomColor = rnd.Next(256); // Gray Fire For Now
			for(int g = 0; g < 400; g += 4){
				_scrn[39600 + g] = (Byte)255;
                _scrn[39600 + (g + 1)] = (Byte)(IsSmoke ? randomColor : 255); // 255;
				_scrn[39600 + (g+2)] = (Byte)randomColor;
                _scrn[39600 + (g + 3)] = (Byte)(IsSmoke ? randomColor : 0); //0;
				randomColor = rnd.Next(256);
			}
            /*_scrn[0] = (byte)255;
            _scrn[1] = (byte)255;
            _scrn[2] = (byte)0;
            _scrn[3] = (byte)0;*/
			return;
		}
		public void Update(){
			SetFire();
            for (int g = 0; g < _scrn.Length; g+=4)
			{
                if ((g / 4) % 100 == 0 || (g / 4) % 100 == 99) {
                    _scrn[g] = (byte)255;
                    _scrn[(g + 1)] = (byte)0;
                    _scrn[(g + 2)] = (byte)0;
                    _scrn[(g + 3)] = (byte)0;
                    continue; }
                Color Temp = ProcessPixel(g / 4);
                //if (Temp.G <= 70)
                //{
                //    _scrn[g] = (byte)255;
                //    _scrn[(g + 1)] = (byte)0;
                //    _scrn[(g + 2)] = (byte)0;
                //    _scrn[(g + 3)] = (byte)0;
                //}
                //else
                //{
                    _scrn[g] = (byte)255;
                    _scrn[(g + 1)] = Temp.R;
                    _scrn[(g + 2)] = Temp.G;
                    _scrn[(g + 3)] = Temp.B;
                //}
			}
			return;
		}
		private Color ProcessPixel(int p){
			//Color PixelColor = new Color(); // Color 1 Below
			int[] j = new int[6];
			//int j1 = 0;
			int FinalColor = 0;
			p += 100;
			for(int i = 0; i < _mtrx.Length; i++)
			{
				if (((p + _mtrx[i]) * 4) > 39995 || ((p + _mtrx[i]) * 4) < 0)
				{
					//PixelColor.R = (byte)255;
					//PixelColor.G = (byte)255;
					//PixelColor.B = (byte)255;
                    //j1 = 0; //255 << 24;
					//j1 |= 255 << 16;
					//j1 |= 255 << 8;
					//j1 |= 255 << 0;
                    j[0] = 0; j[1] = 0; j[2] = 0;
				}else{
					//PixelColor.R = _scrn[(p+_mtrx[i]) * 3];
					//PixelColor.G = _scrn[((p+_mtrx[i]) * 3)+1];
					//PixelColor.B = _scrn[((p+_mtrx[i]) * 3)+2];
                    //j1 = 0; // 255 << 24;
					//j1 |= ((int)(_scrn[((p + _mtrx[i]) * 4)+1])) << 16;
					//j1 |= ((int)(_scrn[((p + _mtrx[i]) * 4)+2])) << 8;
					//j1 |= ((int)(_scrn[((p + _mtrx[i]) * 4)+3])) << 0;
                    j[0] = ((int)(_scrn[((p + _mtrx[i]) * 4) + 1]));
                    j[1] = ((int)(_scrn[((p + _mtrx[i]) * 4) + 2]));
                    j[2] = ((int)(_scrn[((p + _mtrx[i]) * 4) + 3]));
				}
				//FinalColor += j1;
                j[3] += j[0]; j[4] += j[1]; j[5] += j[2];
			}
			//FinalColor /= 9;
            j[3] /= _mtrx.Length + (LessFire ? 1 : 0);
            j[4] /= _mtrx.Length + (LessFire ? 1 : 0);
            j[5] /= _mtrx.Length + (LessFire ? 1 : 0);
	    //j[3] += 1; j[4] += 1; j[5] += 1;
            FinalColor |= 255 << 24;
            FinalColor |= j[3] << 16;
            FinalColor |= j[4] << 8;
            FinalColor |= j[5] << 0;
			return Color.FromArgb(FinalColor);			
		}
	}
}
