using System;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace LogicService.Drawing
{
    public class ColorCollection
    {
        public string Name { get; set; }

        public Color Color { get; set; }

        public SolidColorBrush ColorBrush { get; set; }


        public static Color Black = Color.FromArgb(255, 0, 0, 0);
        public static Color White = Color.FromArgb(255, 255, 255, 255);
        public static Color SilveryWhite = Color.FromArgb(255, 209, 210, 211);
        public static Color Gray = Color.FromArgb(255, 167, 169, 172);
        public static Color DeepGray = Color.FromArgb(255, 128, 130, 133);
        public static Color DeeperGray = Color.FromArgb(255, 88, 89, 91);

        public static Color Magenta = Color.FromArgb(255, 179, 21, 100);
        public static Color Red = Color.FromArgb(255, 230, 27, 27);
        public static Color RedOrange = Color.FromArgb(255, 255, 85, 0);
        public static Color Orange = Color.FromArgb(255, 255, 170, 0);
        public static Color Gold = Color.FromArgb(255, 255, 206, 0);
        public static Color Yellow = Color.FromArgb(255, 255, 230, 0);

        public static Color GrassGreen = Color.FromArgb(255, 162, 230, 27);
        public static Color Green = Color.FromArgb(255, 38, 230, 0);
        public static Color DeepGreen = Color.FromArgb(255, 0, 128, 85);
        public static Color Cyan = Color.FromArgb(255, 0, 170, 204);
        public static Color Blue = Color.FromArgb(255, 0, 76, 230);
        public static Color IndigoBlue = Color.FromArgb(255, 61, 0, 184);

        public static Color Violet = Color.FromArgb(255, 102, 0, 204);
        public static Color Purple = Color.FromArgb(255, 96, 0, 128);
        public static Color Beige = Color.FromArgb(255, 252, 230, 201);//不对
        public static Color LightBrown = Color.FromArgb(255, 187, 145, 103);
        public static Color Brown = Color.FromArgb(255, 142, 86, 46);
        public static Color DeepBrown = Color.FromArgb(255, 97, 61, 48);

        public static Color LightPink = Color.FromArgb(255, 255, 128, 255);
        public static Color PinkOrange = Color.FromArgb(255, 255, 198, 128);
        public static Color PinkYellow = Color.FromArgb(255, 255, 255, 128);
        public static Color PinkGreen = Color.FromArgb(255, 128, 255, 158);
        public static Color PinkBlue = Color.FromArgb(255, 128, 214, 255);
        public static Color PinkPurple = Color.FromArgb(255, 188, 179, 255);

        /// <summary>
        /// ////////////////////////////////////////////////////////////////
        /// </summary>

        public static SolidColorBrush BlackBrush = new SolidColorBrush(Black);
        public static SolidColorBrush WhiteBrush = new SolidColorBrush(White);
        public static SolidColorBrush SilveryWhiteBrush = new SolidColorBrush(SilveryWhite);
        public static SolidColorBrush GrayBrush = new SolidColorBrush(Gray);
        public static SolidColorBrush DeepGrayBrush = new SolidColorBrush(DeepGray);
        public static SolidColorBrush DeeperGrayBrush = new SolidColorBrush(DeeperGray);

        public static SolidColorBrush MagentaBrush = new SolidColorBrush(Magenta);
        public static SolidColorBrush RedBrush = new SolidColorBrush(Red);
        public static SolidColorBrush RedOrangeBrush = new SolidColorBrush(RedOrange);
        public static SolidColorBrush OrangeBrush = new SolidColorBrush(Orange);
        public static SolidColorBrush GoldBrush = new SolidColorBrush(Gold);
        public static SolidColorBrush YellowBrush = new SolidColorBrush(Yellow);

        public static SolidColorBrush GrassGreenBrush = new SolidColorBrush(GrassGreen);
        public static SolidColorBrush GreenBrush = new SolidColorBrush(Green);
        public static SolidColorBrush DeepGreenBrush = new SolidColorBrush(DeepGreen);
        public static SolidColorBrush CyanBrush = new SolidColorBrush(Cyan);
        public static SolidColorBrush BlueBrush = new SolidColorBrush(Blue);
        public static SolidColorBrush IndigoBlueBrush = new SolidColorBrush(IndigoBlue);

        public static SolidColorBrush VioletBrush = new SolidColorBrush(Violet);
        public static SolidColorBrush PurpleBrush = new SolidColorBrush(Purple);
        public static SolidColorBrush BeigeBrush = new SolidColorBrush(Beige);
        public static SolidColorBrush LightBrownBrush = new SolidColorBrush(LightBrown);
        public static SolidColorBrush BrownBrush = new SolidColorBrush(Brown);
        public static SolidColorBrush DeepBrownBrush = new SolidColorBrush(DeepBrown);

        public static SolidColorBrush LightPinkBrush = new SolidColorBrush(LightPink);
        public static SolidColorBrush PinkOrangeBrush = new SolidColorBrush(PinkOrange);
        public static SolidColorBrush PinkYellowBrush = new SolidColorBrush(PinkYellow);
        public static SolidColorBrush PinkGreenBrush = new SolidColorBrush(PinkGreen);
        public static SolidColorBrush PinkBlueBrush = new SolidColorBrush(PinkBlue);
        public static SolidColorBrush PinkPurpleBrush = new SolidColorBrush(PinkPurple);

        /// <summary>
        /// Collections
        /// </summary>

        public static BrushCollection ToolColors = new BrushCollection()
        {
            BlackBrush, WhiteBrush, SilveryWhiteBrush, GrayBrush, DeepGrayBrush, DeeperGrayBrush,

            MagentaBrush, RedBrush, RedOrangeBrush, OrangeBrush, GoldBrush, YellowBrush,

            GrassGreenBrush, GreenBrush, DeepGreenBrush, CyanBrush, BlueBrush, IndigoBlueBrush,

            VioletBrush, PurpleBrush, BeigeBrush, LightBrownBrush, BrownBrush, DeepBrownBrush,

            LightPinkBrush, PinkOrangeBrush, PinkYellowBrush, PinkGreenBrush, PinkBlueBrush, PinkPurpleBrush
        };

        public static ObservableCollection<ColorCollection> PaneColors = new ObservableCollection<ColorCollection>
        {
            new ColorCollection { Color = White, ColorBrush = WhiteBrush, Name = "White" },
            new ColorCollection { Color = Black, ColorBrush = BlackBrush, Name = "Black" },
            new ColorCollection { Color = SilveryWhite, ColorBrush = SilveryWhiteBrush, Name = "SilveryWhite" },
            new ColorCollection { Color = Gray, ColorBrush = GrayBrush, Name = "Gray" },
            new ColorCollection { Color = DeepGray, ColorBrush = DeepGrayBrush, Name = "DeepGray" },
            new ColorCollection { Color = DeeperGray, ColorBrush = DeeperGrayBrush, Name = "DeeperGray" },

            new ColorCollection { Color = Magenta, ColorBrush = MagentaBrush, Name = "Magenta" },
            new ColorCollection { Color = Red, ColorBrush = RedBrush, Name = "Red" },
            new ColorCollection { Color = RedOrange, ColorBrush = RedOrangeBrush, Name = "RedOrange" },
            new ColorCollection { Color = Orange, ColorBrush = OrangeBrush, Name = "Orange" },
            new ColorCollection { Color = Gold, ColorBrush = GoldBrush, Name = "Gold" },
            new ColorCollection { Color = Yellow, ColorBrush = YellowBrush, Name = "Yellow" },

            new ColorCollection { Color = GrassGreen, ColorBrush = GrassGreenBrush, Name = "GrassGreen" },
            new ColorCollection { Color = Green, ColorBrush = GreenBrush, Name = "Green" },
            new ColorCollection { Color = DeepGreen, ColorBrush = DeepGreenBrush, Name = "DeepGreen" },
            new ColorCollection { Color = Cyan, ColorBrush = CyanBrush, Name = "Cyan" },
            new ColorCollection { Color = Blue, ColorBrush = BlueBrush, Name = "Blue" },
            new ColorCollection { Color = IndigoBlue, ColorBrush = IndigoBlueBrush, Name = "IndigoBlue" },

            new ColorCollection { Color = Violet, ColorBrush = VioletBrush, Name = "Violet" },
            new ColorCollection { Color = Purple, ColorBrush = PurpleBrush, Name = "Purple" },
            new ColorCollection { Color = Beige, ColorBrush = BeigeBrush, Name = "Beige" },
            new ColorCollection { Color = LightBrown, ColorBrush = LightBrownBrush, Name = "LightBrown" },
            new ColorCollection { Color = Brown, ColorBrush = BrownBrush, Name = "Brown" },
            new ColorCollection { Color = DeepBrown, ColorBrush = DeepBrownBrush, Name = "DeepBrown" },

            new ColorCollection { Color = LightPink, ColorBrush = LightPinkBrush, Name = "LightPink" },
            new ColorCollection { Color = PinkOrange, ColorBrush = PinkOrangeBrush, Name = "PinkOrange" },
            new ColorCollection { Color = PinkYellow, ColorBrush = PinkYellowBrush, Name = "PinkYellow" },
            new ColorCollection { Color = PinkGreen, ColorBrush = PinkGreenBrush, Name = "PinkGreen" },
            new ColorCollection { Color = PinkBlue, ColorBrush = PinkBlueBrush, Name = "PinkBlue" },
            new ColorCollection { Color = PinkPurple, ColorBrush = PinkPurpleBrush, Name = "PinkPurple" }
        };

    }
}
