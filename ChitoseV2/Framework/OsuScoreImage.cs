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

        private static Pen WhitePen = new Pen(Color.White, StrokeWidth * 2);

        private static Bitmap AcquireAvatar(string userId) => new Bitmap(Extensions.GetHttpStream(new Uri($"https://a.ppy.sh/{userId}")));

        private static Bitmap AcquireBackground(string beatmapId) => new Bitmap(Extensions.GetHttpStream(new Uri($"https://assets.ppy.sh/beatmaps/{beatmapId}/covers/cover.jpg")));

        public static Bitmap CreateScorePanel(User user, Score score, Beatmap beatmap)
        {
            var background = AcquireBackground(beatmap.BeatmapSetId);
            using (var graphics = Graphics.FromImage(background))
            {
                graphics.InterpolationMode = InterpolationMode.High;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                DrawWhiteOverlay(background, graphics);
                DrawAvatar(graphics, user.UserID);
                DrawTitle(graphics, beatmap.Title, beatmap.Difficulty, beatmap.Beatmapper);
                DrawUsername(user, graphics);
                DrawMods(score, graphics); 
                DrawPP(user, score, graphics); 
            }
            return background;
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

        private static void DrawTitle(Graphics graphics, string title, string difficulty, string beatmapper)
        {
            StringFormat titleFormat = new StringFormat();
            GraphicsPath titlePath = new GraphicsPath();
            titleFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip;
            titleFormat.Trimming = StringTrimming.EllipsisCharacter;
            var whitePen = new Pen(Color.White, StrokeWidth * 2);
            var maxWidth = BackgroundWidth - 40 - StrokeWidth * 2;
            var difficultyString = $"[{difficulty}]";
            var titleFont = new Font("Calibri", 36, GraphicsUnit.Point);
            var difficultySize = graphics.MeasureString(difficultyString, titleFont);
            var remainingWidth = maxWidth - difficultySize.Width;
            var titleSize = graphics.MeasureString(title, titleFont);
            titlePath.AddString(title, titleFont.FontFamily, 3, 3f, new RectangleF(10f, 10f, 30, 30), titleFormat);
            graphics.DrawString(title, titleFont, BlueBrush, 20f, 10f, titleFormat);
            titlePath.AddString(difficultyString, titleFont.FontFamily, 3, 3f, new RectangleF(10f, 10f, 30, 30), null);
            graphics.DrawString(difficultyString, titleFont, BlueBrush, 20f + graphics.MeasureString(title, titleFont).Width, 10f);
        }

        private static void DrawUsername(User user, Graphics graphics)
        {
            Font usernameFont = new Font("Calibri", 60, GraphicsUnit.Point);
            GraphicsPath usernamePath = new GraphicsPath();
            usernamePath.AddString(user.Username, usernameFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 60 / 72, new Point(20 + AvatarSize, BackgroundHeight - 100), new StringFormat());
            graphics.DrawPath(WhitePen, usernamePath);
            graphics.FillPath(BlueBrush, usernamePath);
        }

        private static void DrawPP(User user, Score score, Graphics graphics)
        {
            var ppFont = new Font("Calibri", 60, GraphicsUnit.Point);
            GraphicsPath ppPath = new GraphicsPath();
            Point ppPoint = new Point(20 + AvatarSize + (int)graphics.MeasureString(user.Username, new Font("Calibri", 60, GraphicsUnit.Point)).Width, BackgroundHeight - 100);
            ppPath.AddString(score.PP + "pp", ppFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 60 / 72, ppPoint, new StringFormat());
            graphics.DrawPath(WhitePen, ppPath);
            graphics.FillPath(PinkBrush, ppPath); 
        }

        private static void DrawMods(Score score, Graphics graphics)
        {
            string mods = score.Mods.ToString(); 
            Font modFont = new Font("Calibri", 48, GraphicsUnit.Point);
            GraphicsPath modPath = new GraphicsPath();
            modPath.AddString(mods, modFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 60 / 72, new Point(20 + AvatarSize, BackgroundHeight - 200), new StringFormat());
            graphics.DrawPath(WhitePen, modPath);
            graphics.FillPath(PinkBrush, modPath); 
        }

        private static void DrawRank(Score score, Graphics graphics)
        {
            string rank = score.Rank.ToString();
        }

        private static void DrawAcc(Score score, Graphics graphics)
        {
            string acc = $"{score.Accuracy:0.00%}";
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
    }
}
