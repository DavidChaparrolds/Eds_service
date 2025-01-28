namespace LDS_SERVICE_EDS
{
    partial class Lds_service_eds
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.ProcesoConteo = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.ProcesoConteo)).BeginInit();
            // 
            // ProcesoConteo
            // 
            this.ProcesoConteo.Enabled = true;
            this.ProcesoConteo.Interval = 500D;
            this.ProcesoConteo.Elapsed += new System.Timers.ElapsedEventHandler(this.ProcesoConteo_Elapsed);
            // 
            // Lds_service_eds
            // 
            this.ServiceName = "Lds_service_eds";
            ((System.ComponentModel.ISupportInitialize)(this.ProcesoConteo)).EndInit();

        }

        #endregion

        private System.Timers.Timer ProcesoConteo;
    }
}
