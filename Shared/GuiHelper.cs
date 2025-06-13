using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KingdomMod
{
    public static class GuiHelper
    {
        public const int GL_TRIANGLES = 4;

        private static Material _lineMaterial;
        private static Material LineMaterial
        {
            get
            {
                if (_lineMaterial == null)
                {
                    // 使用UI/Default shader，这是Unity内置的UI shader
                    var shader = Shader.Find("UI/Default");
                    if (shader == null)
                    {
                        Debug.LogError("Can't find UI/Default shader!");
                        return null;
                    }
                    _lineMaterial = new Material(shader)
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    // 设置材质属性
                    _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    _lineMaterial.SetInt("_ZWrite", 0);
                    _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                    _lineMaterial.enableInstancing = false;
                }
                return _lineMaterial;
            }
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (LineMaterial == null)
                return;

            LineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadPixelMatrix();

            GL.Begin(GL_TRIANGLES);
            GL.Color(color);

            Vector2 dir = (end - start).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x);
            float halfThickness = thickness / 2f;
            Vector2 offset = normal * halfThickness;

            // 第一个三角形
            GL.Vertex3(start.x + offset.x, start.y + offset.y, 0);
            GL.Vertex3(start.x - offset.x, start.y - offset.y, 0);
            GL.Vertex3(end.x - offset.x, end.y - offset.y, 0);

            // 第二个三角形
            GL.Vertex3(start.x + offset.x, start.y + offset.y, 0);
            GL.Vertex3(end.x - offset.x, end.y - offset.y, 0);
            GL.Vertex3(end.x + offset.x, end.y + offset.y, 0);

            GL.End();
            GL.PopMatrix();
        }
    }

}
