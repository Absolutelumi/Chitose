using ImageProcessor;
using System;
using OsuApi.Model;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ChitoseV2.Framework
{
    class OsuScoreImage
    {
        private const int AvatarSize = 128;

        private const int BackgroundHeight = 250;

        private const int BackgroundWidth = 900;

        private const int StrokeWidth = 5;

        private static Brush BlueBrush = new SolidBrush(Color.FromArgb(255, 34, 187, 221));

        private static Brush PinkBrush = new SolidBrush(Color.FromArgb(255, 238, 34, 153));

        private static Brush SilverBrush = new SolidBrush(Color.Silver);

        private static Pen WhitePen = new Pen(Color.White, StrokeWidth * 2);

        private static Pen SilverPen = new Pen(Color.Silver, StrokeWidth * 2);

        private static Pen PinkPen = new Pen(Color.FromArgb(255, 238, 34, 153), StrokeWidth * 2);

        private static Bitmap AcquireAvatar(string userId) => new Bitmap(Extensions.GetHttpStream(new Uri($"https://a.ppy.sh/{userId}")));

        private static Bitmap AcquireBackground(string beatmapId) => new Bitmap(Extensions.GetHttpStream(new Uri($"https://assets.ppy.sh/beatmaps/{beatmapId}/covers/cover.jpg")));

        private static Font rankFont = new Font("Calibri", 128, GraphicsUnit.Point);

        static OsuScoreImage()
        {
            WhitePen.LineJoin = LineJoin.Round; 
        }

        public static Bitmap CreateScorePanel(User user, Score score, Beatmap beatmap)
        {
            Bitmap background = GetRoundedBackground(AcquireBackground(beatmap.BeatmapSetId)); 
            
            using (var graphics = Graphics.FromImage(background))
            {
                graphics.InterpolationMode = InterpolationMode.High;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                DrawWhiteOverlay(background, graphics);
                DrawAvatar(graphics, user.UserID);
                DrawTitle(graphics, beatmap.Title, beatmap.Difficulty, beatmap.Stars.ToString("#.##"), beatmap.Beatmapper);
                DrawCreator(graphics, beatmap); 
                DrawUsername(user, graphics);
                if (score.Mods != Mods.NM)
                    DrawMods(score, graphics);
                if (score.PP != 0)
                    DrawPP(user, score, graphics);
                DrawRank(score, graphics);
                DrawAcc(score, graphics);
                DrawCombo(score, graphics); 
            }
            return background;
        }
        
        private static Bitmap GetRoundedBackground(Bitmap background)
        {
            Bitmap roundedBackground = new Bitmap(background.Width, background.Height);
            Brush backgroundBrush = new TextureBrush(background); 
            GraphicsPath backgroundPath = GetRoundedCorners(0, 0, background.Width, background.Height, 30);
            using (Graphics graphics = Graphics.FromImage(roundedBackground))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.FillPath(backgroundBrush, backgroundPath); 
                return roundedBackground; 
            }
        }

        private static void DrawAvatar(Graphics graphics, string userId)
        {
            GraphicsPath squarePath = new GraphicsPath();
            squarePath.AddRectangle(new Rectangle(20, BackgroundHeight - 20 - AvatarSize, AvatarSize, AvatarSize));
            graphics.FillPath(Brushes.White, squarePath); 
            using (Bitmap avatar = AcquireAvatar(userId))
            using (GraphicsPath roundedPath = GetRoundedCorners(20, BackgroundHeight - 20 - AvatarSize, AvatarSize, AvatarSize, 10))
            {
                graphics.DrawPath(WhitePen, roundedPath);
                graphics.SetClip(roundedPath);
                graphics.DrawImage(avatar, 20, BackgroundHeight - 20 - AvatarSize, AvatarSize, AvatarSize);
                graphics.ResetClip();
            }
        }

        private static void DrawTitle(Graphics graphics, string title, string difficulty, string stars, string beatmapper)
        {
            Font titleFont = new Font("Calibri", 36, GraphicsUnit.Point);
            StringFormat titleFormat = new StringFormat();
            GraphicsPath titlePath = new GraphicsPath();
            titleFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip;
            titleFormat.Trimming = StringTrimming.EllipsisCharacter;
            string difficultyString = $"[{difficulty}]";
            int maxWidth = BackgroundWidth - 40 - StrokeWidth * 2;
            SizeF difficultySize = graphics.MeasureString(difficultyString, titleFont);
            float remainingWidth = maxWidth - difficultySize.Width;
            SizeF titleSize = graphics.MeasureString(title, titleFont);
            if (titleSize.Width >= BackgroundWidth / 2 - 20)
                title = TrimTitle(title, graphics, titleFont); 
            titlePath.AddString(title + " " + difficultyString + $" {stars}*", titleFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 60 / 150, new Point(15, 10), titleFormat);
            graphics.DrawPath(WhitePen, titlePath);
            graphics.FillPath(BlueBrush, titlePath); 
        }

        private static void DrawCreator(Graphics graphics, Beatmap beatmap)
        {
            string mappedBy = "mapped by " + beatmap.Beatmapper;
            GraphicsPath creatorPath = new GraphicsPath();
            Font creatorFont = new Font("Calibri", 60, GraphicsUnit.Point);
            creatorPath.AddString(mappedBy, creatorFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 60 / 180, new Point(15, 50), new StringFormat());
            graphics.DrawPath(WhitePen, creatorPath);
            graphics.FillPath(BlueBrush, creatorPath); 
        }

        private static void DrawUsername(User user, Graphics graphics)
        {
            Font usernameFont = new Font("Calibri", 60, GraphicsUnit.Point);
            var usernameSize = usernameFont.Size; 
            GraphicsPath usernamePath = new GraphicsPath();
            usernamePath.AddString(user.Username, usernameFont.FontFamily, (int)FontStyle.Regular, 60, new Point(25 + AvatarSize, 170), new StringFormat());
            graphics.DrawPath(WhitePen, usernamePath);
            graphics.FillPath(BlueBrush, usernamePath);
        }

        private static void DrawPP(User user, Score score, Graphics graphics) 
        {
            var ppFont = new Font("Calibri", 224, GraphicsUnit.Point);
            Brush ppBrush = new SolidBrush(Color.FromArgb(150, 238, 34, 153));
            graphics.RotateTransform(30); 
            graphics.DrawString(score.PP + " pp", ppFont, ppBrush, new Point(50, 10)); 
        }

        private static void DrawAcc(Score score, Graphics graphics)
        {
            string acc = $"{score.Accuracy:0.00%}";
            Font accFont = new Font("Calibri", 60, GraphicsUnit.Point);
            GraphicsPath accPath = new GraphicsPath();
            StringFormat accFormat = new StringFormat();
            accFormat.Alignment = StringAlignment.Far; 
            accPath.AddString(acc, accFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 60 / 110, new Point(GetByRank(score, graphics), 170), accFormat);
            graphics.DrawPath(WhitePen, accPath);
            graphics.FillPath(PinkBrush, accPath); 
        }

        private static void DrawCombo(Score score, Graphics graphics)
        {
            string combo = score.Combo.ToString() + "x";
            Font comboFont = new Font("Calibri", 60, GraphicsUnit.Point);
            StringFormat comboFormat = new StringFormat();
            GraphicsPath comboPath = new GraphicsPath(); 
            comboFormat.Alignment = StringAlignment.Far;
            comboPath.AddString(combo, comboFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 60 / 110, new Point(GetByRank(score, graphics), 110), comboFormat);
            graphics.DrawPath(WhitePen, comboPath);
            graphics.FillPath(PinkBrush, comboPath);
        }

        private static void DrawMods(Score score, Graphics graphics)
        {
            string mods = score.Mods.ToString(); 
            Font modFont = new Font("Calibri", 48, GraphicsUnit.Point);
            GraphicsPath modPath = new GraphicsPath();
            modPath.AddString(mods, modFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 48 / 100, new Point(25 + AvatarSize, 120), new StringFormat());
            graphics.DrawPath(WhitePen, modPath);
            graphics.FillPath(PinkBrush, modPath); 
        }

        private static void DrawRank(Score score, Graphics graphics)
        {
            string rank = score.Rank.ToString().Contains("H") ? score.Rank.ToString().Replace("H", string.Empty) : score.Rank.ToString();
            rank = rank.Contains("X") ? rank.Replace("X", "S") : rank;
            GraphicsPath rankPath = new GraphicsPath();
            StringFormat rankFormat = new StringFormat();
            rankFormat.Alignment = StringAlignment.Far; 
            rankPath.AddString(rank, rankFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 128 / 72, new Point(900, 60), rankFormat);
            if (score.Rank == Rank.SH || score.Rank == Rank.SX)
            {
                graphics.DrawPath(PinkPen, rankPath);
                graphics.FillPath(SilverBrush, rankPath);
            }
            else
            {
                graphics.DrawPath(WhitePen, rankPath);
                graphics.FillPath(PinkBrush, rankPath);
            }   
        }


        private static void DrawWhiteOverlay(Bitmap background, Graphics graphics)
        {
            using (var imageFactory = new ImageFactory())
            using (var roundedPath = GetRoundedCorners(10, 10, BackgroundWidth - 20, BackgroundHeight - 20, 20))
            using (var blurredBackground = imageFactory.Load(background).GaussianBlur(10).Image)
            {
                graphics.SetClip(roundedPath);
                graphics.DrawImage(blurredBackground, 0, 0);
                var whiteBrush = new SolidBrush(Color.FromArgb(89, Color.White));
                graphics.FillPath(whiteBrush, roundedPath);
                graphics.ResetClip();
            }
        }

        private static GraphicsPath GetRoundedCorners(int x, int y, int width, int height, int radius)
        {
            int diameter = radius * 2;
            var graphicsPath = new GraphicsPath();
            graphicsPath.AddArc(x, y, diameter, diameter, 180, 90);
            graphicsPath.AddArc(x + width - diameter, y, diameter, diameter, 270, 90);
            graphicsPath.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90);
            graphicsPath.AddArc(x, y + height - diameter, diameter, diameter, 90, 90);
            graphicsPath.CloseFigure();
            return graphicsPath;
        }

        private static string TrimTitle(string title, Graphics graphics, Font titleFont)
        {
            while (graphics.MeasureString(title, titleFont).Width > BackgroundWidth / 2 - 20)
            {
                title = title.Remove(title.Length - 1); 
            }

            title = title + "..."; 

            return title; 
        }

        private static int GetByRank(Score score, Graphics graphics)
        {
            string rank = score.Rank.ToString().Contains("H") ? score.Rank.ToString().Replace("H", string.Empty) : score.Rank.ToString(); 
            int rankSize = (int)graphics.MeasureString(rank, rankFont).Width;

            return 900 - rankSize + 20; 
        }
    }
}
