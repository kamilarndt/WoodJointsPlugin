using System;
using System.Collections.Generic;
using Eto.Drawing;
using Rhino.Geometry;
using WoodJointsPlugin.Models;

namespace WoodJointsPlugin.UI
{
    /// <summary>  
    /// Class for generating preview images of joints  
    /// </summary>  
    public class JointPreviewManager
    {
        // Dictionary of cached preview images for different joint types and dimensions  
        private Dictionary<string, Image> previewCache;

        public JointPreviewManager()
        {
            previewCache = new Dictionary<string, Image>();
        }

        /// <summary>  
        /// Generate a preview image for a specific joint type with dimensions  
        /// </summary>  
        public Image GetJointPreview(JointType jointType, double width, double depth, double clearance, double tailAngle = 15.0)
        {
            // Generate a unique key for caching  
            string cacheKey = $"{jointType}-{width:0.0}-{depth:0.0}-{clearance:0.00}-{tailAngle:0.0}";

            // Check if we have a cached image  
            if (previewCache.ContainsKey(cacheKey))
            {
                return previewCache[cacheKey];
            }

            // Otherwise generate a new preview  
            Image preview;

            switch (jointType)
            {
                case JointType.MortiseAndTenon:
                    preview = GenerateMortiseAndTenonPreview(width, depth, clearance);
                    break;
                case JointType.Dovetail:
                    preview = GenerateDovetailPreview(width, depth, clearance, tailAngle);
                    break;
                default:
                    preview = GenerateDefaultPreview(jointType);
                    break;
            }

            // Cache the preview  
            previewCache[cacheKey] = preview;

            return preview;
        }

        /// <summary>  
        /// Generate a preview image for a mortise and tenon joint  
        /// </summary>  
        private Image GenerateMortiseAndTenonPreview(double width, double depth, double clearance)
        {
            // Create the preview image  
            var bitmap = new Bitmap(300, 200, PixelFormat.Format32bppRgba);

            using (var g = new Graphics(bitmap))
            {
                g.Clear(Colors.Gray); // Replaced LightGray with Gray  

                // Draw a simple representation of a mortise and tenon joint  
                // Scale factors to fit the drawing in the available space  
                float scale = Math.Min(250 / (float)Math.Max(width, depth), 2.0f);
                float centerX = 150;
                float centerY = 100;
                float scaledWidth = (float)width * scale;
                float scaledDepth = (float)depth * scale;
                float scaledClearance = (float)clearance * scale;

                // Draw first piece (with mortise)  
                g.FillRectangle(Colors.LightSlateGray, centerX - scaledWidth * 1.5f, centerY - scaledDepth * 1.5f,
                                scaledWidth * 3f, scaledDepth * 3f);

                // Draw mortise hole  
                g.FillRectangle(Colors.White, centerX - scaledWidth / 2 - scaledClearance,
                               centerY - scaledDepth / 2 - scaledClearance,
                               scaledWidth + scaledClearance * 2, scaledDepth + scaledClearance * 2);

                // Draw second piece (with tenon)  
                float tenonLength = scaledDepth * 2;
                g.FillRectangle(Colors.SlateGray, centerX - scaledWidth * 1.5f, centerY + scaledDepth * 1.5f,
                                scaledWidth * 3f, tenonLength);

                // Draw tenon  
                g.FillRectangle(Colors.DarkSlateGray, centerX - scaledWidth / 2,
                               centerY - scaledDepth / 2,
                               scaledWidth, scaledDepth + tenonLength);

                // Draw title  
                g.DrawText(new Font(FontFamilies.Sans, 12, FontStyle.Bold), Colors.Black,
                           10, 10, "Czop");

                // Draw dimensions  
                g.DrawText(new Font(FontFamilies.Sans, 8), Colors.Black,
                           10, 30, $"Szerokoœæ: {width:0.0} mm");
                g.DrawText(new Font(FontFamilies.Sans, 8), Colors.Black,
                           10, 45, $"G³êbokoœæ: {depth:0.0} mm");
                g.DrawText(new Font(FontFamilies.Sans, 8), Colors.Black,
                           10, 60, $"Luz: {clearance:0.00} mm");
            }

            return bitmap;
        }

