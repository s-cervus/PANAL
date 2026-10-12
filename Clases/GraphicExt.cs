using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data.SQLite;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.CompilerServices;
using APCPA;

namespace APCPA
{
    public static class GraphicExt
    {
        /// <summary>
        /// Método de extensión que inyecta DoubleBuffered vía Reflexión a cualquier control protegido.
        /// </summary>
        public static void FastNativeRun(this Control control)
        {
            PropertyInfo pi = typeof(Control).GetProperty("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (pi != null)
            {
                pi.SetValue(control, true, null);
            }
        }

        /// <summary>
        /// Escudo maestro: recorre todo el formulario e inyecta el buffer doble a cada contenedor.
        /// </summary>
        public static void OpFullUI(this Page form_item)
        {
            // Función local recursiva para barrer el árbol de controles
            void ReadControls(Control mainFather)
            {
                foreach (Control child in mainFather.Controls)
                {
                    // Si el control es un panel layout (los reyes del parpadeo), lo blindamos
                    if (child is TableLayoutPanel || child is FlowLayoutPanel || child is Panel)
                    {
                        child.FastNativeRun();
                    }

                    // Si tiene más hijos dentro (Paneles anidados, GroupBoxes, pestañas), se auto-invoca
                    if (child.HasChildren)
                    {
                        ReadControls(child);
                    }
                }
            }

            // Arrancamos el barrido desde la raíz del formulario
            ReadControls(form_item);
        }

        public static void ClosesAll(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

    }
}