        /// <summary>  
        /// Generate a preview image for a dovetail joint  
        /// </summary>  
        private Image GenerateDovetailPreview(double width, double depth, double clearance, double tailAngle)
        {
            // Create the preview image  
            var bitmap = new Bitmap(300, 200, PixelFormat.Format32bppRgba);

            using (var g = new Graphics(bitmap))
            {
                g.Clear(Colors.Gray); // Replaced LightGray with Gray  

                // Draw a simple representation of a dovetail joint  
                // Scale factors to fit the drawing in the available space  
                float scale = Math.Min(250 / (float)Math.Max(width, depth), 2.0f);
                float centerX = 150;
                float centerY = 100;
                float scaledWidth = (float)width * scale;
                float scaledDepth = (float)depth * scale;

                // Calculate the wider end of the dovetail based on the tail angle  
                float height = scaledDepth;
                float tailWidth = (float)(Math.Tan(tailAngle * Math.PI / 180) * height);
                float widerWidth = scaledWidth + 2 * tailWidth;

                // Points for the dovetail shape  
                PointF[] dovetailPoints = new PointF[]
                {
                       new PointF(centerX - scaledWidth / 2, centerY - height / 2),
                       new PointF(centerX + scaledWidth / 2, centerY - height / 2),
                       new PointF(centerX + widerWidth / 2, centerY + height / 2),
                       new PointF(centerX - widerWidth / 2, centerY + height / 2)
                };

                // Draw dovetail  
                g.FillPolygon(Colors.SlateGray, dovetailPoints);

                // Draw base pieces  
                g.FillRectangle(Colors.LightSlateGray, centerX - widerWidth * 1.2f, centerY + height / 2, widerWidth * 2.4f, height);
                g.FillRectangle(Colors.DarkSlateGray, centerX - scaledWidth * 1.2f, centerY - height * 1.5f, scaledWidth * 2.4f, height);

                // Draw title  
                g.DrawText(new Font(FontFamilies.Sans, 12, FontStyle.Bold), Colors.Black,
                           10, 10, "Jaskó³czy ogon");

                // Draw dimensions  
                g.DrawText(new Font(FontFamilies.Sans, 8), Colors.Black,
                           10, 30, $"Szerokoœæ: {width:0.0} mm");
                g.DrawText(new Font(FontFamilies.Sans, 8), Colors.Black,
                           10, 45, $"G³êbokoœæ: {depth:0.0} mm");
                g.DrawText(new Font(FontFamilies.Sans, 8), Colors.Black,
                           10, 60, $"K¹t: {tailAngle:0.0}°");
                g.DrawText(new Font(FontFamilies.Sans, 8), Colors.Black,
                           10, 75, $"Luz: {clearance:0.00} mm");
            }

            return bitmap;
        }

        /// <summary>  
        /// Generate a default preview image for unsupported joint types  
        /// </summary>  
        private Image GenerateDefaultPreview(JointType jointType)
        {
            var bitmap = new Bitmap(300, 200, PixelFormat.Format32bppRgba);

            using (var g = new Graphics(bitmap))
            {
                g.Clear(Colors.Gray); // Replaced LightGray with Gray  
                g.DrawText(new Font(FontFamilies.Sans, 12, FontStyle.Bold), Colors.Black,
                           10, 10, $"Podgl¹d dla {jointType.ToDisplayString()} jest niedostêpny");
            }

            return bitmap;
        }

        /// <summary>  
        /// Clear the preview cache  
        /// </summary>  
        public void ClearCache()
        {
            foreach (var image in previewCache.Values)
            {
                image.Dispose();
            }
            previewCache.Clear();
        }
    }
}
